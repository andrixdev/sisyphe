using UnityEngine;
using System;
using System.Collections;

public class JointKineghost : MonoBehaviour {
	
	public Transform joint;
	
	public Vector3 pos = Vector3.zero;
	public Vector3 velocity = Vector3.zero;
	public Vector3 acceleration = Vector3.zero;
	
	private Vector3 lastPos;
	private Vector3 lastVel = Vector3.zero;
	
	public void Start() {
		pos = joint.transform.position;
		lastPos = pos;
	}
	
	public void Update() {
		// Update acceleration and velocity vectors
		pos = joint.transform.position;
		velocity = pos - lastPos;
		acceleration = velocity - lastVel;
		
		// Override buffers for next step
		lastPos = pos;
		lastVel = velocity;
		
		// Now use new velocity/acceleration
		this.transform.position = pos;
		//this.transform.eulerAngles = velocity;
		this.transform.localScale = velocity;// Problem with eulerAngles is it's modulo-[0;360]-ified when passed to VFX
		//this.transform.localScale = acceleration;
	}
	
}
