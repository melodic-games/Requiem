using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillShotBehaviour : MonoBehaviour {

    private Rigidbody rb;

    public bool autoTargetPlayer = false;
    
    public Transform targetTransform;
    public Vector3 targetPosition;

    //private Vector3 heading;

    private Transform myTransform;

    private Vector3 rbVelNorm;
    private float rbVelMag;

    private float topSpeed;
    public float topSpeedTarget = 220;


    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        if (autoTargetPlayer)
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        myTransform = transform;
    }

    // Update is called once per frame
    void FixedUpdate () {
        rbVelMag = rb.velocity.magnitude;
        rbVelNorm = rb.velocity.normalized;

        targetPosition = targetTransform.position;

        //heading = (targetPosition - myTransform.position).normalized;

        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(heading, transform.up), Time.deltaTime * 2);

        rb.AddForce(myTransform.forward * 50, ForceMode.Acceleration);

        VelocityClamp();

        LiftForce(myTransform.forward);
	}

    void VelocityClamp()
    {
        topSpeed = Mathf.Lerp(topSpeed, topSpeedTarget, Time.deltaTime * 2);//smoothly adjust top speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, topSpeed);// Mathf.Lerp(rbVelMag, topSpeed, Time.deltaTime * 1f);                    
    }

    private void LiftForce(Vector3 axis)
    {
        //Lift Force
        Vector3 forceVector = Vector3.Cross(axis, -rbVelNorm).normalized;
        Vector3 crossVector = Vector3.Cross(forceVector, axis);
        rb.AddForce(crossVector * rbVelMag * Vector3.Dot(crossVector, -rbVelNorm) * 4);
    }

}
