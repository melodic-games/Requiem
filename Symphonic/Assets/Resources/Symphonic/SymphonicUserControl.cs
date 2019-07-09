using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SymphonicBehaviour))]
[RequireComponent(typeof(Rigidbody))]

public class SymphonicUserControl : MonoBehaviour {

    public SymphonicBehaviour symphonicBehaviour;
    public bool invertFocus = false;
    private float focus;
    private bool thrustButton_inUse = false;

    // Use this for initialization
    void Start () {
        symphonicBehaviour = GetComponent<SymphonicBehaviour>();       
    }

    private void Update()
    {

        if (invertFocus)
            focus = 1- Input.GetAxis("Focus");
        else
            focus = Input.GetAxis("Focus");

        if (Input.GetButtonDown("Jump"))
        symphonicBehaviour.jump = true;

        if (Input.GetButton("Jump"))
            symphonicBehaviour.jump = true;

        symphonicBehaviour.thrust = Input.GetAxis("Thrust");

        if (Input.GetAxisRaw("Thrust") != 0)
        {
            if (thrustButton_inUse == false)
            {
                symphonicBehaviour.thrustAsButtion = true;
                thrustButton_inUse = true;
            }            
        }
        else
        {
            symphonicBehaviour.thrustAsButtion = false;
            thrustButton_inUse = false;
        }

        symphonicBehaviour.focus = focus;
        symphonicBehaviour.rollAxisInput = Input.GetAxis("Horizontal");
        symphonicBehaviour.pitchAxisInput = Input.GetAxis("Vertical");       
        
    }

    private void FindNewTarget()
    {
    }

}
