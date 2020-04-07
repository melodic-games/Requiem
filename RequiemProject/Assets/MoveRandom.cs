using UnityEngine;

public class MoveRandom : MonoBehaviour
{
    public float value = 100;

    void Start()
    {
        transform.position += new Vector3(Random.Range(value, -value), Random.Range(value, -value), Random.Range(value, -value));
    }
}
