using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingReticle : MonoBehaviour
{
    public Texture image;
    public Vector2 position;
    public Vector2 size = new Vector2(64,64);
    public Vector3 worldToScreenVector;
    public CameraControl cameraControl;
    public bool tracking = false;

    private void Start()
    {
        cameraControl = FindObjectOfType<CameraControl>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //position.x = Screen.width / 2;
        //position.y = Screen.height / 2;

        if(cameraControl != null)
        tracking = cameraControl.trackingTarget;

        worldToScreenVector = Camera.main.WorldToScreenPoint(cameraControl.cameraTrackingPosition);

        position.x = worldToScreenVector.x;
        position.y = Screen.height - worldToScreenVector.y;

    }

    private void OnGUI()
    {
        if(tracking)
        if(worldToScreenVector.z > 1)
        if(image != null)
        GUI.DrawTexture(new Rect(position - size/2, size), image);
    }

}
