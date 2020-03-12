using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
   
    [Header("Camera")]
    public Camera thisCamera;
    public static CameraControl instance;

    public bool overrideFOV = false;
    public float camTargetFov;
    public float fovRatio;

    [Header("Character Controller")]
    public BaseCharacterController controller;

    [Header("Camera Position Follow")]    
    public bool followingTarget = true;
    public Transform[] followedTargets;
    public Vector3 cameraFollowPosition;
    public Vector3 camerafollowPositionPrevious;
    public Vector3 followPositionVelocity;
    public float followPositionSpeed;

    [Header("Camera Position Track")]
    public bool trackingTarget = false;
    public Transform[] trackedTargets;   
    public Vector3 cameraTrackingPosition;
    public Vector3 cameratrackingPositionPrevious;

    [Header("Extra Tracking Data")]//Might be able to remove or shorten. Optimize later
    public Vector3 characterTargetingPosition;
    public float objectTrackingLerpValue = 0;
    public Vector3 cameraDirectionResetPosition;
    public float directionResetLerpValue = 0;

    [Header("Input")]
    public float freeCamSpeed = 100;
    public float mouseSensitivity = 100.0f;
    public float joystickSensitivity = 100.0f;
    public Vector2 mouseMovement = Vector2.zero;
    public Vector2 joystickMovement = Vector2.zero;
    public Vector2 rotationFromInput = Vector2.zero;
    public bool overrideCursorLock;

    [Header("Camera Up Direction Management")]
    public Vector3 cameraUpVector = Vector3.up;
    public Vector3 manualUpVector = Vector3.up;
    public float manualUpVectorScaler = 0;
    public Vector3 lean = Vector3.up;
    public bool upReset = true;
    public float upResetLerpSpeed = 1;

    [Header("Camera Surface Based Rotation")]
    public Vector3 characterUpVector = Vector3.up;
    public Vector3 characterUpVectorPrevious = Vector3.up;
    public float surfaceBasedRotationScalar = 0;

    [Header("OrbitPoint")]
    public Vector3 orbitPoint;
    public float orbitPointLerpRatio = 0;
    public float orbitPointLerpRecoverySpeed = .5f;
    public Vector3 characterRelativeOffset = Vector3.zero;
    public Vector3 followVelocityDisplacement;         

    [Header("Zoom")]
    public float zoom;
    public float zoomLerpSpeed = 10;
    public float zoomDistanceMin = 5;
    public float zoomDistanceMax = 10;
    public bool zoomClippingAllowed = false;
       
    [Header("ScreenShake")]
    public float shakeTime = 0;    
    public float shakeMagnatude = 0;
    public float shakeTimeSpeed = 10;

    [Header("Camera Physics")]

    public Vector3 cameraVelocity;
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

        
        if (followedTargets.Length == 0)
        {
            followedTargets = new Transform[1];

            if (followedTargets[0] == null)
            {            
                followedTargets[0] = GameObject.FindGameObjectWithTag("PlayerCharacterAnchorPoint").transform;
                    if (followedTargets[0] == null)
                        Debug.Log("No CharacterAnchorPoint found");
            }
        }
          
        orbitPoint = followedTargets[0].position + targetRotation * characterRelativeOffset;       

        zoom = 5;        
    }


    void Beat(float currentStep)
    {

        //if (currentStep == 4)
        //{
        //   camera.fieldOfView = 80;
        //   shakeMagnatude += 2;        
        //}
        //else
        {

            //camera.fieldOfView = 65;
            //shakeMagnatude += .8f;
        }
    }

    public void CauseCameraShake(float value)
    {                
        shakeMagnatude += value;
    }

    public void CameraResetLerp(float value)
    {
        orbitPointLerpRatio = value;
    }

    public void CameraBumpFOV(float bump)
    {
        thisCamera.fieldOfView += bump;
    }

    public void CameraSetTargetFOV(float FOV)
    {
        if (!overrideFOV)
            camTargetFov = FOV;
    }

    void Waveform(float waveform)
    {
        //camera.fieldOfView = Mathf.Lerp(60, 65, waveform);
    }

    public void CameraBounce(Collision collision)
    {

        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 hitNormal = collision.GetContact(0).normal;

        print("CameraBounce");

        //Find Reflection Vector
        Vector3 reflect = Vector3.Reflect(controller.rbVelocityNormalized, hitNormal);

        //Camera Tracks towards next bounce position if one exists
        RaycastHit bounceFutureCheck;
        if (Physics.Raycast(transform.position, reflect, out bounceFutureCheck, 1000, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore))
        {
            cameraDirectionResetPosition = bounceFutureCheck.point;
            directionResetLerpValue = .1f;
        }
        else
        {
            cameraDirectionResetPosition = hitPoint + reflect * 1000;
            directionResetLerpValue = .1f;
        }

    }

    void Update()
    {

        if (controller != null)
        {
            //adjust how the player should appear on screen depending on state.  
            {

                float camZoomDistanceMin = 3;
                float camCharacterRelativeOffset = 0;


                if (controller.stateMachine.currentModule.GetType() == typeof(SymGround))
                {
                    upResetLerpSpeed = 5;
                    camCharacterRelativeOffset = controller.playerHeight;
                    camZoomDistanceMin = 3;

                    if (controller.focusInput && controller.moveDirectionMag == 0 && controller.rbVelocityMagnitude < 20 && controller.grounded)
                    {
                        camZoomDistanceMin = 2;
                    }

                }

                if (controller.stateMachine.currentModule.GetType() == typeof(SymFlight))
                {
                    camZoomDistanceMin = 5;

                    if (controller.drillDash == 1)
                        camZoomDistanceMin = 3;

                    upResetLerpSpeed = 1;
                    camCharacterRelativeOffset = 0;
                }

                float camZoomDistanceMax = Mathf.LerpUnclamped(5, 10, controller.rbVelocityMagnitude * .01f);
               
                //Shift away from the center of mass, towards the head of our character.                
                characterRelativeOffset = Vector3.Slerp(characterRelativeOffset, new Vector3(0, camCharacterRelativeOffset, 0), Time.deltaTime * 2);
                
                //Adjust Zoom parameters                
                zoomDistanceMin = Mathf.Lerp(zoomDistanceMin, camZoomDistanceMin, Time.deltaTime);
                zoomDistanceMax = Mathf.Lerp(zoomDistanceMax, camZoomDistanceMax, Time.deltaTime);
                               
            }

            //Camera manual up vector set when wall walking
            {
                manualUpVectorScaler = controller.wallRun;
                manualUpVector = -controller.gravity.normalized;

                //Test
                //cameraControl.manualUpVector = Vector3.Lerp(-controller.gravity.normalized, -controller.gravityRaw.normalized, .5f);
            }

            //Camera turns with character while moving on curving surfaces
            {
                surfaceBasedRotationScalar = controller.wallRun;
                characterUpVectorPrevious = characterUpVector;

                if (controller.grounded)
                {
                    //if (Vector3.Dot(characterUpVectorPrevious, controller.surfaceNormal) > .7f)
                    {
                        //cameraControl.characterUpVector = Vector3.Lerp(cameraControl.characterUpVector, transform.up, .1f);
                        //cameraControl.characterUpVector = transform.up;
                        //cameraControl.characterUpVector = -behaviour.gravity.normalized;
                        characterUpVector = Vector3.Lerp(characterUpVector, -controller.gravity.normalized, .1f);
                    }
                }
            }

            //Camera FOV adjust from focus     
            {
                if (controller.focusInput)
                    CameraSetTargetFOV(30);
                else
                    CameraSetTargetFOV(60);
            }

            //CameraShake from turbulence            
            if (!controller.grounded && controller.flightEnabled)
            {
                CauseCameraShake(.025f * Time.deltaTime * Mathf.InverseLerp(20, 60, Mathf.Abs(controller.rbVelocityMagnitude * Vector3.Dot(controller.rbVelocityNormalized, controller.gravityRaw.normalized))));
            }
            

        }

        //Find Camera Speed
        {
            cameraVelocity = (thisCamera.transform.position - cameraPositionPrevious) / Time.deltaTime;

            cameraSpeed = cameraVelocity.magnitude;

            cameraPositionPrevious = thisCamera.transform.position;
        }

        //lower shakeMagnatude over time
        {
            shakeMagnatude = Mathf.Lerp(shakeMagnatude, 0, Time.deltaTime * 4);
            //shakeMagnatude = Mathf.Clamp01(shakeMagnatude);
        }

        {
            if (Time.timeScale == 0 || overrideCursorLock)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
      
        if (Time.deltaTime > 0)// && Time.timeSinceLevelLoad > 5)
        {
            //record mouse and gamepad input
            if (Time.timeScale != 0 && Time.unscaledDeltaTime < .05)
            {
                mouseMovement.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
                mouseMovement.y = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

                joystickMovement.x = Input.GetAxis("Horizontal2") * joystickSensitivity * Time.unscaledDeltaTime;
                joystickMovement.y = Input.GetAxis("Vertical2") * joystickSensitivity * Time.unscaledDeltaTime;
            }
            else
            {
                joystickMovement.y = joystickMovement.x = mouseMovement.y = mouseMovement.x = 0;                
            }

            //Player Rotates Camera 
            if (!trackingTarget)
            {
                rotationFromInput = new Vector2(Mathf.Lerp(rotationFromInput.x, mouseMovement.y + joystickMovement.y, Time.unscaledDeltaTime * 10), Mathf.Lerp(rotationFromInput.y, mouseMovement.x + joystickMovement.x, Time.unscaledDeltaTime * 10));
                targetRotation *= Quaternion.Euler(rotationFromInput.x, rotationFromInput.y, 0.0f);
            }

            //UpVector Calulation                      
            if (gM != null)
            {
                //Use ManualUpVector if needed             
                cameraUpVector = Vector3.Lerp(-gM.ReturnGravity(transform, Vector3.zero).normalized, manualUpVector, manualUpVectorScaler);                  
            }
            else
            {
                cameraUpVector = manualUpVector;
            }

            //Set Camera FOV
            {

                float skyBonus = 0;

                //if (grounded)
                //skyBonus = 30 * Mathf.Abs(Vector3.Dot(cameraUpVector, thisCamera.transform.forward));
                //else
                //skyBonus = 0;                

                thisCamera.fieldOfView = Mathf.Lerp(thisCamera.fieldOfView, camTargetFov + skyBonus, Time.deltaTime * 5);

                fovRatio = Mathf.Lerp(thisCamera.fieldOfView, camTargetFov, Time.deltaTime * 5) / 60;

                //fovRatio = thisCamera.fieldOfView / 60;
            }
            
            //Upright Camera Orientation with Upvector. Have camera up lerped by gravity magnitude, between facing away from gravity, and the custom up vector. If Gm does not exist lerp to the custom up vector        
            if (upReset)
            {
                //Scaled by one minus the absolute value of the dot product between the forward vector and the up vector
                float disable = 1 - Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, cameraUpVector));

                //Camera Lean            
                lean = Vector3.Slerp(lean,
                    Vector3.Slerp(cameraUpVector, Vector3.Slerp(cameraUpVector, cameraVelocity.normalized, .5f), Mathf.InverseLerp(0, 50, cameraSpeed)),
                    Time.deltaTime * 2);

                targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(targetRotation * Vector3.forward, lean), Time.unscaledDeltaTime * disable * upResetLerpSpeed);
            }

            //Camera turns with character while moving on curving surfaces    
            if (characterUpVectorPrevious != characterUpVector && followingTarget)
            {

                Vector3 cross;
                Vector3 forward;
                Quaternion Rotation1;
                Quaternion Rotation2;

                cross = Vector3.Cross(characterUpVectorPrevious, characterUpVector);
                forward = Vector3.Cross(cross, characterUpVector);

                Rotation1 = Quaternion.LookRotation(forward.normalized, characterUpVector.normalized);

                cross = Vector3.Cross(characterUpVectorPrevious, characterUpVector);
                forward = Vector3.Cross(cross, characterUpVectorPrevious);

                Rotation2 = Quaternion.LookRotation(forward.normalized, characterUpVectorPrevious.normalized);

                surfaceBasedRotation = Rotation1 * Quaternion.Inverse(Rotation2) * targetRotation;

                //targetRotation = Quaternion.Slerp(targetRotation, curveRotation, curveRotationScalar);
                targetRotation = surfaceBasedRotation;

            }
            else
            {
                surfaceBasedRotation = targetRotation;
            }

            //Camera Positioning
            if (followingTarget)
            {

                Vector3 followedTargetsPosAverage = Vector3.zero;

                //Get the average position between targets
                if (followedTargets[0] != null)
                {
                    int count = 0;
                    foreach (Transform t in followedTargets)
                    {
                        followedTargetsPosAverage += t.position;
                        count += 1;
                    }
                    followedTargetsPosAverage /= count;
                }

                //Set targeting reticle to target location
                if (followedTargets[0] != null)
                {
                    cameraFollowPosition = followedTargetsPosAverage;
                }

                {

                    followPositionVelocity = (cameraFollowPosition - camerafollowPositionPrevious) / Time.deltaTime;

                    followPositionSpeed = followPositionVelocity.magnitude;

                    camerafollowPositionPrevious = cameraFollowPosition;     
                        

                    //set camera orbitpoint position                                       
                    {                                              
                        followVelocityDisplacement = Vector3.Lerp(followVelocityDisplacement, Vector3.Lerp(Vector3.zero, -(followPositionVelocity * .05f), (Vector3.Dot(followPositionVelocity.normalized, transform.forward) + 1) * .5f), Time.deltaTime * 2);
                        //followVelocityDisplacement = Vector3.Lerp(followVelocityDisplacement, -followPositionVelocity * .05f, Time.deltaTime * 2);
                      
                        orbitPoint = Vector3.Lerp(orbitPoint, cameraFollowPosition + (followVelocityDisplacement * fovRatio) + targetRotation * characterRelativeOffset, orbitPointLerpRatio);

                        //Debug.DrawLine(cameraFollowPosition + targetRotation * characterRelativeOffset, orbitPoint, Color.white);
                   
                        //Recover OrbitPointLerpSpeed if its lowered                                           
                        orbitPointLerpRatio = Mathf.Min(orbitPointLerpRatio + Time.deltaTime * orbitPointLerpRecoverySpeed, 1);
                    }

                    //caluate zoom (displacement from orbit point)
                    {

                        zoom = Mathf.Lerp(zoom, Mathf.LerpUnclamped(zoomDistanceMin, zoomDistanceMax, followPositionSpeed / 100), Time.deltaTime * zoomLerpSpeed);

                        zoom = (zoomDistanceMin + zoomDistanceMax) * .5f;

                        //check for wall intersection         
                        if (!zoomClippingAllowed)
                        {
                            int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));

                            if (Physics.Raycast(orbitPoint, -(targetRotation * Vector3.forward), out hit, zoom, layerMask, QueryTriggerInteraction.Ignore))
                                zoom = hit.distance;
                        }
                    }

                    //set camera position                                       
                    {                       
                        transform.position =  orbitPoint - (targetRotation * Vector3.forward * (zoom - .5f));                   
                    }

                }

            }
            else
            {
                    Vector3 vector = (targetRotation * Vector3.forward * Input.GetAxis("Vertical") + targetRotation * Vector3.right * Input.GetAxis("Horizontal") + targetRotation * Vector3.up * (Input.GetAxis("Jump/Bounce") - Input.GetAxis("Crouching")));
                    transform.position += Vector3.ClampMagnitude(vector, 1) * freeCamSpeed * Time.deltaTime;
            }

            //Cast and lock target.
            if (Input.GetButtonDown("LockOn"))
            {
                if (!trackingTarget)
                {

                    int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));
                    //if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))//if we hit something
                    if (Physics.SphereCast(transform.position, 3f, transform.forward, out hit, Mathf.Infinity, layerMask,QueryTriggerInteraction.Collide))
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

            //Smooth Object Tracking
            {

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

                    //Set tracking position to the average position of tracked targets
                    cameraTrackingPosition = trackedTargetsPosAverage;
                }

                //scale value on tracking lerp
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

                //Character Object Tracking
                {

                   // if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore))
                     //   characterTargetingPosition = Vector3.Lerp(hit.point, cameraTrackingPosition, objectTrackingLerpValue);
                   // else
                        characterTargetingPosition = Vector3.Lerp(transform.position + transform.forward * 1000, cameraTrackingPosition, objectTrackingLerpValue);
                }

                //Track camera to targeting position    
                {
                    targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(cameraTrackingPosition - transform.position, targetRotation * Vector3.up), objectTrackingLerpValue);
                }
            }

            //Smooth Camera Direction Reset
            {                           

                //scale value on tracking lerp 
                {
                    // quickly removes any directionResetLerpValue when the player atempts to rotate the camera on their own.                
                    directionResetLerpValue *= 1 - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical2") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse X") * 4)) - Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Mouse Y") * 4));

                    //smoothly disable tracking.
                    directionResetLerpValue = Mathf.Lerp(directionResetLerpValue, 0, Time.unscaledDeltaTime * 2);
                }

                //Character looks in the same direction as the camera
                {
                    characterTargetingPosition = Vector3.Lerp(characterTargetingPosition, cameraDirectionResetPosition, directionResetLerpValue);
                }

                //Track camera to targeting position    
                {
                    targetRotation = Quaternion.Lerp(targetRotation, Quaternion.LookRotation(cameraDirectionResetPosition - transform.position, targetRotation * Vector3.up), directionResetLerpValue);
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

    private void LateUpdate()
    {
        //Debug.DrawLine(transform.position, orbitPoint);
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



