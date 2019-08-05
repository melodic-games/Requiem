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
                
                GravitySource gS = new GravitySource(obj.transform.position, Mathf.Pow(radius,2) * 10, Mathf.Infinity);
                gravitySources.Add(gS);
                gS.trackingTransform = obj.transform;
            }

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Repulsor"))
            {
                float radius = obj.GetComponent<MeshRenderer>().bounds.extents.x;

                GravitySource gS = new GravitySource(obj.transform.position, Mathf.Pow(radius, 2) * -10, Mathf.Infinity);
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
                gs.position = gs.trackingTransform.position;
        }
    }

    public Vector3 ReturnGravity(Transform mytransform)
    {

        Vector3 gravity;

        Vector3 objectGravity = Vector3.zero;//Gravity caused by objects in the world                               

        foreach (GravitySource gs in gravitySources)
        {
            if (gs.trackingTransform != mytransform)               
            //add case statement here for different gravity types
            objectGravity += gs.mass / Vector3.SqrMagnitude(mytransform.position - gs.position) * (gs.position - mytransform.position).normalized;//sphearical gravity           
        }

        //Cancel out Scene gravity when object gravity magnitude is the same size or bigger.       
        
            float disable = 1;
        if (sceneGravityMag > 0)
            disable = Mathf.Max(0, (sceneGravityMag - objectGravity.magnitude) / sceneGravityMag);

        return gravity = Physics.gravity * disable + objectGravity;        
    }
}
