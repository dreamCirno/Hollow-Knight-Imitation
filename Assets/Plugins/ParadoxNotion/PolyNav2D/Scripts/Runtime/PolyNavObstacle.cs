using UnityEngine;
using System.Collections;


[DisallowMultipleComponent]
[AddComponentMenu("Navigation/PolyNavObstacle")]
///Place on a game object to act as an obstacle
public class PolyNavObstacle : MonoBehaviour {

	public enum ShapeType
	{
		Polygon,
		Box
	}

	///Raised when the state of the obstacle is changed (enabled/disabled).
	public static event System.Action<PolyNavObstacle, bool> OnObstacleStateChange;

	[Tooltip("The Shape used. Changing this will also change the Collider2D component type.")]
	public ShapeType shapeType = ShapeType.Polygon;
	[Tooltip("Added extra offset radius.")]
	public float extraOffset;
	[Tooltip("Inverts the polygon (done automatically if collider already exists due to a sprite).")]
	public bool invertPolygon = false;

	private Collider2D _collider;
	private Collider2D myCollider{
		get {return _collider != null? _collider : _collider = GetComponent<Collider2D>();}
	}


	///The polygon points of the obstacle
	public Vector2[] points{
		get
		{
			if (myCollider is BoxCollider2D){
				var box = (BoxCollider2D)myCollider;
				var tl = box.offset + new Vector2(-box.size.x, box.size.y)/2;
				var tr = box.offset + new Vector2(box.size.x, box.size.y)/2;
				var br = box.offset + new Vector2(box.size.x, -box.size.y)/2;
				var bl = box.offset + new Vector2(-box.size.x, -box.size.y)/2;
				return new Vector2[]{tl, tr, br, bl};
			}

			if (myCollider is PolygonCollider2D){
				Vector2[] tempPoints = (myCollider as PolygonCollider2D).points;				
				if (invertPolygon){
					System.Array.Reverse(tempPoints);
				}
				return tempPoints;			
			}

			return null;
		}
	}

	void Reset(){
		
		if (myCollider == null){
			gameObject.AddComponent<PolygonCollider2D>();
		}

		if (myCollider is PolygonCollider2D){
			shapeType = ShapeType.Polygon;
		}
		
		if (myCollider is BoxCollider2D){
			shapeType = ShapeType.Box;
		}

		myCollider.isTrigger = true;
		if (GetComponent<SpriteRenderer>() != null){
			invertPolygon = true;
		}
	}

	void OnEnable(){
		if (OnObstacleStateChange != null){
			OnObstacleStateChange(this, true);
		}
	}

	void OnDisable(){
		if (OnObstacleStateChange != null){
			OnObstacleStateChange(this, false);
		}
	}

	void Awake(){
		transform.hasChanged = false;
	}
}