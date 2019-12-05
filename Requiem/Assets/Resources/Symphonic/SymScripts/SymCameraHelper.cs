using UnityEngine;

public class SymCameraHelper : MonoBehaviour
{
    private SymBehaviour behaviour;
    private CameraControl cameraControl;
    public float camZoomDistanceMin = 4;
    public float camZoomDistanceMax = 10;
    public float manualUpVectorScaler = 0;
    private Vector3 camLocalOffset = Vector3.zero;
    public Vector3 headHeight = new Vector3(0, .7f, 0);

    void Start()
    {
        behaviour = GetComponent<SymBehaviour>();
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

    public void CameraSetLerp(float lerpValue)
    {        
        cameraControl.lerpSpeed = Mathf.Lerp(0, cameraControl.lerpSpeed, lerpValue);
    }

    void Update()
    {
        //adjust how the player should appear on screen depending on state.  
        {
            
            if (behaviour.stateMachine.currentModule.GetType() == typeof(SymGroundModule))
            {
                cameraControl.upResetLerp = 1;
                camLocalOffset = headHeight;
                camZoomDistanceMin = 5;

                if (behaviour.crouching && behaviour.rbVelocityMagnatude < 20 && behaviour.grounded)
                {
                    camZoomDistanceMin = 4;
                    camLocalOffset = Vector3.zero;
                }
            
                camZoomDistanceMax = Mathf.Lerp(camZoomDistanceMax, Mathf.Lerp(5, 15, Mathf.InverseLerp(35, 40, behaviour.rbVelocityMagnatude)), Time.deltaTime);        
            }
            
            if (behaviour.stateMachine.currentModule.GetType() == typeof(SymFlightModule))
            {
                cameraControl.upResetLerp = 1;
                camLocalOffset = Vector3.zero;
                
                if (behaviour.chargingEnergy)
                {
                    camZoomDistanceMin = 2;
                    camZoomDistanceMax = 5;
                }
                else
                {
                    camZoomDistanceMax = 10;
                    camZoomDistanceMin = 4;                
                }
            }

            if (cameraControl != null)
            {
                //Recover LerpSpeed if its lowered
                {
                    cameraControl.lerpSpeed = Mathf.Min(cameraControl.lerpSpeed + Time.deltaTime * 10, 20); 
                }

                //Shift away from the center of mass, towards the head of our character.
                {
                    cameraControl.localOffset = Vector3.Slerp(cameraControl.localOffset, camLocalOffset, Time.deltaTime * 2);
                }

                //Adjust Zoom parameters
                {
                    cameraControl.zoomDistanceMin = Mathf.Lerp(cameraControl.zoomDistanceMin, camZoomDistanceMin, Time.deltaTime);
                    cameraControl.zoomDistanceMax = Mathf.Lerp(cameraControl.zoomDistanceMax, camZoomDistanceMax, Time.deltaTime);
                }
            }

        }

        //Camera follows upvector curving        
        {
            cameraControl.curveRotationScalar = behaviour.wallRun;

            if (behaviour.grounded && Vector3.Dot(cameraControl.characterUpVectorPrevious, behaviour.surfaceNormal) > .7f)
            {
                cameraControl.characterUpVectorPrevious = cameraControl.characterUpVector;
                cameraControl.characterUpVector = Vector3.Lerp(cameraControl.characterUpVector, transform.up, .1f);
            }
            else //reset values
            {
                cameraControl.characterUpVector = transform.up;
                cameraControl.characterUpVectorPrevious = transform.up;
            }
        }


        //CameraShake from turbulence
        {
            if (!behaviour.grounded && behaviour.flightEnabled)
            {
                CauseCameraShake(.025f * Mathf.InverseLerp(20, 60, Mathf.Abs(behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, behaviour.gravityRaw.normalized))));                                                                   
            }
        }

        //Camera upvector modification from wall walking
        {
            cameraControl.manualUpVectorScaler = manualUpVectorScaler;
            cameraControl.manualUpVector = -behaviour.gravity.normalized;
        }       

    }

    private void OnTriggerEnter(Collider other)
    {
        if (behaviour != null)
            CauseCameraShake(behaviour.rbVelocityMagnatude / 10 );
    }

    public void CameraBounce(Collision collision)
    { 
                    
        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 hitNormal = collision.GetContact(0).normal;  
        
        print("CameraBounce");

        //Find Reflection Vector
        Vector3 reflect = Vector3.Reflect(behaviour.rbVelocityNormalized, hitNormal);

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
        float speedAlongContactNormal = behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, -behaviour.surfaceNormal);

        //Visual Effects
        if (speedAlongContactNormal > 20)
        {            
            CauseCameraShake(speedAlongContactNormal * .1f);     
        }
    }




}
