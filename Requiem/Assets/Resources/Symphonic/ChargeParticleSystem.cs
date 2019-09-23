using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeParticleSystem : MonoBehaviour
{
    public ParticleSystem ps;
    public int state = 0;

    public float charge;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;
        main.maxParticles = 100;
        ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        main.duration = 3;

    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;

        if (state == 1)
        {            
            main.loop = true;
            main.startLifetime = .5f;
            main.startSpeed = -4;
            shape.radius = 2;
            emission.rateOverTime = Mathf.Lerp(0, 40, 1);//33.42f;
            if (!ps.isPlaying) ps.Play();                           
        }
        else if (state == 0)
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);                               
        }

    }
}
