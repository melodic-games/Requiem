using UnityEngine;


public class OperatorControl : MonoBehaviour {

    public GameObject target = null;
    public Rigidbody targetRb;
    private bool targetFindAtempted = false;
    private Animator animator;
    private float lerp = 0;
    //private Rigidbody rb;
    public int movementMode = 0; //0: move to target position with offset, 1: move to world position with offset.
    public Vector3 targetPosition = Vector3.zero;
    private Vector3 deviation;
    private float deviationScale;
    public float xOffsetRotation = 0;
    public float yOffsetRotation = 0;
    public float offsetDistance = 2;
    private Vector3 localOffset = Vector3.zero;
    [Range(0, 4)]
    private float distancePowerMutiplier = 2;
    public Vector3 worldPosition = Vector3.zero;
    private Vector3 orbitPoint;
    private Vector3 velNorm;
    private float velMag;
    private Vector3 previousPosition;

    public TrailRenderer trailRenderer1;
    public TrailRenderer trailRenderer2;
    public TrailRenderer trailRenderer3;
    public TrailRenderer trailRenderer4;

    private float accelDir;

    public SymphonicBehaviour symBehaviour;

    void Start () {

       

        //rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;

        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        targetPosition = transform.position;
        worldPosition = transform.position;

        if (target == null)
        target = GameObject.FindGameObjectsWithTag("Player")[0];
        targetRb = target.GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();

        previousPosition = transform.position;
        orbitPoint = transform.position;

    }

    private void JumpToTarget()
    {
        transform.position = target.transform.position + target.transform.TransformVector(localOffset);
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

            if (trailRenderer1 != null)
            {
                trailRenderer1.widthMultiplier = 0.05f * widthMultiplier;
                trailRenderer1.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

            if (trailRenderer2 != null)
            {
                trailRenderer2.widthMultiplier = 0.05f * widthMultiplier;
                trailRenderer2.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

            if (trailRenderer3 != null)
            {
                trailRenderer3.widthMultiplier = 0.05f * widthMultiplier;
                trailRenderer3.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

            if (trailRenderer4 != null)
            {
                trailRenderer4.widthMultiplier = 0.05f * widthMultiplier;
                trailRenderer4.time = widthMultiplier == 0 ? 0 : 0.2f;
            }

        }   

    }

    void Update()
    {

        {
            if (target == null && !targetFindAtempted)
            {
                target = GameObject.FindGameObjectsWithTag("Player")[0];
                targetRb = target.GetComponent<Rigidbody>();
                targetFindAtempted = true;
            }
            else
            {
                targetFindAtempted = false;
            }
        }

        TrailUpdate();


        Vector3 forwardDirection;
        Vector3 upDirection;
        
        //Update Physics Optimazation Values
        {
            Vector3 velocity = (transform.position - previousPosition)/ Time.deltaTime;
            velNorm = velocity.normalized;
            velMag = velocity.magnitude;
            previousPosition = transform.position;
        }

        //LocalOffset
        {
            float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
            localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;
        }

        //Deviation and lerp
        { 
            if (targetRb != null)
            {            
                lerp = Mathf.Lerp(lerp, Mathf.InverseLerp(2, 80, targetRb.velocity.magnitude), Time.deltaTime * 1);
                deviationScale = Mathf.Lerp(deviationScale, Mathf.Lerp(1, 10, Mathf.InverseLerp(5, 30, targetRb.velocity.magnitude)), Time.deltaTime * 2);
            }
            else
            {
                lerp = Time.deltaTime;
                deviationScale = Mathf.Lerp(deviationScale, 3, Time.deltaTime * 2);
            }

            deviation = new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f) * (deviationScale + 1);
            //deviation = ((target.transform.right * Mathf.Sin(Time.time * 1) * 0.2f) + (target.transform.forward * Mathf.Cos(Time.time * 1) * 0.2f)) * (deviationScale + 1);
        }

        //Movement mode behaviour
        {
            if (movementMode == 0 && target != null)// move to target position with offset, reset worldPosition to current position, find facing directions
            {
                worldPosition = transform.position;

                targetPosition = target.transform.position + localOffset;

                Vector3 groundForward = Vector3.Lerp(target.transform.forward, velNorm, velMag);
                Vector3 flyForward = Vector3.Lerp(velNorm,target.transform.up, velMag);

                if (symBehaviour != null)
                {                
                    if (symBehaviour.grounded)
                    {
                        forwardDirection = groundForward;
                        upDirection = Vector3.up;
                    }
                    else
                    {
                        forwardDirection = flyForward;
                        upDirection = Vector3.Lerp(Vector3.up, -target.transform.forward, velMag);
                    }
                }
                else
                {
                    forwardDirection = groundForward;
                    upDirection = Vector3.up;
                }               
                
            }
            else //Move to world position with offset, find facing directions
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
        }
                   
        
        orbitPoint = Vector3.Lerp(orbitPoint, targetPosition, lerp);

        transform.position = orbitPoint + deviation;      

        //transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, target.transform.rotation.eulerAngles.z);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDirection, upDirection), Time.deltaTime * 10);

        //Set animator values 
        if (animator != null)
        {
            animator.SetFloat("AccelDir", accelDir, 0.3f, Time.deltaTime);
        }

    }


}
