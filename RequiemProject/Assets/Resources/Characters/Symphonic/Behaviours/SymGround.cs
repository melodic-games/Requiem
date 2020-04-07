using UnityEngine;
using SymBehaviourModule;
using System.Collections.Generic;

public class SymGround : Behaviour<BaseCharacterController>
{

    private static SymGround instance;

    private Vector3 targetForward;

    //StairSteping
    private float maxStepHeight = 0.7f; //KneeHeight of character
    private float stepSearchOvershoot = 0.01f;
    private List<ContactPoint> allContactPoints = new List<ContactPoint>();
    private Vector3 lastVelocity; 
    private float acceleratorTime = 0;
    private float acceleratorWindup = 5;

    Vector3 boostTargetDirection;

    public float moveDirectionChangeRate;
    //public bool turningAround = false;

    public float increment = 0;
    public float disable = 1;

    float speedMemory = 0;
    float speedMemoryTime = 0;


    private SymGround()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    public static SymGround Instance
    {
        get
        {
            if (instance == null)
            {
                new SymGround();
            }

            return instance;
        }
    }

    public override void EnterModule(BaseCharacterController owner)
    {
        Debug.Log("EnteringGroundBehaviour");
        //Reset Movement Values for when on ground          
        {                               
            owner.topSpeed = Mathf.Max(owner.surfaceTraversalSpeed, owner.groundWalkSpeed);

            if (owner.surfaceTraversalSpeed > 20)
            {
                owner.canRun = true;
                acceleratorTime = acceleratorWindup;
            }

            owner.moveDirection = Quaternion.LookRotation(owner.rbVelocityNormalized, -owner.gravity) * Vector3.forward * Mathf.Min(1,owner.rbVelocityMagnitude);       
        
            if (owner.rbVelocityMagnitude > 10)
            {
                targetForward = owner.rbVelocityNormalized;
            }
            else
            {
                Debug.Log("targetforward set as transform forward");
                targetForward = owner.transform.forward;
            }
        }

        //Adjust Capsule Collider
        {
            owner.capsuleCollider.radius = owner.colliderRadius;
            owner.capsuleCollider.height = owner.playerHeight;
        }

        owner.turnScale = 0;
        owner.turnCoefficient = 7;

    }

    public override void LateUpdate(BaseCharacterController owner)
    {
        //Debug.DrawRay(owner.transform.position, owner.moveDirectionRaw, Color.black);
        //Debug.DrawRay(owner.transform.position + owner.transform.up * .1f, owner.moveDirection, Color.white);
        //Debug.DrawRay(owner.transform.position + owner.transform.up * .2f, owner.rbVelocityNormalized, Color.red);      
    }

    public override void Locomotion(BaseCharacterController owner)
    {

        //Passive adjustments and calulations 
        {

            //Calulate input muting specific to this module
            {
                if (owner.boosting)
                    owner.muteInput = true;
            }

            //Grounded adjustments and calulations
            if (owner.grounded)
            {
                //Stop the physics engine from rotating the character while under direct control                
                owner.rb.freezeRotation = true;
                
                //Reset angular drag                
                owner.rb.angularDrag = 1000;                

                //Calulate raw move direction, based on camera direction and input
                {
                    Transform camTransform = Camera.main.transform;                              
                    Vector3 directionalForward = Vector3.Lerp(camTransform.forward, -camTransform.up, Vector3.Dot(camTransform.forward, owner.surfaceNormal));                                   
                    Vector3 movez = (directionalForward - (owner.surfaceNormal * Vector3.Dot(directionalForward, owner.surfaceNormal))).normalized;
                    Vector3 movex = (camTransform.right - (owner.surfaceNormal * Vector3.Dot(camTransform.right, owner.surfaceNormal))).normalized;                                       
                    owner.moveDirectionRaw = Mathf.Clamp01(Mathf.Abs(owner.verticalInput) + Mathf.Abs(owner.horizontalInput)) * (movez * owner.verticalInput + movex * owner.horizontalInput).normalized;
                }

                //Input magnatude, used for checking if a player is pressing in a direction
                owner.moveDirectionMag = Mathf.Clamp01(Mathf.Abs(owner.verticalInput) + Mathf.Abs(owner.horizontalInput));

                //Remember how fast we were moving. Keeps acceleration responsive if it dips low due to direction change at low speeds.
                {
                    if (owner.moveDirectionMag == 0)
                        speedMemoryTime -= Time.deltaTime;
                    else
                        speedMemoryTime = 1;

                    speedMemory = Mathf.Max(speedMemory, owner.rbVelocityMagnitude);

                    if (speedMemoryTime < 0)
                        speedMemory = 0;
                }

                //Change rate of acceleration direction. full at any speed below walking, lerps to 2 at full speed
                moveDirectionChangeRate = Mathf.Lerp(10,2,Mathf.InverseLerp(owner.groundWalkSpeed,owner.groundTopSpeed,owner.rbVelocityMagnitude));
                // Mathf.LerpUnclamped(1 / Time.deltaTime, 2, Mathf.InverseLerp(owner.groundWalkSpeed, owner.groundTopSpeed, owner.rbVelocityMagnitude));//2

                //Align moveDirection towards moveDirectionRaw based on moveDirectionChangeRate to smooth out digital input at high speed    
                
                owner.moveDirection = Vector3.Slerp(owner.moveDirection, owner.moveDirectionRaw, moveDirectionChangeRate * Time.deltaTime);

                //Find Slerped direction vector
                //owner.moveDirection = Vector3.Slerp(owner.moveDirection, owner.moveDirectionRaw, moveDirectionChangeRate * Time.deltaTime);

                //Vector3 cross = Vector3.Cross(owner.moveDirection, owner.moveDirectionRaw);

                owner.moveDirection = (owner.moveDirection - (owner.surfaceNormal * Vector3.Dot(owner.moveDirection, owner.surfaceNormal))).normalized;

                owner.moveDirection *= owner.moveDirectionMag;


                //Reset MoveDirection upon pulling in the other direction
                if (Vector3.Dot(owner.moveDirection, owner.moveDirectionRaw) < -.2f)
                {
                    //owner.moveDirection = owner.moveDirectionRaw;
                    Debug.Log("Pulling In other direction");
                }

                if (Vector3.Dot(owner.rbVelocityNormalized, owner.moveDirectionRaw) < -.2f)
                {                        
                    Debug.Log("breaking");
                }

                //Character faces the move direction, or velocity direction, or the current direction                                      
                {
                    owner.rb.angularVelocity = Vector3.zero;                                     

                    //Find Upwards vector                    
                    Vector3 upwards = Vector3.Lerp(-owner.gravity.normalized, owner.surfaceNormal, owner.rbVelocityMagnitude * .1f);

                    //Find Forward vector                                          
                    if (owner.moveDirectionMag > .1f && !owner.muteInput)
                        targetForward = owner.moveDirection;
                    else if (owner.rbVelocityMagnitude > 1 && owner.focusInput && !owner.muteInput)
                        targetForward = Camera.main.transform.forward;
                    
                    //Crush the forward vector to make it orthogonal to upwards vector                    
                    targetForward = (targetForward - (upwards * Vector3.Dot(targetForward, upwards))).normalized;     
                    
                    if (targetForward == Vector3.zero)
                        targetForward = owner.transform.forward;

                    //Find rotation speed                    
                    float rotationSpeedScale = Mathf.Lerp(5, 5, Mathf.InverseLerp(owner.groundWalkSpeed, owner.groundRunSpeed, owner.rbVelocityMagnitude));
                     owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(targetForward, upwards), Time.deltaTime * rotationSpeedScale);
                    if (targetForward == Vector3.zero) { Debug.Log("Here1"); }
               

                }     

                //turnScale Calulation   
                {                         
                    //Check if turinging around
                    if (Vector3.Dot(targetForward, owner.transform.forward) < -.2f)                                     
                        owner.turningAround = true;                   
                        
                    //Check if we are facing the target forward enough to no longer be considered turning around
                    if (Mathf.Clamp01((Vector3.Dot(targetForward, owner.transform.forward) * 10) - 9) > .9f)
                        owner.turningAround = false;
                                      
                    if (!owner.turningAround)
                        increment = .5f * Time.deltaTime;
                    else
                        increment = 0;
                            
                      disable = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, owner.transform.forward) * 10);

                      owner.turnScale = Mathf.Clamp01(owner.turnScale + increment) * owner.moveDirectionMag * disable;                 
                }

                //WrapAroundSurfaces     

                //Crush the velocity vector to make it orthogonal to upwards vector                    
                Vector3 newVelocity = (owner.rbVelocityNormalized - (owner.surfaceNormal * Vector3.Dot(owner.rbVelocityNormalized, owner.surfaceNormal))).normalized * owner.rbVelocityMagnitude;

                //Vector3 newVelocity = Vector3.Cross(Vector3.Cross(owner.surfaceNormal, owner.rb.velocity.normalized), owner.surfaceNormal) * owner.rbVelocityMagnitude;           
                
                if (owner.rbVelocityMagnitude > .1f)
                {
                    if (Vector3.Dot(owner.rb.velocity, newVelocity) > .95f)
                    {
                        owner.rb.velocity = Vector3.Slerp(owner.rb.velocity, newVelocity, owner.wallRun);                      

                        if (owner.groundIsBelow)
                            owner.transform.position = owner.groundHit.point;                        

                        // if (Physics.Raycast(owner.transform.position + owner.transform.up, -owner.transform.up, out RaycastHit hitInfo, 10, ~((1 << 8) | (1 << 2) | (1 << 10))))
                         //   owner.transform.position = Vector3.Lerp(owner.transform.position, hitInfo.point, Time.deltaTime * 5);
                    }           
                }
               
            }
            else //Airborn
            {
                //Allow the physics engine to rotate the character                
                owner.rb.freezeRotation = false;
                
                //Reset angualr drag                
                owner.rb.angularDrag = 5;

                //turnScale Calulation
                owner.turnScale = 0;
                
            }

            //Power Level charge and decharge
            {
                if (acceleratorTime >= acceleratorWindup || owner.focusInput)
                {
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime, 1);
                    owner.chargingEnergy = true;
                }
                else if(owner.grounded)
                {                                        
                    if (owner.moveDirectionMag == 0)                    
                        owner.energyLevel = Mathf.Max(owner.energyLevel - Time.deltaTime, 0);                    
                        owner.chargingEnergy = false;
                }                    
                else
                {
                    owner.energyLevel = Mathf.Max(owner.energyLevel - Time.deltaTime, 0);
                    owner.chargingEnergy = false;
                }
            }

        }

        //Ground locomotion  
        if (owner.grounded)
        {

            //Execute StairStepUp
            {
                //Filter through the ContactPoints to see if we're grounded and to see if we can step up
                ContactPoint groundCP = default;
                bool foundGround = BaseCharacterFunctions.FindGround(out groundCP, allContactPoints);

                Vector3 stepUpOffset = default;
                bool stepUp = false;
                if (foundGround)
                    stepUp = BaseCharacterFunctions.FindStep(out stepUpOffset, maxStepHeight, stepSearchOvershoot, allContactPoints, groundCP, owner.rb.velocity);

                //Steps
                if (stepUp)
                {
                    owner.rb.velocity = lastVelocity;
                    owner.rb.position += stepUpOffset;
                }

                allContactPoints.Clear();
                lastVelocity = owner.rb.velocity;
            }

            //Boost
            if (!owner.muteInput)
            {
                if (owner.boostBuffer <= owner.boostBufferMax)
                {                   
                    owner.boostBuffer = Mathf.Infinity;                                      
                    //GameManager.manager.PauseGameForDuration(owner.boostWindup);    
                    
                    if (owner.moveDirectionMag > .1f || owner.canRun)
                    {
                        if (Vector3.Dot(targetForward, owner.transform.forward) > .8)
                        {
                            owner.rb.velocity = targetForward * owner.boostSpeed;
                            owner.transform.rotation = Quaternion.LookRotation(targetForward, -owner.gravity);
                        }
                        else
                        {
                            owner.rb.velocity = owner.transform.forward * owner.boostSpeed;
                        }

                        owner.turnScale = 1;
                        
                        owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                        float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);        
                        //owner.cameraHelper.CameraResetLerp(Mathf.Lerp(0, 1, (dot + 1) / 2));                                          
                        //owner.cameraHelper.CauseCameraShake(1);
                        owner.moveDirection = owner.moveDirectionRaw;          
                        owner.boosting = true;
                        owner.boostTime = .1f;
                        owner.canRun = true;
                        acceleratorTime = acceleratorWindup;
                        owner.topSpeed = Mathf.Max(owner.boostSpeed, owner.topSpeed);
                        owner.muteInput = true;
                        BaseCharacterFunctions.ShockWave(owner.transform.position + owner.transform.up * owner.playerHeight * .5f, owner.transform.rotation, owner.rbVelocityMagnitude * Vector3.Dot(owner.rbVelocityNormalized, -owner.surfaceNormal), owner.rb, owner.shockwavePrefab);
                        //Debug.Log("Dash!");        
                        owner.ResetBodyDynamicBones();
                    }
                }                
            }

            ////Maintain Boost
            //if (owner.boosting)
            //{
            //    boostTargetDirection = Vector3.Cross(Vector3.Cross(boostTargetDirection, -owner.surfaceNormal), owner.surfaceNormal).normalized;
            //    owner.rb.velocity = boostTargetDirection * owner.boostSpeed;               
            //    owner.rb.drag = 0;
            //}
                
            if (!owner.muteInput)
            {   
                //Jumping.
                if (owner.jumpBuffer < owner.jumpBufferMax)
                {
                    owner.grounded = false;

                    //Allow the physics engine to rotate the character                    
                    owner.rb.freezeRotation = false;

                    if (!owner.crouching || owner.moveDirectionMag == 1)
                    {
                        owner.rb.velocity += owner.surfaceNormal * owner.jumpPower;
                        owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                        owner.rbVelocityMagnitude = owner.rb.velocity.magnitude;
                        Debug.Log("Jump");
                    }
                    else
                    {
                        //GameManager.manager.PauseGameForDuration(dashWindup);
                        owner.rb.velocity += owner.surfaceNormal * owner.jumpPower * 2;              
                        Debug.Log("Power Jump");
                        owner.stateMachine.ChangeModule(SymFlight.Instance);
                    }

                    owner.animator.SetTrigger("Jump");
                    owner.wallRun = 0;
                    owner.coyoteTime = owner.coyoteTimeMax;
                }
                    owner.jumpBuffer = owner.jumpBufferMax;

                ////Forward Acceleration on ground                       
                //float acceleration = Mathf.Lerp(5, 50, Mathf.InverseLerp(0, owner.groundRunSpeed, speedMemory));
                //if (owner.canRun)
                //    acceleration = 50;
                //owner.rb.velocity += owner.moveDirection * acceleration * Mathf.Clamp01(characterFacingTargetForward) * Time.deltaTime;


                //Forward Acceleration on ground                
                {
                    float acceleration = Mathf.Lerp(5, owner.groundAccelerationBase, Mathf.InverseLerp(0, owner.groundRunSpeed, owner.rbVelocityMagnitude));
                    //if (owner.canRun)
                    acceleration = owner.groundAccelerationBase;
                    
                    //Disable acceleration if not facing move direction
                    acceleration *= Mathf.Clamp01(Vector3.Dot(owner.moveDirection.normalized, owner.transform.forward));

                    if (owner.crouching)
                    {
                        acceleration *= 0;
                    }

                    owner.rb.velocity += owner.moveDirection * acceleration * Time.deltaTime;
                }


                //Turn force                                                                             
                //owner.rb.velocity += BaseCharacterFunctions.RedirectForce(owner.transform.forward, owner.rbVelocityNormalized, owner.rbVelocityMagnitude, owner.turnScale * owner.turnCoefficient) * Time.deltaTime;                                                                                                           
                owner.rb.velocity = Vector3.Lerp(owner.rb.velocity, (owner.orientationTensor.forward * owner.rbVelocityMagnitude), .1f * owner.turnScale * owner.turnScale * owner.turnScale);

            }            
            
            //Recalulate velocity magnatude after applying force.                        
            owner.rbVelocityMagnitude = owner.rb.velocity.magnitude;
            
            //Speed Control after applying force
            {
                //automate canRun
                {
                    if (owner.rbVelocityMagnitude < owner.groundWalkSpeed * .5f && owner.moveDirectionMag == 0) owner.canRun = false;
                    if (owner.rbVelocityMagnitude > owner.groundRunSpeed) owner.canRun = true;
                }

                //Accelerator
                {
                    if ((owner.rbVelocityMagnitude > owner.groundRunSpeed) || (owner.canRun && owner.rbVelocityMagnitude > owner.groundRunSpeed - 1))
                        acceleratorTime += Time.deltaTime;
                    else
                        acceleratorTime = 0;

                    acceleratorTime = Mathf.Clamp(acceleratorTime, 0, acceleratorWindup);
                }

                //Ground Top Speed Control
                if (owner.grounded)
                {

                    //Lower top speed over time if its greater than max
                    if (owner.topSpeed > owner.groundTopSpeed)
                    {
                        owner.topSpeed = Mathf.Lerp(owner.topSpeed, owner.groundTopSpeed, Time.deltaTime);
                    }

                    //Clamp to run speed if we can run, and we are lower than running speed
                    if (owner.canRun && owner.rbVelocityMagnitude <= owner.groundRunSpeed)
                    {
                        owner.topSpeed = owner.groundRunSpeed;
                    }

                    //Enable accelerator effect
                    if (acceleratorTime >= acceleratorWindup)
                    {
                        owner.topSpeed = Mathf.Max(owner.topSpeed, owner.groundTopSpeed);
                    }

                    //Clamp to walk speed if we cant run, and we are lower than walking speed
                    if (!owner.canRun && owner.rbVelocityMagnitude <= owner.groundWalkSpeed)
                    {
                        owner.topSpeed = owner.groundWalkSpeed;
                    }

                    if (owner.boosting)
                    {
                        owner.topSpeed = Mathf.Max(owner.boostSpeed, owner.topSpeed, owner.groundTopSpeed);
                    }

                    //Clamp top speed                    
                    {
                        owner.rb.velocity = Vector3.ClampMagnitude(owner.rb.velocity, owner.topSpeed);
                        owner.rbVelocityMagnitude = Mathf.Min(owner.rbVelocityMagnitude, owner.topSpeed);
                    }
                }

                //Control Drag            
                {
                    //if (owner.rbVelocityMagnitude <= owner.groundRunSpeed + 1 && owner.moveDirectionMag == 0)                    
                    //    owner.rb.drag = 10;                                         
                    //else                    
                    //    owner.rb.drag = 0;

                    BaseCharacterFunctions.GroundDrag(owner);
                }
                
                //Enable friction    
                {
                    if (owner.rbVelocityMagnitude < .5f && owner.moveDirectionMag == 0 )
                    {
                        owner.phyMat.dynamicFriction = 1;
                        owner.phyMat.staticFriction = 1;
                        owner.phyMat.frictionCombine = PhysicMaterialCombine.Maximum;
                    }
                    else
                    {
                        owner.phyMat.dynamicFriction = 0;
                        owner.phyMat.staticFriction = 0;
                        owner.phyMat.frictionCombine = PhysicMaterialCombine.Minimum;
                    }

                }
            }

        }
        else //Airborn motion
        {
            BaseCharacterFunctions.ForwardAirbornDirectionControl(owner, owner.orientationTensor, owner.targetOrientationTensor);
            BaseCharacterFunctions.LateralAirbornDirectionControl(owner, owner.orientationTensor, owner.targetOrientationTensor);

            BaseCharacterFunctions.ManualAirbornAngularAcceleration(owner,
                owner.weightedPitchInput + owner.virtualVerticalInput * (1 - Mathf.Abs(owner.weightedPitchInput)),
                owner.weightedRollInput + owner.virtualHorizontalInput * (1 - Mathf.Abs(owner.weightedRollInput))
                );

            BaseCharacterFunctions.AirbornDrag(owner);
        }

    }    

    public override void OnCollisionEnter(BaseCharacterController owner, Collision collision)
    {       
        if (!owner.grounded)
        //if (Vector3.Dot(owner.surfaceNormal, -owner.gravity.normalized) > .5f)
        if (Mathf.Sign(owner.surfaceVerticalSpeed) < 1)
        {

            //Character Effects
            owner.rb.angularVelocity = Vector3.zero;
            owner.grounded = true;
            Debug.Log("Grounded from OnCollisionEnter");
                          
            if (owner.surfaceTraversalSpeed > owner.groundWalkSpeed)
            {
                owner.canRun = true;                    
            }

            //Convert surface traversal speed into energy
            owner.energyLevel = Mathf.Max(owner.energyLevel, Mathf.InverseLerp(owner.groundRunSpeed, owner.groundTopSpeed, owner.surfaceTraversalSpeed));

            owner.topSpeed = Mathf.Max(owner.surfaceTraversalSpeed, owner.groundWalkSpeed);
            
        }

    }

    public override void OnCollisionStay(BaseCharacterController owner, Collision collision)
    {

        if (!owner.grounded)
            if (owner.surfaceVerticalSpeed <= 0)
            {
                //Character Effects       
                owner.rb.angularVelocity = Vector3.zero;
                owner.grounded = true;
                Debug.Log("Grounded from OnCollisionStay");
            }
            else
            {
                //Report if moving away from surface (should happen during jumps)
                Debug.Log("verticalSpeed > 1 in OnCollisionStay");
            }

    }

    public override void ExitModule(BaseCharacterController owner)
    {
        owner.canRun = false;
        owner.boosting = false;
        owner.boostTime = 0;
        acceleratorTime = 0;      
        owner.moveDirection = Vector3.zero;
        owner.moveDirectionRaw = Vector3.zero;
        owner.moveDirectionMag = 0;
    }

}
