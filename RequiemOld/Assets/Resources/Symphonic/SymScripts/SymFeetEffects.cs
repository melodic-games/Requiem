using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SymFeetEffects : MonoBehaviour
{
    public ParticleSystem ps;
    public SymBehaviour behaviour;
    public Transform anchor;
    public bool systemActive = true;

    private void Start()
    {
        transform.parent = null;
    }

    private void Update()
    {
        //if (!EditorApplication.isPlaying)
            //transform.position = anchor.position + Vector3.up * .1f;
    }

    void FixedUpdate()
    {
        if (anchor != null)
        {
            transform.position = anchor.position + Vector3.up * .1f;
            transform.rotation = behaviour.transform.rotation;

            
            {
                var main = ps.main;
                var shape = ps.shape;
                var emission = ps.emission;

                float dot = Vector3.Dot(behaviour.transform.forward, behaviour.rb.velocity);

                if (Mathf.Sign(dot) == -1)
                {
                    main.startSpeed = -15;
                    shape.angle = 45;
                }
                else
                {
                    main.startSpeed = -5;
                    shape.angle = 0;
                }

                if (behaviour.grounded && behaviour.rbVelocityMagnatude > 20)
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
