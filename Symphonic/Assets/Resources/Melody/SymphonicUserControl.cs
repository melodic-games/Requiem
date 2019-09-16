using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SymphonicBehaviour))]
[RequireComponent(typeof(Rigidbody))]

public class SymphonicUserControl : MonoBehaviour {

    public SymphonicBehaviour symphonicBehaviour;
    public bool invertFocus = false;
    private float focus;
    public bool thrustAsButton_ResponceDisabled = false;

    public float jumpBuffer = 0;
    public float jumpBufferMax = .1f;


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

        //if (Input.GetAxis("DPAD – Vertical") == 1) { symphonicBehaviour.dPad = 0; symphonicBehaviour.ChangeSignature();  }
        //if (Input.GetAxis("DPAD – Horizontal") == 1) { symphonicBehaviour.dPad = 1; symphonicBehaviour.ChangeSignature(); }
        //if (Input.GetAxis("DPAD – Vertical") == -1) { symphonicBehaviour.dPad = 2; symphonicBehaviour.ChangeSignature(); }
        //if (Input.GetAxis("DPAD – Horizontal") == -1) { symphonicBehaviour.dPad = 3; symphonicBehaviour.ChangeSignature(); }

        //JumpBuffer Reset
        {
            if (Input.GetButtonDown("Jump/Glide"))
                symphonicBehaviour.jumpBuffer = 0;
        }

        //Running
        {
            if (Input.GetButton("Run"))
                symphonicBehaviour.canRun = true;
        }


       // if (!symphonicBehaviour.grounded)
      //  if (Input.GetButton("Jump/Glide"))
      //      symphonicBehaviour.jump = true;

        symphonicBehaviour.thrust = Input.GetAxisRaw("Thrust");
        
        if (Input.GetAxisRaw("Thrust") == 1)
        {
            if (thrustAsButton_ResponceDisabled == false)
            {                              
                symphonicBehaviour.thrustAsButtion = true;                
                thrustAsButton_ResponceDisabled = true;
            }            
        }
        else if (Input.GetAxisRaw("Thrust") == 0)
        {
            symphonicBehaviour.thrustAsButtion = false;
            thrustAsButton_ResponceDisabled = false;
        }

        symphonicBehaviour.focus = focus;
        symphonicBehaviour.rollAxisInput = Input.GetAxis("Horizontal");
        symphonicBehaviour.pitchAxisInput = Input.GetAxis("Vertical");
        //symphonicBehaviour.yawAxisInput = Input.GetAxis("Bumbers");

    }

    private void FindNewTarget()
    {
    }

}
