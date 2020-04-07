using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class WaypointGUI : MonoBehaviour
{
    public Texture image;

    public Vector3[] positionWaypoints;
    public Transform[] objectWaypoints;

    public Camera cam;

    public GUIContent content;

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnGUI()
    {

        foreach (Vector3 waypoint in positionWaypoints)
        {   
            Vector2 size = new Vector2(250,250);               
            
            Vector2 position = cam.WorldToScreenPoint(waypoint);
            
            //position = cam.WorldToViewportPoint(waypoint);
            Rect rect = new Rect(position.x,Screen.height - position.y,size.x,size.y);                       
            //GUI.DrawTexture(rect, image);

            GUI.Label(rect, content);
        }

        foreach (Transform waypoint in objectWaypoints)
        {
            Vector2 size = new Vector2(250, 250);

            Vector2 position = cam.WorldToScreenPoint(waypoint.position);

            //position = cam.WorldToViewportPoint(waypoint);
            Rect rect = new Rect(position.x, Screen.height - position.y, size.x, size.y);
            //GUI.DrawTexture(rect, image);

            GUI.Label(rect, content);
        }

    }
}
