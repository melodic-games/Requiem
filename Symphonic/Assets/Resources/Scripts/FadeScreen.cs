using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{

    public Texture image;
    [Range(0, 1)]
    public float alpha = 1;
    [Range(0,1)]
    public float target = 0;

    private void Update()
    {
        if (Time.time > .5f)
        {
            //alpha = Mathf.Lerp(alpha, target, Time.deltaTime * 1);

            float dir = Mathf.Sign(target - alpha);
            float dist = Mathf.Abs(target - alpha);
            if (dist > Time.deltaTime)
            {
                alpha = Mathf.Clamp01(alpha + dir * Time.deltaTime);
            }
            else
            {
                alpha = target;
            }   
                
        }
    }

    private void OnGUI()
    {
        if (image != null)
        {
            GUI.color = new Vector4(1, 1, 1, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), image);
            GUI.color = new Vector4(1, 1, 1, 1);
        }
    }
}
