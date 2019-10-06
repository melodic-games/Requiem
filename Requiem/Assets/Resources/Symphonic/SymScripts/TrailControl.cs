using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TrailControl : MonoBehaviour
{

    public TrailRenderer trailRenderer;
    public Transform anchor;
    private Rigidbody rb;
    private float rbVelocityMagnatude;
    bool detach;
    private SymBehaviour behaviour;
    public float widthBase = 0.05f;
    private float velocityScale;
    public float beginTrailSpeed = 30;
    public float trailGrowRange = 30;

    void Start()
    {
        transform.position = anchor.position;
        transform.parent = anchor;
        rb = GetComponent<Rigidbody>();
        behaviour = GetComponent<SymBehaviour>();
    }

    void LateUpdate()
    {
        //if (!EditorApplication.isPlaying)
        //{
        //    if (anchor != null)
        //        transform.position = anchor.position;
        //}
        //else
        {            
            if (behaviour != null)
            {
                rbVelocityMagnatude = behaviour.rbVelocityMagnatude;     
                
                if (Mathf.Abs(behaviour.localAngularVelocity.x) > .2f)
                    detach = true;
                else
                    detach = false;

                SymphonicTrails();
            }            
        }
    }

    void SymphonicTrails()
    {
        
        velocityScale = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, rbVelocityMagnatude);                                                             
            
        trailRenderer.widthMultiplier = widthBase;

        trailRenderer.time = 0.2f;

        if (velocityScale == 0 || detach)
        {                   
            transform.parent = null;
        }
        else
        {
            transform.position = anchor.position;
            transform.parent = anchor;                                        
        }

    }
}
