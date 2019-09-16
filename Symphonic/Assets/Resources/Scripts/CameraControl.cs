using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    //target information
    [Header("Target Information")]    
    public Vector3 characterTargetingPosition = Vector3.zero;
    public Vector3 cameraTargetingPosition = Vector3.zero;
    public GameObject[] targets;
    public int followIndexPosition = 0;
    public int trackIndexPosition = 1;
    public bool followingTarget = true;
    public bool tracking = true;

    private Vector3 trackingPreviousPosition;
    private Vector3 followPreviousPosition;

    //camera data  
    [Header("Camera Data")]
    public float freeCamSpeed = 100;
    public float mouseSensitivity = 100.0f;
    public float joystickSensitivity = 100.0f;
    private Vector2 mouseMovement = Vector2.zero;
    private Vector2 joystickMovement = Vector2.zero;
    public Vector3 localOffset = Vector3.zero;   
    public float lerpSpeed = 20;  
    public float trackLerpScale = 0;
    private Vector3 upVector = Vector3.up;
    public Vector3 manualUpVector = Vector3.up;
    public float manualUpVectorScaler = 0;
    public float zoom;
    public bool clippingAllowed = false;
    public float zoomDistanceMin = 2;
    public float zoomDistanceMax = 10;
    public bool upReset = true;
    public bool lockCursor;
    public float shakeTime = 0;
    [Range(0,20)]
    public float shakeTimeSpeed = 10;

    private Vector3 positionPrevious;
    private Vector3 velocity;
    private Vector3 dampedTrackingVelocity;
    private Vector3 velNorm;
    private float velMag;

    public float shakeMagnatude = 1;

    public Vector2 rot = Vector2.zero;
    private Quaternion targetRotation;
    private Vector3 orbitPoint;
    private Rigidbody rb;

    //Gravity data
    [Header("Gravity Data")]
    public GravityManager gM;
    

    void Start () {
        rb = GetComponent<Rigidbody>();

        positionPrevious = transform.position;
       
        targetRotation = transform.rotation;       

        if (targets[followIndexPosition] == null)
            targets[followIndexPosition] = GameObject.FindGameObjectWithTag("Player");
        
            orbitPoint = transform.position - transform.forward * 2;

            zoom = 10;
    }

    Vector3 CalulateCentripetalForce(Vector3 vel, Vector3 velPrev)
    {

        Vector3 average = (vel + velPrev) / 2;
       
        Vector3 radial = Vector3.Cross(vel, velPrev);

        Vector3 tangent = Vector3.Cross(radial, average);
       
        return tangent;
    }

    void Beat(float currentStep)
    {
        shakeMagnatude += 1f;
        if (currentStep == 4)
           shakeMagnatude += 1;        
    }

    void Update()
    {
        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        //lower shakeMagnatude over time
        { 
            shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 4);
            //shakeMagnatude = Mathf.Clamp01(shakeMagnatude);
        }

        //Update Physics Values
        {
            dampedTrackingVelocity = Vector3.Lerp(dampedTrackingVelocity,velocity,Time.deltaTime);
            velocity = (transform.position - positionPrevious) / Time.deltaTime;
            velNorm = velocity.normalized;
            velMag = velocity.magnitude;            
            positionPrevious = transform.position;
        }
      
        //record mouse and gamepad input
        {
            mouseMovement.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseMovement.y = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            joystickMovement.x = Input.GetAxis("Horizontal2") * joystickSensitivity * Time.deltaTime;
            joystickMovement.y = Input.GetAxis("Vertical2") * joystickSensitivity * Time.deltaTime;
        }

        //Player Rotates Camera 
        if (!tracking)
        {
            float disable = Mathf.Clamp01(Time.time / 4);
            rot = new Vector2(Mathf.Lerp(rot.x, mouseMovement.y + joystickMovement.y, Time.deltaTime * 10), Mathf.Lerp(rot.y, mouseMovement.x + joystickMovement.x, Time.deltaTime * 10 * disable));
            targetRotation *= Quaternion.Euler(rot.x, rot.y, 0.0f);
        }

        //Upvector Adjust. Have camera up lerped by gravity magnitude, between facing away from gravity, and the custom up vector. If Gm does not exist lerp to the custom up vector
        if (upReset)
        {
            if(gM != null)
            {
                //Calulate Up Direction Vector By Gravity
                Vector3 gmReturn = -gM.ReturnGravity(transform);                
                gmReturn = Vector3.Lerp(gmReturn, manualUpVector, manualUpVectorScaler);
                upVector = Vector3.Lerp(manualUpVector,gmReturn.normalized, gmReturn.magnitude);
            }
            else
            {
                upVector = Vector3.Slerp(upVector, manualUpVector, Time.deltaTime);
            }

            float disable = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, upVector));
            targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, upVector), Time.deltaTime * disable);            
        }

        //Camera Positioning
        if (followingTarget)       
        {
            rb.isKinematic = true;
            
            //If we have a target to follow
            if (targets[followIndexPosition] != null)
            {
                float followTargetSpeed = 0;

                if (Time.deltaTime > 0)
                {
                    followTargetSpeed = (targets[followIndexPosition].transform.position - followPreviousPosition).magnitude / Time.deltaTime;

                    followPreviousPosition = targets[followIndexPosition].transform.position;
                }

                //set camera orbitpoint position                                       
                {
                    float disable = Mathf.Clamp01(Time.time / 10);
                    orbitPoint = Vector3.Lerp(orbitPoint, targets[followIndexPosition].transform.position + targetRotation * localOffset, Time.deltaTime * lerpSpeed * disable);
                }

                //caluate zoom (displacement from orbit point)
                {
                    float targetZoom = Mathf.LerpUnclamped(zoomDistanceMin, zoomDistanceMax, followTargetSpeed / 100);
                    zoom = Mathf.Lerp(zoom, targetZoom, Time.deltaTime * 10);

                    //check for wall intersection         
                    if(!clippingAllowed)
                    {
                        RaycastHit hit;
                        int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10)); //layerMask = ~layerMask;

                        if (Physics.Raycast(orbitPoint, -(targetRotation * Vector3.forward), out hit, zoom, layerMask, QueryTriggerInteraction.Ignore))
                            zoom = hit.distance;
                    }
                }
                   
                //set camera position                                       
                {                 
                    transform.position = orbitPoint - (targetRotation * Vector3.forward * (zoom - .2f));
                }    

            }

        }
        else
        {
            rb.isKinematic = false;
            rb.AddForce(targetRotation * Vector3.forward * Input.GetAxis("Vertical") * freeCamSpeed, ForceMode.Acceleration);
            rb.AddForce(targetRotation * Vector3.right * Input.GetAxis("Horizontal") * freeCamSpeed, ForceMode.Acceleration);
            rb.AddForce(targetRotation * Vector3.up * Input.GetAxis("Triggers") * freeCamSpeed, ForceMode.Acceleration);        
        }

        //Cast and lock target.
        if (Input.GetButtonDown("LockOn"))
        {
            if (!tracking)
            {
                RaycastHit hit;
                int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));
                //if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))//if we hit something
                if  (Physics.SphereCast(transform.position, 3f, transform.forward, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.transform.gameObject.tag == "Trackable")
                    {
                        targets[trackIndexPosition] = hit.transform.gameObject;
                        tracking = true;
                    }
                }
            }
            else
            {
                tracking = false;
            }
        }

        //Object Tracking
        {
            //scale value on tracking lerp
            if (tracking)
            {
                trackLerpScale = Mathf.Lerp(trackLerpScale,1,Time.deltaTime * 1);//smoothly enable tracking.     

                //Set targeting reticle to target location
                if (targets[trackIndexPosition] != null)
                {                     
                    cameraTargetingPosition = targets[trackIndexPosition].transform.position;
                    characterTargetingPosition = transform.position + transform.forward * 4000; 
                }
            }
            else
            {
                //disable quickly removes any trackLerpScale when the player atempts to rotate the camera on their own.
                float disableTracking = 1;
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4));
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4));
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse X") * 4));
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse Y") * 4));
           
                //smoothly disable tracking, but allow the player to turn it off fast.
                trackLerpScale = Mathf.Lerp(trackLerpScale * disableTracking, 0, Time.deltaTime * 2);
            }          

            if (!tracking)
            { 
                if(targets[trackIndexPosition] != null)
                characterTargetingPosition = transform.position + transform.forward * 40;
            }

            {
                //Track camera to targeting position    
                targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation((cameraTargetingPosition - transform.position).normalized, targetRotation * Vector3.up), trackLerpScale);
            }
        }

        //Increment shake time everyframe
        {
            shakeTime += Time.deltaTime * shakeTimeSpeed;
        }

        //Apply Rotation
        transform.rotation = targetRotation * ReturnShake(1, shakeMagnatude) * ReturnShake( .01f, 1f);       
          
    }

    Quaternion ReturnShake(float frequency, float magnatude)
    {     
        return Quaternion.Euler(GetNoise(shakeTime * frequency) * magnatude);   
        
    }

    Vector3 GetNoise(float position)
    {
        return new Vector3(
            (Mathf.PerlinNoise(1, position) - .5f) * 2,
            (Mathf.PerlinNoise(10, position) - .5f) * 2,
            (Mathf.PerlinNoise(0, position) - .5f) * 2);        
    }
}



