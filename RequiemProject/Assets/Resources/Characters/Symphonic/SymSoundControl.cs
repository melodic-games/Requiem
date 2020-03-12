using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SymSoundControl : MonoBehaviour
{

    public BaseCharacterController controller;
    public AudioManager audioManager; 

    // Start is called before the first frame update
    void Start()
    {

        controller = GetComponent<BaseCharacterController>();

        audioManager = FindObjectOfType<AudioManager>();

    }

    // Update is called once per frame
    void Update()
    {

        if (controller.rbVelocityMagnitude > controller.groundRunSpeed)
        {
            audioManager.theme.volume = Mathf.MoveTowards(audioManager.theme.volume, 1, Time.deltaTime * .1f);
        }
        else
        {
            audioManager.theme.volume = Mathf.MoveTowards(audioManager.theme.volume, 0, Time.deltaTime * .1f);
        }
    }


}
