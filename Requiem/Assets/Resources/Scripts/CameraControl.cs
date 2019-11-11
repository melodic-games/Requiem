using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    //target information
    [Header("Target Information")]    
    public Vector3 characterTargetingPosition = Vector3.zero;
    public Vector3 cameraTargetingPosition = Vector3.zero;
    public Transform[] followTargets;
    public Transform[] trackTargets;
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
    public float disableTracking = 1;
    public Vector3 upVector = Vector3.up;
    public Vector3 upVectorPrevious = Vector3.up;
    public Vector3 manualUpVector = Vector3.up;
    public float manualUpVectorScaler = 0;
    public bool upReset = true;
    public float upResetLerp = 1;
    public float zoom;
    public float zoomLerpSpeed = 10;
    public bool clippingAllowed = false;
    public float zoomDistanceMin = 5;
    public float zoomDistanceMax = 10;

    public bool lockCursor;
    public float shakeTime = 0;
    
    [Range(0,20)]
    public float shakeTimeSpeed = 10;
    private RaycastHit hit;


    private Vector3 positionPrevious;
    private Vector3 velocity;
    private Vector3 dampedTrackingVelocity;
    private Vector3 velNorm;
    private float velMag;

    public float shakeMagnatude = 1;

    public Vector2 rot = Vector2.zero;
    private Quaternion targetRotation;
    private Quaternion curveRotation;
    private Vector3 orbitPoint;
    private Rigidbody rb;



    //Gravity data
    [Header("Gravity Data")]
    public GravityManager gM;
    

    void Start () {
        rb = GetComponent<Rigidbody>();

        positionPrevious = transform.position;
       
        targetRotation = transform.rotation;       

        if (followTargets[0] == null)
            followTargets[0] = GameObject.FindGameObjectWithTag("Player").transform;
        
            orbitPoint = transform.position - transform.forward * 2;

            zoom = 5;
    }


    void Beat(float currentStep)
    {
        shakeMagnatude += 1f;
        if (currentStep == 4)
           shakeMagnatude += 1;        
    }

    void Update()
    {
        if (Time.timeSinceLevelLoad > 5)
        {
            {
                if (lockCursor)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
            }

            //lower shakeMagnatude over time
            {
                shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 4);
                //shakeMagnatude = Mathf.Clamp01(shakeMagnatude);
            }

            //Update Physics Values
            {
                dampedTrackingVelocity = Vector3.Lerp(dampedTrackingVelocity, velocity, Time.deltaTime);
                velocity = (transform.position - positionPrevious) / Time.deltaTime;
                velNorm = velocity.normalized;
                velMag = velocity.magnitude;
                positionPrevious = transform.position;
            }

            //record mouse and gamepad input
            if (Time.timeSinceLevelLoad > 1)
            {
                mouseMovement.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                mouseMovement.y = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                joystickMovement.x = Input.GetAxis("Horizontal2") * joystickSensitivity * Time.deltaTime;
                joystickMovement.y = Input.GetAxis("Vertical2") * joystickSensitivity * Time.deltaTime;
            }

            //Player Rotates Camera 
            if (!tracking)
            {
                rot = new Vector2(Mathf.Lerp(rot.x, mouseMovement.y + joystickMovement.y, Time.deltaTime * 10), Mathf.Lerp(rot.y, mouseMovement.x + joystickMovement.x, Time.deltaTime * 10));
                targetRotation *= Quaternion.Euler(rot.x, rot.y, 0.0f);
            }


            //UpVector Calulation
            {
                upVectorPrevious = upVector;

                if (gM != null)
                {
                    //Use ManualUpVector if needed
                    upVector = Vector3.Lerp(-gM.ReturnGravity(transform, Vector3.zero).normalized, manualUpVector, manualUpVectorScaler);
                }
                else
                {
                    upVector = manualUpVector;
                }

            }

            //UpVector Curving Rotates Camera
            //if (upVectorPrevious != upVector)
            //{

            //    Vector3 cross;
            //    Vector3 forward;
            //    Quaternion Rotation1;
            //    Quaternion Rotation2;

            //    cross = Vector3.Cross(upVectorPrevious, upVector);
            //    forward = Vector3.Cross(cross, upVector);

            //    Rotation1 = Quaternion.LookRotation(forward.normalized, upVector.normalized);

            //    cross = Vector3.Cross(upVectorPrevious, upVector);
            //    forward = Vector3.Cross(cross, upVectorPrevious);

            //    Rotation2 = Quaternion.LookRotation(forward.normalized, upVectorPrevious.normalized);            

            //    curveRotation = Rotation1 * Quaternion.Inverse(Rotation2) * targetRotation;

            //    targetRotation = curveRotation;// Quaternion.Slerp(targetRotation, curveRotation, Time.deltaTime * 10);

            //}
            //else
            //{
            //    curveRotation = targetRotation;
            //}

            //Upvector Adjust. Have camera up lerped by gravity magnitude, between facing away from gravity, and the custom up vector. If Gm does not exist lerp to the custom up vector        
            if (upReset)
            {
                //Scaled by one minus the absolute value of the dot product between the forward vector and the up vector
                float disable = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, upVector));
                targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, upVector), Time.deltaTime * disable * upResetLerp);
            }

            //Camera Positioning
            if (followingTarget)
            {
                rb.isKinematic = true;

                Vector3 followPosAverage = Vector3.zero;

                //Get the average position between targets
                {
                    int count = 0;
                    foreach (Transform t in followTargets)
                    {
                        followPosAverage += t.position;
                        count += 1;
                    }
                    followPosAverage /= count;
                }


                {
                    float followTargetSpeed = 0;

                    if (Time.deltaTime > 0)
                    {
                        followTargetSpeed = (followPosAverage - followPreviousPosition).magnitude / Time.deltaTime;

                        followPreviousPosition = followPosAverage;
                    }

                    //set camera orbitpoint position                                       
                    {
                        orbitPoint = Vector3.Lerp(orbitPoint, followPosAverage + targetRotation * localOffset, Time.deltaTime * lerpSpeed);
                    }

                    //caluate zoom (displacement from orbit point)
                    {

                        zoom = Mathf.Lerp(zoom, Mathf.LerpUnclamped(zoomDistanceMin, zoomDistanceMax, followTargetSpeed / 100), Time.deltaTime * zoomLerpSpeed);

                        //check for wall intersection         
                        if (!clippingAllowed)
                        {
                            int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10)); //layerMask = ~layerMask;

                            if (Physics.Raycast(orbitPoint, -(targetRotation * Vector3.forward), out hit, zoom, layerMask, QueryTriggerInteraction.Ignore))
                                zoom = hit.distance;
                        }
                    }

                    //set camera position                                       
                    {
                        float disable = Mathf.Clamp01((Time.timeSinceLevelLoad - 5) / 10);

                        transform.position = Vector3.Lerp(transform.position, orbitPoint - (targetRotation * Vector3.forward * (zoom - .2f)), disable);
                        //transform.position = targets[followIndexPosition].transform.position - (targetRotation * Vector3.forward * (zoom - .2f));
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

                    int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));
                    //if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))//if we hit something
                    if (Physics.SphereCast(transform.position, 3f, transform.forward, out hit, Mathf.Infinity, layerMask))
                    {
                        if (hit.transform.gameObject.tag == "Trackable")
                        {
                            trackTargets[0] = hit.transform;
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

                Vector3 trackPosAverage = Vector3.zero;

                //Get the average position between targets
                if (trackTargets[0] != null)
                {
                    int count = 0;
                    foreach (Transform t in trackTargets)
                    {
                        trackPosAverage += t.position;
                        count += 1;
                    }
                    trackPosAverage /= count;
                }

                //scale value on tracking lerp
                if (tracking)
                {
                    trackLerpScale = Mathf.Lerp(trackLerpScale, 1, Time.deltaTime * 1);//smoothly enable tracking.     

                    //Set targeting reticle to target location
                    if (trackTargets[0] != null)
                    {
                        cameraTargetingPosition = trackPosAverage;
                    }

                    disableTracking = 1;

                }
                else
                {
                    // quickly removes any trackLerpScale when the player atempts to rotate the camera on their own.                
                    disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4));
                    disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4));
                    disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse X") * 4));
                    disableTracking *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse Y") * 4));

                    //smoothly disable tracking, but allow the player to turn it off fast with "disableTracking".
                    trackLerpScale = Mathf.Lerp(trackLerpScale * disableTracking, 0, Time.deltaTime * 2);
                }

                //Character Object Tracking
                {
                    if (trackTargets[0] != null)
                        characterTargetingPosition = trackPosAverage;
                    else
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
            transform.rotation = targetRotation * ReturnShake(1, shakeMagnatude) * ReturnShake(.01f, 1f);
        }
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



