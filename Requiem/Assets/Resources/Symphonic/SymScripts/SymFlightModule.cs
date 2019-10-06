using UnityEngine;
using SymBehaviourModule;

public class SymFlightModule : Module<SymBehaviour>
{
    private static SymFlightModule instance;

    //Flight State
    public float moveAcceleration;
    public float topSpeed = 180;
    private float flightAccelerationBase = 10;
    private float angularAccelerationBase = 10;
    private float spinUp = 0;
    private float liftScale = 1;
    private Vector3 heading = Vector3.up;
    private Vector3 angularGain = new Vector3(1, 1, 1);
    private float lateralDrag;

    //Input Data
    private float rollAxisInput = 0;
    private float pitchAxisInput = 0;
    private float yawAxisInput = 0;

    private float thrustInput = 0;
    private bool thrustInputAsButtion = false;

    private float bounceBuffer = Mathf.Infinity;
    private float bounceBufferMax = .5f;

    private float focusInput = 0;

    private float liftCoefficient = 5;

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
        heading = Camera.main.transform.forward;
        bounceBuffer = bounceBufferMax;
        owner.rb.angularDrag = 5;        
    }

    public override void UpdateModule(SymBehaviour owner)
    {
        //Aquire Input
        {
            rollAxisInput = owner.controlSource.rollAxisInput;
            pitchAxisInput = owner.controlSource.pitchAxisInput;
            thrustInput = owner.controlSource.thrustInput;
            focusInput = owner.controlSource.focusInput;
        }      
    }

    public override void Locomotion(SymBehaviour owner)
    {

        //Passive adjustments and calulations 
        {
            //bounceBuffer Update
            {
                bounceBuffer += Time.deltaTime;
            }

            //Power Level charge and decharge
            {

                if (Mathf.Abs(owner.localAngularVelocity.y) > 10)
                {
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime, 1);
                    owner.chargingEnergy = true;
                }
                else if (Mathf.Abs(owner.localAngularVelocity.y) < 2)
                {
                    owner.energyLevel = Mathf.Max(owner.energyLevel - Time.deltaTime, 0);
                    owner.chargingEnergy = false;
                }

            }

            //Disable gravity when focused
            {
                if (focusInput == 1)
                    owner.enableGravity = 0;
            }

            //Disable gravity when spining
            {
                owner.enableGravity = owner.enableGravity * (1 - Mathf.InverseLerp(0, 7, Mathf.Abs(owner.localAngularVelocity.y)));
            }

            //Set Spinup values
            {
                if (Mathf.Abs(rollAxisInput) == 1)
                {
                    spinUp += Mathf.Lerp(1, 3, Mathf.InverseLerp(80, topSpeed, owner.rbVelocityMagnatude)) * Time.deltaTime;
                }
                else
                {
                    spinUp -= 3 * Time.deltaTime;
                }

                spinUp = Mathf.Clamp01(spinUp);
            }

            //Calulate angular acceleration force gain.
            {
                angularGain = new Vector3(5 - (4 * thrustInput), 4 * (1 + spinUp * Mathf.Lerp(1, 2, Mathf.InverseLerp(30, topSpeed, owner.rbVelocityMagnatude))), 1);
            }

            //liftScale Calulation
            {
                liftScale = Mathf.Clamp01(liftScale + 1 * Time.deltaTime) * thrustInput;
            }
        }

        //The character flies "upwards" from their walking orientation normally
        Vector3 forward = owner.transform.up;
        //Making the flight upvector come from the characters back
        Vector3 up = -owner.transform.forward;            

        //determin TargetHeading
        {
            Vector3 focusedHeading = Camera.main.transform.forward;
            Vector3 signedVelocityNormal = owner.rbVelocityNormalized * Mathf.Sign(Vector3.Dot(owner.rbVelocityNormalized, forward));
            Vector3 unfocusedHeading = Vector3.Lerp(signedVelocityNormal, forward, thrustInput);//Lerp between the Velocity and the characters flight forward for no rotation effect. Stabalizes forward flight at low thrust levels
            unfocusedHeading = Vector3.Lerp(forward, unfocusedHeading, owner.rbVelocityMagnatude);//extra scale based on velocity for when stationary in air
            heading = Vector3.Lerp(unfocusedHeading, focusedHeading, focusInput);//Lerp between using the unfocusedHeading, and using the camera.
        }

        //Align towards the target heading.                    
        {
            Vector3 axis = Vector3.Cross(forward, heading);//Get the axis to turn around.
            float disable = 1-Mathf.Clamp01(Mathf.Abs(pitchAxisInput * 2) + Mathf.Abs(rollAxisInput * 2));//Scalar to disable auto turning                                               
            owner.rb.AddTorque(axis * disable * 10, ForceMode.Acceleration);
        }


        //Angular Acceleration (OLD BUT WORKING)
        {
            //Pitch - x
            owner.rb.AddTorque(owner.transform.right * pitchAxisInput * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);
            //Roll - y
            owner.rb.AddTorque(-forward * rollAxisInput * angularAccelerationBase * angularGain.y, ForceMode.Acceleration);
        }

        ////Angular Acceleration              
        //{
        //    Vector3 axis = ((owner.transform.right * pitchAxisInput) + (-forward * rollAxisInput)).normalized;
        //    float input = Mathf.Clamp01(Mathf.Abs(pitchAxisInput) + Mathf.Abs(rollAxisInput));
        //    //Make angualr gain a weighted average
        //    owner.rb.AddTorque(axis * input * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);//NOT FINISHED. Needs to use both angular gains
        //}

        //Forward Acceleration  
        {
            float calulatedBoost = 0;
            //Player Activated Boost               
            {
                owner.rb.velocity = Vector3.ClampMagnitude(owner.rb.velocity + forward * topSpeed * owner.energyLevel * thrustInput, topSpeed);
                //Disable Charging           
                if (thrustInput == 1)
                if (owner.energyLevel == 1)
                {
                    float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);
                    //Disable camera lerp when pushed back by a shockwave, but behind the character.                        
                    owner.cameraHelper.CameraSetLerp((dot + 1) * .5f);
                    owner.gameObject.SendMessage("Explode");                   
                }
               
                //Expend EnergyLevel
                owner.energyLevel *= 1 - thrustInput;
            }

            //Diving downwards doubles acceleration base speed
            float diveBoost = flightAccelerationBase * Mathf.Clamp01(Vector3.Dot(forward, owner.gravity.normalized));
            moveAcceleration = flightAccelerationBase + calulatedBoost + diveBoost;

            owner.rb.AddForce(forward * thrustInput * moveAcceleration, ForceMode.Acceleration);
        }

        //Control Drag
        {
            //drag along the characters flight up vector, signed. Ignoring side axis drag.
            lateralDrag = Vector3.Dot(owner.rbVelocityNormalized, up);
            //drag along the characters flight forward vector, only using force along the negative forward axis.
            float backwardsDrag = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, -forward));
            //Disable drag while summersaulting
            float disable = 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.x / 2));
            //max velocity adjustment
            float highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(topSpeed - 20, topSpeed, owner.rbVelocityMagnatude));
            //backwards + lateral + highEnd
            owner.rb.drag = Mathf.Lerp(0, 0.2f, backwardsDrag * disable) + Mathf.Lerp(0, 1f, Mathf.Abs(lateralDrag) * disable) + highEndDrag;
        }

        //Lift force
        {
            Vector3 force = SymUtils.RedirectForce(forward, owner.rbVelocityNormalized, owner.rbVelocityMagnatude, liftScale * liftCoefficient);
            owner.rb.AddForce(force, ForceMode.Acceleration);
        }
                 
    }


    public override void OnCollissionStay(SymBehaviour owner, Collision collision)
    {

    }


    public override void OnCollisionEnter(SymBehaviour owner, Collision collision)
    {
        owner.groundNormal = collision.GetContact(0).normal;
        owner.ExitCurrentModule();
        owner.flightEnabled = false;
        owner.grounded = true;
    }
    //public override void OnCollisionEnter(SymBehaviour owner, Collision collision)
    //{      
    //    Vector3 hitPoint = collision.GetContact(0).point;
    //    Vector3 hitNormal = collision.GetContact(0).normal;

    //    //bounce if we pressed the jump button and will return to flight           
    //    if (bounceBuffer <= bounceBufferMax && owner.flightEnabled)
    //    {

    //        bounceBuffer = Mathf.Infinity;

    //        //Character Effects
    //        {
    //            //Find Reflection Vector
    //            Vector3 reflect = Vector3.Reflect(owner.rbVelocityNormalized, hitNormal);
    //            //Reflect Velocity
    //            owner.rb.velocity = reflect * owner.rbVelocityMagnatude;
    //            //Cancel Angular Velocity Caused By Collission
    //            owner.rb.angularVelocity = Vector3.zero;
    //            //Reflect Orientation
    //            owner.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(reflect, owner.rbVelocityNormalized), reflect), hitNormal);
    //        }

    //        owner.cameraHelper.CameraBounce(collision);

    //    }
    //}

    public override void ExitModule(SymBehaviour owner)
    {
       
    }
}
