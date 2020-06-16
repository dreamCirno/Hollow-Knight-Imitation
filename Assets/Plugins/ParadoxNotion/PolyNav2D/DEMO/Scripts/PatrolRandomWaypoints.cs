using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//example. moving between some points at random
[RequireComponent(typeof(PolyNavAgent))]
public class PatrolRandomWaypoints : MonoBehaviour{

	public List<Vector2> WPoints = new List<Vector2>();
	public float delayBetweenPoints = 0f;

	private PolyNavAgent _agent;
	private PolyNavAgent agent{
		get {return _agent != null? _agent : _agent = GetComponent<PolyNavAgent>();}
	}

	void OnEnable(){
		agent.OnDestinationReached += MoveRandom;
		agent.OnDestinationInvalid += MoveRandom;
	}

	void OnDisable(){
		agent.OnDestinationReached -= MoveRandom;
		agent.OnDestinationInvalid -= MoveRandom;
	}


	IEnumerator Start(){
		yield return new WaitForSeconds(1);
		if (WPoints.Count > 0){
			MoveRandom();
		}
	}

	void MoveRandom(){
		StartCoroutine(WaitAndMove());
	}

	IEnumerator WaitAndMove(){
		yield return new WaitForSeconds(delayBetweenPoints);
		agent.SetDestination( WPoints[Random.Range(0, WPoints.Count)] );
	}

	void OnDrawGizmosSelected(){
		for ( int i = 0; i < WPoints.Count; i++){
			Gizmos.DrawSphere(WPoints[i], 0.05f);			
		}
	}
}
