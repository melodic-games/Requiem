using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GravityManager : MonoBehaviour
{
    private float sceneGravityMag = Physics.gravity.magnitude;

    public static GravityManager gM;   
    public List<GravitySource> gravitySources;
    public List<GravityChain> gravityChains;
    public Vector3 sceneGravity = new Vector3(0, -9.81f, 0);
    public bool sceneGravityUpdatesHere = true;

    public float maxObjectGravity = 10;


    void Awake()
    {

        if (gM == null)
            gM = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);
         
        //Populate Initial Sources
        {
            //gravitySources = new List<GravitySource>();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Environment"))
            {
                float radius = obj.GetComponent<MeshRenderer>().bounds.extents.x;
                
                GravitySource gS = new GravitySource(obj.transform.position, Mathf.Pow(radius,2) * 10, Mathf.Infinity, 0);
                gravitySources.Add(gS);
                gS.trackingTransform = obj.transform;
            }

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Repulsor"))
            {
                float radius = obj.GetComponent<MeshRenderer>().bounds.extents.x;

                GravitySource gS = new GravitySource(obj.transform.position, Mathf.Pow(radius, 2) * -10, Mathf.Infinity, 0);
                gravitySources.Add(gS);
                gS.trackingTransform = obj.transform;
            }

            gravityChains = new List<GravityChain>();
        }
    }

    private void ManualSceneGravity(bool enable)
    {
        if (enable)
            Physics.gravity = sceneGravity;
        else
            sceneGravity = Physics.gravity;
    }

    private void FixedUpdate()
    {
        ManualSceneGravity(sceneGravityUpdatesHere);
        sceneGravityMag = sceneGravity.magnitude;
    }

    private void LateUpdate()
    {
        foreach (GravitySource gs in gravitySources)
        {
            if (gs.trackingTransform != null)
            {
                gs.position = gs.trackingTransform.position;
                gs.rotation = gs.trackingTransform.rotation;
            }

            if (Time.time > gs.creationTime + gs.durationTime + gs.decayTime)
            {
               // gravitySources.Remove(gs);
            }

        }
    }

    public Vector3 ReturnGravity(Transform mytransform, Vector3 positionOffset)
    {

        Vector3 myPosition = mytransform.position + positionOffset;

        Vector3 gravity;

        Vector3 objectGravity = Vector3.zero;//Gravity caused by objects in the world                               

        foreach (GravitySource gs in gravitySources)
        {
            if (gs.trackingTransform != mytransform)
            {

                gs.durationScaler = Mathf.Lerp(1,0,Mathf.InverseLerp(gs.durationTime + gs.creationTime, gs.durationTime + gs.creationTime + gs.decayTime, Time.time));

                //Switch for different gravity types

                switch (gs.type)
                {
                    default:
                        Debug.LogError("No Gravity Type Specified. Reverting to default.");
                        gs.type = GravityType.Spherical;
                        goto case GravityType.Spherical;                       
                    case GravityType.Spherical:  
                        objectGravity += Mathf.Min(gs.mass / Vector3.SqrMagnitude(myPosition - gs.position), maxObjectGravity) * (gs.position - myPosition).normalized * gs.durationScaler;                        
                        break;
                    case GravityType.Planar:
                        break;
                    case GravityType.Radial:
                        
                        Vector3 dir = Vector3.down;

                        if (gs.trackingTransform != null)
                        {
                            RaycastHit hit;
                            Physics.Raycast(transform.position, -gs.trackingTransform.transform.up, out hit, 1000, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore);
                            dir = -hit.normal;
                        }
                                               
                        objectGravity +=  dir * 10 * gs.durationScaler;
                        break;
                    case GravityType.Cylindrical:
                        break;                    
                }                       
            }

            objectGravity = Vector3.ClampMagnitude(objectGravity, 10);
        }

        //Cancel out Scene gravity when object gravity magnitude is the same size or bigger.       
        
            float disable = 1;
        if (sceneGravityMag > 0)
            disable = Mathf.Max(0, (sceneGravityMag - objectGravity.magnitude) / sceneGravityMag);

        return gravity = Physics.gravity * disable + objectGravity;        
    }
}

//Gravity Type Enumeration
public enum GravityType
{
    Spherical = 0, Planar = 1, Radial = 2, Cylindrical = 3
}

[System.Serializable]
public class GravitySource
{
    public Vector3 position;
    public Quaternion rotation;
    public float mass;
    public GravityType type;//0 Spherical, 1 Planar, 2 Cylindrical   
    float arcLimit = 360;
    bool cylindricalTopless = false;
    Vector2 planarSize;
    public float radius;
    public Transform trackingTransform = null;


    public float creationTime = 0;
    public float durationTime = Mathf.Infinity;//How long a gravity source last after it is created.
    public float decayTime = 3;// In seconds, linear degredation after duration unpon which a gravity source will phaze out of existance. 
    public float durationScaler = 1;

    public GravitySource()//default
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        mass = 100f;
        creationTime = 0;
        durationTime = Mathf.Infinity;
        decayTime = 0;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime)//Spherical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        this.mass = mass;
        creationTime = Time.time;
        this.durationTime = durationTime;
        this.decayTime = decayTime;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, Vector2 planarSize, Quaternion rotation, bool doubleSided)//Planar
    {
        this.position = position;
        this.rotation = rotation;
        type = GravityType.Planar;
        this.mass = mass;
        creationTime = Time.time;
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.planarSize = planarSize;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, float radius, Quaternion rotation, bool doubleSided)//Radial
    {
        this.position = position;
        this.rotation = rotation;
        type = GravityType.Radial;
        this.mass = mass;
        creationTime = Time.time;
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.radius = radius;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, float arcLimit, bool topless, Quaternion rotation)//Cylinderical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;

        this.mass = mass;
        creationTime = Time.time;
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.arcLimit = arcLimit;
        cylindricalTopless = topless;
    }

}

[System.Serializable]
public class GravityChain
{
    List<GravitySource> chainedGravitysources;

}
