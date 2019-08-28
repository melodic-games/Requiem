using UnityEngine;

//Signature Enumeration Data
public enum Signature
{
    Pure = 0,
    Echo = 1,
    Reverb = 2,
    PerfectCrescendo = 3
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]

public class SymphonicBehaviour : MonoBehaviour {

    //Character   
    [Header("Character Info")]
    public Vector3 targetHeading = Vector3.up;
    public float moveAcceleration = 20;//Base force value used to accelerate the character    
    public float angularAccelerationBase = 10;//Base force value used to rotate the character   
    public float previousMediumDensity = 0;
    public float currentMediumDensity = 0;
    public float playerHeight = 2;
    public float colliderRadiusMin = .25f;//Used for standing 
    public float colliderRadiusMax = .4f;//Used for flight Curl 
    public bool grounded = false;
    public bool canFly = true;
    public float boostCharge;
    public float jumpPower = 5;
    private bool insideVolume = false;
    private Vector3 camLocalOffset = Vector3.zero;
    private float camZoomDistanceMin = 4;
    private float camZoomDistanceMax = 10;

    //Object Componets
    [Header("Componets")]
    public Animator animator;
    public Rigidbody rb;//Rigidbody
    public CapsuleCollider capsuleCollider;
    public DynamicBone[] dynamicBones;   
    public TrailRenderer trailRendererLeft;
    public TrailRenderer trailRendererRight; 
    private CameraControl cameraControl;
    public Object particleExplosion;
    public Material material;
    public Material emissionMaterial;
 
    [ColorUsage(true, true)] public Color[] signatureColors = new Color[] 
    {
        new Vector4(0, 5.146699f, 9.082411f, 1),
        new Vector4(0, 9.082411f, 9.082411f, 1),
        new Vector4(9.082411f, 0.7568676f, 0, 1),
        new Vector4(29.64f, 10.56273f, 0, 1)
    };

    public float emission = 1f;
    public Vector4 emissionColor = Color.white;



    //Physics        
    [Header("Physics")]
    public Vector3 netForce = Vector3.zero;
    public float rbVelocityMagnatude;
    public Vector3 rbVelocityNormalized;
    public Vector3 localAngularVelocity;
    private float lateralDrag;
    public Vector3 groundNormal = Vector3.up;
    public Vector3 angularGain = new Vector3(1, 1, 1);
    public float liftScale = 1;
    public float rollSpinUp = 0;

    [Header("Gravity")]
    public GravityManager gravityManager;
    public Vector3 gravity = Vector3.down;

    //Input data      
    [Header("Input Data")]
    public float thrust = 0;
    public bool thrustAsButtion = false;
    public float rollAxisInput = 0;
    public float pitchAxisInput = 0;
    public float yawAxisInput = 0;
    public bool jump = false;
    public float focus = 0;
    private float strafe = 0;
    public bool useUpVector = false;
    private int dPad = 0;
    

    private void Reset()
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

        material = Resources.Load<Material>("Symphonic/Mat_Symphonic");

        emissionMaterial = Resources.Load<Material>("Symphonic/Mat_Emission");

        capsuleCollider = GetComponent<CapsuleCollider>();        
        capsuleCollider.radius = .25f;//.5 for sphere mode
        capsuleCollider.height = playerHeight;//0 for sphere mode
        capsuleCollider.direction = 1;//Y-Axis                     
    }

    void Start()
    {
        cameraControl = Camera.main.GetComponent<CameraControl>();               
        targetHeading = Camera.main.transform.forward;
        gravityManager = FindObjectOfType<GravityManager>();
        rb.maxAngularVelocity = 20;
        rb.centerOfMass = Vector3.zero;
    }

    private void Update()
    {
        foreach(DynamicBone bone in dynamicBones)
        {
            bone.m_Stiffness = Mathf.InverseLerp(3, 20, Mathf.Abs(localAngularVelocity.y));
            bone.UpdateParameters();
        }

        //Set animator values 
        if (animator != null)
        {
            animator.SetFloat("Speed", rbVelocityMagnatude, 0.1f, Time.deltaTime);
            animator.SetFloat("RotZ", Mathf.Abs(localAngularVelocity.z), 0.1f, Time.deltaTime);
            animator.SetFloat("RotX", Mathf.Abs(localAngularVelocity.x), 0.1f, Time.deltaTime);
            animator.SetFloat("RotY", Mathf.Abs(localAngularVelocity.y), 0.1f, Time.deltaTime);
            animator.SetFloat("Drag", rb.drag / .2f, 0.3f, Time.deltaTime);
            animator.SetFloat("DragDir", lateralDrag, 0.1f, Time.deltaTime);
            animator.SetFloat("Charge", Mathf.Clamp01(emission - thrust * 10), 0.1f, Time.deltaTime);
            animator.SetBool("Grounded", grounded);
            animator.SetBool("CanFly", canFly);
        }

        //Trail Control
        {
            float widthMultiplier;

            float beginTrailSpeed = 40;
            float trailGrowRange = 30;

            float velScale = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, rbVelocityMagnatude);// Mathf.Clamp01((rbVelocityMagnatude - beginTrailSpeed) / trailGrowRange);
            float widthScale = 1 + (Mathf.Clamp01(Mathf.Abs(localAngularVelocity.y) / 20) * 4);
            float angxDisable = 1 - Mathf.Clamp01(Mathf.Abs(localAngularVelocity.x / 5));// if pitch speed is 0.2 or greater disable width.              

            widthMultiplier = velScale * angxDisable * widthScale;

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

        //Emission Control
        {


            if (Mathf.InverseLerp(10, 20, Mathf.Abs(localAngularVelocity.y)) > 0)
            {
                emission += (2f + emission) * Time.deltaTime;
            }
            else
            {
                emission -= 2f * Time.deltaTime;
            }

            float emissionMin = 0;
            if (!grounded && canFly)
                emissionMin = .1f;

            emission = Mathf.Clamp01(emission);

            emissionColor = signatureColors[0];            
            
            if (material != null)
                material.SetColor("_EmissiveColor", emissionColor * emission);

            if (emissionMaterial != null)
                emissionMaterial.SetColor("_EmissiveColor", emissionColor * 1);
            
        }

        //adjust how the player should appear on screen.  
        {
            Vector3 headHeight = new Vector3(0, 1f, 0);

            if (!grounded)
            {
                camLocalOffset = Vector3.zero;
                camZoomDistanceMin = 4;
                camZoomDistanceMax = 10;
            }
            else
            {
                camLocalOffset = headHeight;
                camZoomDistanceMin = 8;
                camZoomDistanceMax = 12;
            }

            cameraControl.localOffset = Vector3.Slerp(cameraControl.localOffset, camLocalOffset, Time.deltaTime * 2);
            cameraControl.zoomDistanceMin = Mathf.Lerp(cameraControl.zoomDistanceMin, camZoomDistanceMin, Time.deltaTime * 2);
            cameraControl.zoomDistanceMax = Mathf.Lerp(cameraControl.zoomDistanceMax, camZoomDistanceMax, Time.deltaTime * 2);

        }

    }

    void FixedUpdate()
    {    
        netForce = Vector3.zero;        

        if (insideVolume)
        {
            //rb.angularVelocity = rb.angularVelocity.normalized * 20;
        }

        //Intangibility 
        {
            if (Mathf.Abs(localAngularVelocity.y) > 15)                
            {
                capsuleCollider.isTrigger = true;
                cameraControl.clippingAllowed = true;
            }
            else
            {
                capsuleCollider.isTrigger = false;
                cameraControl.clippingAllowed = false;
            }
        }  

        //Update values
        {
            rbVelocityMagnatude = rb.velocity.magnitude;
            rbVelocityNormalized = rb.velocity.normalized;
            localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
        }

        //Gravity       
        {
            if (gravityManager != null)
            {
                gravity = gravityManager.ReturnGravity(transform);
            }
            else
            {
                gravity = Vector3.Lerp(gravity, Vector3.zero, Time.deltaTime);
            }

            netForce += gravity * Mathf.InverseLerp(7,0,Mathf.Abs(localAngularVelocity.y)) * (1-focus);
        }

        // Main Behaviour function
        {
            Locomotion();
        }

        //apply all net forces acting on object caused by character behaviour 
        {
            rb.AddForce(netForce, ForceMode.Acceleration);
        }

    }

    void Locomotion()
    {
        if (grounded)
        {

            Vector3 moveDirection;

            //check to make sure we have not left the ground 
            {
                RaycastHit hit;

                if (!Physics.Raycast(transform.position, gravity, out hit, 1.8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                {
                    grounded = false;
                }
                else
                {
                    groundNormal = hit.normal;
                }
            }

            //Reset Movement Values for when on ground          
            {
                rb.angularDrag = 1000;
                moveAcceleration = 40;
                rb.drag = 1f;
                //rb.velocity = Vector3.ClampMagnitude(rb.velocity, 5);
            }

            //Calulate the move direction, based on camera direction and input
            {
                moveDirection = Vector3.ClampMagnitude(
                Vector3.Cross(Camera.main.transform.right, groundNormal) * pitchAxisInput - Vector3.Cross(Camera.main.transform.forward, groundNormal) * rollAxisInput, 1);
            }

            //Set rotation on ground.
            {
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -gravity.normalized), -gravity.normalized);
                rb.angularVelocity = Vector3.zero;
            }

            //Adjust Capsule Collider
            {
                capsuleCollider.radius = colliderRadiusMin;
                capsuleCollider.height = playerHeight;
            }

            //Character faces the move direction    
            {
                if (moveDirection.magnitude > .1f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection, -gravity.normalized), Time.deltaTime * 10);
            }

            //Jumping requires instant velocity change. Cant use netForce field.
            if (jump)
            {
                animator.SetTrigger("Jump");
                jump = false;
                grounded = false;
                rb.AddForce(transform.up * jumpPower, ForceMode.VelocityChange);
            }

            //Dash abblity
            if (thrustAsButtion)
            {
                thrustAsButtion = false;
                rb.AddForce(moveDirection * 10, ForceMode.VelocityChange);
            }


            //Forward Acceleration on ground
            {
                netForce += moveDirection * moveAcceleration;
            }

        }
        else
        {
            //reset these values for flight
            {            
                rb.angularDrag = 5;
            }

            //adjust Capsule Collider
            {
                float t = Mathf.InverseLerp(3.9f, 4, Mathf.Abs(localAngularVelocity.x));
                capsuleCollider.radius = Mathf.Lerp(colliderRadiusMin, colliderRadiusMax, t);
                capsuleCollider.height = Mathf.Lerp(playerHeight, 0, t);
            }

            Vector3 flightForward;
            Vector3 flightUp;

            //Define flight orientation relative to chracter transform, and velocity, not currently working at all
            {
                flightForward = Vector3.Lerp(transform.forward, transform.up, 1);//Fix Later
                flightUp = Vector3.Lerp(transform.up, -transform.forward, 1);//Fix Later
            }

            //determin TargetHeading
            {
                Vector3 focusedHeading = Camera.main.transform.forward;

                Vector3 signedVelocityNormal = rbVelocityNormalized * Mathf.Sign(Vector3.Dot(rbVelocityNormalized, flightForward));
                Vector3 unfocusedHeading = Vector3.Lerp(signedVelocityNormal, flightForward, thrust);//Lerp between using the Velocity and the characters flight forward for no rotation effect. Stabalizes forward flight at low thrust levels
                unfocusedHeading = Vector3.Lerp(flightForward, unfocusedHeading, rbVelocityMagnatude);//extra scale based on velocity for when stationary in air

                targetHeading = Vector3.Lerp(unfocusedHeading, focusedHeading, focus);//Lerp between using the unfocusedHeading, and using the camera.
            }

            //adjust target heading relative to obstacles 
            {
                //Water
                {
                    //check if player is above water surface, and adjust target direction to be perpendicular to the upvector to water.                                                   
                    int layerMask = 1 << 4;
                    float distance = 500;
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, rbVelocityNormalized, out hit, distance, layerMask, QueryTriggerInteraction.Collide))
                    {
                        //float dot = Vector3.Dot(rbVelNorm, hit.normal);

                        float angle = Vector3.Angle(rbVelocityNormalized, -hit.normal);

                        {
                            //float amount = Mathf.Max(-dot, 0) * Mathf.Lerp(1, 0, hit.distance / 50);                               

                            Vector3 perpVector = Vector3.Cross(rbVelocityNormalized, hit.normal);

                            Vector3 waterSurfaceForward = Vector3.Cross(hit.normal, perpVector);

                            targetHeading = Vector3.Lerp(targetHeading, waterSurfaceForward, 0);//Fix later
                        }
                    }
                }
            }

            //Align towards the target heading.                    
            {

                Vector3 axis = Vector3.Cross(flightForward, targetHeading);//Get the axis to turn around.

                float input =  Mathf.Clamp01(Mathf.Abs(pitchAxisInput * 2) + Mathf.Abs(rollAxisInput * 2));//Scalar to negate auto turning                                               

                rb.AddTorque(axis * (1 - input) * 10, ForceMode.Acceleration);

                //Adjust player flight upVector. Currently not working correctly
                if (useUpVector)
                {
                    Vector3 upVector = Camera.main.transform.up;

                    float lean = Vector3.Dot(flightForward, Camera.main.transform.forward);

                    upVector = Vector3.Slerp(Camera.main.transform.forward, Camera.main.transform.up, lean);

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-upVector, flightForward), Time.deltaTime * (1 - input) * focus * 2);
                }

            }

            //Set Spinup values
            {
                if (Mathf.Abs(rollAxisInput) == 1)
                {
                    rollSpinUp += 3 * Time.deltaTime;
                }
                else
                {
                    rollSpinUp -= 3 * Time.deltaTime;
                }

                rollSpinUp = Mathf.Clamp01(rollSpinUp);              
            }

            //Calulate angular acceleration ramp.
            {
                angularGain = new Vector3(5 - (4 * thrust), 4 * (1 + rollSpinUp * 2), 1);//* Mathf.Lerp(.5f, 1, Mathf.InverseLerp(0, 20, rbVelocityMagnatude))
                //new Vector3(1, 4, 1);
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

                if (cameraControl != null)
                    cameraControl.shakeMagnatude += .05f * thrust * t * Mathf.InverseLerp(20, 70, rbVelocityMagnatude);
            }

            //Boost
            {

                //Boosting
                if (thrustAsButtion)
                {                   
                    thrustAsButtion = false;
                    if (emission == 1)
                    {

                       // flightForward = Vector3.Lerp(flightForward, Camera.main.transform.forward, focus);
                        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-Camera.main.transform.up), focus);

                        rb.AddForce(flightForward * 80 * (1 + rollSpinUp), ForceMode.VelocityChange);
                        if (cameraControl != null)
                            cameraControl.shakeMagnatude += 1;
                        if (particleExplosion != null)
                            Instantiate(particleExplosion, transform.position, transform.rotation);
                    }
                }

                //Other boost options
                {

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

                //topSpeed
                float topSpeed = 100;

                //max velocity adjustment
                float highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(0, topSpeed, rbVelocityMagnatude));

                rb.drag = Mathf.Lerp(0.01f, 0.2f, backwardsDrag * disable) + Mathf.Lerp(0.01f, 1f, Mathf.Abs(lateralDrag) * disable);
            }

            //Lift force
            {
                liftScale = Mathf.Min(liftScale + 1 * Time.deltaTime, 1) * Mathf.Abs(thrust);
                LiftForce(flightForward, liftScale);
            }

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
            float t = Mathf.InverseLerp(20, 10, localAngularVelocity.magnitude);
          
            if (cameraControl.targetingReticle != null)
            {
                animator.SetLookAtWeight(t);
                animator.SetLookAtPosition(cameraControl.targetingReticle.position);
            }
        }
    }

    private void LiftForce(Vector3 axis, float scale)
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
        emission += 1 * beatScale;     
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            previousMediumDensity = currentMediumDensity;
            currentMediumDensity = .1f;            
        }
        CauseCameraShake(rbVelocityMagnatude / 20);
    }

    private void OnTriggerExit(Collider other)
    {
        currentMediumDensity = previousMediumDensity;
        previousMediumDensity = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!grounded)
        {
            float shake = 0;
            Vector3 hitPoint = collision.GetContact(0).point;
            Vector3 hitNormal = collision.GetContact(0).normal;

            if (rbVelocityMagnatude > 20)
            {
                shake = rbVelocityMagnatude/20;

                if (Vector3.Dot(-hitNormal, transform.up) > .5f) //head first
                {
                    //Stop the character
                    {
                        rb.velocity = Vector3.zero;
                    }

                    //Visual Effects
                    {                        
                        emission = 1;
                        if (particleExplosion != null)
                            Instantiate(particleExplosion, hitPoint + hitNormal, Quaternion.LookRotation(hitNormal) * Quaternion.Euler(90,0,0));
                    }

                    //Explosion force
                    {
                        float power = 25;
                        float radius = 50;
                        float upForce = 5;

                        Collider[] colliders = Physics.OverlapSphere(hitPoint, radius);
                        foreach (Collider collider in colliders)
                        {
                            Rigidbody colliderRb = collider.GetComponent<Rigidbody>();
                            if (colliderRb != null)
                            {
                                if (colliderRb != rb)
                                    colliderRb.AddExplosionForce(power, hitPoint, radius, upForce, ForceMode.VelocityChange);
                            }
                        }
                    }
                }
            }

            //Character effects
            {

                //rb.velocity = Vector3.zero;

                RaycastHit hit;

                if (Physics.Raycast(transform.position, gravity, out hit, 1.8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                {
                    grounded = true;
                    // transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, hitNormal), hitNormal);
                    // rb.angularVelocity = Vector3.zero;
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

    private void OnCollisionExit(Collision collision)
    {
       insideVolume = false;
    }

    private void RedirectVelocity(Quaternion fromOrientation, Quaternion toOrientation)
    {
        rbVelocityNormalized = toOrientation * (Quaternion.Inverse(fromOrientation) * rbVelocityNormalized);
        rb.velocity = rbVelocityNormalized * rbVelocityMagnatude;
    }

}
