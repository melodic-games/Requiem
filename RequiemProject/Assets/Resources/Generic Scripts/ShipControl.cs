using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour {

    public Vector3 dir;
    public Transform anchor;
    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        dir = (anchor.transform.position - transform.position).normalized; //(new Vector3(anchor.transform.position.x + Random.Range(-30, 30),250, anchor.transform.position.z + Random.Range(-30, 30)) - transform.position);
                   
        float t = Mathf.InverseLerp(100, 150, Vector3.Distance(transform.position, anchor.transform.position));
        float scale = Mathf.Lerp(0, 90, t);
            
        rb.AddForce(Vector3.Lerp(dir, transform.forward,.5f) * scale * Mathf.Clamp01(Vector3.Dot(dir, transform.forward) + .25f),ForceMode.Acceleration);

        Quaternion targetRotation = Quaternion.Lerp(Quaternion.LookRotation(dir.normalized, Vector3.up), Quaternion.LookRotation(Vector3.Cross(transform.right, Vector3.up)),Mathf.InverseLerp(120,80, Vector3.Distance(transform.position, anchor.transform.position)));

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * .2f);
              
    }

    

}
