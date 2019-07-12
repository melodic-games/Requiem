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
    public Vector3 localOffset = Vector3.zero;
    public bool overrideLocalOffset = false;
    public float lerpSpeed = 20;// scalar used to tweak the tightness on the camera offset   
    
    public Vector3 upVector = Vector3.up;   
    public float mouseMoveScalar = 0;
    public float zoom;
    public float zoomDistanceMin = 2;
    public float zoomDistanceMax = 10;
    public bool upReset = true;
    
    public float shakeMagnatude = 1;

    public Vector2 rot = Vector2.zero;
    public Quaternion targetRotation;
    private Rigidbody rb;

    //Gravity data
    [Header("Gravity Data")]
    public GravityManager gM;

    void Start () {
        rb = GetComponent<Rigidbody>();

        targetRotation = transform.rotation;       

        if (targets[followIndexPosition] == null)
            targets[followIndexPosition] = GameObject.FindGameObjectWithTag("Player");
    }
  
    void Beat(float currentStep)
    {
        shakeMagnatude += 1f;
        if (currentStep == 4)
           shakeMagnatude += 1;        
    }

    void LateUpdate()
    {            
        //Cursor.lockState = CursorLockMode.Locked;

        shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 8);

        //Player Driven Rotation 
        {
            //record mouse input
            Vector2 mouseMovement = Vector2.zero;
            Vector2 joystickMovement = Vector2.zero;     

           // mouseMovement.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
           // mouseMovement.y = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;  
                
            joystickMovement.x = Input.GetAxis("Horizontal2") * joystickSensitivity * Time.deltaTime;
            joystickMovement.y = Input.GetAxis("Vertical2") * joystickSensitivity * Time.deltaTime;
            

            Vector2 finalRot;// = new Vector2(mouseMovement.y + joystickMovement.y, mouseMovement.x + joystickMovement.x);

            finalRot = rot = new Vector2(
                Mathf.Lerp(rot.x, mouseMovement.y + joystickMovement.y, Time.deltaTime * 10),
                Mathf.Lerp(rot.y, mouseMovement.x + joystickMovement.x, Time.deltaTime * 10));

            targetRotation *= Quaternion.Euler(finalRot.x, finalRot.y, 0.0f);
        }

        //Fix camera so that the upvector is facing away from ground
        if(upReset)
        {
            //Calulate Up Direction Vector By Gravity
            upVector = -gM.ReturnGravity(transform).normalized;
            float scale = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, upVector));
            targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, upVector), Time.deltaTime * scale);
        }

        if (overrideLocalOffset)
            localOffset = Vector3.zero;


        //Lerp Towards Target
        if (followingTarget)       
        {
            rb.isKinematic = true;

            float followTargetSpeed = 0;

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
                    float disable = 1;// Mathf.Clamp01(Time.time / 10);                
                    transform.position = Vector3.Lerp(transform.position, targets[followIndexPosition].transform.position + transform.TransformDirection(localOffset) - (targetRotation * Vector3.forward * zoom), Time.deltaTime * lerpSpeed * disable);
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


        if (Input.GetButtonDown("LockOn"))
        {
            if (!tracking)
            {
                RaycastHit hit;
                int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));
                //if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))//if we hit something
                  if  (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, Mathf.Infinity, layerMask))
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

        if (tracking)
        {
            {
                //track where target is         
                Quaternion lookRotation;

                float scalar = 1;
                scalar *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4));
                scalar *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4));
                scalar *= 1 - Mathf.Clamp01(mouseMoveScalar * 4);

                lookRotation = Quaternion.LookRotation((targets[trackIndexPosition].transform.position - transform.position).normalized, targetRotation * Vector3.up);
                targetRotation = Quaternion.Lerp(targetRotation, lookRotation, Time.deltaTime * scalar * 5);
            }

            targetingReticle.position = targets[trackIndexPosition].transform.position;
        }

        transform.rotation = targetRotation * Shake(shakeMagnatude);
       
        if (!tracking)
        {
            if (targetingReticle != null)
                targetingReticle.position = transform.position + transform.forward * 40;            
        }
          
    }

    Quaternion Shake(float magnatude)
    {
        return Quaternion.Euler(Random.Range(-1, 1) * shakeMagnatude, Random.Range(-1, 1) * shakeMagnatude, Random.Range(-1, 1) * shakeMagnatude);     
    }
}



