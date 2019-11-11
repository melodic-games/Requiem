using UnityEngine;
using System.Collections;

public class InfinteDustfield : MonoBehaviour
{
    private Transform myTransform;
    public Transform suckTarget;
    public Transform pullTarget;
    public ParticleSystem.Particle[] points;
    private ParticleSystem ps;

    public Vector3 wind;
    public Vector3 windTarget;
    public float windTimer = 0;
    public float windTimerMax = 5;
    

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
        if (points == null) CreateStars();

        bool suck = Input.GetButton("Crouching");
        bool pull = false;

        for (int i = 0; i < maxCount; i++) 
        {
           
            
            {
                if (suckTarget != null && suck) { 
                points[i].position = Vector3.MoveTowards(points[i].position, suckTarget.position, Time.deltaTime * 5);//Pull towards target
                if ((points[i].position - suckTarget.position).sqrMagnitude < .1f)
                    Reset(i);
                }

                if (pullTarget != null & pull)
                    points[i].position = Vector3.MoveTowards(points[i].position, points[i].position - (pullTarget.position - points[i].position).normalized * 1000, Time.deltaTime * 5);//Push away from target
                                      
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