using UnityEngine;
using System.Collections;

public class InfinteDustfield : MonoBehaviour
{
    private Transform myTransform;
    public Transform suckTarget;    
    public ParticleSystem.Particle[] points;
    private ParticleSystem ps;

    public Vector3 wind;
    private Vector3 gravity;
    public Vector3 windTarget;
    public float windTimer = 0;
    public float windTimerMax = 5;

    public GravityManager gm;

    public int maxCount = 100;
    public float size = 1;
    public float maxDistance = 10;
    public float shrinkRange = 2;
    private float maxDistanceSqr;
    public float shrinkRangeSqr;
   



    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        myTransform = transform;
        maxDistanceSqr = maxDistance * maxDistance;
        shrinkRangeSqr = shrinkRange * shrinkRange;
        
    }

    private void CreateStars()
    {
        points = new ParticleSystem.Particle[maxCount];

        for (int i = 0; i < maxCount; i++) {
            points[i].position = Random.insideUnitSphere * maxDistance + myTransform.position;
            points[i].rotation3D = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            //points[i].startColor = new Color(1, 1, 1, 1);
            points[i].startSize = size;
        }
    }

    private void Update()
    {
        windTimer +=Time.deltaTime;

        if (windTimer > windTimerMax)
        {
            windTimer = 0;
            windTimerMax = Random.Range(5f, 7f);
            windTarget = new Vector3(Random.Range(1f, -1f), Random.Range(1f, -1f), Random.Range(1f, -1f));
        }

        wind = Vector3.Lerp(wind, windTarget, Time.deltaTime);
    }

    private void Reset(int i)
    {
        points[i].position = Random.insideUnitSphere.normalized * maxDistance + myTransform.position;
        points[i].startSize = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        gravity = gm.ReturnGravity(transform, Vector3.zero).normalized;

        if (points == null) CreateStars();

        bool suck = Input.GetButton("Crouching");        

        for (int i = 0; i < maxCount; i++) 
        {
           
            
            {
                if (suckTarget != null && suck) { 
                points[i].position = Vector3.MoveTowards(points[i].position, suckTarget.position, Time.deltaTime * 15);
                if ((points[i].position - suckTarget.position).sqrMagnitude < .1f)
                    Reset(i);
                }

                points[i].position = Vector3.MoveTowards(points[i].position, points[i].position + gravity * 1000, Time.deltaTime * 1);
                                      
            }
            

            {
                points[i].position += wind * Time.deltaTime;
            }           

            float distSqr = (points[i].position - myTransform.position).sqrMagnitude;

            float fallOff = Mathf.InverseLerp(maxDistanceSqr, maxDistanceSqr - shrinkRangeSqr, distSqr);

            //Reset Position and size if too far away
            if (distSqr > maxDistanceSqr)
            {
                Reset(i);
            }

            //Fade out particle
            {
                points[i].startSize = Mathf.Lerp(
                    points[i].startSize,
                    size * fallOff,
                    Time.deltaTime * 5
                    );
            }

        }        

        ps.SetParticles(points, points.Length);

    }
}