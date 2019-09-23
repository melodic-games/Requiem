using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillShotSpawn : MonoBehaviour {

    public GameObject prefab;
    public Transform anchor;

	// Use this for initialization
	void Start () {
        StartCoroutine(Shoot());
	}

    private void Update()
    {

        

        if (anchor != null) {
            Vector3 vec = anchor.right * Mathf.Sin(Time.time * 2) + anchor.up * Mathf.Cos(Time.time * 2);
            transform.rotation = Quaternion.LookRotation(vec);
        }
    }


    IEnumerator Shoot()
    {
        while (true)
        {
            int shots = 10;

            while (shots > 0)
            {
              GameObject obj = Instantiate(prefab);
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;
                //obj.transform.rotation *= Quaternion.Euler(15 * shots, 0, 0);
                shots--;
                yield return new WaitForSeconds(.1f);
            }

            yield return new WaitForSeconds(10);
        }
      
    }

}
