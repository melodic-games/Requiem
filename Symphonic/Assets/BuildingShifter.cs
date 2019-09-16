using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingShifter : MonoBehaviour
{

    public Vector3 targetScale = new Vector3(25,.00001f,25);
    public Vector3 targetPos = Vector3.zero;
    Vector3 startPos;
    public float timer;
    public float timerMax = 4;


    private void Start()
    {
        startPos = transform.position;
    }

    void findScale()
    {
       
        targetPos.x = Random.Range(50, -50);
        //targetPos.y = Random.Range(20, -20);
        targetPos.z = Random.Range(50, -50);

        targetScale.y = Random.Range(5, 500);     
    }

    // Update is called once per frame
    void Update()
    {
        
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 3);
            transform.position = Vector3.Lerp(transform.position, startPos + targetPos,Time.deltaTime * 3);
        }

        if(timer < 0)
        {
            timer = timerMax;
            findScale();
        }

        timer += -Time.deltaTime;


    }
}
