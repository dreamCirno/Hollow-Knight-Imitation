using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMenuBackButton : UIButton
{
    [SerializeField] Animator audioMenuAnimator;

    public override void MousePointerClick()
    {
        base.MousePointerClick();
        audioMenuAnimator.Play("FadeOut");
    }

}
