using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class PointerScript : MonoBehaviour
{
    public Transform thecamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = thecamera.position + thecamera.forward * 1;
        Vector3 forward = transform.forward;
        transform.rotation = Quaternion.LookRotation(forward, thecamera.up);
    }
}
