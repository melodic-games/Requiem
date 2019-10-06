using UnityEngine;
using SymBehaviourModule;
using System.Collections.Generic;
using ConsoleCMD;

public class SymBaseModule : Module<SymBehaviour>
{

    private static SymBaseModule instance;

    //Ground State
    public float moveAcceleration = 40;
    public float topSpeed = 10;    
    private float groundTopSpeedBase = 10;

    //StairSteping
    private float maxStepHeight = 0.7f; //KneeHeight of character
    private float stepSearchOvershoot = 0.01f;
    private List<ContactPoint> allCPs = new List<ContactPoint>();
    private Vector3 lastVelocity;

    private float jumpPower = 10;

    private bool locomotionInputInterupt = false;

    private bool dashing = false;
    private float dashSpeed = 100;
    private float dashTime = 0;
    private float dashDuration = .2f;
    private float dashInterupt = .1f;
    private float dashWindup = .02f;

    private bool accelerator = false;
    private float acceleratorTime = 0;
    private float acceleratorWindup = 3;

    public float traction = 1;
    private float wallWalkScaler = 0;

    private bool useCoyoteTime = false;
    private float coyoteTime = 0;
    private float coyoteTimeMax = .2f;

    //Airborn State
    private float angularAccelerationBase = 10;
    private Vector3 angularGain = new Vector3(1, 1, 1);

    //Input data          
    private float horizontalInput;
    private float verticalInput;

    private float dashBuffer = Mathf.Infinity;
    private float dashBufferMax = .5f;

    private Vector3 moveDirection;
    private Vector3 moveDirectionRaw;
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
        //Reset Movement Values for when on ground          
        {           
            owner.rb.drag = 1f;
            jumpBuffer = jumpBufferMax;
            topSpeed = Mathf.Max(owner.traversalSpeed, groundTopSpeedBase);

            if (Vector3.Dot(owner.groundNormal, -owner.gravity.normalized) < .5f)
            {
                wallWalkScaler = 1;
            }

            if (owner.traversalSpeed > 20)
            {
                canRun = true;
                acceleratorTime = acceleratorWindup;
            }            

            owner.transform.rotation = Quaternion.LookRotation(Vector3.Cross(owner.transform.right, -owner.gravity), -owner.gravity);
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
            
            //Direct Input
            horizontalInput = owner.controlSource.horizontalInput;
            verticalInput = owner.controlSource.verticalInput;

            //Crouching
            owner.crouching = owner.controlSource.crouching;            

            //Jump
            if (owner.controlSource.jump)
            {
                jumpBuffer = 0;
            }

            //Dash
            if (!dashing)
                if (owner.controlSource.dash)
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

            //Check to make sure we have not left the ground and grab surface data       
            if (owner.grounded)
            {                                                   
                RaycastHit hit;
                Ray raycastRay = new Ray(owner.transform.position, Vector3.Lerp(owner.gravity, owner.gravity.magnitude * -owner.groundNormal, wallWalkScaler));
                //Debug.DrawRay(owner.transform.position, raycastRay.direction * (owner.playerHeight * .5f + .1f));
                if (Physics.Raycast(raycastRay, out hit, owner.playerHeight, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                {
                    owner.groundNormal = hit.normal;
                    coyoteTime = 0;
                    useCoyoteTime = false;
                }
                else
                {
                    if (!dashing)
                        coyoteTime += Time.deltaTime;
                    useCoyoteTime = true;
                }

                //if we have exausted our coyoteTime we are no longer grounded
                if (coyoteTime > coyoteTimeMax)
                {
                    owner.grounded = false;                        
                    wallWalkScaler = 0;
                }

                //CoyoteTime disables gravity
                {
                    if (coyoteTime < coyoteTimeMax && useCoyoteTime)
                        owner.enableGravity = 0;
                }           
            }
            
            //Ground adjustments and calulations
            if (owner.grounded)
            { 

                //Reset angular drag
                {
                    owner.rb.angularDrag = 1000;
                    if(owner.crouching)
                        owner.rb.angularDrag = 3;
                }

                //Calulate the move direction, based on camera direction and input
                {
                    Vector3 movez = Vector3.Cross(Camera.main.transform.right, owner.groundNormal);
                    Vector3 movex = -Vector3.Cross(Camera.main.transform.forward, owner.groundNormal);

                    moveDirectionRaw = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)) * (movez * verticalInput + movex * horizontalInput).normalized;                                                         
                    
                    //Reset MoveDirection upon pulling in the other direction
                    if (Vector3.Dot(moveDirection, moveDirectionRaw) < -.2f) 
                    {
                        moveDirection = moveDirectionRaw;
                        Debug.Log("switch");
                    }
                          
                    moveDirection = Vector3.Slerp(moveDirection, moveDirectionRaw, Time.deltaTime * Mathf.Lerp(10,1,Mathf.InverseLerp(0,dashSpeed,owner.rbVelocityMagnatude)));                  
                }

                //Wall walking enabled by speed
                {
                    wallWalkScaler = Mathf.Min(wallWalkScaler + Mathf.InverseLerp(20, 40, owner.rbVelocityMagnatude) * Time.deltaTime, 1);
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

                //Send camera helper data
                {
                    owner.cameraHelper.manualUpVectorScaler = wallWalkScaler;                    
                }

                //Gravity redirection from wall walking
                {
                    owner.gravity = Vector3.Lerp(owner.gravity, owner.gravity.magnitude * -owner.groundNormal, wallWalkScaler);
                }

                //Set rotation on ground, and prevent rigidbody from tipping over                     
                {        
                    owner.transform.rotation = Quaternion.LookRotation(Vector3.Cross(owner.transform.right, owner.groundNormal), owner.groundNormal);
                    if (!owner.crouching)
                    owner.rb.angularVelocity = Vector3.zero;
                }

                //Input magnatude, used for checking if a player is pressing in a direction
                {
                    moveDirectionMag = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
                }

                //Character faces the move direction or velocity direction                       
                {                                  
                    if (moveDirectionMag > .1f && !locomotionInputInterupt)
                    {
                        if (!owner.crouching)
                            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(moveDirection, owner.groundNormal), Time.deltaTime * 5);
                    }
                    else if (owner.rbVelocityMagnatude > 1 & moveDirectionMag < .1f)
                    {
                        float scale = Mathf.InverseLerp(5, 10, owner.rbVelocityMagnatude) * 5;
                        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, Quaternion.LookRotation(owner.rbVelocityNormalized, owner.groundNormal), Time.deltaTime * scale);
                    }                
                }

                //Calulate Traction
                {
                    traction = Mathf.Clamp01(Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, owner.transform.forward)) + .2f);
                    if (owner.crouching) traction = .2f;
                }

                //canRun reset
                {
                    if (owner.rbVelocityMagnatude < 5) canRun = false;
                }

                //Accelerator
                {
                    //AcceleratorTime Update
                    {
                        if (canRun)
                            acceleratorTime += Time.deltaTime;
                        else                       
                            acceleratorTime = 0;

                        acceleratorTime = Mathf.Clamp(acceleratorTime, 0, acceleratorWindup);
                    }

                    //Accelerator Calulation        
                    {
                        if (acceleratorTime >= acceleratorWindup && canRun)
                        {
                            accelerator = true;
                        }
                        else
                        {
                            accelerator = false;
                        }
                    }
                }

                //calulate locomotion input interruption
                {
                    locomotionInputInterupt = false;

                    if (dashTime < dashInterupt && dashing)
                        locomotionInputInterupt = true;

                    if (owner.landingLag < owner.landingLagMax)
                        locomotionInputInterupt = true;
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
                    if (dashing && !locomotionInputInterupt && moveDirectionMag == 0)
                    {
                        dashing = false;
                        dashTime = 0;
                    }
                }

                //Control Drag                
                {
                    if (!dashing)
                    {
                        //Grip
                        float disable = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, moveDirection));

                        //max velocity adjustment
                        float highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(topSpeed - 1, topSpeed, owner.rbVelocityMagnatude));

                        float stopDrag;
                        stopDrag = (1 - moveDirectionMag) * 4;
                        if (accelerator)
                            stopDrag = (1 - moveDirectionMag) * .5f;
                        if (owner.crouching)
                            stopDrag = .1f;

                        owner.rb.drag = (stopDrag + highEndDrag) * traction;
                    }
                }

            }
            else //Airborn
            {
                //Reset angualr drag
                {
                    owner.rb.angularDrag = 3;
                }

                //Reset angualr drag
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

            //Dash      
            if (owner.energyLevel == 1)
            {
                if (!locomotionInputInterupt)
                    if (dashBuffer <= dashBufferMax)
                    {
                        dashBuffer = Mathf.Infinity;
                        if (moveDirectionMag > .1f)
                        {
                            owner.cameraHelper.CauseCameraShake(5);
                            owner.rb.velocity = moveDirectionRaw.normalized * dashSpeed;
                            owner.rbVelocityNormalized = owner.rb.velocity.normalized;
                            float dot = Vector3.Dot(-owner.rbVelocityNormalized, Camera.main.transform.forward);
                            //Disable camera lerp when pushed back by a shockwave, but behind the character.                        
                            owner.cameraHelper.CameraSetLerp((dot + 1) * .5f);
                            owner.transform.rotation = Quaternion.LookRotation(moveDirectionRaw, -owner.gravity);
                            moveDirection = moveDirectionRaw;
                            owner.energyLevel = 1;
                            dashing = true;
                            dashTime = 0;
                            canRun = true;
                            acceleratorTime = acceleratorWindup;
                            accelerator = true;
                            topSpeed = dashSpeed;
                            locomotionInputInterupt = true;
                            owner.SendMessage("Explode");
                        }

                    }

                //Maintain Dash
                if (dashing)
                {
                    owner.rb.velocity = owner.rbVelocityNormalized * dashSpeed;
                    owner.rb.drag = 0;
                }
            }

            //Jumping.
            if (!locomotionInputInterupt)
                if (jumpBuffer < jumpBufferMax)
                {                   
                    jumpBuffer = Mathf.Infinity;
                    owner.grounded = false;                    
                    owner.rb.velocity += owner.groundNormal * jumpPower;
                    owner.animator.SetTrigger("Jump");
                    wallWalkScaler = 0;
                    coyoteTime = coyoteTimeMax;
                }

            //Ground Top Speed Control
            if (!locomotionInputInterupt)
            {
                if (accelerator)
                {

                    if (topSpeed > dashSpeed)
                    {
                        topSpeed -= dashSpeed * 1f * Time.deltaTime;
                        topSpeed = Mathf.Max(topSpeed, dashSpeed);
                    }
                    else
                    {
                        topSpeed += dashSpeed * .1f * Time.deltaTime;
                        topSpeed = Mathf.Min(topSpeed, dashSpeed);
                    }

                }
                else
                {
                    topSpeed -= dashSpeed * 1f * Time.deltaTime;
                    float offset = 0;
                    if (canRun) offset = groundTopSpeedBase;
                    topSpeed = Mathf.Max(topSpeed, groundTopSpeedBase + offset);
                }

            }

            //Forward Acceleration on ground
            if (!locomotionInputInterupt && !owner.crouching)
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
            if (!locomotionInputInterupt)
            {                
                Vector3 force = SymUtils.RedirectForce(moveDirection, owner.rbVelocityNormalized, owner.rbVelocityMagnatude, traction);
                owner.rb.AddForce(force * 5 , ForceMode.Acceleration);
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

    public override void OnCollissionStay(SymBehaviour owner, Collision collision)
    {
        if (!owner.grounded)
        {
            owner.grounded = true;
        }
        else
        {
            //Stair Steping
            //allCPs.AddRange(collision.contacts);
        }
        
    }

    public override void OnCollisionEnter(SymBehaviour owner, Collision collision)
    {
            
        if (!owner.grounded)
        {                                
            //Character Effects            
            owner.grounded = true;

            //Check if we should ground the character normally, or if the character should stick to walls
            if (Vector3.Dot(owner.groundNormal, -owner.gravity.normalized) < .5f && owner.energyLevel  == 1)
            {
                wallWalkScaler = 1;
            }

            //Adjust accelerator 
            {
                if (owner.traversalSpeed > 20)
                {
                    canRun = true;
                    acceleratorTime = acceleratorWindup;
                }

                topSpeed = Mathf.Max(owner.traversalSpeed,groundTopSpeedBase);
                owner.transform.rotation = Quaternion.LookRotation(Vector3.Cross(owner.transform.right, -owner.gravity), -owner.gravity);
            }
                                              
        }
        else
        {
            //Stair Steping
            //allCPs.AddRange(collision.contacts);

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
