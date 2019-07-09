using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGroundPosition : MonoBehaviour {
    public Vector3 offset = Vector3.zero;
    public Transform target;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(target.transform.position.x, 0, target.transform.position.z) + offset;
	}
}
