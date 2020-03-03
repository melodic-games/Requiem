using UnityEngine;
using UnityEditor;

public class TrailControl : MonoBehaviour
{
    private TrailRenderer[] trailRenderers = new TrailRenderer[] { null, null };
    public SymBehaviour behaviour;
    public float widthBase = 0.05f;
    private float trailTime;
    public float beginTrailSpeed = 30;
    public float trailGrowRange = 30;
    private float distanceScale = 1;
    public Material trailMaterial;

    void Start()
    {
        Animator animator = GetComponent<Animator>();
               
        behaviour = GetComponent<SymBehaviour>();

        trailRenderers[0] = animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject.AddComponent<TrailRenderer>();
        trailRenderers[1] = animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject.AddComponent<TrailRenderer>();

        foreach (TrailRenderer t in trailRenderers)
        {
            t.widthMultiplier = widthBase;
            t.time = 0;
            t.material = trailMaterial;
        }
    }

    void LateUpdate()
    {
                      
        if (behaviour != null)
        {         
            
            trailTime = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, behaviour.rbVelocityMagnatude);

            if (behaviour.controlSource.thrustInput != 1)
            {
                trailTime *= Mathf.InverseLerp(10, 0, Mathf.Abs(behaviour.localAngularVelocity.y));
                trailTime *= Mathf.InverseLerp(4, 0, Mathf.Abs(behaviour.localAngularVelocity.x));
            }

            float trailMaxSeconds = Mathf.Lerp(1, 5, Mathf.InverseLerp(50, 100, behaviour.rbVelocityMagnatude));

            distanceScale = Mathf.Max(0 ,Vector3.Distance(Camera.main.transform.position, behaviour.transform.position) - 10) * .1f;

            foreach (TrailRenderer t in trailRenderers)
            {
                t.time = Mathf.Lerp(0, trailMaxSeconds, trailTime);

                t.widthMultiplier = widthBase * (1 + distanceScale);

            }
            
        }            
        
    }


}
