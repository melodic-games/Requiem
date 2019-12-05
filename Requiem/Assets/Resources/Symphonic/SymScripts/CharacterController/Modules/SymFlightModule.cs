using UnityEngine;
using SymBehaviourModule;

public class SymFlightModule : Module<SymBehaviour>
{
    private static SymFlightModule instance;

    //Flight State
    public float moveAcceleration;
    private float flightTopSpeed = 220;
    private float flightAccelerationBase = 10;
    private float angularAccelerationBase = 10;
    private float drillDash = 0;
    private float liftScale = 0;
    private Vector3 heading = Vector3.up;
    private Vector3 angularGain = new Vector3(1, 1, 1);
    private float lateralDrag;

    private SymFlightModule()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    public static SymFlightModule Instance
    {
        get
        {
            if (instance == null)
            {
                new SymFlightModule();
            }

            return instance;
        }
    }

    public override void EnterModule(SymBehaviour owner)
    {
        Debug.Log("EnteringFlightModule");
        owner.flightEnabled = true;
        heading = Camera.main.transform.forward;
        owner.jumpBuffer = owner.jumpBufferMax;
        owner.rb.angularDrag = 5;
        liftScale = 0;


    }

    public override void Locomotion(SymBehaviour owner)
    {
       
        //The character flies "upwards" from their walking orientation normally
        Vector3 forward = owner.transform.up;
        //Right is still right
        Vector3 right = owner.transform.right;
        //Making the flight upvector come from the characters back
        Vector3 up = -owner.transform.forward;

        //Passive adjustments and calulations 
        {
            //Calulate locomotion input interruption specific to this module
            {
                if (owner.boostTime < owner.boostDuration && owner.boosting)
                    owner.locomotionInputInterupt = true;
            }

            //Air Top Speed Control            
            {

                owner.topSpeed = flightTopSpeed;

            }

            //Power Level charge and decharge
            {

                if (Mathf.Abs(owner.localAngularVelocity.y) > 10)
                {
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime, 1);
                    owner.chargingEnergy = true;
                }
                else
                {                    
                    owner.chargingEnergy = false;
                }

            }

            //drillDash Charge
            {
                if (Mathf.Abs(owner.horizontalInput) == 1)
                {
                    drillDash += Mathf.Lerp(1, 3, Mathf.InverseLerp(80, owner.topSpeed, owner.rbVelocityMagnatude)) * Time.deltaTime;
                }
                else
                {
                    drillDash -= 3 * Time.deltaTime;
                }

                drillDash = Mathf.Clamp01(drillDash);
            }

            //Calulate angular acceleration force gain.
            {
                //angularGain = new Vector3(5 - (4 * owner.thrustInput), 4 * (1 + spinUp * Mathf.Lerp(1, 2, Mathf.InverseLerp(30, topSpeed, owner.rbVelocityMagnatude))), 1);
                angularGain = new Vector3(5 - (4 * owner.thrustInput), 4 * (1 + drillDash * 2), 1);
                //angularGain = new Vector3(5 - (4 * owner.thrustInput), 1 + (4 * owner.thrustInput), 1);
            }

            //liftScale Calulation
            {
                liftScale = Mathf.Clamp01(liftScale + 1 * Time.deltaTime) * owner.thrustInput;
            }

            //Control Drag
            {
                //drag along the characters flight up vector, signed. Ignoring side axis drag.
                lateralDrag = Vector3.Dot(owner.rbVelocityNormalized, up);
                //drag along the characters flight forward vector, only using force along the negative forward axis.
                float backwardsDrag = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, -forward));
                //Disable drag while summersaulting
                float disable = 1;
                disable *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.x / 2));
                //Disable drag while rolling, but not trusting
                if (owner.thrustInput == 0)
                {
                    disable *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.y / 5));
                }
                //max velocity adjustment
                float highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(owner.topSpeed - 20, owner.topSpeed, owner.rbVelocityMagnatude));
                //backwards + lateral + highEnd
                owner.rb.drag = Mathf.Lerp(0, 0.2f, backwardsDrag * disable) + Mathf.Lerp(0, 1f, Mathf.Abs(lateralDrag) * disable) + highEndDrag;
            }

            //Determin TargetHeading
            {
                //Are we flying forwards or backwards?
                float facingDirection = Mathf.Sign(Vector3.Dot(owner.rbVelocityNormalized, forward));               

                //Find unfocused heading
                Vector3 unfocusedHeading = owner.rbVelocityNormalized * facingDirection;
                
                //Find Focus Heading
                Vector3 focusedHeading = Camera.main.transform.forward;

                //If input is interupted rotation can not be manually adjusted
                if (owner.locomotionInputInterupt)
                    focusedHeading = unfocusedHeading;

                //if we are flying forwards, Lerp between the Velocity and the characters flight forward to cancel auto rotation
                //Stabalizes forward flight at low thrust levels, while allowing you to break and still auto orient to ground                                   
                if (facingDirection == 1)                
                    unfocusedHeading = Vector3.Lerp(owner.rbVelocityNormalized, forward, owner.thrustInput);
                
                //disable heading when velocity is low for when near stationary in air
                unfocusedHeading = Vector3.Lerp(forward, unfocusedHeading, owner.rbVelocityMagnatude);
                
                //Lerp between using the unfocusedHeading, and focused heading.
                heading = Vector3.Lerp(unfocusedHeading, focusedHeading, owner.focusInput);
            }

            //Align the facing axis towards the target heading.                    
            {
                //Disable auto turning if input is detected
                float disable = 1 - Mathf.Clamp01(Mathf.Abs(owner.verticalInput * 2)); 
                                                  
                //If rolling don't try to change the upvector of the character, just let it roll
                float ignoreUpModification = Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.y * .5f));                

                //Up vector aims towards heading (or inverse heading, whichever is closer) if forwards is facing away from heading. 
                Vector3 upVector = Vector3.Slerp(heading * Mathf.Sign(Vector3.Dot(up,heading) + .5f), up, Vector3.Dot(forward, heading));

                //Apply ignoreUpModification, and disable modification if not focusing
                upVector = Vector3.Slerp(upVector, up, ignoreUpModification * (1 - owner.focusInput));

                //Reduce turning based on intersection of pitching arc and heading, but set to full if rolling
                heading = Vector3.Slerp(forward, heading, 1 - Mathf.Clamp01(Mathf.Abs(Vector3.Dot(right, heading) * 2)) + ignoreUpModification);

                //Find target rotation 
                Quaternion targetRotation = Quaternion.LookRotation(heading, upVector) * Quaternion.Euler(90, 0, 0);                              
                            
                //Rotate around character towards target rotation
                SymUtils.SetRotationAroundOffset(owner.transform, Vector3.up * owner.playerHeight * .5f, Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * 4 * disable));                
                
                //Record the amount of direct rotation per second for the animator
                {
   

                    Quaternion rotationdelta = Quaternion.Inverse(targetRotation) * owner.transform.rotation;

                   // rotationdelta


                    //Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;
                    //Vector3 kinematicAngularVelocity = angularDisplacement / Time.deltaTime;

                    //owner.localKinematicAngularVelocity = owner.transform.TransformDirection(kinematicAngularVelocity);
                }

                //Reorient the world space angular velocity to match the rotation just applied 
                owner.rb.angularVelocity = owner.transform.TransformDirection(owner.localAngularVelocity);
                
            }



        }

        //Locomotion
        if (!owner.locomotionInputInterupt)
        { 

            //Angular Acceleration (OLD BUT WORKING)     
            {
                //Pitch - x
                owner.rb.AddTorque(owner.transform.right * owner.verticalInput * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);
                //Roll - y
                owner.rb.AddTorque(-forward * owner.horizontalInput * angularAccelerationBase * angularGain.y, ForceMode.Acceleration);
            }

            //Angular Acceleration                  
            //{
            //    Vector3 axis = ((owner.transform.right * pitchAxisInput) + (-forward * rollAxisInput)).normalized;
            //    float input = Mathf.Clamp01(Mathf.Abs(pitchAxisInput) + Mathf.Abs(rollAxisInput));
            //    //Make angualr gain a weighted average
            //    owner.rb.AddTorque(axis * input * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);//NOT FINISHED. Needs to use both angular gains
            //}

            //Player Activated Boost        
            if (owner.boostBuffer <= owner.boostBufferMax && owner.energyLevel == 1)
            {
                owner.boostBuffer = Mathf.Infinity;
                owner.boostTime = 0;
                owner.boosting = true;

                //Boost                
                owner.rb.velocity = forward * owner.topSpeed;// Vector3.ClampMagnitude(owner.rb.velocity + , topSpeed);
                owner.rb.angularVelocity = Vector3.zero;

                //Pause For a few frames (Feels bad)
                //GameManager.manager.PauseGameForDuration(boostWindup);
                
                if (owner.energyLevel == 1)
                {

                    //Disable camera lerp when pushed back by a shockwave, but behind the character.     
                    float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);

                    owner.cameraHelper.CameraSetLerp((dot + 1) * .5f);

                    owner.cameraHelper.CauseCameraShake(5);

                    //Visual Effect
                    owner.gameObject.SendMessage("Explode");
                }

                //Expend Energy
                owner.energyLevel = 0;
            }

            //Forward Acceleration      
            {
                //Diving downwards increases acceleration.
                float diveBoost = flightAccelerationBase * Mathf.Clamp01(Vector3.Dot(forward, owner.gravity.normalized));
                //Spinning increases acceleration.
                float spinBoost = flightAccelerationBase * Mathf.InverseLerp(5,10, Mathf.Abs(owner.localAngularVelocity.y));
                moveAcceleration = flightAccelerationBase + diveBoost + spinBoost;

                owner.rb.AddForce(forward * owner.thrustInput * moveAcceleration, ForceMode.Acceleration);
            }

        }

        //Lift force
        {
            Vector3 force = SymUtils.RedirectForce(forward, owner.rbVelocityNormalized, owner.rbVelocityMagnatude, liftScale * owner.liftCoefficient);
            owner.rb.AddForce(force, ForceMode.Acceleration);
        }

    }

    public override void OnCollisionEnter(SymBehaviour owner, Collision collision)
    {
        //bounce if we pressed the jump button and will return to flight           
        if (owner.jumpBuffer <= owner.jumpBufferMax)
        {

            owner.jumpBuffer = Mathf.Infinity;
            

            //Character Effects
            {
                //Find Reflection Vector
                Vector3 reflect = Vector3.Reflect(owner.rbVelocityNormalized, owner.surfaceNormal);
                //Reflect Velocity
                owner.rb.velocity = reflect * owner.rbVelocityMagnatude;
                //Reflect Orientation
                Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(reflect, owner.rbVelocityNormalized), reflect), owner.surfaceNormal);
                SymUtils.SetRotationAroundOffset(owner.transform, Vector3.up * owner.playerHeight * .5f, rotation);
                //Reorient the world space angular velocity to match the rotation just applied 
                owner.rb.angularVelocity = owner.transform.TransformDirection(owner.localAngularVelocity);
            }

            if (owner.rbVelocityMagnatude > 20)
            {
                owner.cameraHelper.CameraBounce(collision);
                owner.impactLag = .5f;
            }

        }
        else
        {
            owner.grounded = true;         
            owner.flightEnabled = false;
            owner.ExitCurrentModule();            
        }
    }
    
    public override void OnCollisionStay(SymBehaviour owner, Collision collision)
    {

    }

    public override void ExitModule(SymBehaviour owner)
    {
       
    }
}
