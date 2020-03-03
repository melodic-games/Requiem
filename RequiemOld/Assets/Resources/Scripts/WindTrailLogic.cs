using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(SphereCollider))]

public class WindTrailLogic : MonoBehaviour {

    public float trailWidth = 0.05f;
    private float trailScalar = 0;
    private float startTime = 0;
    private TrailRenderer trail;

    private bool insideTrigger = false;

    private SphereCollider trigger;

    public Transform effector;
    public Vector3 effectorRelativeOffset = Vector3.zero;
    public float effectorMinRadius = 5;
    public float effectorMaxRadius = 20;

    public float speed = 10;
    public float duration = 5;

    public float temp;

	void Start () {
        trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = effectorMaxRadius;

        trail = GetComponent<TrailRenderer>();
        startTime = Time.time + duration/2;
        trail.time = 1;
        //speed += Random.Range(-3, 3);
    }

    private void Update()
    {

        trigger.radius = effectorMaxRadius + 2;

        if (Time.time > startTime && Time.time < startTime + duration)
        {
            trailScalar = Mathf.Lerp(trailScalar, 1, Time.deltaTime * 4);
        }
        else
        {
            trailScalar = Mathf.Lerp(trailScalar, 0, Time.deltaTime * 4);
        }      

        trail.widthMultiplier = trailWidth * trailScalar;

        if (Time.time > startTime + duration * 2)
        {
            Destroy(gameObject);
        }
    }
    
    void FixedUpdate ()
    {
        
        if(insideTrigger && effector != null)
        {
            trail.endColor = Color.red;
            trail.startColor = Color.red;

            float effectorStrenght = Mathf.Lerp(1, 0, Mathf.Clamp01(((transform.position - effector.position).magnitude - effectorMinRadius) / effectorMaxRadius));

            temp = effectorStrenght;

            Vector3 displacement;
            
            displacement = transform.forward * speed * Time.deltaTime;            

            transform.position = Vector3.Lerp(transform.position, effector.TransformVector(effectorRelativeOffset) + effector.position, effectorStrenght) + displacement;

            //transform.position = Vector3.Lerp(effector.position, transform.position, effectorStrenght * .01f * Time.deltaTime);

            transform.rotation = Quaternion.Lerp(transform.rotation, effector.rotation, effectorStrenght);

            effectorRelativeOffset = effector.InverseTransformVector(transform.position - effector.position);
        }
        else
        {
            trail.endColor = Color.white;
            trail.startColor = Color.white;

            transform.position += transform.forward * speed * Time.deltaTime;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == effector)        
        effectorRelativeOffset = effector.InverseTransformVector(transform.position - effector.position);
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.transform == effector)
        {
            insideTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == effector)
        {
            insideTrigger = false;
        }
    }

}
