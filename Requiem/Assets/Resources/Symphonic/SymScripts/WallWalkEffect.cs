using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class WallWalkEffect : MonoBehaviour
{
    public ParticleSystem ps;   
    public SymphonicBehaviour behaviour;
    public Transform anchor;
    public bool burst = false;
    public bool hasFired = false;
    public bool systemActive = true;
    private void Start()
    {
        transform.parent = null;
    }

    void Update()
    {
        if (anchor != null)
        {
            //if (EditorApplication.isPlaying)
            {
                var main = ps.main;
                var shape = ps.shape;
                var emission = ps.emission;

                if (behaviour.energyLevel == 1)
                {
                    systemActive = true;
                    if (burst && hasFired)
                        systemActive = false;
                }
                else
                {
                    hasFired = false;
                    systemActive = false;
                }

                if (systemActive)
                {
                    transform.position = anchor.position;
                    if (!ps.isPlaying)
                    {
                        ps.Play();
                        hasFired = true;
                    }                   
                }
                else
                {
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            //else
            //{
            //    transform.position = anchor.position;
            //}
        }
        
    }

}
