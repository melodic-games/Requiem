using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialColor : MonoBehaviour {
    public Material material;
    public float emission = 1f;
    public Rigidbody rb;
    public float localangularvelocityz;
    [ColorUsage(true, true)] public Color color;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        localangularvelocityz = rb.angularVelocity.magnitude;//Mathf.Abs(transform.InverseTransformDirection(rb.angularVelocity).z);        

        float scale = Mathf.Lerp(0, 1, (localangularvelocityz-15) / 5);
        
        if (scale > 0)
            emission += .5f * Time.deltaTime;
        else
            emission -= .5f * Time.deltaTime;

        emission = Mathf.Clamp(emission, 0, 1);

        Color finalColor = color * emission;//Mathf.LinearToGammaSpace(emission);

        material.SetColor("_EmissionColor", finalColor);
    }
}
