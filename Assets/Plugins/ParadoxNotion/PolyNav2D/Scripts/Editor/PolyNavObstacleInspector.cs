using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PolyNavObstacle))]
public class PolyNavObstacleInspector : Editor {

	private PolyNavObstacle obstacle{
		get {return target as PolyNavObstacle;}
	}

	public override void OnInspectorGUI(){

		base.OnInspectorGUI();

		if (GUI.changed){
			EditorApplication.delayCall += CheckChangeType;
		}
	}

	void CheckChangeType(){
		var collider = obstacle.GetComponent<Collider2D>();
		if (obstacle.shapeType == PolyNavObstacle.ShapeType.Polygon && !(collider is PolygonCollider2D) ){
			UnityEditor.Undo.DestroyObjectImmediate(collider);
			var col = obstacle.gameObject.AddComponent<PolygonCollider2D>();
			UnityEditor.Undo.RegisterCreatedObjectUndo(col, "Change Shape Type");
		}

		if (obstacle.shapeType == PolyNavObstacle.ShapeType.Box && !(collider is BoxCollider2D) ){
			UnityEditor.Undo.DestroyObjectImmediate(collider);
			var col = obstacle.gameObject.AddComponent<BoxCollider2D>();
			UnityEditor.Undo.RegisterCreatedObjectUndo(col, "Change Shape Type");
		}			
	}
}
