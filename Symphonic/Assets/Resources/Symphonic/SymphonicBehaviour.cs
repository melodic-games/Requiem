using UnityEngine;
using System.Collections;


public class SignatureClass
{
    public string name = "";
}

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
    public float moveAcceleration = 20;//Base force value used to accelerate the character    
    public float angularAccelerationBase = 10;//Base force value used to rotate the character   
    private float liftScale = 1;
    private Vector3 angularCoefficents = new Vector3(1, 1, 1);
    private float rollSpinUp = 0;
    private float pitchSpinup = 0;
    private float boostCharge = 0;
    private float currentMedium = 0;
    public float playerHeight = 2;
    public float colliderRadiusMin = .25f;//Used for standing 
    public float colliderRadiusMax = .4f;//Used for flight Curl 
    public bool usingFeet = false;
    public bool flying = false;
    public float jumpPower = 10;
    private bool insideVolume = false;
    private Vector3 camLocalOffsetTarget = Vector3.zero;

    //Object Componets
    [Header("Componets")]
    public Animator animator;
    public Rigidbody rb;//Rigidbody
    public CapsuleCollider capsuleCollider;
    public GameObject leftHand;
    public GameObject rightHand;
    public Object trailPrefab;
    public TrailRenderer trailRendererLeft;
    public TrailRenderer trailRendererRight;
    public GameObject hairLeft;
    public GameObject hairRight;
    public GameObject hairMount;    
    private CameraControl cameraControl;
    public Object boostParticleExplosion;
    public Object landParticleExplosion;
    public SkinnedMeshRenderer bodyRenderer;
    public Material[] materials;
    private DynamicBone[] dynamicBones = new DynamicBone[3];
    private Vector4[] hairValues = new Vector4[] 
    {
        new Vector4(0.3f, .15f, 0, 1),
        new Vector4(0.2f, .05f, 0, 1)
    };
 
    [ColorUsage(true, true)] public Color[] signatureColors = new Color[] 
    {
        new Vector4(0, 5.146699f, 9.082411f, 1),
        new Vector4(0, 9.082411f, 9.082411f, 1),
        new Vector4(9.082411f, 0.7568676f, 0, 1),
        new Vector4(29.64f, 10.56273f, 0, 1)
    };

    public float emission = 1f;
    Vector4 color = Color.white;

    //Gravity     
    public GravityManager gM;
    public Vector3 gravity = Vector3.down;

    //Physics        
    [Header("Physics optimization")]
    public Vector3 netForce = Vector3.zero;
    public float rbVelocityMagnatude;
    public Vector3 rbVelocityNormalized;
    public Vector3 localAngularVelocity;
    private float lateralDrag;
    public Vector3 groundNormal = Vector3.up;

    //Input data      
    [Header("Input Data")]
    public float thrust = 0;
    public bool thrustAsButtion = false;
    public float rollAxisInput = 0;
    public float pitchAxisInput = 0;
    public float yawAxisInput = 0;
    public bool jump = false;
    public float focus = 0;    
    public float strafe = 0;
    public bool useUpVector = false;
    public int dPad = 0;
    Vector3 targetHeading = Vector3.up;

    //Signature State data
    [Header("Signature")]
    public Signature currentSignature = Signature.Pure;
    

    private void Reset()
    {
        gameObject.layer = 10;
        gameObject.tag = "Player";

        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Symphonic/Animation/SymphonicController");

        leftHand = GameObject.Find("LeftHand");
        rightHand = GameObject.Find("RightHand");

       // hairLeft = GameObject.Find("LeftHair1");
       // hairRight = GameObject.Find("RightHair1");
       // hairMount = GameObject.Find("HairMount");

        rb = GetComponent<Rigidbody>();    
        rb.useGravity = false;
        rb.angularDrag = 5;
        rb.drag = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        

        boostParticleExplosion = Resources.Load<Object>("Prefabs/Explosion1");
        landParticleExplosion = Resources.Load<Object>("Symphonic/LandExplosion");

        trailPrefab = Resources.Load<Object>("Prefabs/Trail");
        trailRendererLeft = (Instantiate(trailPrefab, leftHand.transform.position, leftHand.transform.rotation, leftHand.transform) as GameObject).GetComponent<TrailRenderer>();
        trailRendererRight = (Instantiate(trailPrefab, rightHand.transform.position, rightHand.transform.rotation, rightHand.transform) as GameObject).GetComponent<TrailRenderer>();

        materials = new Material[]
        {
            Resources.Load<Material>("Symphonic/MatSym"),
            Resources.Load<Material>("Symphonic/Mat_Hair"),
            Resources.Load<Material>("Symphonic/Mat_Emission")
        };

        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        //bodyRenderer.materials = materials;

        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.center = new Vector3(0, -.2f, 0);//Vector3.zero for sphere mode
        capsuleCollider.radius = .25f;//.5 for sphere mode
        capsuleCollider.height = playerHeight;//0 for sphere mode
        capsuleCollider.direction = 1;//Y-Axis               
        
        for (int i = 0; i < 3; i++)
        {
            //dynamicBones[i] = gameObject.AddComponent<DynamicBone>() as DynamicBone;
           // int hairIndex = 0; if (i == 2) hairIndex = 1;
            //dynamicBones[i].m_Damping = hairValues[hairIndex].x;
           // dynamicBones[i].m_Elasticity = hairValues[hairIndex].y;
           // dynamicBones[i].m_Stiffness = hairValues[hairIndex].z;
          //  dynamicBones[i].m_Inert = hairValues[hairIndex].w;
        }

      //  dynamicBones[0].m_Root = hairLeft.transform;
       // dynamicBones[1].m_Root = hairRight.transform;
      //  dynamicBones[2].m_Root = hairMount.transform;

        gM = FindObjectOfType<GravityManager>();
    }

    void Start()
    {
        cameraControl = Camera.main.GetComponent<CameraControl>();               
        targetHeading = Camera.main.transform.forward;
        gM = FindObjectOfType<GravityManager>();
        rb.maxAngularVelocity = 20;
    }

    void ChangeSignature()
    {             
        switch (dPad)
        {
            default:
                currentSignature = Signature.Pure;
                break;
            case 1:
                currentSignature = Signature.Echo;
                break;
            case 2:
                currentSignature = Signature.Reverb;
                break;
            case 3:
                currentSignature = Signature.PerfectCrescendo;
                break;   
        }
    }

    void SignatureBehaviour(Signature currentSignature)
    {                
        switch (currentSignature)
        {                               
            default:                            
                break;
            case Signature.Pure: //Default normal ground movement and flight
               
                if (!flying)
                {

                    Vector3 moveDirection;
                                                                        
                    if (usingFeet)//ground movement
                    {
                        //check to make sure we have not left the ground 
                        {
                            RaycastHit hit;
                                                
                            if (!Physics.Raycast(transform.position, gravity, out hit, 1.8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                            {
                                usingFeet = false;
                            }
                            else
                            {
                                groundNormal = hit.normal;
                            }
                        }

                        //Reset Movement Values for when on ground          
                        {
                            rb.angularDrag = 1000;
                            moveAcceleration = 20;
                            rb.drag = 10f;
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
                            jump = false;
                            usingFeet = false;
                            rb.AddForce(transform.up * jumpPower, ForceMode.VelocityChange);
                        }

                        //Dash abblity
                        if (thrustAsButtion)
                        {
                            thrustAsButtion = false;
                            rb.AddForce(moveDirection * 155, ForceMode.VelocityChange);
                        }
                    }
                    else //airborn movment, but not flight
                    {
                        //No ground raycast as we are in air, use gravity for up direction.
                        {
                            groundNormal = -gravity.normalized;
                        }

                        //Reset Movement Values for when in air 
                        {
                            rb.angularDrag = 5;
                            moveAcceleration = 1;
                            rb.drag = .1f;                            
                        }
                        
                        //Calulate the move direction, based on camera direction and input
                        {
                            moveDirection = Vector3.ClampMagnitude(
                            Vector3.Cross(Camera.main.transform.right, groundNormal) * pitchAxisInput - Vector3.Cross(Camera.main.transform.forward, groundNormal) * rollAxisInput, 1);       
                        }

                        //Calulate angular acceleration ramp.                
                        {
                           // angularCoefficents = new Vector3(5, 4, 1);
                        }

                        //Angular Acceleration Input         
                        {
                           // rb.AddTorque(transform.right * pitchAxisInput * angularAccelerationBase * angularCoefficents.x, ForceMode.Acceleration);
                        }

                        //adjust Capsule Collider
                        {
                            float t = Mathf.InverseLerp(0, 2, Mathf.Abs(localAngularVelocity.x));                        
                            capsuleCollider.radius = Mathf.Lerp(colliderRadiusMin, colliderRadiusMax, t);
                            capsuleCollider.height = Mathf.Lerp(playerHeight, 0, t);
                        }
                        Quaternion previousRotation = transform.rotation;
                        //Character faces the move direction    
                        {
                            if (moveDirection.magnitude > .1f)
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection, -gravity.normalized), Time.deltaTime * 10);
                        }

                        //Velocity follows move direction
                        {
                            RedirectVelocity(previousRotation, transform.rotation);
                        }

                        //Go into flying mode if focused, thrusting, and not grounded
                        if (focus == 1 && thrust == 1)
                        {
                            flying = true;
                        }

                        if (rbVelocityMagnatude > 30)
                        {                            
                            flying = true;
                        }
                        
                    }

                    //Forward Acceleration in air, and on ground
                    {
                        Debug.DrawLine(transform.position, transform.position + moveDirection.normalized * 5, Color.black);
                        netForce += moveDirection * moveAcceleration * 5;                     
                    }

                }
                                                                                       
                if (flying)
                {
                    //reset these values for flight
                    {
                        moveAcceleration = 20;
                        rb.angularDrag = 5;
                    }

                    //adjust Capsule Collider
                    {
                        float t = Mathf.InverseLerp(0, 2, Mathf.Abs(localAngularVelocity.x));
                        capsuleCollider.radius = Mathf.Lerp(colliderRadiusMin, colliderRadiusMax, t);
                        capsuleCollider.height = Mathf.Lerp(playerHeight, 0, t);
                    }

                    Vector3 flightForward;
                    Vector3 flightUp;

                    //Define flight orientation relative to chracter transform, and velocity
                    {
                        flightForward = Vector3.Lerp(transform.forward, transform.up, 1);//Fix Later
                        flightUp = Vector3.Lerp(transform.up, -transform.forward, 1);//Fix Later
                    }

                    //determin TargetHeading
                    {
                        Vector3 focusedHeading = Camera.main.transform.forward;

                        Vector3 signedVelocityNormal = rbVelocityNormalized * Mathf.Sign(Vector3.Dot(rbVelocityNormalized, flightForward));
                        Vector3 unfocusedHeading = Vector3.Lerp(signedVelocityNormal,flightForward,thrust);//Lerp between using the Velocity and the characters flight forward for no rotation effect. Stabalizes forward flight at low thrust levels

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

                    //Auto align towards the target heading.                    
                    {
                           
                        Vector3 axis = Vector3.Cross(flightForward, targetHeading);//Get the axis to turn around.

                        float input = Mathf.Clamp01(Mathf.Abs(pitchAxisInput) + Mathf.Abs(rollAxisInput));//Scalar to negate auto turning                                               

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
                            rollSpinUp += 1 * Time.deltaTime;
                        }
                        else
                        {
                            rollSpinUp -= 1 * Time.deltaTime;
                        }

                        if (insideVolume)
                        {
                            rollSpinUp = 1;
                        }

                        if (Mathf.Abs(pitchAxisInput) == 1)
                        {
                            pitchSpinup += 1 * Time.deltaTime;
                        }
                        else
                        {
                            pitchSpinup -= 1 * Time.deltaTime;
                        }

                        rollSpinUp = Mathf.Clamp01(rollSpinUp);
                        pitchSpinup = Mathf.Lerp(Mathf.Clamp01(pitchSpinup),0,thrust);
                    }

                    //Calulate angular acceleration ramp.                
                    {
                        angularCoefficents = new Vector3(5 - (4 * thrust), 4, 1);//new Vector3(1, 4, 1);//(1 + pitchSpinup * 5)
                    }

                    //Angular Acceleration              
                    {
                        //Roll
                        rb.AddTorque(-flightForward * rollAxisInput * angularAccelerationBase * angularCoefficents.y * (1 + rollSpinUp * 2), ForceMode.Acceleration);
                        //Pitch
                        rb.AddTorque(transform.right * pitchAxisInput * angularAccelerationBase * angularCoefficents.x, ForceMode.Acceleration);
                        //Yaw
                        rb.AddTorque(flightUp * yawAxisInput * angularAccelerationBase * angularCoefficents.z, ForceMode.Acceleration);
                    }

                    //Forward Acceleration             
                    {
                        netForce += flightForward * Mathf.Max(0, thrust) * moveAcceleration * (1 + rollSpinUp);
                    }

                    //Boost
                    {
                        //charge boost value
                        {
                            boostCharge += rollSpinUp * Time.deltaTime;
                            boostCharge = Mathf.Clamp01(boostCharge);
                        }

                        //Boost upon full trust start with enough energy.
                        if (rbVelocityMagnatude < 4 && thrust == 1)//(boostCharge == 1 && thrust == 1)//
                        {
                            rb.velocity = Vector3.zero;
                            boostCharge = 0;
                            emission += 1;
                            rb.AddForce(flightForward * 80 * (1 + rollSpinUp), ForceMode.VelocityChange);
                            if (cameraControl != null)
                                cameraControl.shakeMagnatude += 1;                                                      
                            if (boostParticleExplosion != null)
                                Instantiate(boostParticleExplosion, transform.position, transform.rotation);
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
                break;
            case Signature.Echo:
            
                break;
            case Signature.Reverb:
          
                break;
            case Signature.PerfectCrescendo:
              
                break;
        }              
    }

    void FixedUpdate()
    {    
        netForce = Vector3.zero;

        //Intangibility 
        {
            if (rollSpinUp == 1)                
            {
                capsuleCollider.isTrigger = true;
            }
            else
            {
                capsuleCollider.isTrigger = false;
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
            if(gM != null)
            gravity = gM.ReturnGravity(transform);

            netForce += gravity * (1 - rollSpinUp);
        }

        // Main Symphonic Behaviour function
        {
            SignatureBehaviour(currentSignature);
        }

        //apply all net forces acting on object caused by character behaviour 
        {
            rb.AddForce(netForce, ForceMode.Acceleration);
        }

        //Hair Scale
        {
            float scale = Mathf.Clamp01(1-Mathf.Abs(localAngularVelocity.x/10));
            if (usingFeet) scale = 0.0001f;

            if (hairLeft != null)
                hairLeft.transform.localScale = new Vector3(scale, scale, scale);

            if (hairRight != null)
                hairRight.transform.localScale = new Vector3(scale, scale, scale);
        }

        //Set animator values 
        if (animator != null)
        {                          
            animator.SetFloat("Speed", rbVelocityMagnatude, 0.1f, Time.deltaTime);
            animator.SetFloat("RotZ", Mathf.Abs(localAngularVelocity.z), 0.1f, Time.deltaTime);
            animator.SetFloat("RotX", Mathf.Abs(localAngularVelocity.x), 0.1f, Time.deltaTime);
            animator.SetFloat("RotY", Mathf.Abs(localAngularVelocity.y), 0.1f, Time.deltaTime);          
            animator.SetFloat("Drag", rb.drag/.2f, 0.1f, Time.deltaTime);
            animator.SetFloat("DragDir", lateralDrag, 0.1f, Time.deltaTime);
            animator.SetBool("Grounded", usingFeet);
        }

        //Trail 
        {
            float widthMultiplier;

            float beginTrailSpeed = 40;
            float trailGrowRange = 30;

            float velScale = Mathf.Clamp01((rbVelocityMagnatude - beginTrailSpeed) / trailGrowRange);
            float widthScale = 1 + (Mathf.Clamp01(Mathf.Abs(localAngularVelocity.y) / 20) * 2);
            float angxDisable = 1 - Mathf.Clamp01(Mathf.Abs(localAngularVelocity.x / 5));// if pitch speed is 0.2 or greater disable width.              

            widthMultiplier = velScale * angxDisable * widthScale;

            widthMultiplier *= Mathf.Clamp01(Vector3.Dot(rbVelocityNormalized, transform.up));            

            if (trailRendererLeft != null)
            {
                trailRendererLeft.widthMultiplier = 0.05f * widthMultiplier;
                trailRendererLeft.time = widthMultiplier == 0 ? 0 : 0.6f;
            }

            if (trailRendererRight != null)
            {
                trailRendererRight.widthMultiplier = 0.05f * widthMultiplier;
                trailRendererRight.time = widthMultiplier == 0 ? 0 : 0.6f;
            }
        }

        //Emission Control
        {
            float value = Mathf.Lerp(0, 1, (Mathf.Abs(localAngularVelocity.y) - 15) / 5);

            if (value > 0)
                emission += .5f * Time.deltaTime;
            else
                emission -= .5f * Time.deltaTime;

            emission = Mathf.Clamp(emission, 0, 1);


            color = Vector4.Lerp(color, signatureColors[(int)currentSignature],Time.deltaTime * 2);

            if (materials != null)
            {
                if(materials[0] != null)
                    materials[0].SetColor("_EmissiveColor", color * emission);
                if (materials[1] != null)
                    materials[1].SetColor("_EmissiveColor", color * 1);
                if (materials[2] != null)
                    materials[2].SetColor("_EmissiveColor", color * 1);
            }
        }



        if (Input.GetAxis("DPAD – Vertical") == 1) { dPad = 0; ChangeSignature(); color = new Vector4(5, 5, 5, 1); }
        if (Input.GetAxis("DPAD – Horizontal") == 1) { dPad = 1; ChangeSignature(); color = new Vector4(5, 5, 5, 1); }
        if (Input.GetAxis("DPAD – Vertical") == -1) { dPad = 2; ChangeSignature(); color = new Vector4(5, 5, 5, 1); }
        if (Input.GetAxis("DPAD – Horizontal") == -1) { dPad = 3; ChangeSignature(); color = new Vector4(5, 5, 5, 1); }

        //adjust where the player should appear on screen.  
        {
            Vector3 headHeight = new Vector3(0, 1f, 0);

            if (flying)
            {
                camLocalOffsetTarget = Vector3.zero;
                cameraControl.zoomDistanceMin = 2;
                cameraControl.zoomDistanceMax = 10;
            }
            else
            {
                camLocalOffsetTarget = headHeight;
                cameraControl.zoomDistanceMin = 4;
                cameraControl.zoomDistanceMax = 10;
            }


            cameraControl.localOffset = Vector3.Slerp(cameraControl.localOffset, camLocalOffsetTarget, Time.deltaTime * 2);
        }

    }

    void AudioEffects()
    {
        float scale = Mathf.InverseLerp(20, 50, rbVelocityMagnatude);
        //windSource.volume = scale;       
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
        //boostCharge = 1;
    }    
    
    IEnumerator OldBoost(Vector3 dir, float magnitude, float startTime, float duration, float cooldown)
    {        
        yield return new WaitForSeconds(startTime);
        duration = Time.time + duration;              
        while (Time.time < duration)
        {rb.AddForce(transform.TransformVector(dir) * magnitude, ForceMode.Acceleration); yield return new WaitForFixedUpdate();}  
        yield return new WaitForSeconds(cooldown);       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rollSpinUp == 1) insideVolume = true;

        if (!usingFeet)
        {
            float shake = 0;
            Vector3 hitPoint = collision.GetContact(0).point;
            Vector3 hitNormal = collision.GetContact(0).normal;

            if (rbVelocityMagnatude > 20)
            {
                shake = rbVelocityMagnatude/20;

                if (Vector3.Dot(-hitNormal, transform.up) > .8f) //head first
                {

                    //Visual Effects
                    {                        
                        emission = 1;
                        if (landParticleExplosion != null)
                            Instantiate(landParticleExplosion, hitPoint, transform.rotation);
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
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, hitNormal), hitNormal);
                rb.angularVelocity = Vector3.zero;
                //rb.velocity = Vector3.zero;
                usingFeet = true;
                //Exit Flight
                if (usingFeet)
                {
                    flying = false;
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
