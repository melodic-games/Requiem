using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTrailGenerator : MonoBehaviour {

    public Vector3 windDirection;
    public float windSpeed = 20;
    public GameObject trailPrefab;
    public Transform origin;
    public Rigidbody originRb;
    private WindTrailLogic trailLogic;   
	
	void Start () {
        originRb = origin.GetComponent<Rigidbody>();
        StartCoroutine(SpawnTrails());
	}
	
    void OnDestroy()
    {
        StopCoroutine(SpawnTrails());
    }

    IEnumerator SpawnTrails()
    {
        while(true)
        {
            GameObject trail = Instantiate(trailPrefab);
            trail.transform.rotation = origin.rotation;
            //trail.transform.rotation = Quaternion.LookRotation(windDirection);
            //trail.transform.parent = origin;

            float xRot = Random.Range(0, 360);
            float yRot = Random.Range(0, 360);

            float cosx = Mathf.Cos(xRot * Mathf.Deg2Rad);

            Vector3 pos = new Vector3(Mathf.Sin(yRot * Mathf.Deg2Rad) * cosx, Mathf.Sin(xRot * Mathf.Deg2Rad), Mathf.Cos(yRot * Mathf.Deg2Rad) * cosx) * Random.Range(5, 10);

            trail.transform.position = origin.transform.position + pos;
            trailLogic = trail.GetComponent<WindTrailLogic>();

            trailLogic.speed = originRb.velocity.magnitude + 30;
            trailLogic.duration = Random.Range(2, 5);
            trailLogic.effector = origin;

            yield return new WaitForSeconds(Random.Range(1, 1));
        }
    }
}
