using UnityEngine;
using SymBehaviourModule;
using System.Collections.Generic;

public class SymGroundModule : Module<SymBehaviour>
{

    private static SymGroundModule instance;

    //Ground State
    public float moveAcceleration = 40;  
    public float topSpeedMax = 100;    
    private float topSpeedWalk = 10;//was 10
    private float topSpeedRun = 20;

    //StairSteping
    private float maxStepHeight = 0.7f; //KneeHeight of character
    private float stepSearchOvershoot = 0.01f;
    private List<ContactPoint> allCPs = new List<ContactPoint>();
    private Vector3 lastVelocity;

    private float jumpPower = 10;  

    private bool accelerator = false;
    private float acceleratorTime = 0;
    private float acceleratorWindup = 1;

    public float traction = 1;

    //Airborn State
    private float angularAccelerationBase = 10;
    private Vector3 angularGain = new Vector3(1, 1, 1);

    private SymGroundModule()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    public static SymGroundModule Instance
    {
        get
        {
            if (instance == null)
            {
                new SymGroundModule();
            }

            return instance;
        }
    }

    public override void EnterModule(SymBehaviour owner)
    {
        Debug.Log("EnteringBaseModule");
        //Reset Movement Values for when on ground          
        {                       
            owner.jumpBuffer = owner.jumpBufferMax;
            owner.topSpeed = Mathf.Max(owner.surfaceTraversalSpeed, topSpeedWalk);

            if (owner.surfaceTraversalSpeed > 20)
            {
                owner.canRun = true;
                acceleratorTime = acceleratorWindup;
            }

            owner.moveDirection = Quaternion.LookRotation(owner.rbVelocityNormalized, -owner.gravity) * Vector3.forward * Mathf.Min(1,owner.rbVelocityMagnatude);
        }

        //Adjust Capsule Collider
        {
            owner.capsuleCollider.radius = owner.colliderRadius;
            owner.capsuleCollider.height = owner.playerHeight;
        }

    }
   
    public override void Locomotion(SymBehaviour owner)
    {

        //Passive adjustments and calulations 
        {

            //Calulate locomotion input interruption specific to this module
            {
                if (owner.boostTime < owner.boostInterupt && owner.boosting)
                    owner.locomotionInputInterupt = true;
            }

            //Grounded adjustments and calulations
            if (owner.grounded)
            {
                //Stop the physics engine from rotating the character while under direct control
                {
                    owner.rb.freezeRotation = true;
                }

                //Reset angular drag
                {
                    owner.rb.angularDrag = 1000;
                    if (owner.crouching)
                        owner.rb.angularDrag = 3;
                }

                //Calulate move direction, based on camera direction and input
                {

                    Transform camTransform = Camera.main.transform;
                              
                    Vector3 forward = Vector3.Lerp(camTransform.forward, -camTransform.up, Vector3.Dot(camTransform.forward, owner.surfaceNormal));                                   

                    Vector3 movez = (forward - (owner.surfaceNormal * Vector3.Dot(forward, owner.surfaceNormal))).normalized;

                    Vector3 movex = (camTransform.right - (owner.surfaceNormal * Vector3.Dot(camTransform.right, owner.surfaceNormal))).normalized;                   

                    
                    owner.moveDirection = Mathf.Clamp01(Mathf.Abs(owner.verticalInput) + Mathf.Abs(owner.horizontalInput)) * (movez * owner.verticalInput + movex * owner.horizontalInput).normalized;                                                       
                }

                //Input magnatude, used for checking if a player is pressing in a direction
                {
                    owner.moveDirectionMag = Mathf.Clamp01(Mathf.Abs(owner.verticalInput) + Mathf.Abs(owner.horizontalInput));
                }

                //Wall running enabled by speed
                {
                    owner.wallRun = Mathf.Min(owner.wallRun + Mathf.InverseLerp(20, 40, owner.rbVelocityMagnatude) * Time.deltaTime, 1);
                }                             

                //Character faces the move direction, or stays upright, or velocity direction                                      
                {
                                      
                    Vector3 upwards = Vector3.Lerp(-owner.gravity.normalized, owner.surfaceNormal, owner.rbVelocityMagnatude * .1f);

                    if  (!owner.locomotionInputInterupt)
                    {
                        if (owner.moveDirectionMag > .1f)
                        {
                            owner.rb.angularVelocity = Vector3.zero;

                            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(owner.moveDirection, upwards), Time.deltaTime * 5);
                        }
                        else if (owner.crouching)
                        {
                            //Forward vector
                            Vector3 forwards = owner.transform.forward;

                            //Force forward vector perpendicular to the ground
                            Vector3 crushedForwards = (forwards - (upwards * Vector3.Dot(forwards, upwards))).normalized;

                            //Find rotation
                            Quaternion rotation = Quaternion.LookRotation(crushedForwards, upwards);

                            //Apply rotation overtime
                            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, Time.deltaTime * 5);
                        }
                        else 
                        {
                            owner.rb.angularVelocity = Vector3.zero;

                            //Forward vector
                            Vector3 forwards = Vector3.Lerp(owner.transform.forward, owner.rbVelocityNormalized, owner.rbVelocityMagnatude);

                            //Force forward vector perpendicular to the ground
                            Vector3 crushedForwards = (forwards - (upwards * Vector3.Dot(forwards, upwards))).normalized;

                            //Find rotation
                            Quaternion rotation = Quaternion.LookRotation(crushedForwards, upwards);

                            //Apply rotation overtime
                            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, Time.deltaTime * 5);
                        }
                    }

                }

                //WrapAroundSurfaces                                
                {

                    Vector3 newVelocity = Vector3.Cross(Vector3.Cross(owner.surfaceNormal, owner.rb.velocity.normalized), owner.surfaceNormal) * owner.rb.velocity.magnitude;

                    if(owner.rbVelocityMagnatude > .1f)
                    {
                        if (Vector3.Dot(owner.rb.velocity, newVelocity) > .95f)
                        {
                            owner.rb.velocity = Vector3.Slerp(owner.rb.velocity, newVelocity, owner.wallRun);

                            //if (owner.groundIsBelow)
                            //    owner.transform.position = owner.groundHit.point;                        

                            if (Physics.Raycast(owner.transform.position + owner.transform.up, -owner.transform.up, out RaycastHit hitInfo, 10, ~((1 << 8) | (1 << 2) | (1 << 10))))
                                owner.transform.position = Vector3.Lerp(owner.transform.position, hitInfo.point, Time.deltaTime * 5);
                        }
                        else
                        {
                            Debug.Log("Angle to great, aborting wrap around!");
                        }
                    }

                }

                //Calulate Traction
                {                    
                    //scale acceleration with the collinearity of move direction and character forward 
                    traction = Mathf.Clamp01(Vector3.Dot(owner.moveDirection, owner.transform.forward));
                }

                //canRun reset
                {
                    if (owner.rbVelocityMagnatude < topSpeedWalk * .5f && owner.moveDirectionMag == 0) owner.canRun = false;
                }

                //Accelerator
                {
                    //AcceleratorTime Update
                    {
                        if ((owner.rbVelocityMagnatude > topSpeedRun) || (owner.canRun && owner.rbVelocityMagnatude > topSpeedRun - 1))
                            acceleratorTime += Time.deltaTime;
                        else
                            acceleratorTime = 0;

                        acceleratorTime = Mathf.Clamp(acceleratorTime, 0, acceleratorWindup);
                    }

                    //Accelerator Calulation        
                    {
                        if (acceleratorTime >= acceleratorWindup)
                        {
                            accelerator = true;
                        }
                        else
                        {
                            accelerator = false;
                        }
                    }
                }

                //Ground Top Speed Control
                if (!owner.boosting)
                {
                    //if (accelerator)
                    //{
                    //    topSpeedCurrent = topSpeedMax;
                    //}
                    //else
                    {                       
                        if (owner.canRun)
                        {
                            owner.topSpeed = Mathf.Max(topSpeedRun, owner.rbVelocityMagnatude);
                        }
                        else if (owner.crouching)
                        {
                            owner.topSpeed = topSpeedMax;
                        }
                        else
                        {
                            owner.topSpeed = Mathf.Max(topSpeedWalk, owner.rbVelocityMagnatude);
                        }

                        if (owner.rbVelocityMagnatude > topSpeedMax)
                            owner.topSpeed = Mathf.Lerp(owner.topSpeed, topSpeedMax, Time.deltaTime);

                        //clamp top speed
                        owner.rb.velocity = Vector3.ClampMagnitude(owner.rb.velocity, owner.topSpeed);
                        owner.rbVelocityMagnatude = Mathf.Min(owner.rbVelocityMagnatude, owner.topSpeed);
                    }

                }

                //Control Drag, enable friction               
                {                                     
                    
                    if (owner.rbVelocityMagnatude <= topSpeedRun + 1 && owner.moveDirection == Vector3.zero)
                    {
                        owner.rb.drag = 8;
                    }
                    else
                    {
                        owner.rb.drag = 0;
                    }

                    if (owner.rbVelocityMagnatude < .1f && owner.moveDirection == Vector3.zero && !owner.crouching)
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
            else //Airborn
            {

                //Allow the physics engine to rotate the character                
                owner.rb.freezeRotation = false;
                
                //Reset angualr drag                
                owner.rb.angularDrag = 3;
                
                //Reset drag                
                owner.rb.drag = 0;
                
                //Calulate angular acceleration force gain.                
                angularGain = new Vector3(5, 4, 1);          
                
            }

            //Power Level charge and decharge
            {
                if (accelerator || owner.crouching)
                {
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime, 1);
                    owner.chargingEnergy = true;
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
                bool foundGround = SymUtils.FindGround(out groundCP, allCPs);

                Vector3 stepUpOffset = default;
                bool stepUp = false;
                if (foundGround)
                    stepUp = SymUtils.FindStep(out stepUpOffset, maxStepHeight, stepSearchOvershoot, allCPs, groundCP, owner.rb.velocity);

                //Steps
                if (stepUp)
                {
                    owner.rb.velocity = lastVelocity;
                    owner.rb.position += stepUpOffset;
                }

                allCPs.Clear();
                lastVelocity = owner.rb.velocity;
            }

            if (!owner.locomotionInputInterupt)
            {
                //Boost
                if (owner.energyLevel == 1)
                {
                    if (owner.boostBuffer <= owner.boostBufferMax)
                    {
                        owner.boostBuffer = Mathf.Infinity;
                        if (owner.moveDirectionMag > .1f)
                        {
                            //GameManager.manager.PauseGameForDuration(dashWindup);
                            owner.cameraHelper.CauseCameraShake(5);
                            owner.rb.velocity = owner.moveDirection.normalized * Mathf.Max(owner.boostSpeed, owner.rbVelocityMagnatude);
                            owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                            float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);
                            //Disable camera lerp when pushed back by a shockwave, but behind the character.
                            //if (owner.rbVelocityMagnatude > dashSpeed)
                            {
                                owner.cameraHelper.CameraSetLerp((dot + 1) * .5f);
                            }
                            owner.transform.rotation = Quaternion.LookRotation(owner.moveDirection, -owner.gravity);
                            owner.energyLevel = 1;
                            owner.boosting = true;
                            owner.boostTime = 0;
                            owner.canRun = true;
                            acceleratorTime = acceleratorWindup;
                            accelerator = true;
                            owner.topSpeed = Mathf.Max(owner.boostSpeed, owner.rbVelocityMagnatude);
                            owner.locomotionInputInterupt = true;
                            owner.SendMessage("Explode");
                            Debug.Log("Dash!");
                        }

                    }

                    //Maintain Dash
                    if (owner.boosting)
                    {
                        Vector3 dir = Vector3.Cross(Vector3.Cross(owner.rbVelocityNormalized, -owner.surfaceNormal), owner.surfaceNormal);
                        owner.rb.velocity = dir * Mathf.Max(owner.boostSpeed, owner.rbVelocityMagnatude);
                        owner.rb.drag = 0;
                    }
                }

                
                if (!owner.boosting)
                {   
                    //Jumping.
                    if (owner.jumpBuffer < owner.jumpBufferMax)
                    {
                        owner.jumpBuffer = Mathf.Infinity;
                        owner.grounded = false;

                        //Allow the physics engine to rotate the character                    
                        owner.rb.freezeRotation = false;

                        if (!owner.crouching || owner.energyLevel != 1)
                        {
                            owner.rb.velocity += owner.surfaceNormal * jumpPower;
                            Debug.Log("Jump");
                        }
                        else
                        {
                            //GameManager.manager.PauseGameForDuration(dashWindup);
                            owner.rb.velocity += owner.surfaceNormal * 180;
                            owner.energyLevel = 0;
                            Debug.Log("Power Jump");
                            owner.stateMachine.ChangeModule(SymFlightModule.Instance);
                        }

                        //Update Physics Values
                        owner.rbVelocityMagnatude = owner.rb.velocity.magnitude;
                        owner.rbVelocityNormalized = owner.rb.velocity.normalized;

                        owner.animator.SetTrigger("Jump");
                        owner.wallRun = 0;
                        owner.coyoteTime = owner.coyoteTimeMax;
                    }

                    if (!owner.crouching)
                    {
                        //Forward Acceleration on ground                
                        {
                            owner.rb.AddForce(owner.moveDirection * moveAcceleration * traction, ForceMode.Acceleration);
                        }

                        //Grip force             
                        {
                            Vector3 force = SymUtils.RedirectForce(owner.moveDirection, owner.rbVelocityNormalized, owner.rbVelocityMagnatude, 1);
                            owner.rb.AddForce(force * 2, ForceMode.Acceleration);
                        }
                    }
                    //Angular Acceleration while crouched              
                    else
                    {                        
                        Vector3 axis = -owner.transform.up * owner.horizontalInput;                             
                        float scale = Mathf.InverseLerp(5, 10, owner.rbVelocityMagnatude);                        
                        owner.rb.AddTorque(axis * angularAccelerationBase * angularGain.x * scale, ForceMode.Acceleration);
                    }

                }
            }

        }
        else //Airborn motion
        {
            Vector3 heading;
            Vector3 forward = owner.transform.up;
            Vector3 up = -owner.transform.forward;

            //determin TargetHeading
            {
                //Are we flying forwards or backwards?
                float facingDirection = Mathf.Sign(Vector3.Dot(owner.rbVelocityNormalized, forward));

                //Find Focus Heading
                Vector3 focusedHeading = Camera.main.transform.forward;

                //Find unfocused heading
                Vector3 unfocusedHeading = owner.rbVelocityNormalized * facingDirection;

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

                Vector3 facingAxis = forward;

                if (owner.focusInput == 1)
                    facingAxis = Vector3.Lerp(-up, forward, owner.rbVelocityMagnatude * .1f);

                Vector3 rotationAxis = Vector3.Cross(facingAxis, heading);//Get the axis to turn around.

                float disable = 1 - Mathf.Clamp01(Mathf.Abs(owner.verticalInput * 2) + Mathf.Abs(owner.horizontalInput * 2));//Scalar to disable auto turning                                               
                owner.rb.AddTorque(rotationAxis * disable * 10, ForceMode.Acceleration);
            }

            //Angular Acceleration              
            {
                Vector3 axis = ((owner.transform.right * owner.verticalInput) + (-forward * owner.horizontalInput)).normalized;
                float input = Mathf.Clamp01(Mathf.Abs(owner.verticalInput) + Mathf.Abs(owner.horizontalInput));
                //Make angualr gain a weighted average
                owner.rb.AddTorque(axis * input * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);
            }
        }

    }    

    public override void OnCollisionEnter(SymBehaviour owner, Collision collision)
    {
        if (!owner.grounded)
            if (Mathf.Sign(owner.surfaceVerticalSpeed) < 1)
            {

                //Character Effects
                owner.grounded = true;
                Debug.Log("Grounded from OnCollisionEnter");
                          
                if (owner.surfaceTraversalSpeed > topSpeedWalk)
                {
                    owner.canRun = true;                    
                }

                //Convert surface traversal speed into energy
                owner.energyLevel = Mathf.Max(owner.energyLevel, Mathf.InverseLerp(topSpeedRun, topSpeedMax, owner.surfaceTraversalSpeed));

                owner.topSpeed = Mathf.Max(owner.surfaceTraversalSpeed, topSpeedWalk);
            
            }
            else
            {
                //Report if moving away from surface
                Debug.Log("verticalSpeed > 1 in OnCollisionEnter");
            }

    }

    public override void OnCollisionStay(SymBehaviour owner, Collision collision)
    {

        if (!owner.grounded)
            if (Mathf.Sign(owner.surfaceVerticalSpeed) < 1)
            {
                //Character Effects            
                owner.grounded = true;
                Debug.Log("Grounded from OnCollisionStay");
            }
            else
            {
                //Report if moving away from surface (should happen during jumps)
                Debug.Log("verticalSpeed > 1 in OnCollisionStay");
            }

    }

    public override void ExitModule(SymBehaviour owner)
    {
        owner.canRun = false;
        owner.boosting = false;
        owner.boostTime = owner.boostDuration;
        acceleratorTime = 0;
        accelerator = false;
        owner.jumpBuffer = owner.jumpBufferMax;
    }

}
