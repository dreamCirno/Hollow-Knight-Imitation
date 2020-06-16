using UnityEngine;
using System.Collections.Generic;
using Polygon = PolyNav2D.Polygon;
using PolyMap = PolyNav2D.PolyMap;

//You can provide a custom PolyMap to PolyNav for usage. Doing this disregards all PolyNavObstacles.

[ExecuteInEditMode] // <- This is not required of course, but only to visualize the resulting map in editor.
public class CustomMapProvider : MonoBehaviour {

	void OnEnable(){

		//Clockwise points of master polygon.
		var masterPolyPoints = new Vector2[]{ new Vector2(-10, -10), new Vector2(10, -10), new Vector2(10, 10), new Vector2(-10, 10) };
		var masterPoly = new Polygon(masterPolyPoints);


		var obstaclePolygons = new List<Polygon>();

		//Counter-Clokwise points of obstacle polygon.
		var obstaclePoints = new Vector2[]{ new Vector2(-5, -5), new Vector2(-5, 5), new Vector2(5, 5), new Vector2(5, -5) };
		var obstaclePoly = new Polygon(obstaclePoints);
		obstaclePolygons.Add(obstaclePoly);
		
		///Create more polys here
		///Create more polys here


		//Create the PolyMap.
		var map = new PolyMap(masterPoly, obstaclePolygons.ToArray());

		//Provide the map.
		PolyNav2D.current.SetCustomMap(map);
	}
}
