using System;
using UnityEngine;

public class BlackBorder : MonoBehaviour
{

    public Texture image;
    [Range(0, 1)]
    public float scale = 1;
    [Range(0, 1)]
    public float target = 0;

    public float height = 0;


    private void Update()
    {
       
        if (Input.GetAxisRaw("Focus") == 1)
            target = 1;
        else
            target = 0;

        if (Time.time > .5f)
        {            
            float dir = Mathf.Sign(target - scale);
            float dist = Mathf.Abs(target - scale);
            if (dist > Time.deltaTime)
            {
                scale = Mathf.Clamp01(scale + dir *  6 * Time.deltaTime);
            }
            else
            {
                scale = target;
            }
        }

        height = Screen.height / 12;
    }

    private void OnGUI()
    {
        if(image != null)
        {
            float forceEnable = 1 - Mathf.Clamp01(Time.time / 2);

            GUI.DrawTexture(new Rect(0, 0, Screen.width, height * Mathf.Clamp01(scale + forceEnable)), image);
            GUI.DrawTexture(new Rect(0, Screen.height, Screen.width, -height * Mathf.Clamp01(scale + forceEnable)), image);
        }
    }
}
