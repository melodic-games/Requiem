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
            gravitySources = new List<GravitySource>();

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

    public Vector3 ReturnGravity(Transform mytransform)
    {

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
                        objectGravity += Mathf.Min(gs.mass / Vector3.SqrMagnitude(mytransform.position - gs.position), maxObjectGravity) * (gs.position - mytransform.position).normalized * gs.durationScaler;
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
        }

        //Cancel out Scene gravity when object gravity magnitude is the same size or bigger.       
        
            float disable = 1;
        if (sceneGravityMag > 0)
            disable = Mathf.Max(0, (sceneGravityMag - objectGravity.magnitude) / sceneGravityMag);

        return gravity = Physics.gravity * disable + objectGravity;        
    }
}
