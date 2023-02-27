using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlaySpeedModifier : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    void Awake()
    {
        animator.SetFloat("offset", Random.Range(0f, 1f));
        animator.speed = 1 + (Random.Range(0, 3) - 1) * 0.05f;
        animator.Play(0);
    }
}
