using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaTriggerControl : MonoBehaviour {

    public Text text;
    public string textText;
    public Animator anim;
    private float checkTime = 0;

    void OnTriggerEnter(Collider other)
    {
        if(Time.time > checkTime)
        if(other.tag == "MainCamera")
        {
            checkTime = Time.time + 60;
            anim.SetTrigger("Play");
            text.text = textText;
        }
        
    }

}
