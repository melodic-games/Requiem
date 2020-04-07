using UnityEngine;
using System.Collections.Generic;

public class BaseCharacterFunctions
{

    public static Vector3 RedirectForce(Vector3 forward, Vector3 velocityNormal, float velocityMagnatuide, float scale)
    {
        Vector3 forceVector = Vector3.Cross(forward, -velocityNormal).normalized;
        Vector3 crossVector = Vector3.Cross(forceVector, forward);
        return crossVector * velocityMagnatuide * Vector3.Dot(crossVector, -velocityNormal) * scale;
    }

    public static void SetRotationAroundOffset(Transform myTransform, Vector3 localOffset, Quaternion rotation)
    {
        myTransform.position = (myTransform.position + myTransform.rotation * localOffset) - rotation * localOffset;
        myTransform.rotation = rotation;
    }

    public static void SetRotationAroundOffset(Rigidbody myRigidbody, Vector3 localOffset, Quaternion rotation)
    {
        myRigidbody.MovePosition(myRigidbody.position + myRigidbody.rotation * localOffset - rotation * localOffset);
        myRigidbody.MoveRotation(rotation);
    }

    public static void ShockWave(Vector3 location, Quaternion rotation, float force, Rigidbody sourceObject, Object shockwavePrefab)
    {
        //float force = 25;
        float radius = 50;
        float upForce = 5;

        Collider[] colliders = Physics.OverlapSphere(location, radius);
        foreach (Collider collider in colliders)
        {
            Rigidbody colliderRb = collider.GetComponent<Rigidbody>();
            if (colliderRb != null)
            {
                if (colliderRb != sourceObject)
                    colliderRb.AddExplosionForce(force, location, radius, upForce, ForceMode.VelocityChange);
            }
        }

        if (shockwavePrefab != null)
            Object.Instantiate(shockwavePrefab, location, rotation);

    }

    public static void GetTargetOrientationTensor(BaseCharacterController behaviour, Tensor3 flightTensor, out Tensor3 targetOrientationTensor)
    {
        //Aimed heading is the cameras facing direction
        Vector3 aimedHeading = Camera.main.transform.forward;

        //Find the Forward Component of the Target Orientation Tensor
        {
            //Are we flying forwards or backwards?
            float facingDirection = Mathf.Sign(Vector3.Dot(behaviour.rbVelocityNormalized, flightTensor.forward));

            //Find automated heading
            Vector3 automatedHeading = behaviour.rbVelocityNormalized * facingDirection;

            //Disable automatedHeading when velocity is low
            automatedHeading = Vector3.Lerp(flightTensor.forward, automatedHeading, behaviour.rbVelocityMagnitude * .01f);

            //If input is interupted rotation can not be manually adjusted
            if (behaviour.muteInput)
                aimedHeading = automatedHeading;

            //Diables automatedHeading if flying forward and applying thrust.
            if (facingDirection == 1)
                automatedHeading = Vector3.Lerp(behaviour.rbVelocityNormalized, flightTensor.forward, behaviour.thrustInput);


            //Fix Later
            automatedHeading = flightTensor.forward;
            
            //pick between using the automatedHeading and aimed heading when the character is focused.
            targetOrientationTensor.forward = behaviour.focusInput ? aimedHeading : automatedHeading;
        }

        //Find the Right Component of the Target Orientation Tensor
        {
            //Pick between the velocity direction and aimed heading 
            Vector3 targetForwards = behaviour.focusInput ? aimedHeading : behaviour.rbVelocityNormalized;

            //Find right vector from target forwards and up 
            Vector3 targetRight = Vector3.Cross(targetForwards, behaviour.gravity);
            
                if (targetRight == Vector3.zero)
                    targetRight = behaviour.transform.right;
                
            //Flip target right if left is closer towards flightTensor.right
            targetOrientationTensor.right = targetRight;// * Mathf.Sign(Vector3.Dot(flightTensor.right, targetRight));

        }

        //Up Tensor isn't used
        targetOrientationTensor.up = Vector3.zero;
        
    

    }

    public static void ForwardAirbornDirectionControl(BaseCharacterController behaviour, Tensor3 flightTensor, Tensor3 targetOrientationTensor)
    {

        //Align the flightTensor forward axis towards the targetOrientationTensor forward.                    

        //Disable auto turning if input is detected
        float scale = 1 - Mathf.Clamp01(Mathf.Abs(behaviour.verticalInput * 2));

        //If rolling don't try to change the upvector of the character, just let it roll
        float ignoreUpModification = Mathf.Clamp01(Mathf.Abs(behaviour.localAngularVelocity.y * .5f));

        ignoreUpModification = 1;

        if (behaviour.focusInput)
            if (ignoreUpModification != 1)
                Debug.Log("Not working");

        //Up vector aims towards heading (or inverse heading, whichever is closer) if forwards is facing away from heading. 
        Vector3 upVector;

        float facing = Mathf.Sign(Vector3.Dot(flightTensor.up, targetOrientationTensor.forward) + .05f);

        //upVector = Vector3.Slerp(targetOrientationTensor.forward * facing), flightTensor.up, Vector3.Dot(flightTensor.forward, targetOrientationTensor.forward));


        //Crush forward vector onto y/x plain
        Vector3 crushedForwards = (targetOrientationTensor.forward - (flightTensor.forward * Vector3.Dot(targetOrientationTensor.forward, flightTensor.forward))).normalized;

        float dot = Mathf.Clamp01(Vector3.Dot(flightTensor.forward, crushedForwards));

        upVector = Vector3.Slerp(targetOrientationTensor.forward * facing, flightTensor.up, dot);

        //Apply ignoreUpModification, and disable modification if not focusing
        upVector = Vector3.Slerp(upVector, flightTensor.up, ignoreUpModification * (behaviour.focusInput ? 0 : 1));

        float intersection = ignoreUpModification + 1 - Mathf.Abs(Vector3.Dot(flightTensor.right, targetOrientationTensor.forward));

        //Reduce turning based on intersection of pitching arc and heading, but set to full if rolling
        targetOrientationTensor.forward = Vector3.Slerp(flightTensor.forward, targetOrientationTensor.forward, intersection);

        Quaternion targetRotation;

        if (behaviour.focusInput)
        {
            //Find target rotation for up fix
            targetRotation = Quaternion.LookRotation(flightTensor.forward, upVector) * Quaternion.Euler(90, 0, 0);

            //Rotate around characters center, towards target rotation
            SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f,
                    Quaternion.Slerp(behaviour.transform.rotation, targetRotation, Time.deltaTime * 2 * scale)
                    );

            //Reorient the world space angular velocity to match the rotation just applied 
            behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);
        }

        //if (behaviour.focusInput)
        {
            //Find target rotation for forward fix
            targetRotation = Quaternion.LookRotation(targetOrientationTensor.forward, flightTensor.up) * Quaternion.Euler(90, 0, 0);
            if (targetOrientationTensor.forward == Vector3.zero) { Debug.Log("Here"); }
            if (flightTensor.up == Vector3.zero) { Debug.Log("Here"); }

            {
                Vector3 cross = Vector3.Cross(flightTensor.forward, targetOrientationTensor.forward);

                if (behaviour.verticalInput == 0)
                {

                }

                //if(behaviour.horizontalInput == 0){}
            }


            //Rotate around characters center, towards target rotation
            SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f,
                Quaternion.Slerp(behaviour.transform.rotation, targetRotation, Time.deltaTime * (2 + (1 - behaviour.thrustInput) * 2) * scale)
                );

            //Reorient the world space angular velocity to match the rotation just applied 
            behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);
        }

    }

    public static void LateralAirbornDirectionControl(BaseCharacterController behaviour, Tensor3 flightTensor, Tensor3 targetOrientationTensor)
    {
        //Align the flightTensor right axis towards the targetOrientationTensor right.                    

        //enable if pitching, disable if roll input is detected
        float scalar = Mathf.Abs(behaviour.weightedPitchInput) * (1 - Mathf.Clamp01(Mathf.Abs(behaviour.weightedRollInput * 2)));

        //Disable if thrusting
        scalar *= 1 - behaviour.thrustInput;

        //Smoothly disable if we are moving along the gravity axis and not focusing        
        if (behaviour.focusInput == false)
            scalar *= 1 - Mathf.Abs(Vector3.Dot(behaviour.rbVelocityNormalized, behaviour.gravity.normalized));

        //Smoothly diable if we are moving to slow
        scalar *= Mathf.InverseLerp(0, 10, behaviour.rbVelocityMagnitude);

        //Find target rotation             
        Quaternion targetRotation = Quaternion.LookRotation(targetOrientationTensor.right, flightTensor.up) * Quaternion.Euler(90, -90, 0);

        //Rotate around characters center, towards target rotation
        SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f,
            Quaternion.RotateTowards(behaviour.transform.rotation, targetRotation, scalar * 90 * Time.deltaTime)
            );

        //Reorient the world space angular velocity to match the rotation just applied 
        behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);

    }

    public static void ManualAirbornAngularAcceleration(BaseCharacterController owner, float pitchInput, float rollInput)
    {

         Vector2 angularGain = new Vector2(6, 5);

        //Pitch - x
        float pitchAcceleration = owner.angularAccelerationBase * (angularGain.x - (4.8f * owner.thrustInput));
        owner.rb.AddTorque(owner.transform.right * pitchInput * pitchAcceleration, ForceMode.Acceleration);

        //Roll - y
        float rollAcceleration = Mathf.Lerp(1, .5f, Mathf.Abs(pitchInput)) * owner.angularAccelerationBase * Mathf.Max(0, angularGain.y * (1 + owner.spinUp * 2));
        owner.rb.AddTorque(-owner.orientationTensor.forward * rollInput * rollAcceleration, ForceMode.Acceleration);

    }

    public static void AirbornDrag(BaseCharacterController owner)
    {
        {          
            //Directional Drag along the characters flight forward vector, only using force along the negative forward axis.
            float backwardsAirDrag = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, -owner.orientationTensor.forward));          
         
            //Disable backwards drag if pitching (4 is the threshold for when the animation transions into flight curl)
            backwardsAirDrag *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.x) - 4);

            //If not thrusting disable backwards drag while rolling
            if (owner.thrustInput == 0)
                backwardsAirDrag *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.y) * .5f - 5);

            owner.backwardsAirDrag = Mathf.Lerp(owner.backwardsAirDrag, (backwardsAirDrag + backwardsAirDrag * owner.thrustInput) * .5f, Time.deltaTime * 5);
            //owner.backwardsAirDrag = (backwardsAirDrag + backwardsAirDrag * owner.thrustInput) * .5f;
        }

        {
            //Directional Drag along the characters flight up vector, signed. Ignoring side axis drag.
            //lateralAirDrag = Vector3.Dot(rbVelocityNormalized, orientationTensor.up);
            //Directional Drag along any side
            owner.lateralAirDrag = 1 - Mathf.Abs(Vector3.Dot(owner.rbVelocityNormalized, owner.orientationTensor.forward));

            //DisableLateralAirDrag if not going slow
            owner.lateralAirDrag *= Mathf.InverseLerp(2, 0, owner.rbVelocityMagnitude);

            //Disable lateral air drag if pitching (4 is the threshold for when the animation transions into flight curl)
            owner.lateralAirDrag *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.x) - 4);

            //Diable lateral air drag if rolling
            owner.lateralAirDrag *= 1 - Mathf.Clamp01(Mathf.Abs(owner.localAngularVelocity.y) - 5);
        }

        //Max velocity adjustment
        //owner.highEndDrag = Mathf.Lerp(0, 10, Mathf.InverseLerp(owner.flightTopSpeed - 20, owner.flightTopSpeed, owner.rbVelocityMagnitude));

        //backwards + lateral + highEnd               
        owner.rb.drag = Mathf.Lerp(0, 2, owner.backwardsAirDrag) + Mathf.Lerp(0, 0.2f, Mathf.Abs(owner.lateralAirDrag));
        //rb.drag = Mathf.Lerp(0, 1, backwardsAirDrag) + highEndDrag;

    }

    public static void GroundDrag(BaseCharacterController owner)
    {        
        //Directional Drag along the characters flight forward vector, only using force along the negative forward axis.
        float backwardsAirDrag = Mathf.Clamp01(Vector3.Dot(owner.rbVelocityNormalized, -owner.orientationTensor.forward));

        owner.backwardsAirDrag = Mathf.Lerp(owner.backwardsAirDrag, backwardsAirDrag * (owner.crouching ? 1 : 0) , Time.deltaTime * 4);     
                              
        owner.rb.drag = Mathf.Lerp(0, 2, owner.backwardsAirDrag);

        if (owner.rbVelocityMagnitude <= owner.groundRunSpeed + 1 && owner.moveDirectionMag == 0 && !owner.crouching)
            owner.rb.drag += 10;        
    }

    public static bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs)
    {
        groundCP = default;
        bool found = false;
        foreach (ContactPoint cp in allCPs)
        {
            if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundCP.normal.y))
            {
                groundCP = cp;
                found = true;
            }
        }

        return found;
    }

    public static bool FindStep(out Vector3 stepUpOffset, float maxStepHeight, float stepSearchOvershoot, List<ContactPoint> allCPs, ContactPoint groundCP, Vector3 curentVelocity)
    {
        stepUpOffset = default;

        Vector2 velocityXZ = new Vector2(curentVelocity.x, curentVelocity.z);
        if (velocityXZ.SqrMagnitude() < 0.0001f)
        {
            return false;
        }

        foreach (ContactPoint cp in allCPs)
        {
            if (ResolveStepUp(out stepUpOffset, maxStepHeight, stepSearchOvershoot, cp, groundCP))
                return true;
        }
        return false;

    }

    public static bool ResolveStepUp(out Vector3 stepUpOffset, float maxStepHeight, float stepSearchOvershoot, ContactPoint stepTestCP, ContactPoint groundCP)
    {
        stepUpOffset = default;
        Collider stepCol = stepTestCP.otherCollider;

        if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
        {
            return false;
        }

        if (!(stepTestCP.point.y - groundCP.point.y < maxStepHeight))
        {
            return false;
        }

        RaycastHit hitInfo;
        float stepHeight = groundCP.point.y + maxStepHeight + 0.0001f;
        Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;
        Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * stepSearchOvershoot);
        Vector3 direction = Vector3.down;
        if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, maxStepHeight))
        {
            return false;
        }

        Vector3 stepUpPoint = new Vector3(stepTestCP.point.y, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * stepSearchOvershoot);
        Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

        stepUpOffset = stepUpPointOffset;
        return true;

    }


    //public void CameraBounce(Collision collision)
    //{

    //    Vector3 hitPoint = collision.GetContact(0).point;
    //    Vector3 hitNormal = collision.GetContact(0).normal;

    //    print("CameraBounce");

    //    //Find Reflection Vector
    //    Vector3 reflect = Vector3.Reflect(controller.rbVelocityNormalized, hitNormal);

    //    //Camera Tracks towards next bounce position if one exists
    //    RaycastHit bounceFutureCheck;
    //    if (Physics.Raycast(transform.position, reflect, out bounceFutureCheck, 1000, ~((1 << 8) | (1 << 2) | (1 << 10)), QueryTriggerInteraction.Ignore))
    //    {
    //        cameraDirectionResetPosition = bounceFutureCheck.point;
    //        cameraDirectionResetLerpValue = .1f;
    //    }
    //    else
    //    {
    //        cameraDirectionResetPosition = hitPoint + reflect * 1000;
    //        cameraDirectionResetLerpValue = .1f;
    //    }

    //}

}