using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    //target information
    [Header("Target Information")]
    public Transform targetingReticle;
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
    public bool overrideLocalOffset = false;
    public float lerpSpeed = 20;  
    private float trackLerpScale = 0;
    private Vector3 upVector = Vector3.up;
    public Vector3 manualUpVector = Vector3.up;
    public float mouseMoveScalar = 0;
    public float zoom;
    public float zoomDistanceMin = 2;
    public float zoomDistanceMax = 10;
    public bool upReset = true;

    private Vector3 positionPrevious;
    private Vector3 velocity;
    private Vector3 dampedTrackingVelocity;
    private Vector3 velNorm;
    private float velMag;

    public float shakeMagnatude = 1;

    public Vector2 rot = Vector2.zero;
    public Quaternion targetRotation;
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

    void LateUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        //lower shakeMagnatude over time
        { 
            shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 8);
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
            rot = new Vector2(Mathf.Lerp(rot.x, mouseMovement.y + joystickMovement.y, Time.deltaTime * 10),Mathf.Lerp(rot.y, mouseMovement.x + joystickMovement.x, Time.deltaTime * 10));
            targetRotation *= Quaternion.Euler(rot.x, rot.y, 0.0f);
        }

        //Upvector Adjust. Have camera up lerped by gravity magnitude, between facing away from gravity, and the custom up vector. If Gm does not exist lerp to the custom up vector
        if (upReset)
        {
            if(gM != null)
            {
                //Calulate Up Direction Vector By Gravity
                Vector3 gmReturn = -gM.ReturnGravity(transform);

                upVector = Vector3.Lerp(manualUpVector,gmReturn.normalized, gmReturn.magnitude);

            }
            else
            {
                upVector = Vector3.Slerp(upVector, manualUpVector, Time.deltaTime);
            }

            float scale = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, upVector));
            targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, upVector), Time.deltaTime * scale);
        }

        //Tilt camera in the direction of turns.
        {
            //Vector3 force = CalulateCentripetalForce(velocity, dampedTrackingVelocity);                     
        }

        {
            if (overrideLocalOffset)
                localOffset = Vector3.zero;
        }

        //Lerp Towards Target
        if (followingTarget)       
        {
            rb.isKinematic = true;

            float followTargetSpeed = 0;
           // float baseTargetFOV = 60;

            if (targets[followIndexPosition] != null)
            {

                {
                    followTargetSpeed = (targets[followIndexPosition].transform.position - followPreviousPosition).magnitude / Time.deltaTime;

                    followPreviousPosition = targets[followIndexPosition].transform.position;
                }         

                //caluate zoom
                {
                    zoom = Mathf.Lerp(zoom, Mathf.Lerp(zoomDistanceMin, zoomDistanceMax, followTargetSpeed / 100), Time.deltaTime * 10);

                    //check for wall intersection         
                    RaycastHit hit;
                    int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10)); //layerMask = ~layerMask;

                    if (Physics.Raycast(targets[followIndexPosition].transform.position + transform.TransformDirection(localOffset), -transform.forward, out hit, zoom, layerMask, QueryTriggerInteraction.Ignore))
                        zoom = hit.distance;
                }

                //set camera position                                       
                {
                    float disable = Mathf.Clamp01(Time.time / 10);
                    transform.position =  Vector3.Lerp(transform.position, targets[followIndexPosition].transform.position + transform.TransformDirection(localOffset) - (targetRotation * Vector3.forward * zoom), Time.deltaTime * lerpSpeed * disable);
                }

                //Adjust FOV (not needed)
                {
                    //Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, Mathf.Lerp(baseTargetFOV,105,Mathf.InverseLerp(0,80,followTargetSpeed)), Time.deltaTime * 2);
                }

            }
            else
            {
                //Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, baseTargetFOV, Time.deltaTime * 2);
            }
        }
        else
        {
            rb.isKinematic = false;
            rb.AddForce(targetRotation * Vector3.forward * Input.GetAxis("Vertical") * freeCamSpeed, ForceMode.Acceleration);
            rb.AddForce(targetRotation * Vector3.right * Input.GetAxis("Horizontal") * freeCamSpeed, ForceMode.Acceleration);
            rb.AddForce(targetRotation * Vector3.up * Input.GetAxis("Triggers") * freeCamSpeed, ForceMode.Acceleration);

           // Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, Time.deltaTime * 2);
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
                    targetingReticle.position = targets[trackIndexPosition].transform.position;
            }
            else
            {
                //disable quickly removes any trackLerpScale when the player atempts to rotate the camera on their own.
                float disableTracking = 1;
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4));
                disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4));
                disableTracking *= 1 - Mathf.Clamp01(mouseMoveScalar * 4);
                //smoothly disable tracking, but allow the player to turn it off fast.
                trackLerpScale = Mathf.Lerp(trackLerpScale * disableTracking, 0, Time.deltaTime * 2);
            }          

            if (!tracking && targetingReticle != null)
            {
                RaycastHit hit;
                int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));
                Physics.Raycast(transform.position, transform.forward, out hit, 500, layerMask);//if we hit something                
                Vector3 trackPos = transform.position + transform.forward * hit.distance;
                targetingReticle.position = Vector3.Lerp(targetingReticle.position, trackPos, Time.deltaTime);
            }

            {
                //Track camera to targeting reticle    
                targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation((targetingReticle.position - transform.position).normalized, targetRotation * Vector3.up), trackLerpScale);
            }
        }
        
        //Apply Rotation
        transform.rotation = targetRotation * Shake(shakeMagnatude);       
          
    }

    Quaternion Shake(float magnatude)
    {
        return Quaternion.Euler(Random.Range(-1, 1) * shakeMagnatude, Random.Range(-1, 1) * shakeMagnatude, Random.Range(-1, 1) * shakeMagnatude);     
    }
}



