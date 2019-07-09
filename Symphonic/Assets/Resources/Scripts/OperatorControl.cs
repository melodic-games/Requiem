using UnityEngine;


public class OperatorControl : MonoBehaviour {

    public GameObject target = null;
    private bool targetFindAtempted = false;

    public int movementMode = 0; //0: move to target position with offset, 1: move to world position with offset.
    public float lerpSpeed = 20;

    public Vector3 targetPosition = Vector3.zero;
    
    public float xOffsetRotation = 0;
    public float yOffsetRotation = 0;
    public float offsetDistance = 2;

    private Vector3 localOffset = Vector3.zero;

    [Range(0, 1)]
    public float deviationScale = 1;

    public Vector3 worldPosition = Vector3.zero;  
    
    private Vector3 previousPositon;
    private Vector3 velNorm;
    private float velMag;

    public TrailRenderer trail;
    public float trailtime = 0.6f;
   
    void Start () {

        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        targetPosition = transform.position;
        worldPosition = transform.position;

        if (target == null)
            target = GameObject.FindGameObjectsWithTag("Player")[0];
                 
            transform.position = target.transform.position + target.transform.TransformVector(localOffset);
        
        previousPositon = transform.position;   
    }

    private void Update()
    {
        trail.time = trailtime;

        if (target == null && !targetFindAtempted)
        {
            target = GameObject.FindGameObjectsWithTag("Player")[0];
            targetFindAtempted = true;
        }
        else
        {
            targetFindAtempted = false;
        }
    }

    void LateUpdate ()
    {    

        velNorm = (transform.position - previousPositon).normalized;
        velMag = (transform.position - previousPositon).magnitude / Time.deltaTime;

        previousPositon = transform.position;
        
        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        Vector3 deviation = Vector3.Lerp(Vector3.zero, new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f), deviationScale);

        Vector3 forwardDirection = transform.forward;
        Vector3 upDirection = Vector3.up;

        if (movementMode == 0 && target != null)//0: move to target position with offset
        {  
            worldPosition = transform.position;                                          

            targetPosition = target.transform.position + localOffset;

            forwardDirection = Vector3.Lerp(target.transform.forward, velNorm, velMag);
            upDirection = Vector3.up;                       
        }
        else //1: move to world position with offset.
        {
            targetPosition = worldPosition + localOffset;
            if (target != null)
            {
                // look towards target and velocity direction
                forwardDirection = Vector3.Lerp((target.transform.position - transform.position).normalized, velNorm, velMag); 
                upDirection = target.transform.up;
            }
            else
            {
                // look towards velocity direction
                forwardDirection = Vector3.Lerp(transform.forward, velNorm, velMag);
                upDirection = transform.up;
            }
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition + deviation, Time.deltaTime * lerpSpeed);

        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, upDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        
    }


}
