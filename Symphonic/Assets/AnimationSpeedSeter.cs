using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedSeter : MonoBehaviour
{
    public Animator animator;
    public float desiredSpeed;
    private void Update()
    {
        animator.speed = desiredSpeed;
    }
   
}
