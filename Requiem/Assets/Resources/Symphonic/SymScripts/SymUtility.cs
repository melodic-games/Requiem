using UnityEngine;
using System.Collections.Generic;

public class SymUtils
{

    public static Vector3 RedirectForce(Vector3 forward, Vector3 velocityNormal, float velocityMagnatuide , float scale)
    {        
        Vector3 forceVector = Vector3.Cross(forward, -velocityNormal).normalized;
        Vector3 crossVector = Vector3.Cross(forceVector, forward);        
        return crossVector * velocityMagnatuide * Vector3.Dot(crossVector, -velocityNormal) * scale;
    }

    public static void ShockWave(Vector3 location, float force, Rigidbody sourceObject)
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

    }

    public static void ShockWave(Vector3 location, float force, GameObject sourceObject)//Generic Version
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
                if (colliderRb != sourceObject.GetComponent<Rigidbody>())
                    colliderRb.AddExplosionForce(force, location, radius, upForce, ForceMode.VelocityChange);
            }
        }

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
