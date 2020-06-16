using UnityEngine;

public class RemindTuteDoor : TuteDoor
{
    [SerializeField] Animator animator;
    [SerializeField] Animator introText;

    bool isHidden;

    void Update()
    {
        CheckIsDead();
        if (!sprite.enabled && !isHidden)
        {
            isHidden = true;
            animator.SetTrigger("FadeOut");
        }
    }

    protected override void Dead()
    {
        base.Dead();
        Invoke("DisplayIntroText", 2);
    }

    private void DisplayIntroText()
    {
        introText.enabled = true;
    }
}
