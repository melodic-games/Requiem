using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayMenu : MonoBehaviour
{

    public GameObject pauseText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
            pauseText.SetActive(true);
        else
            pauseText.SetActive(false);
    }
}
