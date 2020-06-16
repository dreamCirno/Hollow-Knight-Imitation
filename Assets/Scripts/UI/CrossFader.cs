using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CrossFader : MonoBehaviour
{
    [SerializeField] private bool fadeInOnStart;
    private Animator crossfader;

    private int animatorFadeInString;
    private int animatorFadeOutString;

    private void Start()
    {
        animatorFadeInString = Animator.StringToHash("CrossfadeIn");
        animatorFadeOutString = Animator.StringToHash("CrossfadeOut");
        if (fadeInOnStart)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        if (crossfader == null)
            crossfader = GetComponent<Animator>();
        crossfader.Play(animatorFadeInString);
    }

    public void FadeOut()
    {
        if (crossfader == null)
            crossfader = GetComponent<Animator>();
        crossfader.Play(animatorFadeOutString);
    }
}
