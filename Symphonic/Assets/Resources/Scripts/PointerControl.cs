using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointerControl : MonoBehaviour {
    public Transform target;
    public Vector3 offset;

    public SymphonicBehaviour signature;

	
	// Update is called once per frame
	void LateUpdate () {        
        transform.position = target.position + transform.forward * offset.z + transform.up * offset.y + transform.right * offset.x;
        transform.rotation = target.rotation;

        if (signature != null)
        {
            Vector3 upwards = -Camera.main.transform.forward;
            transform.rotation = Quaternion.LookRotation(signature.transform.forward,upwards);
        }
	}
}
