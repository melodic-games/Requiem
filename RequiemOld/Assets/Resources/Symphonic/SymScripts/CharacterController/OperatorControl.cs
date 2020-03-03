using UnityEngine;


public class OperatorControl : MonoBehaviour
{

    public Transform target = null;
    private GravityManager gM;
    private Animator animator;
    private float lerp = 0;
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 deviation;
    private Vector3 overShoot = Vector3.zero;
    private float deviationScale;
    public float xOffsetRotation = 0;
    public float yOffsetRotation = 0;
    public float offsetDistance = 2;
    private Vector3 localOffset = Vector3.zero;
    [Range(0, 4)]
    public Vector3 worldPosition = Vector3.zero;
    private Vector3 orbitPoint;
    public Vector3 forwardDirection;
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

    public SymBehaviour symBehaviour;

    void Start()
    {
        gM = FindObjectOfType<GravityManager>();
        symBehaviour = FindObjectOfType<SymBehaviour>();

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

    void Update()
    {
               
        if (Time.deltaTime != 0)
        {           

            //Update Physics Values
            {
                velocity = (transform.position - previousPosition) / Time.deltaTime;
                velNorm = velocity.normalized;
                velMag = velocity.magnitude;
                previousPosition = transform.position;
                if (target != null && Time.deltaTime != 0)
                    targetVelocity = (target.transform.position - targetPreviousPosition) / Time.deltaTime;
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
                lerp = Mathf.Lerp(lerp, Mathf.InverseLerp(5, 80, targetVelMag), Time.deltaTime * 1);
                deviationScale = Mathf.Lerp(deviationScale, 3, Time.deltaTime * 2);
                deviation = new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f) * (deviationScale + 1);
            }

            Vector3 gravity;

            if (symBehaviour.grounded)        
            {
                gravity = symBehaviour.gravity;
            }
            else
            { 
                gravity = gM.ReturnGravity(transform, Vector3.zero).normalized;
            }


            //Movement mode behaviour        
            if (target != null)// move to target position with offset, reset worldPosition to current position, find facing directions
            {
                worldPosition = transform.position;
                overShoot = Vector3.Lerp(overShoot, targetVelocity * .1f, Time.deltaTime);
                targetPosition = target.position + localOffset + overShoot;
                forwardDirection = velNorm;
            }

            orbitPoint = Vector3.Lerp(orbitPoint, targetPosition, lerp);

            transform.position = orbitPoint + deviation;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDirection, -gravity), Time.deltaTime * 10);

        }
    }

}

//using UnityEngine;


//public class OperatorControl : MonoBehaviour {

//    public Transform[] targets = null;
//    private GravityManager gM;
//    private Animator animator;
//    public Rigidbody targetRb;
//    public float lerp = 0;
//    public Vector3 goalPosition = Vector3.zero;
//    private Vector3 deviation;
//    public float xOffsetRotation = 0;
//    public float yOffsetRotation = 0;
//    public float offsetDistance = 2;
//    private Vector3 localOffset = Vector3.zero;
//    private Vector3 orbitPoint;
//    Vector3 forwardDirection;
//    private float velMag;
//    private Vector3 previousPosition;

//    private Vector3 targetFacingDirection;
//    private Vector3 targetVelocity;
//    private Vector3 targetVelNorm;
//    private float targetVelMag;
//    private Vector3 targetsPreviousPosition;
//    public Vector3 overShoot;

//    private PIDController pid;
//    [SerializeField]
//    private Vector3 gains = new Vector3(1,0,0);
//    public float error;
//    public bool useRedirect = false;

//    public SymphonicBehaviour symBehaviour;

//    void Start () {       
//        gM = FindObjectOfType<GravityManager>();

//        pid = new PIDController();

//        forwardDirection = transform.forward;

//        float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
//        localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

//        goalPosition = transform.position;       

//        animator = GetComponent<Animator>();

//        orbitPoint = transform.position;
//    }

//    void Update()
//    {

//        Vector3 posAverage = Vector3.zero;

//        //Update Physics Values        
//        Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
//        Vector3 velNorm = velocity.normalized;
//        velMag = velocity.magnitude;
//        previousPosition = transform.position;

//        if (targets != null)
//        //Get the average position between targets
//        {
//            int count = 0;
//            foreach (Transform t in targets)
//            {
//                posAverage += t.position;
//                count += 1;
//            }
//            posAverage /= count;
//        }

//        if (targets != null)
//        targetsPreviousPosition = posAverage;            


//        //Injected Deviation                                
//        deviation = new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f) * 2;
//        overShoot = Vector3.Lerp(overShoot, targetRb.velocity / 10, Time.deltaTime);

//        goalPosition = posAverage + deviation + overShoot;
//        Debug.DrawLine(transform.position, goalPosition);

//        Vector3 dir = (goalPosition - transform.position).normalized;

//        Vector3 error = goalPosition - transform.position;

//        Vector3 factor = pid.GetFactorFromPIDController(gains, error);

//        transform.position += factor;

//        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velNorm, -gM.ReturnGravity(transform).normalized), Time.deltaTime * 10);

//    }

//    private void OnCollisionEnter(Collision collision)
//    {

//    }

//}
