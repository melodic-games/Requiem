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
    public float emissionColorIndex;
    public float landingType = 0;
    public float landingLag = 100;
    public float landingLagMax = 2;

    //Physics   
    public float rbVelocityMagnatude;
    public Vector3 rbVelocityNormalized;
    public float verticalSpeed;
    public float traversalSpeed;
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
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
       
        capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.radius = .25f;
        capsuleCollider.height = playerHeight;
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

        if (controlSource.jump && controlSource.crouching && controlSource.thrustInput == 1 && energyLevel == 1)
        {
            stateMachine.ChangeModule(SymFlightModule.Instance);
            rb.velocity = transform.up * rbVelocityMagnatude;
            flightEnabled = true;
            grounded = false;
        }

        //increment landing lag
        if (grounded)
        {
            landingLag += Time.deltaTime;
        }

        stateMachine.Update();

        //Update physics values every frame.
        {
            rbVelocityMagnatude = rb.velocity.magnitude;
            rbVelocityNormalized = rb.velocity.normalized;
            localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

            verticalSpeed = rbVelocityMagnatude * Vector3.Dot(rbVelocityNormalized, -gravity.normalized);
            traversalSpeed = rbVelocityMagnatude * (1 - Mathf.Abs(Vector3.Dot(rbVelocityNormalized, -gravity.normalized)));
        }

    }

    private void FixedUpdate()
    {
              
        //Gravity Calulation    
        {
            //reset gravity disable
            {
                enableGravity = 1;
            }

            if (gravityManager != null)
            {
                gravity = gravityManager.ReturnGravity(transform);
            }
            else
            {
                gravity = Vector3.Lerp(gravity, Vector3.zero, Time.deltaTime);
            }
        }        

        //Move the character
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

        groundNormal = collision.GetContact(0).normal;     
        landingType = 0;
        
        //Land hard if conditions are right
        if (Mathf.Abs(Vector3.Dot(rbVelocityNormalized, groundNormal)) > 0.85f && rbVelocityMagnatude > 40)
        {                        
            landingLag = 0;
            landingType = 1;
            rb.velocity = Vector3.zero;          
        }

        //Calulate ground speed relative to the collission surface (needed for some modules)
        traversalSpeed = rbVelocityMagnatude * (1 - Mathf.Abs(Vector3.Dot(rbVelocityNormalized, -groundNormal)));

        //Run Module Specific Collission 
        stateMachine.CollissionEnter(collision);
    }



}
