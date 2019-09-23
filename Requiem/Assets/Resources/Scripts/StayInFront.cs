using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInFront : MonoBehaviour {
    public Rigidbody rb;
    void FixedUpdate () {
        transform.position = rb.worldCenterOfMass + rb.velocity;
	}
}
