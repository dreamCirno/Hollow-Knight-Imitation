using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class HitEffect : MonoBehaviour
{
    Animator effectAnimator;

    private void Start()
    {
        effectAnimator = GetComponent<Animator>();
    }

    public void PlayHitAnimation()
    {
        effectAnimator.Play("Hit");
    }
}
