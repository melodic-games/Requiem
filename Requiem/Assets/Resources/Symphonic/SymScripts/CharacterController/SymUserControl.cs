using UnityEngine;
using SymControl;

public class SymUserControl : SymControlSource<SymBehaviour>  {

    public override void CollectInput()
    {
        //Focus Input
        {
            focusInput = Input.GetAxis("Focus");
        }

        //Jump
        {
            jump = Input.GetButtonDown("Jump/Bounce");            
        }

        //Bounce
        {
            bounce = Input.GetButtonDown("Jump/Bounce");                
        }
        
        //Crouching
        {
            crouching = Input.GetButton("Crouching");                
        }

        //Running
        {
            canRun =  Input.GetButton("Run");               
        }

        thrustInput = Input.GetAxisRaw("Thrust");

        if (Input.GetAxisRaw("Thrust") == 1)
        {
            if (dash_ResponceDisabled == false)
            {
                dash = true;
                dash_ResponceDisabled = true;
            }
            else
            {
                dash = false;
            }
        }
        else
        {
            dash = false;
            dash_ResponceDisabled = false;
        }
        
        rollAxisInput = Input.GetAxis("Horizontal");
        pitchAxisInput = Input.GetAxis("Vertical");

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

    }


}
