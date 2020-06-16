using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveButton : UIButton
{
    [SerializeField] CanvasGroup saveProfileScreen;

    private GameSceneManager gameSceneManager;

    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
    }

    public override void MousePointerEnter()
    {
        base.MousePointerEnter();
    }

    public override void MousePointerExit()
    {
        base.MousePointerExit();
    }

    public override void MousePointerClick()
    {
        base.MousePointerClick();
        GetComponent<AudioSource>().Play();
        gameSceneManager.DelayLoadNextScene();
    }
}
