using UnityEngine;

public class CameraHelper : MonoBehaviour
{
    private BaseCharacterController controller;
    private CameraControl cameraControl;
    public float camZoomDistanceMin = 4;
    public float camZoomDistanceMax = 10;    
    private Vector3 characterRelativeOffset = Vector3.zero;
    public Vector3 headHeight = new Vector3(0, .7f, 0);

    void Start()
    {
        controller = GetComponent<BaseCharacterController>();
        cameraControl = FindObjectOfType<CameraControl>();
    }

    public void CauseCameraShake(float value)
    {
        if (cameraControl != null)
        {
            cameraControl.shakeMagnatude += value;
        }
        else
        {
            print("No CameraControl!");
        }
    }

    public void CameraResetLerp(float value)
    {
        //if (isActiveAndEnabled)
        cameraControl.orbitPointLerpRatio = value;
    }

    public void CameraBumpFOV(float bump)
    {        
        cameraControl.thisCamera.fieldOfView += bump;
    }

    public void CameraSetTargetFOV(float FOV)
    {
        if (!cameraControl.overrideFOV)
        cameraControl.camTargetFov = FOV;
    }

    void Update()
    {
        //adjust how the player should appear on screen depending on state.  
        {
            
            if (controller.stateMachine.currentModule.GetType() == typeof(SymGround))
            {
                cameraControl.upResetLerpSpeed = 5;
                characterRelativeOffset = headHeight;
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

                cameraControl.upResetLerpSpeed = 1;
                characterRelativeOffset = Vector3.zero;               
            }

            camZoomDistanceMax = Mathf.LerpUnclamped(5, 10, controller.rbVelocityMagnitude * .01f);//Mathf.Lerp(camZoomDistanceMax, ), Time.deltaTime);

            if (cameraControl != null)
            {


                //Shift away from the center of mass, towards the head of our character.
                {
                    cameraControl.characterRelativeOffset = Vector3.Slerp(cameraControl.characterRelativeOffset, characterRelativeOffset, Time.deltaTime * 2);
                }

                //Adjust Zoom parameters
                {
                    cameraControl.zoomDistanceMin = Mathf.Lerp(cameraControl.zoomDistanceMin, camZoomDistanceMin, Time.deltaTime);
                    cameraControl.zoomDistanceMax = Mathf.Lerp(cameraControl.zoomDistanceMax, camZoomDistanceMax, Time.deltaTime);
                }
            }

        }

        //Camera manual up vector set when wall walking
        {
            cameraControl.manualUpVectorScaler = controller.wallRun;
            cameraControl.manualUpVector = -controller.gravity.normalized;
            
            //Test
            //cameraControl.manualUpVector = Vector3.Lerp(-controller.gravity.normalized, -controller.gravityRaw.normalized, .5f);
        }

        //Camera turns with character while moving on curving surfaces
        {
            cameraControl.surfaceBasedRotationScalar = controller.wallRun;
            cameraControl.characterUpVectorPrevious = cameraControl.characterUpVector;

            if (controller.grounded)
            {
                if (Vector3.Dot(cameraControl.characterUpVectorPrevious, controller.surfaceNormal) > .7f)
                {
                    //cameraControl.characterUpVector = Vector3.Lerp(cameraControl.characterUpVector, transform.up, .1f);
                    //cameraControl.characterUpVector = transform.up;
                    //cameraControl.characterUpVector = -behaviour.gravity.normalized;
                    cameraControl.characterUpVector = Vector3.Lerp(cameraControl.characterUpVector, -controller.gravity.normalized, .1f);
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
        {
            if (!controller.grounded && controller.flightEnabled)
            {
                CauseCameraShake(.025f * Time.deltaTime * Mathf.InverseLerp(20, 60, Mathf.Abs(controller.rbVelocityMagnitude * Vector3.Dot(controller.rbVelocityNormalized, controller.gravityRaw.normalized))));                                                                   
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {      
        CauseCameraShake(controller.rbVelocityMagnitude / 10 );
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
            cameraControl.cameraDirectionResetPosition = bounceFutureCheck.point;
            cameraControl.directionResetLerpValue = .1f;
        }
        else
        {
            cameraControl.cameraDirectionResetPosition = hitPoint + reflect * 1000;
            cameraControl.directionResetLerpValue = .1f;
        }
     
    }

    private void OnCollisionEnter(Collision collision)
    {
        float speedAlongContactNormal = controller.rbVelocityMagnitude * Vector3.Dot(controller.rbVelocityNormalized, -controller.surfaceNormal);

        //Visual Effects
        if (speedAlongContactNormal > 20)
        {            
            CauseCameraShake(speedAlongContactNormal * .1f);     
        }
    }




}
