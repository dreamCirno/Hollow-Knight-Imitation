using UnityEngine;
using System.Collections.Generic;

//example
[RequireComponent(typeof(PolyNavAgent))]
public class FollowTarget : MonoBehaviour{

	public Transform target; 
	
	private PolyNavAgent _agent;
	private PolyNavAgent agent{
		get {return _agent != null? _agent : _agent = GetComponent<PolyNavAgent>();}
	}

	void Update() {
		if (target != null){
			agent.SetDestination( target.position );
		}
	}
}