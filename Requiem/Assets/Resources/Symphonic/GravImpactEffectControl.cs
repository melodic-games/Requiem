using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravImpactEffectControl : MonoBehaviour
{
    public ParticleSystem ps;   
    public SymphonicBehaviour behaviour;
    public bool burst = false;
    public bool hasFired = false;
    public bool enable = true;
    private void Start()
    {
        transform.parent = null;
    }

    void Update()
    {       
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;

        if (behaviour.wallWalkScaler == 1)
        {
            enable = true;
            if (burst && hasFired)
                enabled = false;
        }
        else
        {
            hasFired = false;
            enable = false;
        }
        
        if (enable)
        {
            transform.position = behaviour.transform.position + behaviour.transform.rotation * Vector3.up * -.85f;
            if (!ps.isPlaying)
            {                
                ps.Play();
                hasFired = true;                
            }                       
            transform.rotation = Quaternion.LookRotation(behaviour.groundNormal);
        }
        else
        {                       
            ps.Stop(false,ParticleSystemStopBehavior.StopEmitting);
        }       
        
    }

}
