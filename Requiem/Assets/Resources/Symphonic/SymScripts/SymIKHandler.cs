using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymIKHandler : MonoBehaviour
{
    public Transform leftShoulderAnchor;
    public Transform rightShoulderAnchor;

    public Transform leftHand;
    public Transform rightHand;

    public DynamicBone[] dynamicBones = new DynamicBone[] { null , null};
    public CameraControl cameraControl;

    private Animator animator;
    private Rigidbody rb;

    private Vector3 localAngularVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        //Initilize Dynamic Bones
        {
            dynamicBones[0] = leftShoulderAnchor.gameObject.AddComponent<DynamicBone>();

            dynamicBones[0].m_Root = leftShoulderAnchor;
            dynamicBones[0].m_Exclusions = new List<Transform>() { leftHand };
            dynamicBones[0].m_Damping = 0.1f;
            dynamicBones[0].m_Elasticity = 0.022f;
            dynamicBones[0].m_Inert = 1;            

            dynamicBones[1] = rightShoulderAnchor.gameObject.AddComponent<DynamicBone>();

            dynamicBones[1].m_Root = rightShoulderAnchor;
            dynamicBones[1].m_Exclusions = new List<Transform>() { rightHand };
            dynamicBones[1].m_Damping = 0.1f;
            dynamicBones[1].m_Elasticity = 0.022f;
            dynamicBones[1].m_Inert = 1;
        }
    }

    private void Update()
    {

        localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

        foreach (DynamicBone bone in dynamicBones)
        {
            bone.m_Stiffness =  Mathf.InverseLerp(3, 20, Mathf.Abs(localAngularVelocity.y) + Mathf.Abs(localAngularVelocity.x)); //energyLevel;
            bone.UpdateParameters();
        }
    }

    void OnAnimatorIK()
    {        
        {
            float t = Mathf.InverseLerp(10, 0, localAngularVelocity.magnitude);
            animator.SetLookAtWeight(t, .2f, .8f, 1, .5f);
            animator.SetLookAtPosition(cameraControl.characterTargetingPosition);
        }
    }
}
