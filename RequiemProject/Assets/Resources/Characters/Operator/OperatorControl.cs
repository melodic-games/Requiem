using UnityEngine;

[SelectionBase]
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
    //public float xOffsetRotation = 0;
    //public float yOffsetRotation = 0;
    //public float offsetDistance = 2;
    //private Vector3 localOffset = Vector3.zero;
    [Range(0, 4)]

    private Vector3 orbitPoint;
    public Vector3 forwardDirection;
    public Vector3 upDirection;

    RaycastHit hit;
    Vector3 correction = Vector3.zero;

    private Vector3 velocity;
    private Vector3 velNorm;
    public float velMag;
    private Vector3 previousPosition;

    private Vector3 targetFacingDirection;
    private Vector3 targetVelocity;
    private Vector3 targetVelNorm;
    private float targetVelMag;
    private Vector3 targetPreviousPosition;
 
    public BaseCharacterController controller;

    void Start()
    {
        gM = FindObjectOfType<GravityManager>();     

        forwardDirection = transform.forward;
        upDirection = transform.up;

        //float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
        //localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;

        targetPosition = transform.position;

        animator = GetComponent<Animator>();
    }

    private void JumpToTarget()
    {
        //transform.position = target.position + target.TransformVector(localOffset);
    }

    //private void OnDrawGizmos()
    //{            
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(hit.point + hit.normal * 2, 2);        
    //}

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
                //float cosx = Mathf.Cos(xOffsetRotation * Mathf.Deg2Rad);
                //localOffset = new Vector3(Mathf.Sin(yOffsetRotation * Mathf.Deg2Rad) * cosx, Mathf.Sin(xOffsetRotation * Mathf.Deg2Rad), Mathf.Cos(yOffsetRotation * Mathf.Deg2Rad) * cosx) * offsetDistance;
            }

            Vector3 gravity;

            if (controller.grounded)        
            {
                gravity = controller.gravity;
            }
            else
            { 
                gravity = gM.ReturnGravity(transform, Vector3.zero).normalized;
            }

            //Deviation and lerp
            {
                lerp = Mathf.Lerp(lerp, Mathf.InverseLerp(5, 80, targetVelMag), Time.deltaTime * 1);
                deviationScale = Mathf.Lerp(deviationScale, 3, Time.deltaTime * 2);
                deviation = new Vector3(Mathf.Sin(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f, Mathf.Cos(Time.time * 1) * 0.2f) * (deviationScale + 1);                                              
            }

            if (target != null)// move to target position with offset
            {
                overShoot = Vector3.Lerp(overShoot, targetVelocity * .1f, Time.deltaTime);

                float value = (Mathf.Sin(Time.time) + 1) * .5f;
                targetPosition = target.position - (gravity * .1f) + overShoot + deviation;// Vector3.Lerp(Vector3.zero,Vector3.up,value);
               
                //Collission Correction
                {
                
                    //Vector3 dir = (targetPosition + deviation - target.position).normalized;
                    Vector3 dir = (targetPosition - (target.position - (gravity * .1f))).normalized;

                    int layerMask = ~((1 << 8) | (1 << 2) | (1 << 10));

                    float buffer = 1;

                    //float dist = (targetPosition + deviation - target.position).magnitude + buffer;
                    float dist = (targetPosition - target.position).magnitude + buffer;

                    Debug.DrawRay(target.position - (gravity * .1f), dir * dist);

                    //if (Physics.SphereCast(target.position, 1 , dir, out hit, dist, layerMask)) 
                    if (Physics.Raycast(target.position - (gravity * .1f), dir, out hit, dist, layerMask))
                    {                  
                        
                        Plane plane = new Plane(hit.normal,hit.point);
                           
                        float scale = Mathf.Clamp(Mathf.Clamp01(plane.GetDistanceToPoint(targetPosition)), 0, buffer) / buffer;

                        correction = hit.point - dir * Mathf.Lerp(buffer * .25f, buffer, scale);

                        correction = hit.point - dir;

                        targetPosition = correction;
                      
                        //Debug.DrawLine(dir * dist, correction, Color.red);
                        Debug.DrawLine(target.position - (gravity * .1f), hit.point, Color.red);              

                    }
                    else
                    {
                        
                        correction = Vector3.zero;
                    }
                }

                forwardDirection = velNorm;
                upDirection = -gravity;
            }

            orbitPoint = Vector3.Lerp(orbitPoint, targetPosition, lerp);

            if (correction == Vector3.zero)
            {
                Debug.DrawLine(target.position, target.position - (gravity * .1f), Color.green);
                Debug.DrawLine(target.position - (gravity * .1f), target.position - (gravity * .1f) + overShoot, Color.cyan);
                Debug.DrawLine(target.position - (gravity * .1f) + overShoot, target.position -(gravity * .1f) + overShoot + deviation, Color.blue);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, velMag + 1 * Time.deltaTime);// + deviation;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDirection, upDirection), Time.deltaTime * 10);

        }
    }

}