using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 1;

    void Update()
    {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(speed * Time.deltaTime, axis);
    }
}
