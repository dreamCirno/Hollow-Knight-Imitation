using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownGrass : Breakable
{
    [SerializeField] private AudioClip grassMove;
    [SerializeField] private AudioClip grassCut;
    [SerializeField] private SpriteRenderer grassAlive;
    [SerializeField] private GameObject grassDeadParticle;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }
    private void Update()
    {
        CheckIsDead();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"))
        {
            audioSource.PlayOneShot(grassMove);
        }
    }

    protected override void Dead()
    {
        base.Dead();
        audioSource.PlayOneShot(grassCut);
        grassDeadParticle.SetActive(true);
        grassAlive.enabled = false;
    }
}
