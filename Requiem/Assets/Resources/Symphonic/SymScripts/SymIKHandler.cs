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
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        lookPosition = headBone.position + headBone.forward;
        lookDir = headBone.forward;

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
            bone.m_Stiffness =  Mathf.InverseLerp(3, 10, Mathf.Abs(localAngularVelocity.y) + Mathf.Abs(localAngularVelocity.x)); //energyLevel;
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
