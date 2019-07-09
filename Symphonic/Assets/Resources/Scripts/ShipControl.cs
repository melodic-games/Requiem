using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour {

    private float timer = 0;
    public Vector3 dir;
    public Transform anchor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        timer += -1 * Time.deltaTime;

        if(timer < 0)
        {
            timer = 1;

            dir = (new Vector3(anchor.transform.position.x + Random.Range(-30, 30),50, anchor.transform.position.z + Random.Range(-30, 30)) - transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(dir.normalized, Vector3.up),Time.deltaTime * .2f);
	}
}
