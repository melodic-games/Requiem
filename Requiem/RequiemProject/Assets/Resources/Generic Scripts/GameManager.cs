using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float pauseWaitTime = 0;
    private float pauseWaitCounter = 0;

    private void Start()
    {
        instance = this;
    }

    public void PauseGameForDuration(float pauseWaitTime)
    {
        Time.timeScale = 0;
        this.pauseWaitTime = pauseWaitTime;
        pauseWaitCounter = 0;
    }

    public void PauseToggle()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

    private void Update()
    {

        if (instance == null) instance = this;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseToggle();
        }

        pauseWaitCounter += Time.unscaledDeltaTime;

        if (pauseWaitCounter > pauseWaitTime)
        {
            //Time.timeScale = 1;
        }
    }

}
