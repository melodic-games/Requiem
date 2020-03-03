using UnityEngine;
using SymControl;

public class SymUserControl : SymControlSource<SymBehaviour>  {

    public override void CollectInput()
    {
        if (Time.timeSinceLevelLoad > 5 && Time.timeScale > 0)
        {
            //Focus Input
            {
                focusInput = Input.GetAxis("Focus");
            }

            //Jump
            {
                jump = Input.GetButtonDown("Jump/Bounce");
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

            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }

    }


}
