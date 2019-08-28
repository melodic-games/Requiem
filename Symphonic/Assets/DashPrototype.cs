using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPrototype : MonoBehaviour
{

    public Rigidbody rb;
    private Vector3 moveDir = Vector3.forward;
    private int acceleration = 10;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = transform.forward;
        rb.drag = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if(rb != null)
        {
            moveDir = transform.forward;
            acceleration = 10;
            rb.drag = 1;

            rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);
        }
    }
}
