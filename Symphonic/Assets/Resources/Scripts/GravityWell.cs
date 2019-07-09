using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWell : MonoBehaviour {

    public GameObject target;
    Rigidbody targetRb;
    public float mass;

	// Use this for initialization
	void Start () {
        targetRb = target.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        float distSqr = Vector3.SqrMagnitude(targetRb.worldCenterOfMass - transform.position);

        float acc = (mass * targetRb.mass) / distSqr;

        targetRb.AddForce((transform.position - targetRb.worldCenterOfMass).normalized * acc);

	}
}
