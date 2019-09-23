using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSize : MonoBehaviour
{

    public SymphonicBehaviour symScript;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float scale = symScript.rbVelocityMagnatude / 10;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
