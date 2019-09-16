using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravImpactEffectControl : MonoBehaviour
{
    public ParticleSystem ps;   
    public SymphonicBehaviour behaviour;

    private void Start()
    {
        transform.parent = null;
    }

    void Update()
    {       
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;

        if (behaviour.spiderWalkScaler == 1)
        {
            transform.position = behaviour.transform.position + behaviour.transform.rotation * Vector3.up * -.97f;
            if (!ps.isPlaying)
            {                
                ps.Play();
            }                       
            transform.rotation = Quaternion.LookRotation(behaviour.groundNormal);
        }
        else
        {                       
            ps.Stop(false,ParticleSystemStopBehavior.StopEmitting);
        }       
        
    }

}
