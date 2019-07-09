using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadAndSetUI : MonoBehaviour {

    public BaseSignature signature;
    public Slider slider;
   	
	// Update is called once per frame
	void Update () {
        slider.value = signature.boostEnergy;
	}
}
