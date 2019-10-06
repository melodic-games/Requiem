using UnityEngine;

public class SymAnimationHandler : MonoBehaviour
{

    private Animator animator;
    private Rigidbody rb;
    public SymBehaviour behaviour;

    private void Start()
    {
        behaviour = GetComponent<SymBehaviour>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Set animator values 
        if (behaviour != null)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", behaviour.rbVelocityMagnatude, 0.1f, Time.deltaTime);
                animator.SetFloat("VerticalSpeed", behaviour.verticalSpeed, 0.1f, Time.deltaTime);
                animator.SetFloat("RotZ", Mathf.Abs(behaviour.localAngularVelocity.z), 0.1f, Time.deltaTime);
                animator.SetFloat("RotX", Mathf.Abs(behaviour.localAngularVelocity.x), 0.1f, Time.deltaTime);
                animator.SetFloat("RotY", Mathf.Abs(behaviour.localAngularVelocity.y), 0.1f, Time.deltaTime);
                animator.SetFloat("Drag", rb.drag / .2f, 0.3f, Time.deltaTime);
                animator.SetFloat("DragDir", Vector3.Dot(behaviour.rbVelocityNormalized, -transform.forward), 0.1f, Time.deltaTime);                
                animator.SetFloat("LandingType", behaviour.landingType);
                animator.SetFloat("GroundForwardSpeed", Mathf.Clamp(behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, transform.forward), -30, 30));
                animator.SetFloat("GroundLateralSpeed", Mathf.Clamp(behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, transform.right), -10, 10));
                animator.SetBool("Grounded", behaviour.grounded);
                animator.SetBool("FlightEnabled", behaviour.flightEnabled);

                if (behaviour.chargingEnergy && behaviour.controlSource.thrustInput == 0 )
                    animator.SetFloat("Charge", 1, 0.1f, Time.deltaTime);
                else
                    animator.SetFloat("Charge", 0, 0.1f, Time.deltaTime);


                if (behaviour.crouching)
                    animator.SetFloat("Crouch", 1, 0.1f, Time.deltaTime);
                else
                    animator.SetFloat("Crouch", 0, 0.1f, Time.deltaTime);

                if (behaviour.grounded)
                    animator.speed = Mathf.InverseLerp(12, 30, behaviour.rbVelocityMagnatude) * 1f + 1;
                else
                    animator.speed = 1;
            }
        }
        else
        {
            print("no behaviour in animation handler");
        }
    }

    
}
