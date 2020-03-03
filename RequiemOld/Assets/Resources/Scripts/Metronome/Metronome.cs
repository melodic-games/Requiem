using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {

    public int Base = 4;
    public int Step = 4;
    public float BPM = 120;
    public int CurrentStep = 1;
    public int CurrentMeasure;

    private float interval;
    private float nextTime;

    public GameObject[] targets;

    private void Start()
    {
        StartMetronome();
    }

    private void Update()
    {
        var multiplier = Base / 4;
        var tmpInterval = 60 / BPM;
        interval = tmpInterval / multiplier;       
    }

    public void StartMetronome()
    {
        StopCoroutine("DoTick");
        CurrentStep = 1;
        nextTime = Time.time; // set the relative time to now
        StartCoroutine("DoTick");
    }

    IEnumerator DoTick() // yield methods return IEnumerator
    {
        for (; ; )
        {          
            // do something with this beat
            if (targets != null)
            {
                foreach (GameObject target in targets)
                {
                    target.SendMessage("Beat", CurrentStep);
                }
            }
                
            // add interval to our relative time
            nextTime += interval; 
            yield return new WaitForSeconds(nextTime - Time.time); // wait for the difference delta between now and expected next time of hit
            CurrentStep++;
            if (CurrentStep > Step)
            {
                CurrentStep = 1;
                CurrentMeasure++;
            }
        }
    }
}
