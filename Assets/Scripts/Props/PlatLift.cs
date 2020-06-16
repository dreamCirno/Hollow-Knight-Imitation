using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatLift : MonoBehaviour
{
    public bool isAnimating;

    Animator animator;
    AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"))
        {
            if (!isAnimating)
            {
                isAnimating = true;
                audioSource.Play();
                animator.SetTrigger("Enter");
            }
        }
    }

}
