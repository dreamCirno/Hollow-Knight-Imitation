using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using PathNode = PolyNav2D.PathNode;


///Calculates paths using A*
static class AStar {

	//A* implementation
	public static void CalculatePath(PathNode startNode, PathNode endNode, List<PathNode> allNodes, Action<Vector2[]> callback){
		var path = Internal_CalculatePath(startNode, endNode, allNodes);
		callback( path );
	}

	private static Vector2[] Internal_CalculatePath(PathNode startNode, PathNode endNode, List<PathNode> allNodes){
		
		var openList = new Heap<PathNode>(allNodes.Count);
		var closedList = new HashSet<PathNode>();
		var success = false;

		openList.Add(startNode);

		while (openList.Count > 0){

			var currentNode = openList.RemoveFirst();
			if (currentNode == endNode){
				success = true;
				break;
			}

			closedList.Add(currentNode);

			var linkIndeces = currentNode.links;
			for (var i = 0; i < linkIndeces.Count; i++){
				var neighbour = allNodes[ linkIndeces[i] ];

				if (closedList.Contains(neighbour)){
					continue;
				}

				var costToNeighbour = currentNode.gCost + GetDistance( currentNode, neighbour );
				if (costToNeighbour < neighbour.gCost || !openList.Contains(neighbour) ){
					neighbour.gCost = costToNeighbour;
					neighbour.hCost = GetDistance(neighbour, endNode);
					neighbour.parent = currentNode;

					if (!openList.Contains(neighbour)){
						openList.Add(neighbour);
						openList.UpdateItem(neighbour);
					}
				}
			}
		}

		if (success){ //Retrace Path if one exists
			var path = new List<Vector2>();
			var currentNode = endNode;
			while(currentNode != startNode){
				path.Add(currentNode.pos);
				currentNode = currentNode.parent;
			}
			path.Add(startNode.pos);
			path.Reverse();
			return path.ToArray();
		}

		return null;
	}

	private static float GetDistance(PathNode a, PathNode b){
		return (a.pos - b.pos).magnitude;
	}
}