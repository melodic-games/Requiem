using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseSignature : MonoBehaviour
{
    //Object Componet Data
    public TrailRenderer trail1;
    public TrailRenderer trail2;
    private Rigidbody rb;
    private Animator animator;
    public GameObject particleExplosion;

    //EmissionField data
    public ParticleSystem emissionField;
    private float emissionFieldScalar = 0;

    //Boost
    public AudioSource chargeSoundSource;
    //public AudioClip chargeSoundClip;
    public float boostEnergy = 100;
    public bool boostCharging = false;
    public bool boosting = false;

    //Physics data  
    private Quaternion previousOrientation;
    public float moveAcceleration = 10;
    public float angularAccelerationBase = 10;
    private float angularAccelerationScalar = 1; // air is 1, water 1/3, and boost 20
    Vector3 angularCoefficents = new Vector3(1, 1, 1);

    //Physics optimization data        
    public float rbVelMag;
    public Vector3 rbVelNorm;
    public Vector3 localangularvelocity;

    //State data
    public string currentState = "stateAir";
    private float airDrag = 0;
    private float airAngularDrag = 5;
    private float waterDrag = 1;
    private float waterAngularDrag = 5;
    private float stopAngularDrag = 0;
    private float stopDrag = 3;

    void Start()
    {         
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        previousOrientation = transform.rotation;
        
        chargeSoundSource = GetComponent<AudioSource>();
        
        /*
        {
            rb.velocity = transform.forward * 50;
            rbVelMag = rb.velocity.magnitude;
            rbVelNorm = rb.velocity.normalized;
        }
        */

    }

    void Main()
    {
        rbVelMag = rb.velocity.magnitude;
        rbVelNorm = rb.velocity.normalized;
        localangularvelocity = transform.InverseTransformDirection(rb.angularVelocity);

        if (rb.useGravity)
        {
            //rb.AddForce(-Physics.gravity * Mathf.Min(30,rbVelMag)/30);
            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }

        if (rbVelMag > 100)
        {
            //float mag = Mathf.Lerp(rbVelMag, 100, Time.deltaTime * 2);
            rb.velocity = rbVelNorm * 100;
            rbVelMag = 100;
        }

        SetAnimatorValues();

        previousOrientation = transform.rotation;

    }

    private void Update()
    {
        float widthMultiplier = Mathf.Min(Mathf.Max(rbVelMag - 40, 0) / 30, 1);

        trail1.widthMultiplier = 0.05f * widthMultiplier;
        trail2.widthMultiplier = 0.05f * widthMultiplier;

        trail1.time = widthMultiplier == 0 ? 0 : 0.6f;
        trail2.time = widthMultiplier == 0 ? 0 : 0.6f;

        //Update the emissionFieldSize
        if (emissionField != null)
        {
            emissionFieldScalar = Mathf.Max(0, emissionFieldScalar - 1 * Time.deltaTime);
            var sz = emissionField.sizeOverLifetime;
            sz.size = Mathf.Lerp(sz.size.constant, 1 + 9 * emissionFieldScalar, Time.deltaTime * 2);
        }
    }

    void FixedUpdate()
    {
        Main();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            currentState = "stateWater";
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            currentState = "stateAir";
    }

    public void Movement(float vertAxis, float horiAxis, float pitchAxis)
    {
        //Medium effects by state
        {
            ApplyStateEffects(currentState);
            angularCoefficents = AngularCoefficentsFromInput(vertAxis, horiAxis, pitchAxis);
        }

        {
            //Forward Acceleration
            rb.AddForce(transform.forward * vertAxis * (moveAcceleration + Mathf.Abs(horiAxis) * moveAcceleration * 2), ForceMode.Acceleration);
        }

        {
            //Angular Acceleration Input                   
            {
                rb.AddTorque(transform.forward * -horiAxis * angularAccelerationBase * angularAccelerationScalar * angularCoefficents.z, ForceMode.Acceleration);
                rb.AddTorque(transform.right * pitchAxis * angularAccelerationBase * angularAccelerationScalar * angularCoefficents.x, ForceMode.Acceleration);
            }
        }

        {
            //Update these values
            rbVelMag = rb.velocity.magnitude;
            rbVelNorm = rb.velocity.normalized;
        }
    }

    public void Movement(Vector3 dir)
    {               
        rb.AddForce(dir * moveAcceleration * 2, ForceMode.Acceleration);
        {
            //Update these values
            rbVelMag = rb.velocity.magnitude;
            rbVelNorm = rb.velocity.normalized;
        }
    }

    public void Movement(Vector3 dir, float magnitude, ForceMode mode)
    {
        rb.AddForce(dir * magnitude, mode);
        {
            //Update these values
            rbVelMag = rb.velocity.magnitude;
            rbVelNorm = rb.velocity.normalized;
        }
    }

    public void LiftForce(Vector3 axis)
    {    
        //Lift Force
        rb.AddForce(axis * rbVelMag * Vector3.Dot(axis, -rbVelNorm) * 5);        
    }

    public void TurnVelocity(Vector3 dir)
    {
        //Lift Force     
        //float dot = Vector3.Dot(rbVelNorm, dir);
        rb.velocity = dir * rbVelMag;        
    }

    public void AlignToDirection(Vector3 dir)
    {
        
        //float dot2;
        //dot = Vector3.Dot(transform.forward, vectorToTarget);

        //dot = Vector3.Dot(transform.right, -dir);

        //float dot2 = Vector3.Dot(transform.up, -dir);

        //Vector3 cross = Vector3.Cross(transform.forward, dir);

        //rb.AddTorque(cross * Mathf.Abs(dot2) * angularAccelerationScalar * 10);

        //rb.AddTorque((transform.forward * dot) * angularAccelerationScalar * angularAccelerationBase, ForceMode.Acceleration);
        // * Mathf.Max(0, 1-dot2)
        // + (transform.forward * dot * 2))
        //rb.AddTorque(cross * angularAccelerationScalar * angularAccelerationBase, ForceMode.Acceleration);

        transform.rotation = Quaternion.LookRotation(dir, transform.up);

    }

    //Calulate how the objects demensions effect angular acceleration.    
    Vector3 AngularCoefficentsFromInput(float vertAxis, float horiAxis, float pitchAxis)
    {
        //return new Vector3(5 - (4 * Mathf.Abs(vertAxis)), 1, 1);                
        return new Vector3(5 - (4 * vertAxis), 1, 10);
    }

    public void SetAnimatorValues()
    {
        if (animator != null)
        {
            //set animator values             
            animator.SetFloat("Speed", rbVelMag, 0.1f, Time.deltaTime);            
            //text.text = "Pitch: " + Mathf.Round(localangularvelocity.x) + " Yaw: " + Mathf.Round(localangularvelocity.y) + " Roll: " + Mathf.Round(localangularvelocity.z);

            animator.SetFloat("RotZ", Mathf.Abs(localangularvelocity.z), 0.1f, Time.deltaTime);
            animator.SetFloat("RotX", Mathf.Abs(localangularvelocity.x), 0.1f, Time.deltaTime);
        }
    }

    public void ChargeAbblity(string type)
    {
        switch (type)
        {
            case "Boost":
                boostCharging = true;
                emissionFieldScalar = 1;
                ApplyStateEffects("stateBoostCharging");//overwrite current state effect
                //if (!chargeSoundSource.isPlaying) chargeSoundSource.Play(); 
                break;
            default:
                print("No abblity selected!");
                break;
        }   
    }

    public void ApplyStateEffects(string state)
    {
        switch (state)
        {

            case "stateBoostCharging"://stop
                moveAcceleration = 0;
                rb.drag = stopDrag;
                rb.angularDrag = stopAngularDrag;
                rb.useGravity = false;
                rb.maxAngularVelocity = 20;
                angularAccelerationScalar = 20;                              
                break;
            case "stateWater"://water       
                moveAcceleration = 10;
                rb.drag = waterDrag;
                rb.angularDrag = waterAngularDrag;
                rb.useGravity = false;
                rb.maxAngularVelocity = 20;
                angularAccelerationScalar = 1;                              
                break;
            case "stateAir"://air
                moveAcceleration = 10;
                rb.drag = airDrag;
                rb.angularDrag = airAngularDrag;
                rb.useGravity = true;
                rb.maxAngularVelocity = 50;
                angularAccelerationScalar = 1;
                break;
            default://default air
                moveAcceleration = 10;
                rb.drag = airDrag;
                rb.angularDrag = airAngularDrag;
                rb.useGravity = true;
                rb.maxAngularVelocity = 20;
                angularAccelerationScalar = 1;
                break;
        }
    }

}



