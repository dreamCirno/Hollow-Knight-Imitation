using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : UIButton
{
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] MenuButtonController.MainMenuButton mainMenuButton;

    private void Update()
    {
        if (menuButtonController.index == (int)mainMenuButton)
        {
            animator.SetBool("selected", true);
        }
        else
        {
            animator.SetBool("selected", false);
        }
    }

    public override void MousePointerEnter()
    {
        menuButtonController.index = (int)mainMenuButton;
        base.MousePointerEnter();
    }

    public override void MousePointerExit()
    {
        base.MousePointerExit();
    }

    public override void MousePointerClick()
    {
        base.MousePointerClick();
        menuButtonController.Pressed();
    }
}
