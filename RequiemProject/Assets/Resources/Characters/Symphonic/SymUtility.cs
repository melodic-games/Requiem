using UnityEngine;
using System.Collections.Generic;

public class SymUtility
{

    public static Vector3 RedirectForce(Vector3 forward, Vector3 velocityNormal, float velocityMagnatuide , float scale)
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
        myRigidbody.MovePosition((myRigidbody.position + myRigidbody.rotation * localOffset) - rotation * localOffset);
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
                 
            //Find automated heading          
            Vector3 automatedHeading = Vector3.Lerp(flightTensor.forward, behaviour.rbVelocityNormalized * Mathf.Sign(Vector3.Dot(behaviour.rbVelocityNormalized, flightTensor.forward)), behaviour.rbVelocityMagnitude * .01f);

            //If input is interupted rotation can not be manually adjusted
            if (behaviour.muteInput)  aimedHeading = automatedHeading;
            
            //Set forward of tensor3
            targetOrientationTensor.forward = aimedHeading;           
        }

        //Find the Right Component of the Target Orientation Tensor
        {
            //Pick between the velocity direction and aimed heading 
            Vector3 targetForwards = targetForwards = behaviour.focusInput ? aimedHeading : behaviour.rbVelocityNormalized;

            //Find right vector from target forwards and up 
            Vector3 targetRight = Vector3.Cross(targetForwards, behaviour.gravity);      

            //Flip target right if left is closer towards flightTensor.right
            targetOrientationTensor.right = targetRight * Mathf.Sign(Vector3.Dot(flightTensor.right, targetRight));      
        }

        //Up Tensor isn't used
        targetOrientationTensor.up = Vector3.zero;
    }

    public static void GetTargetOrientationTensor2(BaseCharacterController behaviour, Tensor3 flightTensor, out Tensor3 targetOrientationTensor)
    {
        //Aimed heading is the cameras facing direction
        Vector3 aimedHeading = Camera.main.transform.forward;

        //Find the Forward Component of the Target Orientation Tensor
        {
            //Are we flying forwards or backwards?
            float facingDirection = Mathf.Sign(Vector3.Dot(behaviour.rbVelocityNormalized, flightTensor.forward));

            //Find automated heading
            Vector3 automatedHeading = behaviour.rbVelocityNormalized * facingDirection;

            //If input is interupted rotation can not be manually adjusted
            if (behaviour.muteInput)
                aimedHeading = automatedHeading;

            //Diables automatedHeading if flying forward and applying thrust.
            if (facingDirection == 1)
                automatedHeading = Vector3.Lerp(behaviour.rbVelocityNormalized, flightTensor.forward, behaviour.thrustInput);

            //Disable automatedHeading when velocity is low
            automatedHeading = Vector3.Lerp(flightTensor.forward, automatedHeading, behaviour.rbVelocityMagnitude * .01f);

            //pick between using the automatedHeading and aimed heading when the character is focused.
            targetOrientationTensor.forward = behaviour.focusInput ? aimedHeading : automatedHeading;
        }

        //Find the Right Component of the Target Orientation Tensor
        {
            //Pick between the velocity direction and aimed heading 
            Vector3 targetForwards = targetForwards = behaviour.focusInput ? aimedHeading : behaviour.rbVelocityNormalized;

            //Find right vector from target forwards and up 
            Vector3 targetRight = Vector3.Cross(targetForwards, behaviour.gravity);

            //Flip target right if left is closer towards flightTensor.right
            targetOrientationTensor.right = targetRight * Mathf.Sign(Vector3.Dot(flightTensor.right, targetRight));
        }

        //Up Tensor isn't used
        targetOrientationTensor.up = Vector3.zero;
    }

    public static void ForwardAirbornControl(BaseCharacterController behaviour, Tensor3 flightTensor, Tensor3 targetOrientationTensor)
    {

        //Align the flightTensor forward axis towards the targetOrientationTensor forward.                    
        
        //Disable auto turning if input is detected
        float disable = 1 - Mathf.Clamp01(Mathf.Abs(behaviour.verticalInput * 2));

        //Determin magnitude of rotation
        float magnitude = 2 + (1 - behaviour.thrustInput) * 2;

        //If rolling don't try to change the upvector of the character, just let it roll
        float ignoreUpModification = Mathf.Clamp01(Mathf.Abs(behaviour.localAngularVelocity.y * .5f));

        //Up vector aims towards heading (or inverse heading, whichever is closer) if forwards is facing away from heading. 
        Vector3 upVector = Vector3.Slerp(targetOrientationTensor.forward * Mathf.Sign(Vector3.Dot(flightTensor.up, targetOrientationTensor.forward) + .5f), flightTensor.up, Vector3.Dot(flightTensor.forward, targetOrientationTensor.forward));

        //Apply ignoreUpModification, and disable modification if not focusing
        upVector = Vector3.Slerp(upVector, flightTensor.up, ignoreUpModification * (behaviour.focusInput ? 0 : 1));

        //Reduce turning based on intersection of pitching arc and heading, but set to full if rolling
        targetOrientationTensor.forward = Vector3.Slerp(flightTensor.forward, targetOrientationTensor.forward, 1 - Mathf.Clamp01(Mathf.Abs(Vector3.Dot(flightTensor.right, targetOrientationTensor.forward) * 2)) + ignoreUpModification);

        //Find target rotation 
        Quaternion targetRotation = Quaternion.LookRotation(targetOrientationTensor.forward, upVector) * Quaternion.Euler(90, 0, 0);

        //Find new Rotation
        Quaternion newRotation = behaviour.transform.rotation;

        if (behaviour.focusInput)
            newRotation = Quaternion.Slerp(behaviour.transform.rotation, targetRotation, Time.deltaTime * magnitude * disable);//Quaternion.RotateTowards(behaviour.transform.rotation, targetRotation, Time.deltaTime * magnitude * disable);

        //Rotate around characters center, towards target rotation
        SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f, newRotation);

        //Reorient the world space angular velocity to match the rotation just applied 
        behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);
        
    }

    public static void ForwardAirbornControl2(BaseCharacterController behaviour, Tensor3 flightTensor, Tensor3 targetOrientationTensor)
    {

        //Align the flightTensor forward axis towards the targetOrientationTensor forward.                    

        //Disable auto turning if input is detected
        float disable = 1 - Mathf.Clamp01(Mathf.Abs(behaviour.verticalInput * 2));

        //If rolling don't try to change the upvector of the character, just let it roll
        float ignoreUpModification = Mathf.Clamp01(Mathf.Abs(behaviour.localAngularVelocity.y * .5f));

        //Up vector aims towards heading (or inverse heading, whichever is closer) if forwards is facing away from heading. 
        Vector3 upVector = Vector3.Slerp(targetOrientationTensor.forward * Mathf.Sign(Vector3.Dot(flightTensor.up, targetOrientationTensor.forward) + .5f), flightTensor.up, Vector3.Dot(flightTensor.forward, targetOrientationTensor.forward));

        //Apply ignoreUpModification, and disable modification if not focusing
        upVector = Vector3.Slerp(upVector, flightTensor.up, ignoreUpModification * (behaviour.focusInput ? 0 : 1));

        //Reduce turning based on intersection of pitching arc and heading, but set to full if rolling
        targetOrientationTensor.forward = Vector3.Slerp(flightTensor.forward, targetOrientationTensor.forward, 1 - Mathf.Clamp01(Mathf.Abs(Vector3.Dot(flightTensor.right, targetOrientationTensor.forward) * 2)) + ignoreUpModification);

        //Find target rotation 
        Quaternion targetRotation = Quaternion.LookRotation(targetOrientationTensor.forward, upVector) * Quaternion.Euler(90, 0, 0);

        //Find new Rotation
        Quaternion newRotation = Quaternion.RotateTowards(behaviour.transform.rotation, targetRotation, Time.deltaTime * (45 + ((1 - behaviour.thrustInput) * 45)) * disable);

        //Rotate around characters center, towards target rotation
        SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f, newRotation);

        //Reorient the world space angular velocity to match the rotation just applied 
        behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);

    }

    public static void LateralAirbornControl(BaseCharacterController behaviour, Tensor3 flightTensor, Tensor3 targetOrientationTensor)
    {
        
        //Align the flightTensor right axis towards the targetOrientationTensor right.                    
        
        //enable if pitching, disable if roll input is detected, or if thrusting, or thrusting
        float scalar = Mathf.Abs(behaviour.verticalInput) * (1 - Mathf.Clamp01(Mathf.Abs(behaviour.horizontalInput * 2)));
        scalar *= 1 - behaviour.thrustInput;

        //smoothly disable if we are moving along the gravity axis
        float disable = 1;
        disable *= 1 - Mathf.Abs(Vector3.Dot(targetOrientationTensor.forward, behaviour.gravity.normalized));

        //Find target rotation             
        Quaternion targetRotation = Quaternion.LookRotation(targetOrientationTensor.right, flightTensor.up) * Quaternion.Euler(90, -90, 0);

        //Find new Rotation
        Quaternion newRotation = Quaternion.RotateTowards(behaviour.transform.rotation, targetRotation, scalar * disable * 90 * Time.deltaTime);

        //Rotate around characters center, towards target rotation
        SetRotationAroundOffset(behaviour.transform, Vector3.up * behaviour.playerHeight * .5f, newRotation);

        //Reorient the world space angular velocity to match the rotation just applied 
        behaviour.rb.angularVelocity = behaviour.transform.TransformDirection(behaviour.localAngularVelocity);
        
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


}
