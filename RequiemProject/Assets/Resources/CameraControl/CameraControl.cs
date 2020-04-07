using UnityEngine;

public class CameraControl : MonoBehaviour {
   
    [Header("Camera")]
    public Camera thisCamera;
    public static CameraControl instance;
    public float groundedBehaviourScaleFactor = 1;
    public bool manualDevCameraControl = false;
    public bool overrideFOV = false;
    public float camTargetFov;
    public float skyFOVBonus = 0;
    public float fovRatio;

    [Header("Character Controller")]
    public BaseCharacterController controller;

    [Header("Camera Position Follow")]    
    public bool followingTarget = true;
    public Transform followedTarget = null;
    public Vector3 followPosition;
    public Vector3 followPositionPrevious;
    public Vector3 followPositionVelocity;
    public float followPositionSpeed;
    public float followPositionSpeedPrevious;
    public float followPositionAcceleration;

    [Header("Camera Position Track")]
    public bool trackingTarget = false;
    public Transform[] trackedTargets = { null };   
    public Vector3 cameraTrackingPosition;
    public Vector3 cameratrackingPositionPrevious;

    [Header("Extra Tracking Data")]
    public bool focusCenter = false;
    public float objectTrackingLerpValue = 0;
    public Vector3 cameraDirectionResetPosition;
    public float cameraDirectionResetLerpValue = 0;

    [Header("Input")]
    public float freeCamSpeed = 100;
    public float mouseSensitivity = 100.0f;
    public float joystickSensitivity = 100.0f;
    public Vector2 mouseMovement = Vector2.zero;
    public Vector2 joystickMovement = Vector2.zero;
    public Vector2 rotationFromInput = Vector2.zero;
    public bool overrideCursorLock;
    public Vector3 manualCameraMoveVector;

    [Header("Camera Up Direction Management")]
    public Vector3 cameraUpVector = Vector3.up;
    public Vector3 manualUpVector = Vector3.up;
    public float manualUpVectorScaler = 0;
    public Vector3 lean = Vector3.up;
    public bool upReset = true;
    public float upResetLerpSpeed = 1;

    [Header("Camera Surface Based Rotation")]
    public bool turnVectorNeedsReset = false;
    public Vector3 turnVector = Vector3.up;
    public Vector3 turnVectorPrevious = Vector3.up;
    public float surfaceBasedRotationScalar = 0;

    [Header("Positioning")]
    public Vector3 focusPoint;
    public Vector3 offset = Vector3.zero;
    public Vector3 followVelocityDisplacement;
    
    public float zoomDistance;   
    public float zoomDistanceMin = 5;
    public float zoomDistanceMax = 10;
    public bool zoomClippingAllowed = false;
    public float pitchScale;
    public AnimationCurve pitchCurve;

    [Header("ScreenShake")]
    public float shakeTime = 0;    
    public float unscaledShakeTime = 0;    
    public float shakeMagnatude = 0;
    public float shakeTimeSpeed = 10;

    [Header("Camera Physics")]
    public Vector3 cameraVelocity;
    public Vector3 cameraVelocityDirection;
    public float cameraSpeed;
    public Vector3 cameraPositionPrevious;

    private Quaternion targetRotation;
    private Quaternion surfaceBasedRotation;
    private RaycastHit hit;
    private GravityManager gM;
   
    private void Awake()
    {
        gM = FindObjectOfType<GravityManager>();

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
    }

    void Start () {

        thisCamera = GetComponent<Camera>();
        
        targetRotation = transform.rotation;
        
        if (followedTarget == null)
        {            
            followedTarget = GameObject.FindGameObjectWithTag("PlayerCharacterAnchorPoint").transform;
                if (followedTarget == null)
                    Debug.Log("No CharacterAnchorPoint found");
        }
                
        focusPoint = followedTarget.position + targetRotation * offset;
        zoomDistance = zoomDistanceMax;
    }

    public void CauseCameraShake(float value)
    {                
        shakeMagnatude += value;
    }

    private void ManualControl()
    {
        manualCameraMoveVector = Vector3.Lerp(manualCameraMoveVector,
            transform.forward * Input.GetAxisRaw("Vertical") + Vector3.Cross(cameraUpVector, transform.forward) * Input.GetAxisRaw("Horizontal") + cameraUpVector * (Input.GetAxisRaw("Jump/Bounce") - Input.GetAxisRaw("Crouching")),
            Time.unscaledDeltaTime * 5
            );

        if(!followingTarget)
            transform.position += Vector3.ClampMagnitude(manualCameraMoveVector, 1) * freeCamSpeed * Time.unscaledDeltaTime;                 
    }
   
    void Update()
    {
        //Set camera parameters with character controller values
        if (controller != null)
        {

            if (controller.grounded)                          
                groundedBehaviourScaleFactor = Mathf.Clamp01(groundedBehaviourScaleFactor + Time.deltaTime * 1);            
            else                        
                groundedBehaviourScaleFactor = Mathf.Clamp01(groundedBehaviourScaleFactor - Time.deltaTime * 1);


            if (controller.capsuleCollider != null)
            {
                if (controller.capsuleCollider.isTrigger)
                {
                    zoomClippingAllowed = true;
                }
                else
                {
                    zoomClippingAllowed = false;
                }
            }

            //adjust how the player should appear on screen depending on state.  
            {
                Vector3 groundOffset = controller.transform.up * controller.playerHeight * .38f;
                if (controller.crouching)
                    groundOffset = Vector3.zero;
                                                    
                offset = Vector3.Slerp(offset, Vector3.Lerp(Vector3.zero, groundOffset, Mathf.Pow(groundedBehaviourScaleFactor, 5)), Time.deltaTime * 5);

                upResetLerpSpeed = Mathf.Lerp(1, 5, Mathf.Pow(groundedBehaviourScaleFactor, 2));              
                          
                zoomDistanceMin = 1.6f;

                zoomDistanceMax = Mathf.Max(5, controller.rbVelocityMagnitude * .075f);                  
            }
            
            {
                manualUpVectorScaler = controller.wallRun;
                manualUpVector = -controller.gravity.normalized;
            }

            //Camera turns with character while moving on curving surfaces
            {              
                surfaceBasedRotationScalar = controller.wallRun;

                turnVectorPrevious = turnVector;

                if (controller.grounded)
                {
                    if (turnVectorNeedsReset)
                        turnVectorPrevious = turnVector = controller.surfaceNormal;
                    else
                        turnVector = Vector3.Lerp(turnVector, controller.surfaceNormal, .01f);
                    turnVectorNeedsReset = false;
                }
                else
                    turnVectorNeedsReset = true;
            }

            //Camera FOV adjust from focus     
            {
                if (controller.focusInput)
                      camTargetFov = 45;
                  else
                      camTargetFov = 60;
            }
                       
        }

        if (manualDevCameraControl)
            ManualControl();
        else
            manualCameraMoveVector = Vector3.zero;

        //Find Camera Speed
        {
            cameraVelocity = (transform.position - cameraPositionPrevious) / Time.unscaledDeltaTime;

            cameraVelocityDirection = cameraVelocity.normalized;

            cameraSpeed = cameraVelocity.magnitude;

            cameraPositionPrevious = transform.position;
        }
                     
        //Lock Cursor in window
        if (Time.timeScale == 0 || overrideCursorLock)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        //record mouse and gamepad input
        if (Time.timeScale != 0 || manualDevCameraControl)
        {
            mouseMovement.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
            mouseMovement.y = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

            joystickMovement.x = Input.GetAxis("Horizontal2") * joystickSensitivity * Time.unscaledDeltaTime;
            joystickMovement.y = Input.GetAxis("Vertical2") * joystickSensitivity * Time.unscaledDeltaTime;

            rotationFromInput = new Vector2(Mathf.Lerp(rotationFromInput.x, mouseMovement.y + joystickMovement.y, Time.unscaledDeltaTime * 10), Mathf.Lerp(rotationFromInput.y, mouseMovement.x + joystickMovement.x, Time.unscaledDeltaTime * 10));
        }
        else
        {
            joystickMovement.y = joystickMovement.x = mouseMovement.y = mouseMovement.x = 0;
            rotationFromInput = Vector2.zero;
        }

        //Input Rotates Camera 
        if (!trackingTarget)
            targetRotation *= Quaternion.Euler(rotationFromInput.x, rotationFromInput.y, 0.0f);

        //UpVector Calulation                      
        if (gM != null)
            cameraUpVector = Vector3.Lerp(-gM.ReturnGravity(transform, Vector3.zero).normalized, manualUpVector, manualUpVectorScaler);
        else
            cameraUpVector = manualUpVector;

        //Set Camera FOV
        {
            skyFOVBonus = Mathf.Lerp(0, 30 * Mathf.Abs(Vector3.Dot(cameraUpVector, thisCamera.transform.forward)), groundedBehaviourScaleFactor);

            thisCamera.fieldOfView = Mathf.Lerp(thisCamera.fieldOfView, camTargetFov + skyFOVBonus, Time.deltaTime * 5);
            //thisCamera.fieldOfView = camTargetFov + skyBonus;
            fovRatio = Mathf.Lerp(thisCamera.fieldOfView, camTargetFov, Time.deltaTime * 5) / 60;
        }

        //Upright Camera Orientation with Upvector. Have camera up lerped by gravity magnitude, between facing away from gravity, and the custom up vector. If Gm does not exist lerp to the custom up vector        
        if (upReset)
            UpReset();

        if (followingTarget)
            RotationCurving();

        GrabFollowTargetValues();

        //Camera Shake from follow position velocity acceleration
        CauseCameraShake(Mathf.Abs(Mathf.Pow(followPositionAcceleration * .025f, 2)));

        //Camera Positioning
        if (followingTarget)
            CameraPositioning();

        ObjectTracking();

        //SmoothCameraDirectionReset();

        //Lower shakeMagnatude over time        
        shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 4);

        //Increment shake time everyframe            
        shakeTime += Time.deltaTime * shakeTimeSpeed;
        unscaledShakeTime += Time.unscaledDeltaTime * shakeTimeSpeed;


        //CameraShake from turbulence                                                
        //CauseCameraShake(1f * Time.deltaTime * Mathf.InverseLerp(20, 100, cameraSpeed));

        //Set Rotation
        transform.rotation = targetRotation * ReturnShake(1, shakeMagnatude, shakeTime) * ReturnShake(.01f, 1f, shakeTime) * ReturnShake(1, .25f * Mathf.InverseLerp(20, 100, cameraSpeed), unscaledShakeTime);
        
    }

    void UpReset()
    {

        if (Time.timeScale == 0 && !manualDevCameraControl)
            return;

        //Scaled by one minus the absolute value of the dot product between the forward vector and the up vector
        float disable = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, cameraUpVector));

        //Camera Lean            
        Vector3 leanDirection = Vector3.Slerp(cameraUpVector, -cameraVelocityDirection, .5f * (1 - Mathf.Abs(Vector3.Dot(cameraUpVector, cameraVelocityDirection))));

        lean = Vector3.Slerp(lean,
            Vector3.Slerp(cameraUpVector, leanDirection, Mathf.InverseLerp(0, 50, cameraSpeed)),
            Time.unscaledDeltaTime * 2);

        targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, lean), Time.unscaledDeltaTime * disable * upResetLerpSpeed);
    }

    void RotationCurving()
    {
        //Camera turns while moving on curving surfaces    
        if (turnVectorPrevious == turnVector)
            return;

        Vector3 cross;
        Vector3 forward;
        Quaternion Rotation1;
        Quaternion Rotation2;

        cross = Vector3.Cross(turnVectorPrevious, turnVector);
        forward = Vector3.Cross(cross, turnVector);

        Rotation1 = Quaternion.LookRotation(forward.normalized, turnVector.normalized);

        cross = Vector3.Cross(turnVectorPrevious, turnVector);
        forward = Vector3.Cross(cross, turnVectorPrevious);

        Rotation2 = Quaternion.LookRotation(forward.normalized, turnVectorPrevious.normalized);

        surfaceBasedRotation = Rotation1 * Quaternion.Inverse(Rotation2) * targetRotation;

        targetRotation = Quaternion.Slerp(targetRotation, surfaceBasedRotation, surfaceBasedRotationScalar);        
    }

    void GrabFollowTargetValues()
    {
        //Get the position of folow target
        if (followedTarget != null)
        {
            followPositionPrevious = followPosition;
            followPosition = followedTarget.position;

            if (followPositionPrevious == Vector3.zero)
                followPositionPrevious = followPosition;
        }

        //Followed target velocity computation    
        if (Time.deltaTime > 0)                
        {
            followPositionVelocity = (followPosition - followPositionPrevious) / Time.deltaTime;

            followPositionSpeedPrevious = followPositionSpeed;

            followPositionSpeed = followPositionVelocity.magnitude;

            followPositionAcceleration = followPositionSpeed - followPositionSpeedPrevious;
        }
    }

    void CameraPositioning()
    {
        pitchScale = Mathf.Lerp(1, pitchCurve.Evaluate((Vector3.Dot(cameraUpVector, transform.forward) * .5f) + .5f), groundedBehaviourScaleFactor);

        //Velocity Displacement                
        float displacementScale = .025f * (1 + Mathf.Max(0, followPositionAcceleration));
        followVelocityDisplacement = Vector3.Lerp(followVelocityDisplacement, -followPositionVelocity * displacementScale * pitchScale, Time.deltaTime * 2);

        //set camera focusPoint position                                                       
        focusPoint = followPosition + (followVelocityDisplacement * fovRatio) + offset;

        //caluate zoom (displacement from focus point)              
        zoomDistance = Mathf.Lerp(zoomDistance, Mathf.Lerp(zoomDistanceMin, zoomDistanceMax, pitchScale), Time.deltaTime * 2);
        //zoomDistance = Mathf.Lerp(zoomDistanceMin, zoomDistanceMax, pitchScale);               

        //set camera position   
        Vector3 forward = targetRotation * Vector3.forward;

        transform.position = focusPoint - forward * zoomDistance;   

        if (!zoomClippingAllowed)            
            if (Physics.Raycast(focusPoint, -forward, out hit, zoomDistance + .25f, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point + forward * .25f;
            }
            
    }

    void ObjectTracking()
    {

        //Search for trackingTarget
        if (Input.GetButtonDown("LockOn"))
        {
            if (!trackingTarget)
            {

                if (Physics.SphereCast(transform.position, 3f, transform.forward, out hit, Mathf.Infinity, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.gameObject.tag == "Trackable")
                    {
                        trackedTargets[0] = hit.transform;
                        trackingTarget = true;
                    }
                }
            }
            else
            {
                trackingTarget = false;
            }
        }

        Vector3 trackedTargetsPosAverage = Vector3.zero;

        //Get the average position between targets
        if (trackedTargets.Length != 0)
            if (trackedTargets[0] != null)
            {
                int count = 0;
                foreach (Transform t in trackedTargets)
                {
                    trackedTargetsPosAverage += t.position;
                    count += 1;
                }

                trackedTargetsPosAverage /= count;

                //Set tracking position to the position of tracked target and the follow targets
                if (followedTarget != null && focusCenter)
                    cameraTrackingPosition = transform.position + ((trackedTargetsPosAverage - transform.position).normalized + (followPosition - transform.position).normalized) * .5f;
                else
                    //Set tracking position to the average position of tracked targets
                    cameraTrackingPosition = trackedTargetsPosAverage;
            }

        if (trackingTarget)
        {
            objectTrackingLerpValue = Mathf.Lerp(objectTrackingLerpValue, 1, Time.unscaledDeltaTime * 1);//smoothly enable tracking.   
        }
        else
        {
            // quickly removes any objectTrackingLerpValue when the player atempts to rotate the camera on their own.                
            objectTrackingLerpValue *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse X") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse Y") * 4));

            //smoothly disable tracking.
            objectTrackingLerpValue = Mathf.Lerp(objectTrackingLerpValue, 0, Time.unscaledDeltaTime * 2);
        }

        //Character looks at tracked objects, or the forwards of the camera if no object found
        if (controller != null)
            controller.characterLookAtPosition = Vector3.Lerp(transform.position + transform.forward * 1000, trackedTargetsPosAverage, objectTrackingLerpValue);

        //Track camera to targeting position                    
        targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(cameraTrackingPosition - transform.position, targetRotation * Vector3.up), objectTrackingLerpValue);        
    }

    void SmoothCameraDirectionReset()
    {          
        // quickly removes any directionResetLerpValue when the player atempts to rotate the camera on their own.                
        cameraDirectionResetLerpValue *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse X") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse Y") * 4));
        //smoothly disable tracking.
        cameraDirectionResetLerpValue = Mathf.Lerp(cameraDirectionResetLerpValue, 0, Time.unscaledDeltaTime * 2);

        //Character looks in the same direction as the camera during reset       
        if (controller != null)
            controller.characterLookAtPosition = Vector3.Lerp(controller.characterLookAtPosition, cameraDirectionResetPosition, cameraDirectionResetLerpValue);

        //Track camera to targeting position                    
        targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(cameraDirectionResetPosition - transform.position, targetRotation * Vector3.up), cameraDirectionResetLerpValue);        
    }

    Quaternion ReturnShake(float frequency, float magnatude, float time)
    {     
        return Quaternion.Euler(GetNoise(time * frequency) * magnatude);           
    }

    Vector3 GetNoise(float position)
    {
        return new Vector3(
            (Mathf.PerlinNoise(1, position) - .5f) * 2,
            (Mathf.PerlinNoise(10, position) - .5f) * 2,
            (Mathf.PerlinNoise(0, position) - .5f) * 2);        
    }
}



