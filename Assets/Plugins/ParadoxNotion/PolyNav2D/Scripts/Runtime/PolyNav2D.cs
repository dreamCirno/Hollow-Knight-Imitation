using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

///Main class for map generation and navigation
public class PolyNav2D : MonoBehaviour {

	[Tooltip("All PolyNavObstacles in the selected layers will be monitored and added/removed automatically.")]
	public LayerMask obstaclesMask = -1;
	[Tooltip("The frame interval to auto update obstacle chages (enabled/disable or transform change). Set 0 to disable auto-regenerate.")]
	public int autoRegenerateInterval = 60;
	[Tooltip("The radius from the edges to offset the agents.")]
	public float radiusOffset = 0.1f;


	private List<PolyNavObstacle> navObstacles;
	private bool useCustomMap;
	private PolyMap map;
	private List<PathNode> nodes = new List<PathNode>();
	private PathNode[] tempNodes;

	private Queue<PathRequest> pathRequests = new Queue<PathRequest>();
	private PathRequest currentRequest;
	private bool isProcessingPath;
	private PathNode startNode;
	private PathNode endNode;
	private bool regenerateFlag;


	private Collider2D _masterCollider;
	private Collider2D masterCollider{
		get { return _masterCollider != null? _masterCollider : _masterCollider = GetComponent<Collider2D>(); }
	}


	///The current / first instance of PolyNav2D
	private static PolyNav2D _current;
	public static PolyNav2D current{
		get
		{
			if (_current == null || !Application.isPlaying){
				_current = FindObjectOfType<PolyNav2D>();
			}
			
			return _current;
		}
	}

	///The total nodes count of the map
	public int nodesCount{
		get {return nodes.Count;}
	}

	///Is the pathfinder currently processing a pathfinding request?
	public bool pendingRequest{
		get {return isProcessingPath;}
	}

	void Awake(){
		if (_current == null){
			_current = this;
		}
		regenerateFlag = false;
		isProcessingPath = false;				
		navObstacles = FindObjectsOfType<PolyNavObstacle>().Where( o => obstaclesMask == (obstaclesMask | 1 << o.gameObject.layer) ).ToList();
		PolyNavObstacle.OnObstacleStateChange += MonitorObstacle;
		if (masterCollider != null){
			masterCollider.enabled = false;
			GenerateMap(true);
		}
	}

	void LateUpdate(){
		if (useCustomMap || autoRegenerateInterval <= 0){
			return;
		}

		if (Time.frameCount % autoRegenerateInterval != 0){
			return;
		}

		for (var i = 0; i < navObstacles.Count; i++){
			var obstacle = navObstacles[i];
			if (obstacle.transform.hasChanged){
				obstacle.transform.hasChanged = false;
				regenerateFlag = true;
			}
		}

		if (regenerateFlag == true){
			regenerateFlag = false;
			GenerateMap(false);
		}
	}

	void MonitorObstacle(PolyNavObstacle obstacle, bool active){
		if ( obstaclesMask == (obstaclesMask | 1 << obstacle.gameObject.layer) ){
			if (active){ AddObstacle(obstacle); }
			else { RemoveObstacle(obstacle); }
		}		
	}

	///Adds a PolyNavObstacle to the map.
	void AddObstacle( PolyNavObstacle navObstacle ){
		if (!navObstacles.Contains(navObstacle)){
			navObstacles.Add(navObstacle);
			regenerateFlag = true;
		}
	}

	///Removes a PolyNavObstacle from the map.
	void RemoveObstacle ( PolyNavObstacle navObstacle ){
		if (navObstacles.Contains(navObstacle)){
			navObstacles.Remove(navObstacle);
			regenerateFlag = true;
		}
	}

	///Generate the map
	public void GenerateMap(){GenerateMap(true);}
	public void GenerateMap(bool generateMaster){
		CreatePolyMap(generateMaster);
		CreateNodes();
		LinkNodes(nodes);
	}

	
	//Use this to provide a custom map for generation	
	public void SetCustomMap(PolyMap map){
		useCustomMap = true;
		this.map = map;
		CreateNodes();
		LinkNodes(nodes);
	}



	///Find a path 'from' and 'to', providing a callback for when path is ready containing the path.
	public void FindPath(Vector2 start, Vector2 end, System.Action<Vector2[]> callback){

		if (CheckLOS(start, end)){
			callback( new Vector2[]{start, end} );
			return;
		}

		pathRequests.Enqueue( new PathRequest(start, end, callback) );
		TryNextFindPath();
	}

	//Pathfind next request
	void TryNextFindPath(){

		if (isProcessingPath || pathRequests.Count <= 0){
			return;
		}

		isProcessingPath = true;
		currentRequest = pathRequests.Dequeue();

		if (!PointIsValid(currentRequest.start)){
			currentRequest.start = GetCloserEdgePoint(currentRequest.start);
		}

		//create start & end as temp nodes
		startNode = new PathNode(currentRequest.start);
		endNode = new PathNode(currentRequest.end);

		nodes.Add(startNode);
		LinkStart(startNode, nodes);

		nodes.Add(endNode);
		LinkEnd(endNode, nodes);

		AStar.CalculatePath(startNode, endNode, nodes, RequestDone);
	}


	//Pathfind request finished (path found or not)
	void RequestDone(Vector2[] path){

		//Remove temp start and end nodes
		for (int i = 0; i < endNode.links.Count; i++){
			nodes[ endNode.links[i] ].links.Remove(nodes.IndexOf(endNode));
		}
		nodes.Remove(endNode);
		nodes.Remove(startNode);
		//			

		isProcessingPath = false;
		currentRequest.callback(path);
		TryNextFindPath();
	}




	//helper function
	Vector2[] TransformPoints ( Vector2[] points, Transform t ){
		for (int i = 0; i < points.Length; i++){
			points[i] = t.TransformPoint(points[i]);
		}
		return points;
	}

	//takes all colliders points and convert them to usable stuff
	void CreatePolyMap(bool generateMaster){

		var masterPolys = new List<Polygon>();
		var obstaclePolys = new List<Polygon>();

		//create a polygon object for each obstacle
		for (int i = 0; i < navObstacles.Count; i++){
			var obstacle = navObstacles[i];
			if (obstacle == null){
				continue;
			}
			var rad = (radiusOffset + obstacle.extraOffset) * (obstacle.invertPolygon? -1 : 1);
			var points = InflatePolygon(obstacle.points, rad);
			var transformedPoints = TransformPoints(points, obstacle.transform);
			obstaclePolys.Add(new Polygon(transformedPoints));
		}

		if (generateMaster){

			if (masterCollider is PolygonCollider2D){

				var polyCollider = (PolygonCollider2D)masterCollider;
				//invert the main polygon points so that we save checking for inward/outward later (for Inflate)
				var reversed = new List<Vector2>();
				
				for (int i = 0; i < polyCollider.pathCount; ++i){

					for (int p = 0; p < polyCollider.GetPath(i).Length; ++p){
						reversed.Add( polyCollider.GetPath(i)[p] );
					}
					
					reversed.Reverse();

					var transformed = TransformPoints(reversed.ToArray(), polyCollider.transform);
					var inflated = InflatePolygon(transformed, Mathf.Max(0.01f, radiusOffset) );
				
					masterPolys.Add(new Polygon(inflated));
					reversed.Clear();
				}

			} else if (masterCollider is BoxCollider2D){
				var box = (BoxCollider2D)masterCollider;
				var tl = box.offset + new Vector2(-box.size.x, box.size.y)/2;
				var tr = box.offset + new Vector2(box.size.x, box.size.y)/2;
				var br = box.offset + new Vector2(box.size.x, -box.size.y)/2;
				var bl = box.offset + new Vector2(-box.size.x, -box.size.y)/2;
				var transformed = TransformPoints(new Vector2[]{tl, bl, br, tr}, masterCollider.transform);
				var inflated = InflatePolygon(transformed, Mathf.Max(0.01f, radiusOffset));
				masterPolys.Add(new Polygon(inflated) );
			}
		
		} else {

			if (map != null){
				masterPolys = map.masterPolygons.ToList();
			}
		}

		//create the main polygon map (based on inverted) also containing the obstacle polygons
		map = new PolyMap(masterPolys.ToArray(), obstaclePolys.ToArray());

		//
		//The colliders are never used again after this point. They are simply a drawing method.
		//
	}


	//Create Nodes at convex points (since master poly is inverted, it will be concave for it) if they are valid
	void CreateNodes (){

		nodes.Clear();

		for (int p = 0; p < map.allPolygons.Length; p++){
			var poly = map.allPolygons[p];
			//Inflate even more for nodes, by a marginal value to allow CheckLOS between them
			Vector2[] inflatedPoints = InflatePolygon(poly.points, 0.05f);
			for (int i = 0; i < inflatedPoints.Length; i++){

				//if point is concave dont create a node
				if (PointIsConcave(inflatedPoints, i)){
					continue;
				}

				//if point is not in valid area dont create a node
				if (!PointIsValid(inflatedPoints[i])){
					continue;
				}

				nodes.Add(new PathNode(inflatedPoints[i]));
			}
		}
	}

	//link the nodes provided
	void LinkNodes(List<PathNode> nodeList){

		for (int a = 0; a < nodeList.Count; a++){

			nodeList[a].links.Clear();

			for (int b = 0; b < nodeList.Count; b++){

				if (b > a){
					continue;
				}
				
				if (nodeList[a] == nodeList[b]){
					continue;
				}

				if (CheckLOS(nodeList[a].pos, nodeList[b].pos)){
					nodeList[a].links.Add(b);
					nodeList[b].links.Add(a);
				}
			}
		}
	}

	
	//Link the start node in
	void LinkStart(PathNode start, List<PathNode> toNodes){
		for (int i = 0; i < toNodes.Count; i++){
			if (CheckLOS(start.pos, toNodes[i].pos)){
				start.links.Add(i);
			}			
		}
	}

	//Link the end node in
	void LinkEnd(PathNode end, List<PathNode> toNodes){
		for (int i = 0; i < toNodes.Count; i++){
			if (CheckLOS(end.pos, toNodes[i].pos)){
				end.links.Add(i);
				toNodes[i].links.Add(toNodes.IndexOf(end));
			}			
		}
	}


	///Determine if 2 points see each other.
	public bool CheckLOS (Vector2 posA, Vector2 posB){

		if ( (posA - posB).sqrMagnitude < Mathf.Epsilon ){
			return true;
		}
/*
		if (Physics2D.CircleCast(posA, radiusOffset/2, (posB - posA).normalized, (posA - posB).magnitude, obstaclesMask.value)){
			return false;
		}
		return true;
*/

		for (int i = 0; i < map.allPolygons.Length; i++){
			var poly = map.allPolygons[i];
			for (int j = 0; j < poly.points.Length; j++){
				if (SegmentsCross(posA, posB, poly.points[j], poly.points[(j + 1) % poly.points.Length])){
					return false;
				}
			}
		}
		return true;

	}

	///determine if a point is within a valid (walkable) area.
	public bool PointIsValid (Vector2 point){
/*
		if (Physics2D.OverlapCircle(point, radiusOffset/2, obstaclesMask.value) != null){
			return false;
		}
		return true;
*/
		for (int i = 0; i < map.allPolygons.Length; i++){
			if (i == 0? !PointInsidePolygon(map.allPolygons[i].points, point) : PointInsidePolygon(map.allPolygons[i].points, point)){
				return false;
			}
		}
		return true;
	}


	///Kind of scales a polygon based on it's vertices average normal.
	public static Vector2[] InflatePolygon(Vector2[] points, float dist){

		var inflatedPoints = new Vector2[points.Length];

		for (int i = 0; i < points.Length; i++){
			var a = points[i == 0? points.Length - 1 : i - 1];
			var b = points[i];
			var c = points[(i + 1) % points.Length];

			var ab = (a-b).normalized;
			var cb = (c-b).normalized;
			//var mid = (ab+cb).normalized + (ab+cb);
			var mid = (ab+cb);
			mid *= (!PointIsConcave(points, i)? -dist : dist);

			inflatedPoints[i] = (points[i] + mid);
		}

		return inflatedPoints;
	}

	///Check if or not a point is concave to the polygon points provided
	public static bool PointIsConcave(Vector2[] points, int pointIndex){

		Vector2 current = points[pointIndex];
		Vector2 next = points[(pointIndex + 1) % points.Length];
		Vector2 previous =  points[pointIndex == 0? points.Length - 1 : pointIndex - 1];

		Vector2 left = new Vector2(current.x - previous.x, current.y - previous.y);
		Vector2 right = new Vector2(next.x - current.x, next.y - current.y);

		float cross = (left.x * right.y) - (left.y * right.x);

		return cross > 0;
	}

	///Check intersection of two segments, each defined by two vectors.
	public static bool SegmentsCross (Vector2 a, Vector2 b, Vector2 c, Vector2 d){

		float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));

		if (denominator == 0){
			return false;
		}

	    float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
	    float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

	    if (numerator1 == 0 || numerator2 == 0){
	    	return false;
	    }

	    float r = numerator1 / denominator;
	    float s = numerator2 / denominator;

	    return (r > 0 && r < 1) && (s > 0 && s < 1);
	}

	///Is a point inside a polygon?
	public static bool PointInsidePolygon(Vector2[] polyPoints, Vector2 point){

		float xMin = 0;
		for (int i = 0; i < polyPoints.Length; i++){
			xMin = Mathf.Min(xMin, polyPoints[i].x);
		}

		Vector2 origin = new Vector2(xMin - 0.1f, point.y);
		int intersections = 0;

		for (int i = 0; i < polyPoints.Length; i++){

			Vector2 pA = polyPoints[i];
			Vector2 pB = polyPoints[(i + 1) % polyPoints.Length];

			if (SegmentsCross(origin, point, pA, pB)){
				intersections ++;
			}
		}

		return (intersections & 1) == 1;
	}

	///Finds the closer edge point to the navigation valid area
	public Vector2 GetCloserEdgePoint ( Vector2 point ){

		var possiblePoints= new List<Vector2>();
		var closerVertex = Vector2.zero;
		var closerVertexDist = Mathf.Infinity;

		Vector2[] inflatedPoints = null;
		for (int p = 0; p < map.allPolygons.Length; p++){

			var poly = map.allPolygons[p];
			inflatedPoints = InflatePolygon(poly.points, 0.01f);

			for (int i = 0; i < inflatedPoints.Length; i++){

				Vector2 a = inflatedPoints[i];
				Vector2 b = inflatedPoints[(i + 1) % inflatedPoints.Length];

				Vector2 originalA = poly.points[i];
				Vector2 originalB = poly.points[(i + 1) % poly.points.Length];
				
				Vector2 proj = (Vector2)Vector3.Project( (point - a), (b - a) ) + a;

				if (SegmentsCross(point, proj, originalA, originalB) && PointIsValid(proj)){
					possiblePoints.Add(proj);
				}

				float dist = (point - inflatedPoints[i]).sqrMagnitude;
				if ( dist < closerVertexDist && PointIsValid(inflatedPoints[i])){
					closerVertexDist = dist;
					closerVertex = inflatedPoints[i];
				}
			}
		}

		possiblePoints.Add(closerVertex);
		//possiblePoints = possiblePoints.OrderBy(vector => (point - vector).sqrMagnitude).ToArray(); //Not supported in iOS?
		//return possiblePoints[0];

		var closerDist = Mathf.Infinity;
		var index = 0;
		for (int i = 0; i < possiblePoints.Count; i++){
			var dist = (point - possiblePoints[i]).sqrMagnitude;
			if (dist < closerDist){
				closerDist = dist;
				index = i;
			}
		}
		Debug.DrawLine(point, possiblePoints[index]);
		return possiblePoints[index];
	}





	////////////////////////////////////////
	/////////////STRUCTS - CLASSES//////////
	////////////////////////////////////////

	///Defines the main navigation polygon and its sub obstacle polygons
	public class PolyMap{

		public Polygon[] masterPolygons;
		public Polygon[] obstaclePolygons;
		public Polygon[] allPolygons{get; private set;}

		public PolyMap(Polygon[] masterPolys, params Polygon[] obstaclePolys){
			masterPolygons = masterPolys;
			obstaclePolygons = obstaclePolys;
			var temp = new List<Polygon>();
			temp.AddRange(masterPolys);
			temp.AddRange(obstaclePolys);
			allPolygons = temp.ToArray();
		}

		public PolyMap(Polygon masterPoly, params Polygon[] obstaclePolys){
			masterPolygons = new Polygon[]{ masterPoly };
			obstaclePolygons = obstaclePolys;
			var temp = new List<Polygon>();
			temp.Add(masterPoly);
			temp.AddRange(obstaclePolys);
			allPolygons = temp.ToArray();			
		}
	}

	///Defines a polygon
	public struct Polygon{
		public Vector2[] points;
		public Polygon(Vector2[] points){
			this.points = points;
		}
	}

	//used for internal path requests
	struct PathRequest{
		public Vector2 start;
		public Vector2 end;
		public Action<Vector2[]> callback;

		public PathRequest(Vector2 start, Vector2 end, Action<Vector2[]> callback){
			this.start = start;
			this.end = end;
			this.callback = callback;
		}
	}

	//defines a node for A*
	public class PathNode : IHeapItem<PathNode>{

		public Vector2 pos;
		public List<int> links;
		public float gCost;
		public float hCost;
		public PathNode parent;

		public PathNode(Vector2 pos){
			this.pos = pos;
			this.links = new List<int>();
			this.gCost = 1f;
			this.hCost = 0f;
			this.parent = null;
		}

		public float fCost{
			get { return gCost + hCost; }
		}

		int IHeapItem<PathNode>.heapIndex{ get; set; }

		int IComparable<PathNode>.CompareTo ( PathNode other ){
			int compare = fCost.CompareTo(other.fCost);
			if (compare == 0){
				compare = hCost.CompareTo(other.hCost);
			}
			return -compare;
		}
	}





	////////////////////////////////////////
	///////////GUI AND EDITOR STUFF/////////
	////////////////////////////////////////
	#if UNITY_EDITOR
	
	void OnDrawGizmos (){

		//the original drawn polygons
		if (!useCustomMap){

			if (!Application.isPlaying){
				navObstacles = FindObjectsOfType<PolyNavObstacle>().Where( o => obstaclesMask == (obstaclesMask | 1 << o.gameObject.layer) ).ToList();
				CreatePolyMap(true);
			}

			if (masterCollider is PolygonCollider2D){
				var polyCollider = (PolygonCollider2D)masterCollider;
				for ( int i = 0; i < polyCollider.pathCount; ++i ) {
		            for ( int p = 0; p < polyCollider.GetPath(i).Length; ++p ){
		                DebugDrawPolygon( TransformPoints( polyCollider.GetPath(i), polyCollider.transform ), Color.green );
		            }
		        }
	        
	        } else if (masterCollider is BoxCollider2D){
	        	var box = masterCollider as BoxCollider2D;
				var tl = box.offset + new Vector2(-box.size.x, box.size.y)/2;
				var tr = box.offset + new Vector2(box.size.x, box.size.y)/2;
				var br = box.offset + new Vector2(box.size.x, -box.size.y)/2;
				var bl = box.offset + new Vector2(-box.size.x, -box.size.y)/2;
	        	DebugDrawPolygon(TransformPoints(new Vector2[]{tl, tr, br, bl}, masterCollider.transform), Color.green);
	        }

			foreach(var obstacle in navObstacles){
				if (obstacle != null){
					DebugDrawPolygon(TransformPoints(obstacle.points, obstacle.transform), new Color(1, 0.7f, 0.7f));
				}
			}

	    }
        //


		//the inflated actualy used polygons
		if (map != null){
	        foreach (Polygon pathPoly in map.masterPolygons){
	        	DebugDrawPolygon(pathPoly.points, Color.grey);
	        }

			foreach(Polygon poly in map.obstaclePolygons){
				DebugDrawPolygon(poly.points, Color.grey);
			}
		}
		//
	}

	//helper debug function
	void DebugDrawPolygon(Vector2[] points, Color color){
		for (int i = 0; i < points.Length; i++){
			Gizmos.color = color;
			Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
			Gizmos.color = Color.white;
		}
	}

	[UnityEditor.MenuItem("GameObject/Other/PolyNav Map")]
	[UnityEditor.MenuItem("Tools/ParadoxNotion/PolyNav/Create PolyNav Map")]
	public static void Create(){
		var newNav = new GameObject("@PolyNav2D").AddComponent<PolyNav2D>();
		var c = newNav.gameObject.AddComponent<PolygonCollider2D>();
		c.enabled = false;
		UnityEditor.Selection.activeObject = newNav;
	}

	[UnityEditor.MenuItem("GameObject/Other/PolyNav Obstacle")]
	[UnityEditor.MenuItem("Tools/ParadoxNotion/PolyNav/Create PolyNav Obstacle")]
	public static void CreatePolyNavObstacle(){
		var obs = new GameObject("PolyNavObstacle").AddComponent<PolyNavObstacle>();
		UnityEditor.Selection.activeObject = obs;
	}

	#endif

}