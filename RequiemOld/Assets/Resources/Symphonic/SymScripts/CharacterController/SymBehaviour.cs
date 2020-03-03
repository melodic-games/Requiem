using UnityEngine;
using SymBehaviourModule;
using SymControl;

[RequireComponent(typeof(SymCameraHelper))]
[RequireComponent(typeof(SymAnimationHandler))]
[RequireComponent(typeof(SymVisualControl))]
[RequireComponent(typeof(SymIKHandler))]
[RequireComponent(typeof(SymEffectControl))]

public class SymBehaviour : MonoBehaviour
{
    
    public StateMachine<SymBehaviour> stateMachine { get; set; }   

    //References   
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public SymCameraHelper cameraHelper;
    public Animator animator;
    public PhysicMaterial phyMat;

    //Character   
    public float playerHeight = 1.8f;
    public float colliderRadius = .25f;
    public bool grounded = false;
    public Vector3 surfaceNormal;
    public bool crouching = false;
    public float topSpeed = 10;
    public float energyLevel = 0f;
    public bool chargingEnergy = false;
    public bool flightEnabled = false;
    public float wallRun = 0;
    //Ground Raycast
    public bool groundIsBelow;
    public RaycastHit groundHit;
    //CoyoteTime
    public float coyoteTime = 0;
    public float coyoteTimeMax = .2f;
    //Landing
    public float landingType = 0;
    public float impactLag = 0;
    //Locomotion
    public bool locomotionInputInterupt = false;

    //Boosting
    public bool  boosting = false;
    public float boostSpeed = 100;
    public float boostTime = 0;
    public float boostDuration = .1f;
    public float boostInterupt = .05f;
    public float boostWindup = .05f;

    //Physics   
    public float rbVelocityMagnatude;
    public Vector3 rbVelocityNormalized;
    public float surfaceVerticalSpeed;
    public float surfaceTraversalSpeed;
    public Vector3 localAngularVelocity;
    public Vector3 localKinematicAngularVelocity;

    public bool intangible = false;

    public float enableGravity;
    public GravityManager gravityManager;
    public Vector3 gravity = Vector3.down;   
    public Vector3 gravityRaw = Vector3.down;

    //Control Source 
    public SymControlSource<SymBehaviour> controlSource = new SymUserControl();

    //Ground Input data          
    public float horizontalInput;
    public float verticalInput;

    public Vector3 moveDirection;
    public float moveDirectionMag;
    public bool canRun = false;

    public float boostBuffer = Mathf.Infinity;
    public float boostBufferMax = .5f;

    public float jumpBuffer = Mathf.Infinity;
    public float jumpBufferMax = .5f;

    // Flight Input Data
    public float thrustInput = 0;   
    public float focusInput = 0;

    public float liftCoefficient = 5;


    private void Start()
    {
        
        cameraHelper = GetComponent<SymCameraHelper>();
        animator = GetComponent<Animator>();
        
        gameObject.layer = 10;
        gameObject.tag = "Player";

        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Symphonic/Animation/SymphonicController");

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularDrag = 5;
        rb.drag = 0.1f;
        rb.centerOfMass = new Vector3(0, playerHeight / 2, 0);
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
       
        capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.radius = .25f;
        capsuleCollider.height = playerHeight;
        capsuleCollider.center = new Vector3(0, playerHeight / 2, 0);
        phyMat = Resources.Load<PhysicMaterial>("Symphonic/PhyMatSymphonic");
        capsuleCollider.material = phyMat;

        gravityManager = FindObjectOfType<GravityManager>();
        rb.maxAngularVelocity = 40;

        CreateStateMachine();

    }

    private void CreateStateMachine()
    {
        stateMachine = new StateMachine<SymBehaviour>(this);
        stateMachine.ChangeModule(SymGroundModule.Instance);
    }

    public void ExitCurrentModule()
    {
        stateMachine.ChangeModule(SymGroundModule.Instance);
    }

    private void Update()
    {
        if (stateMachine == null)
        {
            print("No StateMachine");
            CreateStateMachine();
        }

        //Aquire Input
        {
            controlSource.CollectInput();

            //Directional Input
            {
                horizontalInput = controlSource.horizontalInput;
                verticalInput = controlSource.verticalInput;
            }
     
            //Crouching
            { 
                crouching = controlSource.crouching;
            }

            //Jump
            if (controlSource.jump)
            {
                jumpBuffer = 0;
            }

            //Boost    
            if (controlSource.boost && !boosting)
            {
                boostBuffer = 0;
            }

            //Run
            if (controlSource.canRun)
            {
                canRun = true;
            }

            thrustInput = controlSource.thrustInput;
            focusInput = controlSource.focusInput;
        
        }
        
        //count down impact lag        
        {
            impactLag -= Time.deltaTime;
        }

        //Send camera helper data
        {
            cameraHelper.manualUpVectorScaler = wallRun;
        }

        if (energyLevel == 1 && !grounded && (stateMachine.currentModule.GetType() != typeof(SymFlightModule)))
            stateMachine.ChangeModule(SymFlightModule.Instance);


    }

    private void FixedUpdate()
    {

        //Update physics values every frame.
        {
            rbVelocityMagnatude = rb.velocity.magnitude;
            rbVelocityNormalized = rb.velocity.normalized;
            localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
            localKinematicAngularVelocity = Vector3.zero;
        }

        //Gravity, grounded check, coyote time, and ground data grab
        { 
           
            //Gravity Calulation    
            {
                //reset gravity disable
                {
                    enableGravity = 1;
                }

                if (gravityManager != null)
                {
                    gravity = gravityRaw = gravityManager.ReturnGravity(transform, transform.up * playerHeight * .5f);
                }
                else
                {
                    gravity = Vector3.Lerp(gravity, Vector3.zero, Time.deltaTime);
                }
            }

            //if we have exausted coyoteTime we are no longer grounded and should fall
            if (coyoteTime > coyoteTimeMax)
            {
                grounded = false;                
                coyoteTime = 0;
                wallRun = 0;
            }

            if (grounded)
            {

                //Ground check
                {
                    //Find the direction that we check for the ground using last frames groundNormal if wall run is enabled                
                    Vector3 groundCheckDirection = Vector3.Lerp(gravity.normalized, gravity.magnitude * -surfaceNormal, wallRun).normalized;

                    //Preform raycast and grab Surface Data
                    {

                        Vector3 pos = transform.position + -groundCheckDirection * playerHeight *.5f;
                        //Vector3 pos = transform.position + transform.up * playerHeight *.5f;

                        float dist = playerHeight;                        

                        groundIsBelow = Physics.Raycast(pos, groundCheckDirection, out groundHit, dist, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore);

                        Debug.DrawLine(pos, groundHit.point, Color.red);

                        Debug.DrawRay(groundHit.point, groundHit.normal, Color.blue);
                    }
                }

                //Check to make sure we have not left the ground             
                {
                    if (groundIsBelow)
                    {
                        surfaceNormal = groundHit.normal;      

                        //Gravity redirection from wall running with new ground normal                                 
                        gravity = Vector3.Lerp(gravity, gravity.magnitude * -surfaceNormal, wallRun);    
                        
                        coyoteTime = 0;
                    }
                    else
                    {                                                                   
                        //Increment CoyoteTime 
                        coyoteTime += Time.deltaTime;

                        //CoyoteTime disables gravity while in use                
                        if (coyoteTime <= coyoteTimeMax)
                            enableGravity = 0;                        
                    }
                }                                 
            }
        }

        //Calulate Generic Locomotion Interupts
        { 
            locomotionInputInterupt = false;

            if (impactLag > 0)
                locomotionInputInterupt = true;
        }

        //BoostTime update
        {
            //Increment boostTime
            if (boosting)
            {
                boostTime += Time.deltaTime;

                //Reset boosting and boostTime once we have boosted the full duration
                if (boostTime >= boostDuration)
                {
                    boostTime = 0;
                    boosting = false;
                }
            }

        }

        //BoostBuffer Update
        {
            boostBuffer += Time.deltaTime;
        }

        //Allow the player to cancel a boost early if needed
        //if (owner.boosting && !owner.locomotionInputInterupt && owner.moveDirectionMag == 0)
        //{
        //    owner.boosting = false;
        //    owner.boostTime = 0;
        //}
        
        //jumpBuffer Update
        {
            jumpBuffer += Time.deltaTime;
        }

        //Run Locomotion Portion of Module
        {            
            stateMachine.Locomotion();
        }

        //Apply Gravity
        {
            rb.AddForce(gravity * enableGravity, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cameraHelper != null)
        cameraHelper.CauseCameraShake(rbVelocityMagnatude / 20);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (!grounded)
        {
            surfaceNormal = collision.GetContact(0).normal;

            if (jumpBuffer > jumpBufferMax)
            {
                rb.angularVelocity = Vector3.zero;

                Vector3 localOffset = Vector3.up * playerHeight * .5f;
                Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity), -gravity);

                //Reset rotation on ground on impact                
                SymUtils.SetRotationAroundOffset(transform, Vector3.up * playerHeight * .5f, Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity), -gravity));

                //Check if we should ground the character normally, or if the character should stick to walls
                if (Vector3.Dot(surfaceNormal, -gravity.normalized) < .5f)// && energyLevel == 1
                {
                    wallRun = 1;
                    //Reset rotation on ground on impact to stick to walls                      
                    SymUtils.SetRotationAroundOffset(transform, Vector3.up * playerHeight * .5f, Quaternion.LookRotation(Vector3.Cross(transform.right, surfaceNormal), surfaceNormal));
                }

                //Reset Landing Type
                landingType = 0;

                //handle landing lag
                if (Vector3.Dot(rbVelocityNormalized, surfaceNormal) < -0.85f && rbVelocityMagnatude > 40)
                {
                    impactLag = 1;            
                    landingType = 1;
                    rb.velocity = Vector3.zero;
                    Debug.Log("Hard Contact");
                }
                else
                {
                    Debug.Log("Normal Contact");
                }

                //recalulate ground speeds relative to the collission surface now that it has updated
                surfaceVerticalSpeed = Vector3.Dot(rbVelocityNormalized * rbVelocityMagnatude, surfaceNormal);
                surfaceTraversalSpeed = rbVelocityMagnatude - Mathf.Abs(surfaceVerticalSpeed);
            }
            
        }

        //Run Module Specific Collission 
        stateMachine.CollissionEnter(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!grounded)
        {
            surfaceNormal = collision.GetContact(0).normal;

            //recalulate ground speeds relative to the collission surface now that it has updated
            surfaceVerticalSpeed = Vector3.Dot(rbVelocityNormalized * rbVelocityMagnatude, surfaceNormal);
            surfaceTraversalSpeed = rbVelocityMagnatude - Mathf.Abs(surfaceVerticalSpeed);
        }

        //Run Module Specific Collission 
        stateMachine.CollissionStay(collision);
    }

}
