using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Breakable
{
    [SerializeField] private AudioClip grassMove;
    [SerializeField] private AudioClip grassCut;
    [SerializeField] private GameObject grassAlive;
    [SerializeField] private GameObject grassDeadParticle;

    [SerializeField] private SpriteRenderer grassDead;

    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
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
            Vector3 diff = (collision.transform.position - transform.position).normalized;
            if (diff.x > 0)
            {
                animator.SetTrigger("FromRight");
            }
            else if (diff.x < 0)
            {
                animator.SetTrigger("FromLeft");
            }
            audioSource.PlayOneShot(grassMove);
        }
    }

    protected override void Dead()
    {
        base.Dead();
        audioSource.PlayOneShot(grassCut);
        grassDeadParticle.SetActive(true);
        grassAlive.SetActive(false);
        grassDead.enabled = true;
    }

}
