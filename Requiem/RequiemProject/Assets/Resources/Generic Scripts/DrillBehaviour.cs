using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillBehaviour : MonoBehaviour {

    public float offset = 35.47f;
    RaycastHit hit;
    public Transform targetRock;
    public Transform target;

	// Use this for initialization
	void Start () {
		target = GameObject.FindGameObjectWithTag("PlayerCharacterAnchorPoint").transform;
    }
	
	// Update is called once per frame
	void Update () {

        //if (Physics.Raycast(transform.position, (targetRock.position - transform.position).normalized, out hit))
        //{

        //    Debug.DrawLine(transform.position, hit.point);

        //    transform.position = Vector3.Lerp(transform.position, hit.point + (transform.forward * offset), Time.deltaTime * 3f);
        //    //transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(hit.normal),Time.deltaTime);
        //}

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position).normalized, Time.deltaTime);

    }
}
