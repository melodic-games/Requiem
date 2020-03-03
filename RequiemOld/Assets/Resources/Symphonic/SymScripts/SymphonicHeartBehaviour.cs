using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymphonicHeartBehaviour : MonoBehaviour
{

    public Transform heart;
    float scale = 0;

    void FixedUpdate()
    {
        heart.transform.localScale = new Vector3(scale, scale, scale);
        scale = Mathf.Lerp(scale, .2f, Time.deltaTime * 8);
    }

    void Beat(float currentStep)
    {
        if(heart != null)
        {
            scale += .5f;
        if (currentStep == 4)
            scale += .5f;
        }
    }


}
