using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIButton : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public virtual void MousePointerEnter()
    {
        animator.SetBool("selected", true);
    }

    public virtual void MousePointerExit()
    {
        animator.SetBool("selected", false);
    }

    public virtual void MousePointerClick()
    {
        animator.SetTrigger("pressed");
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        FindObjectOfType<AudioManager>().PlayOneShot(audioClip);
    }
}
