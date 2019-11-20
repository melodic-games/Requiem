using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymIKHandler : MonoBehaviour
{
    private DynamicBone[] dynamicBones = new DynamicBone[] { null , null};
    public CameraControl cameraControl;

    private SymBehaviour behaviour;

    private Animator animator;
    private Rigidbody rb;
    private Vector3 localAngularVelocity;

    private Vector3 lookPosition;
    private Vector3 lookDir;
    private Transform headBone;

    public bool useFootIK = false;

    private float leftFootWeight = 1;
    private float rightFootWeight = 1;
    private Vector3 leftFootPosition;
    private Vector3 rightFootPosition;

    private void Start()
    {
        behaviour = GetComponent<SymBehaviour>();

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        Transform leftShoulderAnchor = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        Transform rightShoulderAnchor = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

        Transform leftUpperLegAnchor = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform rightUpperLegAnchor = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        lookPosition = headBone.position + headBone.forward;
        lookDir = headBone.forward;

        //Initilize Dynamic Bone scripts
        {
            dynamicBones[0] = leftShoulderAnchor.gameObject.AddComponent<DynamicBone>();

            dynamicBones[0].m_Root = leftShoulderAnchor;
            dynamicBones[0].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.LeftHand) };
            dynamicBones[0].m_Damping = 0.1f;
            dynamicBones[0].m_Elasticity = 0.022f;
            dynamicBones[0].m_Inert = 1;            

            dynamicBones[1] = rightShoulderAnchor.gameObject.AddComponent<DynamicBone>();

            dynamicBones[1].m_Root = rightShoulderAnchor;
            dynamicBones[1].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.RightHand) };
            dynamicBones[1].m_Damping = 0.1f;
            dynamicBones[1].m_Elasticity = 0.022f;
            dynamicBones[1].m_Inert = 1;

            //dynamicBones[2] = leftUpperLegAnchor.gameObject.AddComponent<DynamicBone>();

            //dynamicBones[2].m_Root = leftUpperLegAnchor;
            //dynamicBones[2].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.LeftFoot) };
            //dynamicBones[2].m_Damping = 0.1f;
            //dynamicBones[2].m_Elasticity = 0.022f;
            //dynamicBones[2].m_Inert = 1;

            //dynamicBones[3] = rightUpperLegAnchor.gameObject.AddComponent<DynamicBone>();

            //dynamicBones[3].m_Root = rightUpperLegAnchor;
            //dynamicBones[3].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.RightFoot) };
            //dynamicBones[3].m_Damping = 0.1f;
            //dynamicBones[3].m_Elasticity = 0.022f;
            //dynamicBones[3].m_Inert = 1;
        }
    }

    private void Update()
    {

        localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);

        float stiff = Mathf.Lerp(0, 1, Mathf.InverseLerp(3, 10, Mathf.Abs(localAngularVelocity.y) + Mathf.Abs(localAngularVelocity.x)));

        foreach (DynamicBone bone in dynamicBones)
        {
            bone.m_Stiffness = stiff;
            //if (behaviour.grounded)
            //if (bone == dynamicBones[2] || bone == dynamicBones[3])
            //    bone.m_Stiffness = 1;
            bone.UpdateParameters();
        }

        Debug.DrawLine(headBone.position, lookPosition);
    }

    void OnAnimatorIK()
    {       
        //Main
        {
            lookDir = Vector3.Lerp(lookDir, (cameraControl.characterTargetingPosition - headBone.position).normalized, Time.deltaTime * 5);
            lookPosition = headBone.position + lookDir;
            animator.SetLookAtPosition(lookPosition);

            float t = Mathf.InverseLerp(10, 0, localAngularVelocity.magnitude);            
            animator.SetLookAtWeight(t, .5f, .8f, 1, 1f);
        }

        //Foot IK
        if (useFootIK)
        {          

            Ray ray = new Ray(Vector3.zero, Vector3.zero);
            RaycastHit hitInfo;
            float maxDist = 0;

            Physics.Raycast(ray, out hitInfo, maxDist);
            leftFootPosition = hitInfo.point;           
            Physics.Raycast(ray, out hitInfo, maxDist);
            rightFootPosition = hitInfo.point;

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
        }
    }
}
