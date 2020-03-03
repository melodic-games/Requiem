using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TrailControl : MonoBehaviour
{
    public TrailRenderer[] trailRenderers = new TrailRenderer[] { null, null };
    public BaseCharacterController controller;
    public float widthBase = 0.05f;
    private float trailTime;
    public float beginTrailSpeed = 30;
    public float trailGrowRange = 30;
    public Material trailMaterial;

    void Start()
    {
        Animator animator = GetComponentInChildren<Animator>();
               
        controller = GetComponent<BaseCharacterController>();

        trailRenderers[0] = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).gameObject.AddComponent<TrailRenderer>();
        trailRenderers[1] = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).gameObject.AddComponent<TrailRenderer>();

        foreach (TrailRenderer t in trailRenderers)
        {
            t.widthMultiplier = widthBase;
            t.time = 0;
            t.material = trailMaterial;
        }
    }

    void LateUpdate()
    {
                      
        if (controller != null)
        {         
            
            trailTime = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, controller.rbVelocityMagnitude);

            if (controller.controlSource.thrustInput != 1)
            {
                trailTime *= Mathf.InverseLerp(10, 0, Mathf.Abs(controller.localAngularVelocity.y));
                trailTime *= Mathf.InverseLerp(4, 0, Mathf.Abs(controller.localAngularVelocity.x));
            }

            float trailMaxSeconds = Mathf.Lerp(1, 5, Mathf.InverseLerp(50, 100, controller.rbVelocityMagnitude));           

            foreach (TrailRenderer t in trailRenderers)
            {
                t.time = Mathf.Lerp(0, trailMaxSeconds, trailTime);

                t.widthMultiplier = widthBase;

            }
            
        }            
        
    }


}

//public class TrailControl2 : MonoBehaviour
//{
//    public float lifetime = 5f; //lifetime of a point on the trail

//    public float minimumVertexDistance = 0.1f; //minimum distance moved before a new point is solidified.

//    public Vector3 velocity; //direction the points are moving

//    LineRenderer line;
//    //position data
//    List<Vector3> points;
//    Queue<float> spawnTimes = new Queue<float>(); //list of spawn times, to simulate lifetime. Back of the queue is vertex 1, front of the queue is the end of the trail.
//                                                  //Length of this queue is one less than the number of positions in the linerenderer, since the linerenderer always has a non-solidified vertex at the object's position.

//    // Use this for initialization
//    void Awake()
//    {
//        line = GetComponent<LineRenderer>();
//        line.useWorldSpace = true;
//        points = new List<Vector3>() { transform.position }; //indices 1 - end are solidified points, index 0 is always transform.position
//        line.SetPositions(points.ToArray());
//    }

//    void AddPoint(Vector3 position)
//    {
//        points.Insert(1, position);
//        spawnTimes.Enqueue(Time.time);
//    }

//    void RemovePoint()
//    {
//        spawnTimes.Dequeue();
//        points.RemoveAt(points.Count - 1); //remove corresponding oldest point at the end
//    }

//    // Update is called once per frame
//    void Update()
//    {

//        //cull based on lifetime
//        while (spawnTimes.Count > 0 && spawnTimes.Peek() + lifetime < Time.time)
//        {
//            RemovePoint();
//        }

//        //move positions
//        Vector3 diff = -velocity * Time.deltaTime;
//        for (int i = 1; i < points.Count; i++)
//        {
//            points[i] += diff;
//        }

//        //add new point
//        if (points.Count < 2 || Vector3.Distance(transform.position, points[1]) > minimumVertexDistance)
//        {
//            //if we have no solidified points, or we've moved enough for a new point
//            AddPoint(transform.position);
//        }

//        //update index 0;
//        points[0] = transform.position;

//        //save result
//        line.positionCount = points.Count;
//        line.SetPositions(points.ToArray());
//    }
//}

