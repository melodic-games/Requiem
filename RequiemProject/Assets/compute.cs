using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class compute : MonoBehaviour
{


    public float Sample100000 = 0;
    public float Sample10000 = 0;
    public float Sample1000 = 0;
    public float Sample100 = 0;
    public float Sample10 = 0;



    // Start is called before the first frame update
    void Start()
    {

        for (int i = 1; i <= 10; i++)
        {
            Sample10 = Mathf.Lerp(Sample10, 1, .1f);
        }

        for (int i = 1; i <= 100; i++)
        {
            Sample100 = Mathf.Lerp(Sample100, 1, .01f);
        }

        for (int i = 1; i <= 1000; i++)
        {
            Sample1000 = Mathf.Lerp(Sample1000, 1, .001f);
        }

        for (int i = 1; i <= 10000; i++)
        {
            Sample10000 = Mathf.Lerp(Sample10000, 1, .0001f);
        }

        for (int i = 1; i <= 100000; i++)
        {
            Sample100000 = Mathf.Lerp(Sample100000, 1, .00001f);
        }

        Debug.Log("Sample10: " + Sample10);
        Debug.Log("Sample100: " + Sample100);
        Debug.Log("Sample1000: " + Sample1000);
        Debug.Log("Sample10000: " + Sample10000);
        Debug.Log("Sample100000: " + Sample100000);

    }

   
}
