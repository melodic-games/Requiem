using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEnabled : MonoBehaviour {
    public GameObject target;
	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("f")) {
            if (target.activeSelf)
            {
                target.SetActive(false);
            }
            else
            {
                target.SetActive(true);
            }

        }
	}
}
