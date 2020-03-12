//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SymIKHandler : MonoBehaviour
//{
//    public DynamicBone[] hairDynamicBones = new DynamicBone[] { null, null };

//    private DynamicBone[] bodyDynamicBones = new DynamicBone[] { null , null};
//    public CameraControl cameraControl;

//    private CharacterController controller;

//    private Animator animator;  

//    private Vector3 lookPosition;
//    private Vector3 lookDir;
//    private Transform headBone;

//    public Vector4 lookWeights = new Vector4(.5f, 1f, .22f, 0f);

//    public bool useFootIK = false;

//    private float leftFootWeight = 1;
//    private float rightFootWeight = 1;
//    private Vector3 leftFootPosition;
//    private Vector3 rightFootPosition;

//    private void Start()
//    {

//        controller = GetComponentInParent<CharacterController>();
//        animator = GetComponent<Animator>();

//        //Transform leftShoulderAnchor = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
//        //Transform rightShoulderAnchor = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

//        //Transform leftUpperLegAnchor = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
//        //Transform rightUpperLegAnchor = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);

//        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
//        lookPosition = headBone.position + headBone.forward;
//        lookDir = headBone.forward;

//        //Initilize Dynamic Bone scripts
//        {
//            bodyDynamicBones[0] = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).gameObject.AddComponent<DynamicBone>();
            
//            bodyDynamicBones[0].m_Root = bodyDynamicBones[0].gameObject.transform;
//            bodyDynamicBones[0].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.LeftHand) };
//            bodyDynamicBones[0].m_Damping = 0.1f;
//            bodyDynamicBones[0].m_Elasticity = 0.022f;
//            bodyDynamicBones[0].m_Inert = 1;            

//            bodyDynamicBones[1] = animator.GetBoneTransform(HumanBodyBones.RightShoulder).gameObject.AddComponent<DynamicBone>();

//            bodyDynamicBones[1].m_Root = bodyDynamicBones[1].gameObject.transform;
//            bodyDynamicBones[1].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.RightHand) };
//            bodyDynamicBones[1].m_Damping = 0.1f;
//            bodyDynamicBones[1].m_Elasticity = 0.022f;
//            bodyDynamicBones[1].m_Inert = 1;

//            //dynamicBones[2] = leftUpperLegAnchor.gameObject.AddComponent<DynamicBone>();

//            //dynamicBones[2].m_Root = leftUpperLegAnchor;
//            //dynamicBones[2].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.LeftFoot) };
//            //dynamicBones[2].m_Damping = 0.1f;
//            //dynamicBones[2].m_Elasticity = 0.022f;
//            //dynamicBones[2].m_Inert = 1;

//            //dynamicBones[3] = rightUpperLegAnchor.gameObject.AddComponent<DynamicBone>();

//            //dynamicBones[3].m_Root = rightUpperLegAnchor;
//            //dynamicBones[3].m_Exclusions = new List<Transform>() { animator.GetBoneTransform(HumanBodyBones.RightFoot) };
//            //dynamicBones[3].m_Damping = 0.1f;
//            //dynamicBones[3].m_Elasticity = 0.022f;
//            //dynamicBones[3].m_Inert = 1;
//        }
//    }

//    private void Update()
//    {

//        float stiff;

//        stiff = Mathf.Lerp(0, 1, Mathf.InverseLerp(3, 10, Mathf.Abs(controller.localAngularVelocity.y) + Mathf.Abs(controller.localAngularVelocity.x)));

//        foreach (DynamicBone bone in bodyDynamicBones)
//        {
//            bone.m_Stiffness = stiff;                       
//            bone.UpdateParameters();
//        }

//        if (controller.grounded)
//            stiff = Vector3.Dot(controller.rbVelocityNormalized, -transform.forward);
//        else
//            stiff = 0;

//        foreach (DynamicBone bone in hairDynamicBones)
//        {
//            bone.m_Stiffness = Mathf.Lerp(bone.m_Stiffness, stiff, Time.deltaTime);
//            bone.UpdateParameters();
//        }

//        //Debug.DrawLine(headBone.position, lookPosition);
//    }

//    void OnAnimatorIK(int layerIndex)
//    {       
//        //Main
//        {
//            lookDir = Vector3.Lerp(lookDir, (cameraControl.characterTargetingPosition - headBone.position).normalized, Time.deltaTime * 2);
//            lookPosition = headBone.position + lookDir;
//            animator.SetLookAtPosition(lookPosition);

//            float t = Mathf.InverseLerp(10, 0, controller.localAngularVelocity.magnitude);
//            animator.SetLookAtWeight(t, lookWeights.w, lookWeights.x, lookWeights.y, lookWeights.z);
//        }

//        //Foot IK
//        if (useFootIK)
//        {          

//            Ray ray = new Ray(Vector3.zero, Vector3.zero);
//            RaycastHit hitInfo;
//            float maxDist = 0;

//            Physics.Raycast(ray, out hitInfo, maxDist);
//            leftFootPosition = hitInfo.point;           
//            Physics.Raycast(ray, out hitInfo, maxDist);
//            rightFootPosition = hitInfo.point;

//            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
//            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);

//            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
//            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
//        }
//    }
//}
