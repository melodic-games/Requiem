using UnityEngine;
using SymBehaviourModule;


public class SymFlight : Behaviour<BaseCharacterController>
{
    private static SymFlight instance;
    private int boostRollDirection = 0;
    private float dischargeTimer = 0;

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
        dischargeTimer = 0;
        owner.flightEnabled = true;           
        owner.rb.angularDrag = 5;
        owner.weightedPitchInput = 0;
        owner.weightedRollInput = 0;
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
                if (owner.boosting)
                    owner.muteInput = true;
            }

            //Air Top Speed Control            
            {
                owner.topSpeed = owner.flightTopSpeed;
            }

            //drillDash Charge
            {
                if (Mathf.Abs(owner.horizontalInput) == 1 && owner.thrustInput == 0)
                    owner.spinUp = Mathf.Clamp01(owner.spinUp + 1 * Time.deltaTime);
                else
                    owner.spinUp = Mathf.Clamp01(owner.spinUp - 1 * Time.deltaTime);
            }
            
            //Power Level charge and decharge
            {

                if (Mathf.Abs(owner.horizontalInput) == 1 && owner.thrustInput == 0)
                {
                    dischargeTimer = 0;
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime * 2, 1);
                    owner.chargingEnergy = true;
                }
                else
                {
                    dischargeTimer += Time.deltaTime;
                    if (dischargeTimer > 1)
                    owner.energyLevel = Mathf.Max(owner.energyLevel - Time.deltaTime * 2, 0);
                    owner.chargingEnergy = false;
                }

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
            BaseCharacterFunctions.ForwardAirbornDirectionControl(owner, owner.orientationTensor, owner.targetOrientationTensor);
            BaseCharacterFunctions.LateralAirbornDirectionControl(owner, owner.orientationTensor, owner.targetOrientationTensor);

            BaseCharacterFunctions.ManualAirbornAngularAcceleration(owner,
                owner.weightedPitchInput + owner.virtualVerticalInput * (1 - Mathf.Abs(owner.weightedPitchInput)),
                owner.weightedRollInput + owner.virtualHorizontalInput * (1 - Mathf.Abs(owner.weightedRollInput))
                );

            //Player Activated Boost                
            if (owner.boostBuffer <= owner.boostBufferMax && owner.energyLevel == 1)
            {
                owner.boostBuffer = Mathf.Infinity;


                BaseCharacterFunctions.ShockWave(owner.transform.position + owner.transform.up * owner.playerHeight * .5f, owner.transform.rotation, owner.rbVelocityMagnitude * Vector3.Dot(owner.rbVelocityNormalized, -owner.surfaceNormal), owner.rb, owner.shockwavePrefab);
                owner.rb.velocity = owner.orientationTensor.forward * (owner.rbVelocityMagnitude + 150);// Vector3.ClampMagnitude(owner.rb.velocity + , topSpeed);            
                owner.localAngularVelocity.x = 0;
                owner.localAngularVelocity.z = 0;
                owner.rb.angularVelocity = owner.transform.TransformDirection(owner.localAngularVelocity);
                owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                owner.energyLevel = 0;

                if (owner.horizontalInput != 0)
                {
                    boostRollDirection = (int)Mathf.Sign(owner.horizontalInput);
                    owner.boostTime = 1f;
                    owner.boosting = true;
                }
                else
                {
                    boostRollDirection = 0;
                }

            }
        }

        //Maintain Boost
        if (owner.boosting)
        {
             Vector2 angularGain = new Vector2(6, 4);
             owner.rb.AddTorque(-owner.orientationTensor.forward * boostRollDirection * owner.angularAccelerationBase * angularGain.y * 3, ForceMode.Acceleration);
        }

        if (!owner.muteInput)
        { 

            //Forward Acceleration      
            {
                owner.rb.velocity += owner.orientationTensor.forward * owner.thrustInput * Time.deltaTime
                    * Mathf.Lerp(owner.flightAccelerationBase, 25, Mathf.InverseLerp(10, 20, owner.rbVelocityMagnitude))//Acceleration
                    * (1 + Mathf.InverseLerp(5, 10, Mathf.Abs(owner.localAngularVelocity.y))) //SpinBoost
                    ;          
            }

            //Clamp top speed                    
            {
                owner.rb.velocity = Vector3.ClampMagnitude(owner.rb.velocity, owner.topSpeed);
                owner.rbVelocityMagnitude = Mathf.Min(owner.rbVelocityMagnitude, owner.topSpeed);
            }

            BaseCharacterFunctions.AirbornDrag(owner);

        }

        //Turn force
        {           
            //owner.rb.velocity += BaseCharacterFunctions.RedirectForce(owner.orientationTensor.forward, owner.rbVelocityNormalized, owner.rbVelocityMagnitude, owner.turnScale * owner.turnCoefficient) * Time.deltaTime;

            owner.rb.velocity = Vector3.Lerp(owner.rb.velocity, (owner.orientationTensor.forward * owner.rbVelocityMagnitude), .025f * owner.turnScale);
        }

    }

    public override void OnCollisionEnter(BaseCharacterController owner, Collision collision)
    {       
        //if (Vector3.Dot(owner.surfaceNormal, -owner.gravity.normalized) > .5f)
        {
            Debug.Log("Grounded from OnCollisionEnter, in flight behaviour");
            owner.rb.angularVelocity = Vector3.zero;
            owner.grounded = true;
            owner.spinUp = 0;
            owner.flightEnabled = false;
            owner.ResetCurrentModule();            
        }
    }
    
    public override void OnCollisionStay(BaseCharacterController owner, Collision collision)
    {

    }

    public override void ExitModule(BaseCharacterController owner)
    {
        owner.boosting = false;
        owner.boostTime = 0;
    }
}
