using UnityEngine;
using SymBehaviourModule;
using System.Collections.Generic;
using ConsoleCMD;

public class SymBaseModule : Module<SymBehaviour>
{

    private static SymBaseModule instance;

    //Ground State
    public float moveAcceleration = 40;
    public float topSpeedCurrent = 10;    
    public float topSpeedMax = 100;    
    private float topSpeedWalk = 10;//was 10
    private float topSpeedRun = 20;

    //StairSteping
    private float maxStepHeight = 0.7f; //KneeHeight of character
    private float stepSearchOvershoot = 0.01f;
    private List<ContactPoint> allCPs = new List<ContactPoint>();
    private Vector3 lastVelocity;

    private float jumpPower = 10;
   
    private bool dashing = false;
    private float dashSpeed = 100;
    private float dashTime = 0;
    private float dashDuration = .1f;
    private float dashInterupt = .05f;
    private float dashWindup = .05f;

    private bool accelerator = false;
    private float acceleratorTime = 0;
    private float acceleratorWindup = 3;

    public float traction = 1;

    //Airborn State
    private float angularAccelerationBase = 10;
    private Vector3 angularGain = new Vector3(1, 1, 1);

    //Input data          
    private float horizontalInput;
    private float verticalInput;

    private float dashBuffer = Mathf.Infinity;
    private float dashBufferMax = .5f;

    private Vector3 moveDirection;
    private float moveDirectionMag;
    private bool canRun = false;

    private float jumpBuffer = Mathf.Infinity;
    private float jumpBufferMax = .5f;

    private SymBaseModule()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }

    public static SymBaseModule Instance
    {
        get
        {
            if (instance == null)
            {
                new SymBaseModule();
            }

            return instance;
        }
    }

    public override void EnterModule(SymBehaviour owner)
    {
        Debug.Log("EnteringBaseModule");
        //Reset Movement Values for when on ground          
        {                       
            jumpBuffer = jumpBufferMax;
            topSpeedCurrent = Mathf.Max(owner.surfaceTraversalSpeed, topSpeedWalk);

            if (Vector3.Dot(owner.surfaceNormal, -owner.gravity.normalized) < .5f)
            {
                owner.wallRun = 1;
            }

            if (owner.surfaceTraversalSpeed > 20)
            {
                canRun = true;
                acceleratorTime = acceleratorWindup;
            }            
            
            moveDirection = Quaternion.LookRotation(owner.rbVelocityNormalized, -owner.gravity) * Vector3.forward * Mathf.Min(1,owner.rbVelocityMagnatude);
        }

        //Adjust Capsule Collider
        {
            owner.capsuleCollider.radius = owner.colliderRadius;
            owner.capsuleCollider.height = owner.playerHeight;
        }

    }

    public override void UpdateModule(SymBehaviour owner)
    {
        //Aquire Input
        {            
            {
                //Direct Input
                horizontalInput = owner.controlSource.horizontalInput;
                verticalInput = owner.controlSource.verticalInput;

                //Crouching
                owner.crouching = owner.controlSource.crouching;
            }

            //Jump
            if (owner.controlSource.jump)
            {
                jumpBuffer = 0;
            }

            //Dash
            if (!dashing)
                if (owner.controlSource.boost)
                {
                    dashBuffer = 0;
                }

            //Run
            if (owner.controlSource.canRun)
            {
                canRun = true;
            }

        }
       
    }
   
    public override void Locomotion(SymBehaviour owner)
    {

        //Passive adjustments and calulations 
        {

            //Calulate locomotion input interruption specific to this module
            {
                if (dashTime < dashInterupt && dashing)
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

                    
                        moveDirection = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)) * (movez * verticalInput + movex * horizontalInput).normalized;                                                       
                }

                //Set Physics Material properties
                {
                    if (!owner.crouching)
                    {
                        owner.phyMat.dynamicFriction = 0;
                        owner.phyMat.frictionCombine = PhysicMaterialCombine.Minimum;
                    }
                    else
                    {
                        owner.phyMat.dynamicFriction = 0;
                        owner.phyMat.frictionCombine = PhysicMaterialCombine.Minimum;
                    }
                }

                //Input magnatude, used for checking if a player is pressing in a direction
                {
                    moveDirectionMag = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
                }

                //Wall running enabled by speed
                {
                    owner.wallRun = Mathf.Min(owner.wallRun + Mathf.InverseLerp(20, 40, owner.rbVelocityMagnatude) * Time.deltaTime, 1);
                }                             

                //Character faces the move direction or velocity direction                                      
                {

                    

                    owner.rb.angularVelocity = Vector3.zero;

                    Vector3 upwards = Vector3.Lerp(-owner.gravity.normalized, owner.surfaceNormal, owner.rbVelocityMagnatude * .1f);

                    if (moveDirectionMag > .1f && !owner.locomotionInputInterupt)
                    {
                        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(moveDirection, upwards), Time.deltaTime * 5);                                                                                         
                    }
                    else
                    {                      
                                                                                                             
                        Quaternion rotation = Quaternion.LookRotation((owner.rbVelocityNormalized - (upwards * Vector3.Dot(owner.rbVelocityNormalized, upwards))).normalized, upwards);

                        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, Time.deltaTime * 5);
                        
                    }

                }

                //WrapAroundSurfaces                                
                {

                    Vector3 newVelocity = Vector3.Cross(Vector3.Cross(owner.surfaceNormal, owner.rb.velocity.normalized), owner.surfaceNormal) * owner.rb.velocity.magnitude;

                    if (Vector3.Dot(owner.rb.velocity, newVelocity) > .866f)
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

                //Calulate Traction
                {
                    traction = Mathf.Clamp01(Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, owner.transform.forward)) + .5f);
                    traction = 1f;
                }

                //canRun reset
                {
                    if (owner.rbVelocityMagnatude < topSpeedWalk * .5f) canRun = false;
                }

                //Accelerator
                {
                    //AcceleratorTime Update
                    {
                        if ((owner.rbVelocityMagnatude > topSpeedRun) || (canRun && owner.rbVelocityMagnatude > topSpeedWalk))
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

                //DashTime update
                {
                    //Increment dashTime
                    if (dashing)
                    {
                        dashTime += Time.deltaTime;
                    }

                    //Reset dashing and dashTime once we have dashed the full duration
                    if (dashTime >= dashDuration)
                    {
                        dashTime = 0;
                        dashing = false;
                    }

                    //Allow the player to cancel a dash early if needed
                    if (dashing && !owner.locomotionInputInterupt && moveDirectionMag == 0)
                    {
                        dashing = false;
                        dashTime = 0;
                    }
                }

                //Ground Top Speed Control
                if (!dashing)
                {
                    if (accelerator)
                    {
                        topSpeedCurrent = topSpeedMax;
                    }
                    else
                    {
                        topSpeedCurrent = topSpeedWalk;
                        if (canRun) topSpeedCurrent = topSpeedRun;
                        if (owner.crouching) topSpeedCurrent = topSpeedMax;
                    }

                }

                //Control Drag and clamp top speed                
                {                                     
                    owner.rb.drag = 0;
                    if (owner.rbVelocityMagnatude < topSpeedRun + 1 && moveDirection == Vector3.zero)
                        owner.rb.drag = 5;

                    owner.rb.velocity = Vector3.ClampMagnitude(owner.rb.velocity, topSpeedCurrent);                    
                }

            }
            else //Airborn
            {

                //Allow the physics engine to rotate the character
                {
                    owner.rb.freezeRotation = false;
                }

                //Reset angualr drag
                {
                    owner.rb.angularDrag = 3;
                }

                //Reset drag
                {
                    owner.rb.drag = 0;
                }

                //JumpBuffer Update
                {
                    jumpBuffer += Time.deltaTime;
                }

                //DashBuffer Update
                {
                    dashBuffer += Time.deltaTime;
                }

                //Calulate angular acceleration force gain.
                {
                    angularGain = new Vector3(5, 4, 1);
                }
            }

            //Power Level charge and decharge
            {

                if (accelerator || owner.crouching)
                {
                    owner.energyLevel = Mathf.Min(owner.energyLevel + Time.deltaTime, 1);
                    owner.chargingEnergy = true;
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

            //Dash      
            if (owner.energyLevel == 1)
            {
                if (!owner.locomotionInputInterupt)
                    if (dashBuffer <= dashBufferMax)
                    {
                        dashBuffer = Mathf.Infinity;
                        if (moveDirectionMag > .1f)
                        {                            
                            //GameManager.manager.PauseGameForDuration(dashWindup);
                            owner.cameraHelper.CauseCameraShake(5);
                            owner.rb.velocity = moveDirection.normalized * Mathf.Max(dashSpeed,owner.rbVelocityMagnatude);                            
                            owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                            float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);
                            //Disable camera lerp when pushed back by a shockwave, but behind the character.
                            //if (owner.rbVelocityMagnatude > dashSpeed)
                            { 
                                owner.cameraHelper.CameraSetLerp((dot + 1) * .5f);
                            }
                            owner.transform.rotation = Quaternion.LookRotation(moveDirection, -owner.gravity);                            
                            owner.energyLevel = 1;
                            dashing = true;
                            dashTime = 0;
                            canRun = true;
                            acceleratorTime = acceleratorWindup;
                            accelerator = true;
                            topSpeedCurrent = Mathf.Max(dashSpeed, owner.rbVelocityMagnatude);
                            owner.locomotionInputInterupt = true;
                            owner.SendMessage("Explode");
                            Debug.Log("Dash!");
                        }

                    }

                //Maintain Dash
                if (dashing)
                {                  
                    Vector3 dir = Vector3.Cross(Vector3.Cross(owner.rbVelocityNormalized, -owner.surfaceNormal), owner.surfaceNormal);
                    owner.rb.velocity = dir * Mathf.Max(dashSpeed, owner.rbVelocityMagnatude);
                    owner.rb.drag = 0;
                }
            }

            //Jumping.
            if (!owner.locomotionInputInterupt)
                if (jumpBuffer < jumpBufferMax)
                {                            
                    jumpBuffer = Mathf.Infinity;
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
                        GameManager.manager.PauseGameForDuration(dashWindup);
                        owner.rb.velocity += owner.surfaceNormal * 180;
                        owner.energyLevel = 0;
                        Debug.Log("Power Jump");
                        owner.stateMachine.ChangeModule(SymFlightModule.Instance);
                    }

                    owner.animator.SetTrigger("Jump");
                    owner.wallRun = 0;
                    owner.coyoteTime = owner.coyoteTimeMax;                                                                            
                }

            //Forward Acceleration on ground
            if (!owner.locomotionInputInterupt)
                if (!dashing && !owner.crouching)
                {
                    owner.rb.AddForce(moveDirection * moveAcceleration * traction, ForceMode.Acceleration);
                }

            //Angular Acceleration while crouched              
            if (owner.crouching)
            {
                //Vector3 axis = ((owner.transform.right * verticalInput) + (-owner.transform.up * horizontalInput)).normalized;
                Vector3 axis = -owner.transform.up * horizontalInput;
                //float input = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));                
                float scale = Mathf.InverseLerp(5, 10, owner.rbVelocityMagnatude);
                //Make angualr gain a weighted average
                owner.rb.AddTorque(axis * angularAccelerationBase * angularGain.x * scale, ForceMode.Acceleration);
            }

            //Grip force
            if (!owner.locomotionInputInterupt && !owner.crouching)
            {                
                Vector3 force = SymUtils.RedirectForce(moveDirection, owner.rbVelocityNormalized, owner.rbVelocityMagnatude, traction);
                owner.rb.AddForce(force * 2 , ForceMode.Acceleration);
            }

        }
        else //Airborn motion
        {
            Vector3 heading;
            Vector3 forward = owner.transform.up;

            //determin TargetHeading
            {
                //Vector3 focusedHeading = Camera.main.transform.forward;
                Vector3 signedVelocityNormal = owner.rbVelocityNormalized * Mathf.Sign(Vector3.Dot(owner.rbVelocityNormalized, forward));
                Vector3 unfocusedHeading = signedVelocityNormal;// Vector3.Lerp(signedVelocityNormal, forward, thrustInput);//Lerp between the Velocity and the characters flight forward for no rotation effect. Stabalizes forward flight at low thrust levels
                unfocusedHeading = Vector3.Lerp(forward, unfocusedHeading, owner.rbVelocityMagnatude);//extra scale based on velocity for when stationary in air
                heading = unfocusedHeading;// Vector3.Lerp(unfocusedHeading, focusedHeading, focusInput);//Lerp between using the unfocusedHeading, and using the camera.
            }

            //Align towards the target heading.                    
            {
                Vector3 axis = Vector3.Cross(forward, heading);//Get the axis to turn around.
                float disable = 1 - Mathf.Clamp01(Mathf.Abs(verticalInput * 2) + Mathf.Abs(horizontalInput * 2));//Scalar to disable auto turning                                               
                owner.rb.AddTorque(axis * disable * 10, ForceMode.Acceleration);
            }

            //Angular Acceleration              
            {
                Vector3 axis = ((owner.transform.right * verticalInput) + (-forward * horizontalInput)).normalized;
                float input = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
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

                //Adjust accelerator             
                if (owner.surfaceTraversalSpeed > 20)
                {
                    canRun = true;
                    acceleratorTime = acceleratorWindup;
                }

                topSpeedCurrent = Mathf.Max(owner.surfaceTraversalSpeed, topSpeedWalk);
            

            }
            else
            {
                Debug.Log("verticalSpeed !< 1");
            }

    }

    public override void OnCollisionStay(SymBehaviour owner, Collision collision)
    {
        
        if (!owner.grounded)
            if (Mathf.Sign(owner.surfaceVerticalSpeed) < 1)
            {
                //Character Effects            
                owner.grounded = true;
            }
            else
            {
                Debug.Log("verticalSpeed !< 1");
            }

    }

    public override void ExitModule(SymBehaviour owner)
    {
        canRun = false;
        dashing = false;
        dashTime = dashDuration;
        acceleratorTime = 0;
        accelerator = false;
        jumpBuffer = jumpBufferMax;
    }

}
