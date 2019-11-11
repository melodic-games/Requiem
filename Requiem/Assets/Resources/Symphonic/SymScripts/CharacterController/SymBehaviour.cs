using UnityEngine;
using SymBehaviourModule;
using SymControl;

[RequireComponent(typeof(SymCameraHelper))]
[RequireComponent(typeof(SymAnimationHandler))]
[RequireComponent(typeof(SymMaterialControl))]
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

    //Control Source 
    public SymControlSource<SymBehaviour> controlSource = new SymUserControl();

    //Character   
    public float playerHeight = 1.8f;
    public float colliderRadius = .25f;
    public bool grounded = false;
    public Vector3 groundNormal;
    public bool crouching = false;
    public float energyLevel = 0f;
    public bool chargingEnergy = false;
    public bool flightEnabled = false;
    public float wallRun = 0;
    //Ground Raycast
    public bool groundIsBelow;
    public RaycastHit groundHit;
    //CoyoteTime
    //public bool useCoyoteTime = false;
    public float coyoteTime = 0;
    public float coyoteTimeMax = .2f;
    //Landing
    public float landingType = 0;
    public float landingLag = 100;
    public float landingLagMax = 2;
    //Locomotion
    public bool locomotionInputInterupt = false;

    //Physics   
    public float rbVelocityMagnatude;
    public Vector3 rbVelocityNormalized;
    public float groundVerticalSpeed;
    public float groundTraversalSpeed;
    public Vector3 localAngularVelocity;

    public bool intangible = false;

    public float enableGravity;
    public GravityManager gravityManager;
    public Vector3 gravity = Vector3.down;   
      
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
        rb.maxAngularVelocity = 20;

        CreateStateMachine();

    }

    private void CreateStateMachine()
    {
        stateMachine = new StateMachine<SymBehaviour>(this);
        stateMachine.ChangeModule(SymBaseModule.Instance);
    }

    public void ExitCurrentModule()
    {
        stateMachine.ChangeModule(SymBaseModule.Instance);
    }

    private void Update()
    {
        if (stateMachine == null)
        {
            print("No StateMachine");
            CreateStateMachine();
        }

        controlSource.CollectInput();        

        //increment landing lag
        if (grounded)
        {
            landingLag += Time.deltaTime;
        }

        //Send camera helper data
        {
            cameraHelper.manualUpVectorScaler = wallRun;
        }

        if (energyLevel == 1 && !grounded && !flightEnabled)
            stateMachine.ChangeModule(SymFlightModule.Instance);

        stateMachine.Update();

        //Update physics values every frame.
        {
            rbVelocityMagnatude = rb.velocity.magnitude;
            rbVelocityNormalized = rb.velocity.normalized;
            localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);            
        }

    }

    private void FixedUpdate()
    {

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
                    gravity = gravityManager.ReturnGravity(transform, transform.up * playerHeight * .5f);
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
                    Vector3 groundCheckDirection = Vector3.Lerp(gravity.normalized, gravity.magnitude * -groundNormal, wallRun).normalized;

                    //Preform raycast and grab Surface Data
                    {

                        Vector3 pos = transform.position + transform.up * playerHeight *.5f;
                        float dist = playerHeight;

                        Debug.DrawRay(pos, groundCheckDirection * dist, Color.red);

                        groundIsBelow = Physics.Raycast(pos, groundCheckDirection, out groundHit, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                    }
                }

                //Check to make sure we have not left the ground             
                {
                    if (groundIsBelow)
                    {
                        groundNormal = groundHit.normal;      

                        //Gravity redirection from wall running with new ground normal                                 
                        gravity = Vector3.Lerp(gravity, gravity.magnitude * -groundNormal, wallRun);    
                        
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

            if (landingLag < landingLagMax)
                locomotionInputInterupt = true;
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
            groundNormal = collision.GetContact(0).normal;
            rb.angularVelocity = Vector3.zero;


            ////Reset rotation on ground on impact
            {
                Vector3 piviot = transform.position + transform.up * playerHeight * .5f;
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity), -gravity);
                transform.position = piviot - transform.up * playerHeight * .5f;
            }

            //Check if we should ground the character normally, or if the character should stick to walls
            if (Vector3.Dot(groundNormal, -gravity.normalized) < .5f)// && energyLevel == 1
            {
                wallRun = 1;

                //Reset rotation on ground on impact to stick to walls                
                Vector3 piviot = transform.position + transform.up * playerHeight * .5f;
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, groundNormal), groundNormal);
                transform.position = piviot - transform.up * playerHeight * .5f;
            }

            //Reset Landing Type
            landingType = 0;

            //handle landing lag
            if (Vector3.Dot(rbVelocityNormalized, groundNormal) < -0.85f && rbVelocityMagnatude > 40)
            {
                //landingLag = 0;
                landingLagMax = 1;
                landingType = 1;
                rb.velocity = Vector3.zero;
                Debug.Log("Hard Contact");
            }
            else
            {
                Debug.Log("Normal Contact");
            }

            //recalulate ground speeds relative to the collission surface now that it has updated
            groundVerticalSpeed = Vector3.Dot(rb.velocity, groundNormal);
            groundTraversalSpeed = rb.velocity.magnitude - Mathf.Abs(groundVerticalSpeed);
        }

        //Run Module Specific Collission 
        stateMachine.CollissionEnter(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!grounded)
        {
            groundNormal = collision.GetContact(0).normal;

            //recalulate ground speeds relative to the collission surface now that it has updated
            groundVerticalSpeed = Vector3.Dot(rb.velocity, groundNormal);
            groundTraversalSpeed = rb.velocity.magnitude - Mathf.Abs(groundVerticalSpeed);
        }

        //Run Module Specific Collission 
        stateMachine.CollissionStay(collision);
    }



}
