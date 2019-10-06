using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SymWallWalkEffect : MonoBehaviour
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

            
            {
                var main = ps.main;
                var shape = ps.shape;
                var emission = ps.emission;

                if (behaviour.energyLevel == 1 && behaviour.grounded)
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
