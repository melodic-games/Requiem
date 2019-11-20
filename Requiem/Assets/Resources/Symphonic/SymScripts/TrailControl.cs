using UnityEngine;
using UnityEditor;

public class TrailControl : MonoBehaviour
{
    private TrailRenderer[] trailRenderers = new TrailRenderer[] { null, null };
    public SymBehaviour behaviour;
    public float widthBase = 0.05f;
    private float scale;
    public float beginTrailSpeed = 30;
    public float trailGrowRange = 30;

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
            scale = Mathf.InverseLerp(beginTrailSpeed, beginTrailSpeed + trailGrowRange, behaviour.rbVelocityMagnatude);
            if (behaviour.controlSource.thrustInput != 1)
            scale *= Mathf.InverseLerp(10,0,Mathf.Abs(behaviour.localAngularVelocity.y));
            foreach(TrailRenderer t in trailRenderers)
            {
                t.time = Mathf.Lerp(0, 1f,scale);
            }
            
        }            
        
    }


}
