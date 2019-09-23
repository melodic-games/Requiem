using System;
using UnityEngine;

public class BraceBorder : MonoBehaviour
{

    public Texture image;
    [Range(0, 1)]
    public float scale = 1;
    [Range(0, 1)]
    public float target = 0;

    public float height = 0;

    public float displacement;


    private void Update()
    {
       
        if (Input.GetAxisRaw("Focus") == 1)
            target = 1;
        else
            target = 0;

        
        {            
            float dir = Mathf.Sign(target - scale);
            float dist = Mathf.Abs(target - scale);
            if (dist > Time.deltaTime)
            {
                scale =  Mathf.Clamp01(scale + dir *  6 * Time.deltaTime);
            }
            else
            {
                scale = target;
            }
        }

        
    }

    private void OnGUI()
    {
        if(image != null)
        {

            float displacement = height * (1-scale);  

            Rect rect1 = new Rect(0, - displacement + height, Screen.width, -height);
            displacement = height * scale; 
            Rect rect2 = new Rect(0, Screen.height - displacement, Screen.width, height);

            GUI.DrawTexture(rect1, image);
            GUI.DrawTexture(rect2, image);
        }
    }
}
