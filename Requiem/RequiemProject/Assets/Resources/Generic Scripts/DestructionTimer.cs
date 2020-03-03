using UnityEngine;

public class DestructionTimer : MonoBehaviour
{
    public float Destroytime = 0;

    void Start()
    {
        Destroy(gameObject,Destroytime);
    }

}
