using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collapser : MonoBehaviour
{
    CharacterController2D characterController;
    GameManager gameManager;
    Animator animator;
    AudioSource audioPlayer;
    Collider2D[] cols;
    bool isTrigger;

    private void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        characterController = FindObjectOfType<CharacterController2D>();
        cols = GetComponentsInChildren<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTrigger && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            isTrigger = true;
            animator.Play("Fall");
            audioPlayer.Play();
        }
    }

    public void StartShake()
    {
        var shakePreset = ProCamera2DShake.Instance.ConstantShakePresets[0];
        ProCamera2DShake.Instance.ConstantShake(shakePreset);
    }

    public void StopShake()
    {
        ProCamera2DShake.Instance.StopConstantShaking();
    }

    public void StopInput()
    {
        gameManager.SetEnableInput(false);
        characterController.StopHorizontalMovement();
    }

    public void ResumeInput()
    {
        gameManager.SetEnableInput(true);
    }

    public void DisableColliders()
    {
        foreach (Collider2D c in cols)
        {
            c.enabled = false;
        }
    }
}
