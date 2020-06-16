using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PolyNavAgent))]
public class PatrolWaypoints : MonoBehaviour {

	public List<Vector2> WPoints = new List<Vector2>();
	private int currentIndex = -1;

	private PolyNavAgent _agent;
	private PolyNavAgent agent{
		get {return _agent != null? _agent : _agent = GetComponent<PolyNavAgent>();}
	}

	void OnEnable(){
		agent.OnDestinationReached += MoveNext;
		agent.OnDestinationInvalid += MoveNext;
	}

	void OnDisable(){
		agent.OnDestinationReached -= MoveNext;
		agent.OnDestinationInvalid -= MoveNext;
	}

	IEnumerator Start(){
		yield return new WaitForSeconds(1);
		if (WPoints.Count > 0){
			MoveNext();
		}
	}

	void MoveNext(){
		currentIndex = (int)Mathf.Repeat(currentIndex + 1, WPoints.Count);
		agent.SetDestination(WPoints[currentIndex]);
	}

	void OnDrawGizmosSelected(){
		for ( int i = 0; i < WPoints.Count; i++){
			Gizmos.DrawSphere(WPoints[i], 0.1f);			
		}
	}
}