using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionController : MonoBehaviour
{

    public Transform target;
    public float distance;
    public Vector3 direction;

    private void Start()
    {
        direction = (transform.position - target.position).normalized;
        distance = (transform.position - target.position).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        direction = Quaternion.Euler(.1f * Time.deltaTime, .1f * Time.deltaTime, .2f * Time.deltaTime) * direction;

        Vector3 displacement = direction * distance;

        transform.position = target.transform.position + displacement;
        transform.rotation = Quaternion.Euler(0, Time.time * 15, 0);
    }
}
