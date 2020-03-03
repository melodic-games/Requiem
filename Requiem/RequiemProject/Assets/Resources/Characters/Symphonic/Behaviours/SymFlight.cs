using UnityEngine;
using SymBehaviourModule;

public class SymFlight : Behaviour<BaseCharacterController>
{
    private static SymFlight instance;

    private float pitchInputAcceleration;
    private float rollInputAcceleration;

    private SymFlight()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    public static SymFlight Instance
    {
        get
        {
            if (instance == null)
            {
                new SymFlight();
            }

            return instance;
        }
    }

    public override void EnterModule(BaseCharacterController owner)
    {
        Debug.Log("EnteringFlightBehaviour");
        owner.flightEnabled = true;       
        owner.jumpBuffer = owner.jumpBufferMax;
        owner.rb.angularDrag = 5;
        pitchInputAcceleration = 0;
        rollInputAcceleration = 0;
        owner.turnScale = 0;
        owner.turnCoefficient = 5;
    }


    public override void LateUpdate(BaseCharacterController owner)
    {

    }

    public override void Locomotion(BaseCharacterController owner)
    {

        //Passive adjustments and calulations 
        {
            //Calulate input mute specific to this module
            {
                if (owner.boostTime < owner.boostDuration && owner.boosting)
                    owner.muteInput = true;
            }

            //Air Top Speed Control            
            {
                owner.topSpeed = owner.flightTopSpeed;
            }

            //Power Level charge and decharge
            {

                if (Mathf.Abs(owner.localAngularVelocity.y) > 10 && owner.thrustInput == 0)
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
                    owner.drillDash += Mathf.Lerp(1, 3, Mathf.InverseLerp(80, owner.topSpeed, owner.rbVelocityMagnitude)) * Time.deltaTime;
                }
                else
                {
                    owner.drillDash -= 3 * Time.deltaTime;
                }

                owner.drillDash = Mathf.Clamp01(owner.drillDash);
            }

            //turnScale Calulation
            {
                float dot = Vector3.Dot(owner.rbVelocityNormalized, owner.orientationTensor.forward);

                float scale = Mathf.Clamp01(dot * 10);

                owner.turnScale = Mathf.Clamp01(owner.turnScale + .5f * Time.deltaTime) * owner.thrustInput * scale;
            }

        }

        //Locomotion
        if (!owner.muteInput)
        {

            //Angular Acceleration
            {
                //Pitch - x
                float pitchAcceleration = Mathf.Lerp(1, 1.5f, Mathf.Abs(owner.horizontalInput)) * owner.angularAccelerationBase * (owner.angularGain.x - (5 * owner.thrustInput));
                //pitchAcceleration = Mathf.Lerp(pitchAcceleration, Mathf.Lerp(1, 1.5f, Mathf.Abs(owner.horizontalInput)) * owner.angularAccelerationBase * (owner.angularGain.x - (5 * owner.thrustInput)), Time.deltaTime * 2);
                pitchInputAcceleration = Mathf.Lerp(pitchInputAcceleration, owner.verticalInput, Time.deltaTime * 5);
                owner.rb.AddTorque(owner.transform.right * pitchInputAcceleration * pitchAcceleration, ForceMode.Acceleration);
                //owner.rb.AddTorque(owner.transform.right * owner.verticalInput * pitchAcceleration, ForceMode.Acceleration);
                //Roll - y
                // rollAcceleration = Mathf.Lerp(rollAcceleration, Mathf.Lerp(1, .55f, Mathf.Abs(owner.verticalInput)) * owner.angularAccelerationBase * (owner.angularGain.y * (1 + owner.drillDash * 2)), Time.deltaTime * 2);
                float rollAcceleration = Mathf.Lerp(1, .55f, Mathf.Abs(owner.verticalInput)) * owner.angularAccelerationBase * (owner.angularGain.y * (1 + owner.drillDash * 2));
                rollInputAcceleration = Mathf.Lerp(rollInputAcceleration, owner.horizontalInput, Time.deltaTime * 5);
                owner.rb.AddTorque(-owner.orientationTensor.forward * rollInputAcceleration * rollAcceleration, ForceMode.Acceleration);
            }
            bool hit = Physics.Raycast(owner.transform.position, owner.orientationTensor.forward, out RaycastHit groundHit, Mathf.Infinity, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore);

            Debug.DrawLine(owner.transform.position, groundHit.point); 

            //Player Activated Boost        
            if (owner.boostBuffer <= owner.boostBufferMax && owner.drillDash == 1)
            {
                owner.boostBuffer = Mathf.Infinity;
                owner.boostTime = 0;
                owner.boosting = true;

                SymUtility.ShockWave(owner.transform.position + owner.transform.up * owner.playerHeight * .5f, owner.transform.rotation, owner.rbVelocityMagnitude * Vector3.Dot(owner.rbVelocityNormalized, -owner.surfaceNormal), owner.rb, owner.shockwavePrefab);

                owner.rb.velocity = owner.orientationTensor.forward * owner.flightTopSpeed;// Vector3.ClampMagnitude(owner.rb.velocity + , topSpeed);            

                if (hit)
                    owner.transform.position = groundHit.point -owner.orientationTensor.forward * groundHit.distance * .1f;

                owner.rbVelocityNormalized = owner.rb.velocity.normalized;

                float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);

                //owner.cameraHelper.CameraResetLerp(Mathf.Lerp(-.1f, 1, (dot + 1) / 2));       

               // owner.cameraHelper.CauseCameraShake(5);
                
                //Expend Energy
                owner.energyLevel = 0;
            }

            //Forward Acceleration      
            {
                //Diving downwards increases acceleration.
                float diveBoost = owner.flightAccelerationBase * Mathf.Clamp01(Vector3.Dot(owner.orientationTensor.forward, owner.gravity.normalized));
                //Spinning increases acceleration.
                float spinBoost = owner.flightAccelerationBase * Mathf.InverseLerp(5,10, Mathf.Abs(owner.localAngularVelocity.y));

                float acceleration = Mathf.Lerp(owner.flightAccelerationBase, 25, Mathf.InverseLerp(10, 20, owner.rbVelocityMagnitude));

                owner.rb.AddForce(owner.orientationTensor.forward * owner.thrustInput * (acceleration + diveBoost + spinBoost), ForceMode.Acceleration);
            }

        }

        //Turn force
        {           
            Vector3 force = SymUtility.RedirectForce(owner.orientationTensor.forward, owner.rbVelocityNormalized, owner.rbVelocityMagnitude, owner.turnScale * owner.turnCoefficient);
            owner.rb.AddForce(force, ForceMode.Acceleration);
        }

    }

    public override void OnCollisionEnter(BaseCharacterController owner, Collision collision)
    {
        if (owner.jumpBuffer > owner.jumpBufferMax)
        //if (Vector3.Dot(owner.surfaceNormal, -owner.gravity.normalized) > .5f)
        {
            Debug.Log("Grounded from OnCollisionEnter, in flight behaviour");
            owner.rb.angularVelocity = Vector3.zero;
            owner.grounded = true;
            owner.drillDash = 0;
            owner.flightEnabled = false;
            owner.ResetCurrentModule();            
        }
    }
    
    public override void OnCollisionStay(BaseCharacterController owner, Collision collision)
    {

    }

    public override void ExitModule(BaseCharacterController owner)
    {
        
    }
}
