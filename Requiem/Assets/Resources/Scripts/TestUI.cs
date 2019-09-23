using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{

    public Transform barTop;
    public Transform barBottom;
    public float scale; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        scale = Input.GetAxis("Focus");

        barTop.position = new Vector3(0, -25.5f * scale, 0);
        barBottom.position = new Vector3(0, 25.5f * scale, 0);
    }
}
