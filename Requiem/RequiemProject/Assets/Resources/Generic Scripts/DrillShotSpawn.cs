using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillShotSpawn : MonoBehaviour {

    public GameObject prefab;
    public Transform anchor;

    float timer = 10;
    float shots;

    private void Update()
    {


        timer -= Time.deltaTime;


        if (timer < 0 && shots < 50)
        for (int count = 5; count > 0; count--)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
            obj.transform.rotation *= Quaternion.Euler(4 * count, 0, 0);
            count--;
            shots++;
            timer = 5;
        }
           

        if (anchor != null) {
            Vector3 vec = anchor.right * Mathf.Sin(Time.time * 2) + anchor.up * Mathf.Cos(Time.time * 2);
            transform.rotation = Quaternion.LookRotation(vec);
        }
    }




}
