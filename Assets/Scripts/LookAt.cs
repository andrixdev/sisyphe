 using UnityEngine;
 using System.Collections;
 
 public class LookAt : MonoBehaviour {
     public Transform target;
     
     // Update is called once per frame
	 void Start () {
         transform.LookAt(target.position);
     }
	 
     void Update () {
         transform.LookAt(target.position);
     }
 }