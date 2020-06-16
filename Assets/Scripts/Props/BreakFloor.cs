using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakFloor : Breakable
{
    Animator animator;
    AudioSource audioPlayer;
    [SerializeField] ParticleSystem[] particleHits;
    [SerializeField] ParticleSystem[] particleWood;
    [SerializeField] ParticleSystem[] particleBits;

    private int maxHealthIndex;

    private void Start()
    {
        maxHealthIndex = health - 1;
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();

        animator.SetInteger("Health", health);
    }

    private void Update()
    {
        CheckIsDead();
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        animator.SetInteger("Health", health);
    }

    public void Destory(AudioClip audioClip)
    {
        particleHits[maxHealthIndex - health].Play();
        particleWood[maxHealthIndex - health].Play();
        particleBits[maxHealthIndex - health].Play();
        audioPlayer.PlayOneShot(audioClip);
    }
}
