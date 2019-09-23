using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]

public class CamControl2 : MonoBehaviour {

    //Target data
    public GameObject target;//target to follow.

    private Rigidbody targetRb = null;
    private Transform targetTransform = null;

    //Control data
    private Vector3 directionVector = Vector3.back;//unit vector in worldspace describing the camera location.
    [Range(0, 2)] public float baseDisplacement = 1;//Absolute minimum distance the camera can be to the target.
    [Range(0, 20)] public float distanceRange = 20;//Length of the spring past the base displacement.


	// Use this for initialization
	void Start () {
        targetTransform = target.GetComponent<Transform>();
        targetRb = target.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        MaintainDistance();
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(targetTransform.forward, Vector3.up),Time.deltaTime);
        directionVector = -transform.forward;
    }


    void MaintainDistance()
    {

        float dot = Vector3.Dot(directionVector, transform.position - targetRb.worldCenterOfMass);

        float distance = Vector3.Distance(targetRb.worldCenterOfMass + (directionVector * dot), targetRb.worldCenterOfMass);

        distance = Mathf.Max(distance, baseDisplacement);
        distance = Mathf.Min(distance, baseDisplacement + distanceRange);
        distance = Mathf.Lerp(distance, baseDisplacement + (distanceRange / 2), Time.deltaTime * 10);

        transform.position = targetRb.worldCenterOfMass + directionVector * distance;

    }
}
