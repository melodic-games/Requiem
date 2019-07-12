using UnityEngine;


public class OperatorControl : MonoBehaviour {

    public GameObject target = null;
    public Rigidbody targetRb;
    private bool targetFindAtempted = false;
    private Animator animator;
    private Rigidbody rb;
    public int movementMode = 0; //0: move to target position with offset, 1: move to world position with offset.
    public Vector3 targetPosition = Vector3.zero;
    private Vector3 deviation;
    private float deviationScale;
    public float xOffsetRotation = 0;
    public float yOffsetRotation = 0;
    public float offsetDistance = 2;
    private Vector3 localOffset = Vector3.zero;
    [Range(0, 4)]
    public float distancePowerMutiplier = 2;
    public Vector3 worldPosition = Vector3.zero;  
    
    private Vector3 velNorm;
    private float velMag;
    private TrailRenderer trail;
    private float trailtime = 0.6f;
    private float accelDir;


    void Start () {

        trail = GetComponent<TrailRenderer>();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        targetPosition = transform.position;
        worldPosition = transform.position;

        if (target == null)
        target = GameObject.FindGameObjectsWithTag("Player")[0];
        targetRb = target.GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();

    }

    private void JumpToTarget()
    {
        transform.position = target.transform.position + target.transform.TransformVector(localOffset);
    }

    private void Update()
    {
        trail.time = trailtime;

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

    void FixedUpdate()
    {
        Vector3 forwardDirection;
        Vector3 upDirection;

        //Update Physics Optimazation Values
        if(rb != null)
        {
            velNorm = rb.velocity.normalized;
            velMag = rb.velocity.magnitude;
        }

        //Determin target position and rotation info
        {
            float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
            localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;
            float t;
            if (targetRb != null)
            {
                t = Mathf.InverseLerp(5,30,targetRb.velocity.magnitude);
            }
            else
            {
                t = 0.5f;
            }

            deviationScale = Mathf.Lerp(deviationScale, Mathf.Lerp(1, 8, t),Time.deltaTime * 3);

            deviation = Vector3.LerpUnclamped(Vector3.zero, new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f), deviationScale);

            if (movementMode == 0 && target != null)// move to target position with offset
            {
                worldPosition = transform.position;

                targetPosition = target.transform.position + localOffset;

                forwardDirection = Vector3.Lerp(target.transform.forward, velNorm, velMag);
                upDirection = Vector3.up;
            }
            else // move to world position with offset.
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

        if (rb != null)
        {         

            Vector3 dirToTarget = ((targetPosition + deviation) - transform.position).normalized;

             float goalSpeedTowardsTarget = Mathf.Pow(Vector3.Distance(transform.position, targetPosition + deviation),distancePowerMutiplier);

            float signedSpeedTowardsTarget = velMag * Vector3.Dot(velNorm, dirToTarget);

            float speed = (goalSpeedTowardsTarget - signedSpeedTowardsTarget) * 40;

            accelDir = Mathf.Clamp(speed,-1,1);
                       
            rb.AddForce(dirToTarget * speed);


            {
                //Lift Force
                Vector3 forceVector = Vector3.Cross(dirToTarget, -velNorm).normalized;
                Vector3 crossVector = Vector3.Cross(forceVector, dirToTarget);
                Vector3 force = crossVector * velMag * Vector3.Dot(crossVector, -velNorm) * 5;
                rb.AddForce(force);
            }          
            
        }

       // transform.position = targetPosition + deviation;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDirection, upDirection), Time.deltaTime * 10);

        //Set animator values 
        if (animator != null)
        {
            animator.SetFloat("AccelDir", accelDir, 0.3f, Time.deltaTime);
        }

    }


}
