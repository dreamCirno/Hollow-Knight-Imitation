using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGate : MonoBehaviour
{
    [Header("Attr")]
    [SerializeField] private bool close;
    [Header("Particle System")]
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem raiseDust;

    private int animatorCloseBool;
    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animatorCloseBool = Animator.StringToHash("Close");

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateAnimatorStatement();
    }

    public void Lock()
    {
        close = true;
    }

    public void Unlock()
    {
        close = false;
    }

    private void UpdateAnimatorStatement()
    {
        if (close != animator.GetBool(animatorCloseBool))
        {
            animator.SetBool(animatorCloseBool, close);
        }
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    public void CameraShake()
    {
        // 相机震动
        var shakePreset = ProCamera2DShake.Instance.ShakePresets[0];
        ProCamera2DShake.Instance.Shake(shakePreset);
    }

    public void PlayDustEffect()
    {
        dust.Play();
    }

    public void PlayRaiseDustEffect()
    {
        raiseDust.Play();
    }
}
