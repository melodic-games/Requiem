using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]

public class SymphonicBehaviour : MonoBehaviour {

    //References
    [Header("References")]
    public Material[] bodyMaterials;
    public Material emissionMaterial;
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    public DynamicBone[] dynamicBones;
    public TrailRenderer trailRendererLeft;
    public TrailRenderer trailRendererRight;
    private CameraControl cameraControl;
    public Object particleExplosion;
    public ChargeParticleSystem chargeParticleSystem;

    //Character   
    [Header("Character Info")]
    public float powerLevel = 0f;
    private Vector3 moveDirection;
    private float inputMag;
    private Vector3 targetHeading = Vector3.up;
    [SerializeField] private float moveAcceleration = 20;//Base force value used to accelerate the character    
    [SerializeField] private float angularAccelerationBase = 10;//Base force value used to rotate the character  
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private float colliderRadius = .25f;
    private bool grounded = false;
    private bool groundCheckHit = false;
    [SerializeField] private float groundedCheckLenght = 1.8f;
    [SerializeField] private float jumpBufferMax = .5f;
    [SerializeField] private float jumpPower = 5;
    private bool dashing = false;
    [SerializeField] private float dashSpeed = 50;
    private float dashTime = 0;
    [SerializeField] private float dashLength = .2f;
    [SerializeField] private float dashInterupt = .05f;
    [SerializeField] private float dashWindup = .02f;
    private bool accelerator = false;
    private float acceleratorTime = 0;
    [SerializeField] private float acceleratorWindup = 3;
    [SerializeField] private float topSpeed = 10;
    [SerializeField] private float groundTopSpeedBase = 10;
    private bool locomotionInterupt = false;
    public float wallWalkScaler = 0;
    private bool useCoyoteTime = false;
    private float coyoteTime = 0;
    private float coyoteTimeMax = .1f;
    private bool intangible = false;
    private float landingType = 0;
    private float landingLagTime = 100;
    [SerializeField] private float landingLagTimeMax = 2;
    private bool flight = true;
    [SerializeField] private float airTopSpeedBase = 180;
    
    private float charging = 0;
    [SerializeField] private float emissionColorIndex;

    private Vector3 camLocalOffset = Vector3.zero;
    [SerializeField] private float camZoomDistanceMin = 4;
    [SerializeField] private float camZoomDistanceMax = 10;
   
    //Physics        
    [Header("Physics")]    
    public float rbVelocityMagnatude;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float groundSpeed;
    public Vector3 rbVelocityNormalized;
    public Vector3 localAngularVelocity;
    public Vector3 netForce = Vector3.zero;
    private float lateralDrag;
    public float feetGrip = 1;
    public Vector3 groundNormal = Vector3.up;
    public Vector3 angularGain = new Vector3(1, 1, 1);
    public float liftScale = 1;
    public float rollSpinUp = 0;
    private float enableGravity;
    public GravityManager gravityManager;
    private Vector3 gravity = Vector3.down;

    //Input data      
    [Header("Input Data")]
    public float jumpBuffer = Mathf.Infinity;
    public float thrust = 0;
    public bool thrustAsButtion = false;
    public bool canRun = false;
    public float rollAxisInput = 0;
    public float pitchAxisInput = 0;
    public float yawAxisInput = 0;
    public float focus = 0;
    
    void Start()
    {
        gameObject.layer = 10;
        gameObject.tag = "Player";

        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Symphonic/Animation/SymphonicController");

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularDrag = 5;
        rb.drag = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        particleExplosion = Resources.Load<Object>("Prefabs/Explosion1");

        //bodyMaterials[0] = Resources.Load<Material>("Symphonic/Mat_Symphonic");
        //bodyMaterials[1] = Resources.Load<Material>("Symphonic/Face");

        //emissionMaterial = Resources.Load<Material>("Symphonic/Mat_Emission");

        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.radius = .25f;//.5 for sphere mode
        capsuleCollider.height = playerHeight;//0 for sphere mode
        capsuleCollider.direction = 1;//Y-Axis                     

        cameraControl = Camera.main.GetComponent<CameraControl>();               
        targetHeading = Camera.main.transform.forward;
        gravityManager = FindObjectOfType<GravityManager>();
        rb.maxAngularVelocity = 20;
        rb.centerOfMass = Vector3.zero;
    }

    private void Update()
    {
        //Update values
        {
            rbVelocityMagnatude = rb.velocity.magnitude;
            rbVelocityNormalized = rb.velocity.normalized;
            localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

            verticalSpeed = rbVelocityMagnatude * Vector3.Dot(rbVelocityNormalized, -gravity.normalized);
            groundSpeed = rbVelocityMagnatude * (1 - Mathf.Abs(Vector3.Dot(rbVelocityNormalized, -gravity.normalized)));
        }

        foreach (DynamicBone bone in dynamicBones)
        {
            bone.m_Stiffness = powerLevel;// Mathf.InverseLerp(3, 20, Mathf.Abs(localAngularVelocity.y));
            bone.UpdateParameters();
        }

        //JumpBuffer Update
        {
            jumpBuffer += Time.deltaTime;
        }

        //Input magnatude, used for checking if a player is pressing in a direction
        {
            if (grounded)
                inputMag = Mathf.Clamp01(Mathf.Abs(pitchAxisInput) + Mathf.Abs(rollAxisInput));
            else
                inputMag = 0;
        }

        //DashTime update
        {
            if (dashing)
            {
                dashTime += Time.deltaTime;
            }

            if (dashTime >= dashLength)
            {
                dashTime = 0;
                dashing = false;
            }

            //Disable Dashing if no input after groundLocomotionInterupt is finished.
            if (dashing && !locomotionInterupt && inputMag == 0)
            {
                dashing = false;
            }
        }
        
        //canRun reset
        {
            if (inputMag == 0) canRun = false;
        }     

        //AcceleratorTime Update
        {
            if (canRun)
                acceleratorTime += Time.deltaTime;

            if (inputMag < .5f)
                acceleratorTime = 0;

            acceleratorTime = Mathf.Clamp(acceleratorTime, 0, acceleratorWindup + 1);
        }



        //Accelerator Calulation        
        {
            if (acceleratorTime > acceleratorWindup && canRun)
            {
                accelerator = true;
            }
            else
            {
                accelerator = false;
            }
        }

        //Coyote Time Extend
        /*
        {            
            switch (accelerator)
            {
                case true:
                    coyoteTimeMax = Mathf.Min(coyoteTimeMax + 2 * Time.deltaTime,.5f);                    
                    break;
                case false:
                    coyoteTimeMax = Mathf.Max(coyoteTimeMax - 2 * Time.deltaTime, .1f);
                    break;
            }
        }
        */

        //Power Level charge and decharge
        {

            if (Mathf.Abs(localAngularVelocity.y) > 10 || accelerator)
            {                             
                powerLevel = Mathf.Min(powerLevel + Time.deltaTime, 1);
                         
                charging = Mathf.Min((powerLevel + 2 * Time.deltaTime) * (1 - thrust), 1);
                if (chargeParticleSystem != null)
                {
                    chargeParticleSystem.state = 1;
                    if (powerLevel == 1)
                        chargeParticleSystem.state = 0;
                }
            }
            else if (Mathf.Abs(localAngularVelocity.y) < 2 && !accelerator)
            {
                powerLevel = Mathf.Max(powerLevel - Time.deltaTime, 0);

                charging = Mathf.Max((powerLevel - 2 * Time.deltaTime) * (1 - thrust), 0);
                if (chargeParticleSystem != null)
                chargeParticleSystem.state = 0;                            
            }

            

            //Set color of body and emission materials          
            {
                Vector4 color;
                
                //Calulate color from index
                {
                   color = SignatureColors.colors[(int) emissionColorIndex];
                }

                foreach (Material material in bodyMaterials)
                {
                    material.SetColor("_EmissiveColor", color);
                    material.SetFloat("_ShellPower", ((1 - powerLevel) * 5) + 1);
                }
                
                    emissionMaterial.SetColor("_EmissiveColor", color);
            }

            //Set shell of body     
            {
                foreach (Material material in bodyMaterials)
                {                 
                    material.SetFloat("_ShellPower", ((1 - powerLevel) * 5) + 1);
                }
            }

        }

        //Trail Control
        {
            float widthMultiplier;

            float beginTrailSpeed = 30;
            float trailGrowRange = 30;

            float velScale = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, rbVelocityMagnatude);
            //float widthScale = 1 + (Mathf.Clamp01(Mathf.Abs(localAngularVelocity.y) / 20) * 4);
            float angxDisable = 1 - Mathf.Clamp01(Mathf.Abs(localAngularVelocity.x / 5));// if pitch speed is 0.2 or greater disable width.              

            widthMultiplier = velScale * angxDisable * (1-charging);

            widthMultiplier *= Mathf.Clamp01((Vector3.Dot(rbVelocityNormalized, transform.up) * 2) - 1);

            if (trailRendererLeft != null)
            {
                trailRendererLeft.widthMultiplier = 0.05f * widthMultiplier;
                trailRendererLeft.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

            if (trailRendererRight != null)
            {
                trailRendererRight.widthMultiplier = 0.05f * widthMultiplier;
                trailRendererRight.time = widthMultiplier == 0 ? 0 : 0.2f;
            }
        }

        //adjust how the player should appear on screen.  
        {
            Vector3 headHeight = new Vector3(0, .7f, 0);

            if (!grounded)
            {
                camLocalOffset = Vector3.zero;
                camZoomDistanceMin = 4;
                if (powerLevel == 1 && rbVelocityMagnatude < 20)
                    camZoomDistanceMin = 1;
                camZoomDistanceMax = 10;
            }
            else
            {
                camLocalOffset = headHeight;
                camZoomDistanceMin = 5;
                camZoomDistanceMax = 5;
            }

            //cameraControl.lerpSpeed = Mathf.Lerp(20,50,Mathf.InverseLerp());
            cameraControl.localOffset = Vector3.Slerp(cameraControl.localOffset, camLocalOffset, Time.deltaTime * 2);
            cameraControl.zoomDistanceMin = Mathf.Lerp(cameraControl.zoomDistanceMin, camZoomDistanceMin, Time.deltaTime);
            cameraControl.zoomDistanceMax = Mathf.Lerp(cameraControl.zoomDistanceMax, camZoomDistanceMax, Time.deltaTime);

        }

        //Set animator values 
        if (animator != null)
        {
            animator.SetFloat("Speed", rbVelocityMagnatude, 0.1f, Time.deltaTime);
            animator.SetFloat("VerticalSpeed", verticalSpeed, 0.1f, Time.deltaTime);            
            animator.SetFloat("RotZ", Mathf.Abs(localAngularVelocity.z), 0.1f, Time.deltaTime);
            animator.SetFloat("RotX", Mathf.Abs(localAngularVelocity.x), 0.1f, Time.deltaTime);
            animator.SetFloat("RotY", Mathf.Abs(localAngularVelocity.y), 0.1f, Time.deltaTime);
            animator.SetFloat("Drag", rb.drag / .2f, 0.3f, Time.deltaTime);
            animator.SetFloat("DragDir", lateralDrag, 0.1f, Time.deltaTime);
            animator.SetFloat("Charge", charging, 0.1f, Time.deltaTime);                      
            animator.SetFloat("LandingType", landingType);
            animator.SetFloat("GroundForwardSpeed", Mathf.Clamp(rbVelocityMagnatude * Vector3.Dot(rbVelocityNormalized, transform.forward),-30,30));
            animator.SetFloat("GroundLateralSpeed", Mathf.Clamp(rbVelocityMagnatude * Vector3.Dot(rbVelocityNormalized, transform.right),-10,10));
            animator.SetBool("Grounded", grounded);
            animator.SetBool("CanFly", flight);
            if (grounded)
                animator.speed = Mathf.InverseLerp(12, 30, rbVelocityMagnatude) * 1f + 1;
            else
                animator.speed = 1;
        }

    }

    void FixedUpdate()
    {    
        netForce = Vector3.zero;        

        //Maintain Spin if inside of an object
        //if (insideVolume)
        {
            //rb.angularVelocity = rb.angularVelocity.normalized * 20;
        }

        //Intangibility 
        {
            if (intangible)                
            {
                capsuleCollider.isTrigger = true;
                cameraControl.clippingAllowed = true;
            }

            if (Mathf.Abs(localAngularVelocity.y) < 10)
            {
                capsuleCollider.isTrigger = false;
                cameraControl.clippingAllowed = false;
            }
        }

        //Gravity Calulation    
        {                                  
            if (gravityManager != null)
            {
                gravity = gravityManager.ReturnGravity(transform);                
            }
            else
            {
                gravity = Vector3.Lerp(gravity, Vector3.zero, Time.deltaTime);
            }
                                        
        }

        // Main Behaviour function
        {
            RaycastHit hit;
            Ray raycastRay = new Ray(transform.position, Vector3.Lerp(gravity, gravity.magnitude * -groundNormal, wallWalkScaler));  

            groundCheckHit = Physics.Raycast(raycastRay, out hit, 1000, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

            //check to make sure we have not left the ground
            {
                if (!dashing)
                {
                    
                    if (groundCheckHit)        
                    {
                        if (hit.distance <= groundedCheckLenght)
                        {
                            groundCheckHit = true;
                            groundNormal = hit.normal;
                        }
                        else
                        {
                            groundCheckHit = false;
                        }

                        if (hit.distance > groundedCheckLenght * 2)
                        {
                            useCoyoteTime = true;
                        }
                        else
                        {
                            useCoyoteTime = false;
                        }
                    }  
                    else
                    {
                        useCoyoteTime = true;
                    }

                }

                //check if we have exausted our coyoteTime
                if (coyoteTime >= coyoteTimeMax)
                {
                    grounded = false;
                    wallWalkScaler = 0;
                    cameraControl.manualUpVectorScaler = 0;
                }

                //CoyoteTime increment happens after checking its value
                if (!groundCheckHit)
                {
                    coyoteTime += Time.deltaTime;
                }

            }

            if (grounded)
            {                        
                //Passive Ground adjustments and calulations 
                {
                    //Ground Effects
                    {     

                    }

                    //Reset Movement Values for when on ground          
                    {
                        rb.angularDrag = 1000;
                        moveAcceleration = 40;
                        rb.drag = 1f;
                    }

                    //Calulate the move direction, based on camera direction and input
                    {
                        Vector3 movez = Vector3.Cross(Camera.main.transform.right, groundNormal);
                        Vector3 movex = -Vector3.Cross(Camera.main.transform.forward, groundNormal);

                        moveDirection = movez * pitchAxisInput + movex * rollAxisInput;
                        

                        //Vector3 project = Vector3.ProjectOnPlane(Camera.main.transform.forward, groundNormal);
                        //Debug.DrawRay(hit.point,project,Color.blue);
                        //Vector3 cross = Vector3.Cross(groundNormal, project);
                        //Debug.DrawRay(hit.point, cross, Color.red);

                        //moveDirection = project * pitchAxisInput + cross * rollAxisInput;

                        //Debug.DrawRay(hit.point + groundNormal * .5f, movez, Color.black);
                        //Debug.DrawRay(hit.point + groundNormal * .5f, movex, Color.black);


                    }

                    //Wall walking enabled by powerlevel
                    {
                        if (powerLevel == 1)
                            wallWalkScaler = 1;
                    }

                    //Gravity modification from wall walking
                    {
                        gravity = Vector3.Lerp(gravity, gravity.magnitude * -groundNormal, wallWalkScaler);
                        //Camera upvector modification
                        {
                            cameraControl.manualUpVectorScaler = wallWalkScaler;
                            cameraControl.manualUpVector = -gravity.normalized;
                        }
                    }

                    //Set rotation on ground.
                    {
                        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity), -gravity);
                        rb.angularVelocity = Vector3.zero;
                    }

                    //Adjust Capsule Collider
                    {
                        capsuleCollider.radius = colliderRadius;
                        capsuleCollider.height = playerHeight;
                    }
                }

                //increment landing lag
                {
                    landingLagTime += Time.deltaTime;
                }

                //calulate locomotion input interruption
                {
                    locomotionInterupt = false;

                    if (dashTime < dashInterupt && dashing)
                        locomotionInterupt = true;

                    if (landingLagTime < landingLagTimeMax)
                        locomotionInterupt = true;

                }  

                //Dash abblity
                if (!locomotionInterupt)
                {
                    if (thrustAsButtion)
                    {
                        thrustAsButtion = false;
                        if (inputMag > .1f)
                        {                           
                            rb.velocity = moveDirection.normalized * dashSpeed;
                            rbVelocityNormalized = rb.velocity.normalized;
                            transform.rotation = Quaternion.LookRotation(moveDirection, -gravity);
                            powerLevel = 1;
                            dashing = true;
                            dashTime = 0;
                            //canRun = true;
                            acceleratorTime = acceleratorWindup;
                            accelerator = true;
                            topSpeed = dashSpeed;
                            locomotionInterupt = true;
                        }
                        
                    }
                }

                //Maintain Dash
                if (dashing)
                {
                    rb.velocity = rbVelocityNormalized * dashSpeed;
                }
                            
                //Player Driven locomotion
                if (!locomotionInterupt)
                {
                    //Character faces the move direction             
                    {
                        if (inputMag > .1f)
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection, -gravity), Time.deltaTime * 5);
                    }

                    //Jumping requires instant velocity change. Cant use netForce field.                 
                    if (jumpBuffer <= jumpBufferMax)
                    {
                        jumpBuffer = Mathf.Infinity;
                        animator.SetTrigger("Jump");                        
                        grounded = false;
                        rb.AddForce(groundNormal * jumpPower, ForceMode.VelocityChange);
                        Debug.Log("Jump!");
                        wallWalkScaler = 0;
                        cameraControl.manualUpVectorScaler = 0;
                        coyoteTime = coyoteTimeMax;
                    }

                    //Ground Top Speed Control
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

                    //Calulate Feet Grip
                    {
                        feetGrip = Mathf.Clamp01(Mathf.Clamp01(Vector3.Dot(rbVelocityNormalized, transform.forward)) + .2f);
                    }

                    //Forward Acceleration on ground
                    {
                        netForce += moveDirection * moveAcceleration * feetGrip;
                    }
                }

                //Control Drag
                {

                    //Grip
                    float disable = Mathf.Clamp01(Vector3.Dot(rbVelocityNormalized, moveDirection));

                    //feetGrip = disable;

                    //max velocity adjustment
                    float highEndDrag = 0;

                    if (!dashing)
                        highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(topSpeed - 1, topSpeed, rbVelocityMagnatude));

                    float stopDrag = (1 - inputMag) * 4;

                    rb.drag =  (stopDrag + highEndDrag) * feetGrip;

                    if (dashing)
                    {
                        rb.drag = 0;
                    }
                }

                //Lift force
                if (!locomotionInterupt)
                {             
                    RedirectForce(moveDirection, feetGrip);
                }

            }
            else if (flight)
            {
                //reset these values for flight
                {
                    rb.angularDrag = 5;
                    topSpeed = airTopSpeedBase;
                }

                //adjust Capsule Collider
                {
                    float t = Mathf.InverseLerp(3.9f, 4, Mathf.Abs(localAngularVelocity.x));
                    capsuleCollider.radius = colliderRadius;
                    capsuleCollider.height = Mathf.Lerp(playerHeight, 0, t);
                }

                //The character flies "upwards" from their walking orientation normally
                Vector3 flightForward = transform.up;
                Vector3 flightUp = -transform.forward;

                //determin TargetHeading
                {
                    Vector3 focusedHeading = Camera.main.transform.forward;

                    Vector3 signedVelocityNormal = rbVelocityNormalized * Mathf.Sign(Vector3.Dot(rbVelocityNormalized, flightForward));
                    Vector3 unfocusedHeading = Vector3.Lerp(signedVelocityNormal, flightForward, thrust);//Lerp between using the Velocity and the characters flight forward for no rotation effect. Stabalizes forward flight at low thrust levels
                    unfocusedHeading = Vector3.Lerp(flightForward, unfocusedHeading, rbVelocityMagnatude);//extra scale based on velocity for when stationary in air

                    targetHeading = Vector3.Lerp(unfocusedHeading, focusedHeading, focus);//Lerp between using the unfocusedHeading, and using the camera.
                }

                //Align towards the target heading.                    
                {
                    Vector3 axis = Vector3.Cross(flightForward, targetHeading);//Get the axis to turn around.

                    float input = Mathf.Clamp01(Mathf.Abs(pitchAxisInput * 2) + Mathf.Abs(rollAxisInput * 2));//Scalar to negate auto turning                                               

                    rb.AddTorque(axis * (1 - input) * 10, ForceMode.Acceleration);
                }

                //Set Spinup values
                {
                    if (Mathf.Abs(rollAxisInput) == 1)
                    {
                        rollSpinUp += Mathf.Lerp(1, 3, Mathf.InverseLerp(80, topSpeed, rbVelocityMagnatude)) * Time.deltaTime;
                    }
                    else
                    {
                        rollSpinUp -= 3 * Time.deltaTime;
                    }

                    rollSpinUp = Mathf.Clamp01(rollSpinUp);
                }

                //Calulate angular acceleration force gain.
                {
                    angularGain = new Vector3(5 - (4 * thrust), 4 * (1 + rollSpinUp * Mathf.Lerp(1, 2, Mathf.InverseLerp(30, topSpeed, rbVelocityMagnatude))), 1);
                }

                //Angular Acceleration              
                {
                    //Roll - y
                    rb.AddTorque(-flightForward * rollAxisInput * angularAccelerationBase * angularGain.y, ForceMode.Acceleration);
                    //Pitch - x
                    rb.AddTorque(transform.right * pitchAxisInput * angularAccelerationBase * angularGain.x, ForceMode.Acceleration);
                    //Yaw - z
                    rb.AddTorque(flightUp * yawAxisInput * angularAccelerationBase * angularGain.z, ForceMode.Acceleration);
                }

                //Forward Acceleration             
                {
                    float t = Mathf.InverseLerp(10, 20, Mathf.Abs(localAngularVelocity.y));

                    moveAcceleration = Mathf.Lerp(10, 20, t);
                    netForce += flightForward * thrust * moveAcceleration;
                    float diveboost = Mathf.Clamp01(Vector3.Dot(flightForward, gravity.normalized));

                    //if (cameraControl != null)
                       // cameraControl.shakeMagnatude += .05f * thrust * t * Mathf.InverseLerp(20, 70, rbVelocityMagnatude) + (1*diveboost);
                }

                //Player Activated Boost
                if (thrustAsButtion)
                {
                    thrustAsButtion = false;

                    //Boosting                    
                    {

                        intangible = true;

                        //rb.AddForce(flightForward * 80 * powerLevel * (1 + rollSpinUp), ForceMode.VelocityChange);

                        rb.velocity = Vector3.ClampMagnitude(rb.velocity + flightForward * 80 * powerLevel * (1 + rollSpinUp), topSpeed);
                     
                        CauseCameraShake(powerLevel);

                        charging = 0;

                        if (chargeParticleSystem != null) chargeParticleSystem.state = 0;

                        if (powerLevel == 1)
                        {
                            if (particleExplosion != null) Instantiate(particleExplosion, transform.position, transform.rotation);
                            ShockWave(transform.position);
                        }

                        //The boost engergy has been consumed
                        powerLevel = 0;
                    }

                }

                //Control Drag
                {
                    //drag along the characters flight up vector, signed. Ignoring side axis drag.
                    lateralDrag = Vector3.Dot(rbVelocityNormalized, flightUp);

                    //drag along the characters flight forward vector, only using force along the negative forward axis.
                    float backwardsDrag = Mathf.Clamp01(Vector3.Dot(rbVelocityNormalized, -flightForward));

                    //Disable drag while summersaulting
                    float disable = 1 - Mathf.Clamp01(Mathf.Abs(localAngularVelocity.x / 2));

                    //max velocity adjustment
                    float highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(topSpeed - 20, topSpeed, rbVelocityMagnatude));

                    // backwards + lateral + highEnd
                    rb.drag = Mathf.Lerp(0, 0.2f, backwardsDrag * disable) + Mathf.Lerp(0, 1f, Mathf.Abs(lateralDrag) * disable) + highEndDrag;
                }

                //Lift force
                {                  
                    liftScale = Mathf.Clamp01(liftScale + 1 * Time.deltaTime ) * thrust;                    
                    RedirectForce(flightForward, liftScale);
                }

            }
            else
            {
                rb.drag = 0;
            }

        }

        //Apply Gravity
        {

            enableGravity = 1;

            //enableGravity = enableGravity * (1 - Mathf.InverseLerp(0, 7, Mathf.Abs(localAngularVelocity.y)));

            if (focus == 1)
                enableGravity = 0;

            if (coyoteTime < coyoteTimeMax && useCoyoteTime)
                enableGravity = 0;

            netForce += gravity * enableGravity;
        }

        //apply all net forces acting on object caused by character behaviour 
        {
            rb.AddForce(netForce, ForceMode.Acceleration);
        }

        //PID CONTROL
        {


        }

    }

    //A callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator && cameraControl)
        {
            float t = Mathf.InverseLerp(10, 0, localAngularVelocity.magnitude);
            animator.SetLookAtWeight(t, .2f, .8f, 1, .5f);    
            animator.SetLookAtPosition(cameraControl.characterTargetingPosition);          
        }
    }

    private void RedirectForce(Vector3 axis, float scale)
    {
        //Lift Force
        Vector3 forceVector = Vector3.Cross(axis, -rbVelocityNormalized).normalized;
        Vector3 crossVector = Vector3.Cross(forceVector, axis);
        //rb.AddForce(crossVector * rbVelMag * Vector3.Dot(crossVector, -rbVelNorm) * 5 * scale);
        netForce += crossVector * rbVelocityMagnatude * Vector3.Dot(crossVector, -rbVelocityNormalized) * 5 * scale;
    }

    void Beat(float currentStep)
    {
        float beatScale = 1;
        powerLevel += 1 * beatScale;     
    }

    private void OnTriggerEnter(Collider other)
    {

        CauseCameraShake(rbVelocityMagnatude / 20);
    }


    private void OnCollisionEnter(Collision collision)
    {

        coyoteTime = 0;

        if (grounded)
        {
            //Stair Steping
            {

            }
        }


        if (!grounded)
        {
            float shake = 0;
            Vector3 hitPoint = collision.GetContact(0).point;
            Vector3 hitNormal = collision.GetContact(0).normal;

            groundNormal = hitNormal;                                                                                         
                                
            //bounce if we pressed the jump button and will return to flight           
            if (jumpBuffer <= jumpBufferMax && flight)
            {
                print("bounce");

                jumpBuffer = Mathf.Infinity;

                //Find Reflection Vector
                Vector3 reflect = Vector3.Reflect(rbVelocityNormalized, hitNormal);

                //Character Effects
                {
                    //Reflect Velocity
                    rb.velocity = reflect * rbVelocityMagnatude;
                    //Cancel Angular Velocity Caused By Collission
                    rb.angularVelocity = Vector3.zero;
                    //Reflect Orientation
                    transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(reflect, rbVelocityNormalized), reflect), hitNormal);
                }

                //Camera Tracks towards next bounce position if one exists
                RaycastHit bounceFutureCheck;
                if (Physics.Raycast(transform.position, reflect, out bounceFutureCheck, 1000, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore))
                {
                    cameraControl.cameraTargetingPosition = bounceFutureCheck.point;
                    cameraControl.trackLerpScale = .1f;
                }
                else
                {
                    //cameraControl.cameraTargetingPosition = transform.position + rb.velocity * 10;
                    //cameraControl.trackLerpScale = .1f;
                }
            }
            else
            {
                print("landing");

                //Character Effects
                {
                    powerLevel = 1;
                    grounded = true;
                    landingType = 0;

                    //Check if we should ground the character normally, or if the character should stick to walls

                    if (Vector3.Dot(hitNormal, -gravity.normalized) < .5f)
                    {
                        wallWalkScaler = 1;                       
                    }

                    //Land hard if conditions are right
                    float dot = Mathf.Abs(Vector3.Dot(rbVelocityNormalized, hitNormal));
                    if (dot > 0.85f && rbVelocityMagnatude > 40)
                    {
                        landingLagTime = 0;
                        landingType = 2;
                        rb.velocity = Vector3.zero;
                        print("hard!");
                    }
                    else //Adjust accelerator 
                    {
                        
                        //Calulate ground speed relative to the collission surface
                        groundSpeed = rbVelocityMagnatude * (1 - Mathf.Abs(Vector3.Dot(rbVelocityNormalized, -groundNormal)));

                        if (groundSpeed > 20)
                        {
                            canRun = true;
                            acceleratorTime = acceleratorWindup;
                        }                                                   
                        topSpeed = groundSpeed;
                        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity), -gravity);
                    }

                }

                //Visual Effects
                {
                    if (particleExplosion != null)
                        Instantiate(particleExplosion, hitPoint + hitNormal, Quaternion.LookRotation(hitNormal) * Quaternion.Euler(90, 0, 0));

                    shake = rbVelocityMagnatude / 20;
                }

                //Explosion force
                {
                    ShockWave(hitPoint);
                }
            }

            CauseCameraShake(shake);
        }
    }

    private void CauseCameraShake(float value)
    {
        if (cameraControl != null)
            cameraControl.shakeMagnatude += value;
    }
    
    void ShockWave(Vector3 location)
    {       
        float power = 25;
        float radius = 50;
        float upForce = 5;

        Collider[] colliders = Physics.OverlapSphere(location, radius);
        foreach (Collider collider in colliders)
        {
            Rigidbody colliderRb = collider.GetComponent<Rigidbody>();
            if (colliderRb != null)
            {
                if (colliderRb != rb)
                    colliderRb.AddExplosionForce(power, location, radius, upForce, ForceMode.VelocityChange);
            }
        }
    
    }

    private void RedirectVelocity(Quaternion fromOrientation, Quaternion toOrientation)//No use as of yet
    {
        rbVelocityNormalized = toOrientation * (Quaternion.Inverse(fromOrientation) * rbVelocityNormalized);
        rb.velocity = rbVelocityNormalized * rbVelocityMagnatude;
    }
}
