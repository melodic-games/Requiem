using System;
using UnityEngine;

public class BlackBorder : MonoBehaviour
{

    public Texture image;
    [Range(0, 1)]
    public float scale = 1;
    public float height = 0;

    private void Update()
    {
        scale = Mathf.Lerp(scale, Input.GetAxis("Focus"), Time.deltaTime * 5);
        height = Screen.height / 7;
    }

    private void OnGUI()
    {
        if(image != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, height * scale), image);
            GUI.DrawTexture(new Rect(0, Screen.height, Screen.width, -height * scale), image);
        }
    }
}
