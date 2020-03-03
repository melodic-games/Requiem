using UnityEngine;
using UnityEditor;

public class SymFeetEffects : MonoBehaviour
{
    public ParticleSystem ps;
    public BaseCharacterController controller;
    public Transform anchor;
    public bool systemActive = true;

    private void Update()
    {
        //if (!EditorApplication.isPlaying)
            //transform.position = anchor.position + Vector3.up * .1f;
    }

    void LateUpdate()
    {
        if (anchor != null)
        {
            transform.position = anchor.position;
            transform.rotation = controller.transform.rotation * Quaternion.Euler(0, 180, 0);

            
            {
                var main = ps.main;
                var shape = ps.shape;
                var emission = ps.emission;

                float dot = Vector3.Dot(controller.transform.forward, controller.rb.velocity);

                if (Mathf.Sign(dot) == -1)
                {
                    main.startSpeed = -15;
                    shape.angle = 45;
                    shape.radius = 0;
                }
                else
                {
                    main.startSpeed = -5;
                    shape.angle = 0;
                    shape.radius = 0.15f;
                }

                if (controller.grounded && controller.rbVelocityMagnitude > 20)
                {
                    systemActive = true;
                }
                else
                {                 
                    systemActive = false;
                }

                if (systemActive)
                {
                    
                    if (!ps.isPlaying)
                    {                       
                        ps.Play();               
                    }                   
                }
                else
                {
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }

            }

        }
        
    }

}
