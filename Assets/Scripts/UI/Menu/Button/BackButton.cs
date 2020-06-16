using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : UIButton
{
    public Animator saveProfileScreenAnimator;
    public Animator mainMenuScreenAnimator;
    public Animator logoTitleAnimator;

    public override void MousePointerClick()
    {
        base.MousePointerClick();
        StartCoroutine(DelayDisplayMainMenuScreen());
    }

    IEnumerator DelayDisplayMainMenuScreen()
    {
        saveProfileScreenAnimator.Play("FadeOut");
        yield return new WaitForSeconds(1f);
        mainMenuScreenAnimator.Play("FadeIn");
        logoTitleAnimator.Play("FadeIn");
    }
}
