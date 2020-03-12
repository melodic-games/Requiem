using UnityEngine;

public class CharacterAnimationHandler : MonoBehaviour
{

    public Animator animator;
    private Rigidbody rb;
    public BaseCharacterController controller;

    private void Start()
    {
        controller = GetComponent<BaseCharacterController>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        //Debug.DrawRay(transform.position, behaviour.rbVelocityNormalized, Color.black);
        //Debug.DrawRay(transform.position, transform.right, Color.red);
        //Debug.DrawRay(transform.position, transform.forward, Color.blue);
    }

    void Update()
    {
        //Set animator values 
        if (controller != null) 
        {
            if (animator != null)
            {                                
                animator.SetFloat("Speed", controller.rbVelocityMagnitude, 0.1f, Time.deltaTime);
                animator.SetFloat("VerticalSpeed", controller.rbVelocityMagnitude * Vector3.Dot(controller.rbVelocityNormalized, -controller.gravity.normalized), 0.1f, Time.deltaTime);
                animator.SetFloat("RotZ", Mathf.Abs(controller.localAngularVelocity.z), 0.1f, Time.deltaTime);
                animator.SetFloat("RotX", Mathf.Abs(controller.localAngularVelocity.x), 0.1f, Time.deltaTime);
                animator.SetFloat("RotY", Mathf.Abs(controller.localAngularVelocity.y), 0.1f, Time.deltaTime);
                animator.SetFloat("Drag", rb.drag / .2f, 0.3f, Time.deltaTime);
                animator.SetFloat("DragDir", Vector3.Dot(controller.rbVelocityNormalized, -transform.forward), 0.1f, Time.deltaTime);                
                animator.SetFloat("LandingType", controller.landingType);                
                animator.SetFloat("GroundForwardSpeed", Mathf.Clamp(controller.rbVelocityMagnitude * Vector3.Dot(controller.rbVelocityNormalized, transform.forward), -30, 30), 0.1f, Time.deltaTime);
                animator.SetFloat("GroundLateralSpeed", controller.rbVelocityMagnitude * -Vector3.Dot(controller.rbVelocityNormalized, transform.right), 0.1f, Time.deltaTime);
                animator.SetBool("Grounded", controller.grounded);
                animator.SetBool("FlightEnabled", controller.flightEnabled);

                if (controller.chargingEnergy && controller.controlSource.thrustInput == 0 )
                    animator.SetFloat("Charge", 1, 0.1f, Time.deltaTime);
                else
                    animator.SetFloat("Charge", 0, 0.1f, Time.deltaTime);

                if (controller.crouching && controller.moveDirectionMag == 0)
                    animator.SetFloat("Crouch", 1, 0.1f, Time.deltaTime);
                else
                    animator.SetFloat("Crouch", 0, 0.1f, Time.deltaTime);

                if (controller.grounded)
                    animator.speed = Mathf.InverseLerp(controller.groundRunSpeed, controller.groundTopSpeed, controller.rbVelocityMagnitude) * 1.5f + 1;
                else
                    animator.speed = 1;
            }
            else
            {
                print("no animator in animation handler");
            }
        }
        else
        {
            print("no behaviour in animation handler");
        }
    }

    
}
