using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
[AddComponentMenu("Navigation/PolyNavAgent")]
///Place this on a game object to find it's path
public class PolyNavAgent : MonoBehaviour{

	///The target PolyNav2D map this agent is assigned to.
	[SerializeField]
	private PolyNav2D _map			 = null;
	///The max speed
	public float maxSpeed            = 3.5f;
	///The object mass
	public float mass                = 20;
	///The distance to stop at from the goal
	public float stoppingDistance    = 0.1f;
	///The distance to start slowing down
	public float slowingDistance     = 1;
	///The rate at which it will accelerate
	public float accelerationRate    = 2;
	///The rate at which it will slow down
	public float decelerationRate    = 2;
	///The lookahead distance for Slowing down and agent avoidance.
	///Set to 0 to eliminate the slowdown but the avoidance too, as well as increase performance.
	public float lookAheadDistance   = 1;
	///The avoidance radius of the agent. 0 for no avoidance	
	public float avoidRadius         = 0;
	///Custom center offset from original transform position.
	public Vector2 centerOffset      = Vector2.zero;
	///Rotate transform as well?
	public bool rotateTransform      = false;
	///Speed to rotate at moving direction.
	public float rotateSpeed         = 350;
	///Should the agent repath? Disable for performance.
	public bool repath 				 = true;
	///Should the agent be forced restricted within valid areas? Disable for performance.
	public bool restrict 			 = false;
	///Go to closer point if requested destination is invalid? Disable for performance.
	public bool closerPointOnInvalid = true;
	///Will debug the path (gizmos). Disable for performance.
	public bool debugPath            = true;


	///Raised when a new destination is started after path found
	public event System.Action OnNavigationStarted;
	///Raised when the destination is reached
	public event System.Action OnDestinationReached;
	///Raised when the destination is or becomes invalid
	public event System.Action OnDestinationInvalid;
	///Raised when a "corner" point has been reached while traversing the path
	public event System.Action<Vector2> OnNavigationPointReached;

	private event System.Action<bool> reachedCallback;


	private Vector2 velocity          = Vector2.zero;
	private float maxForce            = 100;
	private int requests              = 0;
	private List<Vector2> _activePath = new List<Vector2>();
	private Vector2 _primeGoal        = Vector2.zero;
	private float accelerationValue = 0;

	private static List<PolyNavAgent> allAgents = new List<PolyNavAgent>();

	///The position of the agent
	public Vector2 position{
		get {return transform.position + (Vector3)centerOffset;}
		set {transform.position = new Vector3(value.x, value.y, transform.position.z) - (Vector3)centerOffset;}
	}

	///The current active path of the agent
	public List<Vector2> activePath{
		get { return _activePath; }
		set
		{
			_activePath = value;
			if (_activePath.Count > 0 && _activePath[0] == position){
				_activePath.RemoveAt(0);
			}
		}
	}

	///The current goal of the agent
	public Vector2 primeGoal{
		get { return _primeGoal;}
		set { _primeGoal = value;}
	}

	///Is a path pending?
	public bool pathPending{
		get	{ return requests > 0;}
	}

	///The PolyNav map instance the agent is assigned to.
	public PolyNav2D map{
		get { return _map != null? _map : PolyNav2D.current; }
		set {_map = value;}
	}

	///Does the agent has a path?
	public bool hasPath{
		get { return activePath.Count > 0;}
	}

	///The point that the agent is currenty going to. Returns the agent position if no active path
	public Vector2 nextPoint{
		get { return hasPath? activePath[0] : position; }
	}

	///The remaining distance of the active path. 0 if none
	public float remainingDistance{
		get
		{
			if (!hasPath){
				return 0;
			}

			float dist= Vector2.Distance(position, activePath[0]);
			for (int i= 0; i < activePath.Count; i++){
				dist += Vector2.Distance(activePath[i], activePath[i == activePath.Count - 1? i : i + 1]);
			}

			return dist;			
		}
	}

	///The moving direction of the agent
	public Vector2 movingDirection{
		get { return hasPath? velocity.normalized : Vector2.zero; }
	}

	///The current speed of the agent
	public float currentSpeed{
		get {return velocity.magnitude;}
	}

	//////
	//////

	void OnEnable(){ allAgents.Add(this); }
	void OnDisable(){ allAgents.Remove(this); }
	void Awake(){ primeGoal = position; }

	///Set the destination for the agent. As a result the agent starts moving
	public bool SetDestination(Vector2 goal){ return SetDestination(goal, null); }

	///Set the destination for the agent. As a result the agent starts moving. Only the callback from the last SetDestination will be called upon arrival
	public bool SetDestination(Vector2 goal, Action<bool> callback){

		if (map == null){
			Debug.LogError("No PolyNav2D assigned or in scene!");
			return false;
		}

		//goal is almost the same as the last goal. Nothing happens for performace in case it's called frequently
		if ((goal - primeGoal).sqrMagnitude < Mathf.Epsilon){
			return true;
		}

		reachedCallback = callback;
		primeGoal = goal;

		//goal is almost the same as agent position. We consider arrived immediately
		if ((goal - position).sqrMagnitude < stoppingDistance){
			OnArrived();
			return true;
		}

		//check if goal is valid
		if (!map.PointIsValid(goal)){
			if (closerPointOnInvalid){
				SetDestination(map.GetCloserEdgePoint(goal), callback);
				return true;
			} else {
				OnInvalid();
				return false;
			}
		}

		//if a path is pending dont calculate new path
		//the prime goal will be repathed anyway
		if (requests > 0){
			return true;
		}

		//compute path
		requests++;
		map.FindPath(position, goal, SetPath);

		return true;
	}

	///Clears the path and as a result the agent is stop moving
	public void Stop(){
		activePath.Clear();
		velocity = Vector2.zero;
		requests = 0;
		primeGoal = position;
	}


	//the callback from map for when path is ready to use
	void SetPath(Vector2[] path){
		
		//in case the agent stoped somehow, but a path was pending
		if (requests == 0){
			return;
		}

		requests --;

		if (path == null || path.Length == 0){
			OnInvalid();
			return;
		}

		activePath = path.ToList();
		if (OnNavigationStarted != null){
			OnNavigationStarted();
		}
	}

	//main loop
	void LateUpdate(){

		if (map == null){
			return;
		}

		//when there is no path just restrict
		if (!hasPath){
			Restrict();
			return;
		}

		if (maxSpeed <= 0){
			return;
		}

		//calculate velocities
		if (remainingDistance < slowingDistance){
			
			accelerationValue = 0;
			velocity += Arrive(nextPoint) / mass;

		} else {

			velocity += Seek(nextPoint) / mass;
			accelerationValue += accelerationRate * Time.deltaTime;
			accelerationValue = Mathf.Clamp01(accelerationValue);
			velocity *= accelerationValue;
		}

		velocity = Truncate(velocity, maxSpeed);
		//

		//slow down if wall ahead
		LookAhead();

		//move the agent
		position += velocity * Time.deltaTime;

		//restrict just after movement
		Restrict();

		//rotate if must
		if (rotateTransform){
			float rot = -Mathf.Atan2(movingDirection.x, movingDirection.y) * 180 / Mathf.PI;
			float newZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, rot, rotateSpeed * Time.deltaTime);
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, newZ);
		}

		if (repath){

			//repath if there is no LOS with the next point
			if (map.CheckLOS(position, nextPoint) == false){
				Repath();
			}

			//in case just after repath-ing there is no path
			if (!hasPath){
				OnArrived();
				return;
			}
		}

		//Check and remove if we reached a point. proximity distance depends
		float proximity = (activePath[activePath.Count -1] == nextPoint)? stoppingDistance : 0.05f;
		if ((position - nextPoint).sqrMagnitude <= proximity){

			activePath.RemoveAt(0);

			//if it was last point, means the path is complete and no longer have an active path.
			if (!hasPath){
			
				OnArrived();
				return;
			
			} else {

				if (repath){
					//repath after a point is reached
					Repath();
				}

				if (OnNavigationPointReached != null){
					OnNavigationPointReached(position);
				}
			}
		}

		//little trick. Check the next waypoint ahead of the current for LOS and if true consider the current reached.
		//helps for tight corners and when agent has big innertia
		if (activePath.Count > 1 && map.CheckLOS(position, activePath[1])){
			activePath.RemoveAt(0);
			if (OnNavigationPointReached != null){
				OnNavigationPointReached(position);
			}
		}
	}


	//recalculate path to prime goal if there is no pending requests
	void Repath(){

		if (requests > 0){
			return;
		}

		requests ++;
		map.FindPath(position, primeGoal, SetPath);
	}

	//stop the agent and callback + message
	void OnArrived(){

		Stop();

		if (reachedCallback != null){
			reachedCallback(true);
		}

		if (OnDestinationReached != null){
			OnDestinationReached();
		}
	}

	//stop the agent and callback + message
	void OnInvalid(){

		Stop();

		if (reachedCallback != null){
			reachedCallback(false);
		}

		if (OnDestinationInvalid != null){
			OnDestinationInvalid();
		}
	}

	
	//seeking a target
	Vector2 Seek(Vector2 pos){

		Vector2 desiredVelocity= (pos - position).normalized * maxSpeed;
		Vector2 steer= desiredVelocity - velocity;
		steer = Truncate(steer, maxForce);
		return steer;
	}

	//slowing at target's arrival
	Vector2 Arrive(Vector2 pos){

		var desiredVelocity = (pos - position);
		float dist= desiredVelocity.magnitude;

		if (dist > 0){
			var reqSpeed = dist / (decelerationRate * 0.3f);
			reqSpeed = Mathf.Min(reqSpeed, maxSpeed);
			desiredVelocity *= reqSpeed / dist;
		}

		Vector2 steer = desiredVelocity - velocity;
		steer = Truncate(steer, maxForce);
		return steer;
	}

	//slowing when there is an obstacle ahead.
	void LookAhead(){

		//if agent is outside dont LookAhead since that causes agent to constantely be slow.
		if (lookAheadDistance <= 0 || !map.PointIsValid(position)){
			return;
		}

		var currentLookAheadDistance= Mathf.Lerp(0, lookAheadDistance, velocity.magnitude/maxSpeed);
		var lookAheadPos= position + velocity.normalized * currentLookAheadDistance;

		Debug.DrawLine(position, lookAheadPos, Color.blue);

		if (!map.PointIsValid(lookAheadPos)){
			velocity -= (lookAheadPos - position);
		}

		if (avoidRadius > 0){
			
			for (int i = 0; i < allAgents.Count; i++){
				var otherAgent = allAgents[i];
				if (otherAgent == this || otherAgent.avoidRadius <= 0){
					continue;
				}

				var mlt = otherAgent.avoidRadius + this.avoidRadius;
				var dist = ( lookAheadPos - otherAgent.position ).magnitude;
				var str = ( lookAheadPos - otherAgent.position ).normalized * mlt;
				var steer = Vector3.Lerp( (Vector3)str, Vector3.zero, dist/mlt);
				velocity += (Vector2)steer;

				Debug.DrawLine(otherAgent.position, otherAgent.position + str, new Color(1,0,0,0.1f));
			}
		}
	}

	//keep agent within valid area
	void Restrict(){

		if (!restrict){
			return;
		}

		if (!map.PointIsValid(position)){
			position = map.GetCloserEdgePoint(position);
		}
	}
    
    //limit the magnitude of a vector
    Vector2 Truncate(Vector2 vec, float max){
        if (vec.magnitude > max) {
            vec.Normalize();
            vec *= max;
        }
        return vec;
    }


    ////////////////////////////////////////
    ///////////GUI AND EDITOR STUFF/////////
    ////////////////////////////////////////
    
    void OnDrawGizmos(){

    	Gizmos.color = new Color(1,1,1,0.1f);
    	Gizmos.DrawWireSphere(position, avoidRadius);

    	if (!hasPath){
    		return;
    	}

		if (debugPath){
			Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
			Gizmos.DrawLine(position, activePath[0]);
			for (int i= 0; i < activePath.Count; i++){
				Gizmos.DrawLine(activePath[i], activePath[(i == activePath.Count - 1)? i : i + 1]);
			}
		}	
    }
}
