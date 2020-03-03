using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillShotBehaviour : MonoBehaviour {

    private Rigidbody rb;

    public bool autoTargetPlayer = false;
    
    public Transform targetTransform;
    public Vector3 targetPosition;

    private bool dive;

    private RaycastHit hit;

    private float acceleration = 50;

    Vector3 heading;

    private Transform myTransform;

    private Vector3 rbVelNorm;
    private float rbVelMag;

    private float topSpeed;
    public float topSpeedTarget = 220;


    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        if (autoTargetPlayer)
            targetTransform = GameObject.FindGameObjectWithTag("PlayerCharacterAnchorPoint").transform;

        heading = transform.forward;

        myTransform = transform;
    }

    // Update is called once per frame
    void FixedUpdate () {

        rbVelMag = rb.velocity.magnitude;
        rbVelNorm = rb.velocity.normalized;

        acceleration = Mathf.Lerp(acceleration, 200, Time.deltaTime);

        if (dive == false)
        targetPosition = targetTransform.position;

        float dist = Vector3.Distance(myTransform.position, targetPosition);

        if (dist < 25)
            dive = true;
        if (dist > 200)
            dive = false;

        if (dive == false)
        {
            heading = (targetPosition - myTransform.position).normalized;
            myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.LookRotation(heading, myTransform.up), Time.deltaTime * 2);
            myTransform.Rotate(30 * Time.deltaTime, 0, 180 * Time.deltaTime, Space.Self);
        }
        //* Mathf.Clamp01(Vector3.Dot(transform.forward,heading))
        rb.AddForce(myTransform.forward * acceleration , ForceMode.Acceleration);

  
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100, ~((1 << 8) | (1 << 2) | (1 << 10))))
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Max(hit.point.y - myTransform.position.y, rb.velocity.y), rb.velocity.z);
            myTransform.position = new Vector3(myTransform.position.x, Mathf.Max(hit.point.y + 1, myTransform.position.y), myTransform.position.z);
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, Mathf.Lerp(220, 110, Mathf.InverseLerp(0, 100, Vector3.Distance(myTransform.position, targetPosition))));

        LiftForce(myTransform.forward);
	}

    private void LiftForce(Vector3 axis)
    {
        //Lift Force
        Vector3 forceVector = Vector3.Cross(axis, -rbVelNorm).normalized;
        Vector3 crossVector = Vector3.Cross(forceVector, axis);
        rb.AddForce(crossVector * rbVelMag * Vector3.Dot(crossVector, -rbVelNorm) * 8);
    }

}
