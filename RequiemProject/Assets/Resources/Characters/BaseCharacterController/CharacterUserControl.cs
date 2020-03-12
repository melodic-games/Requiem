using UnityEngine;
using CharacterControl;

public class CharacterUserControl : CharacterControlSource<BaseCharacterController>  {

    public override void CollectInput()
    {
        if (Time.timeScale > 0)//Time.timeSinceLevelLoad > 5
        {
            //Focus Input
            if (Input.GetAxisRaw("Focus") == 1)
            {
                focusInput = true;
            }
            else
            {
                focusInput = false;
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

            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }

    }


}
