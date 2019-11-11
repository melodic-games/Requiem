using UnityEngine;
using SymControl;

public class SymUserControl : SymControlSource<SymBehaviour>  {

    public override void CollectInput()
    {
        if (Time.timeSinceLevelLoad > 5)
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
                canRun = Input.GetButton("Run");
            }

            thrustInput = Input.GetAxisRaw("Thrust");

            if (Input.GetAxisRaw("Thrust") == 1)
            {
                if (boost_ResponceDisabled == false)
                {
                    boost = true;
                    boost_ResponceDisabled = true;
                }
                else
                {
                    boost = false;
                }
            }
            else
            {
                boost = false;
                boost_ResponceDisabled = false;
            }

            rollAxisInput = Input.GetAxis("Horizontal");
            pitchAxisInput = Input.GetAxis("Vertical");

            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }

    }


}
