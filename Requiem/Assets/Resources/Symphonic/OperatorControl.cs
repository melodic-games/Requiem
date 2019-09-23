using UnityEngine;


public class OperatorControl : MonoBehaviour {

    public Transform target = null;
    private GravityManager gM;    
    private Animator animator;
    private float lerp = 0;   
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 deviation;
    private float deviationScale;
    public float xOffsetRotation = 0;
    public float yOffsetRotation = 0;
    public float offsetDistance = 2;
    private Vector3 localOffset = Vector3.zero;
    [Range(0, 4)]
    public Vector3 worldPosition = Vector3.zero;
    private Vector3 orbitPoint;
    Vector3 forwardDirection;
    private Vector3 velocity;
    private Vector3 velNorm;
    private float velMag;
    private Vector3 previousPosition;

    private Vector3 targetFacingDirection;
    private Vector3 targetVelocity;
    private Vector3 targetVelNorm;
    private float targetVelMag;
    private Vector3 targetPreviousPosition;

    public TrailRenderer trailRenderer;

    public SymphonicBehaviour symBehaviour;

    void Start () {       
        gM = FindObjectOfType<GravityManager>();

        forwardDirection = transform.forward;

        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        targetPosition = transform.position;
        worldPosition = transform.position;
      
        animator = GetComponent<Animator>();
        
        orbitPoint = transform.position;
    }

    private void JumpToTarget()
    {
        transform.position = target.position + target.TransformVector(localOffset);
    }

    private void TrailUpdate()
    {
      
        //Trail 
        {
            float widthMultiplier;

            float beginTrailSpeed = 40;
            float trailGrowRange = 30;

            float velScale = Mathf.Clamp01((velMag - beginTrailSpeed) / trailGrowRange);

            float widthScale;
            float angxDisable;

            if (symBehaviour != null)
            {
                widthScale = .5f + (Mathf.Clamp01(Mathf.Abs(symBehaviour.localAngularVelocity.y) / 20) * 2);
                angxDisable = 1 - Mathf.Clamp01(Mathf.Abs(symBehaviour.localAngularVelocity.x / 5));// if pitch speed is 0.2 or greater disable width.             
            }
            else
            {
                widthScale = .5f;
                angxDisable = 1;            
            }

            widthMultiplier = velScale * angxDisable * widthScale;
            
            widthMultiplier *= Mathf.Clamp01((Vector3.Dot(velNorm, transform.forward) * 2) - 1);

            if (trailRenderer != null)
            {
                trailRenderer.widthMultiplier = 0.05f * widthMultiplier;
                trailRenderer.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

        }   

    }

    void Update()
    {

        TrailUpdate();       
        
        //Update Physics Values
        {
            velocity = (transform.position - previousPosition) / Time.deltaTime;
            velNorm = velocity.normalized;
            velMag = velocity.magnitude;
            previousPosition = transform.position;
            if(target != null)
            targetVelocity = (target.transform.position - targetPreviousPosition)/ Time.deltaTime;            
            targetVelMag = targetVelocity.magnitude;
            targetVelNorm = targetVelocity.normalized;    
            if (target != null)
            targetPreviousPosition = target.transform.position;

            targetFacingDirection = Vector3.Lerp(targetFacingDirection, targetVelocity.normalized, targetVelMag);
        }



        //LocalOffset
        {
            float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
            localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;
        }

        //Deviation and lerp
        {
            lerp = Mathf.Lerp(lerp, Mathf.InverseLerp(2, 80, targetVelMag), Time.deltaTime * 1);
            deviationScale = Mathf.Lerp(deviationScale, 3, Time.deltaTime * 2);          
            deviation = new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f) * (deviationScale + 1);        
        }

        Vector3 gravity = gM.ReturnGravity(transform).normalized;

        //Movement mode behaviour        
        if (target != null)// move to target position with offset, reset worldPosition to current position, find facing directions
        {
            worldPosition = transform.position;
            targetPosition = target.position + localOffset;
            forwardDirection = velNorm;// Vector3.Lerp(target.forward, velNorm,1);                                          
        }
           
        orbitPoint = Vector3.Lerp(orbitPoint, targetPosition, lerp);
       // orbitPoint = Vector3.SmoothDamp(orbitPoint, targetPosition, ref velocity, .3f, 200);

        transform.position = orbitPoint + deviation;              

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDirection, -gravity), Time.deltaTime * 10); 

    }


}
