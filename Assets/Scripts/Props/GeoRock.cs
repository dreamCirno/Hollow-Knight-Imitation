using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoRock : Breakable
{
    [SerializeField] GameObject coin;
    [SerializeField] int minSpawnCount = 1;
    [SerializeField] int maxSpawnCount = 4;
    [SerializeField] float maxBumpHorizontalForce = 400;
    [SerializeField] float minBumpVerticalForce = 600;
    [SerializeField] float maxBumpVerticalForce = 800;
    [SerializeField] ParticleSystem leftParticle;
    [SerializeField] ParticleSystem rightParticle;
    [SerializeField] ParticleSystem upParticle;
    [SerializeField] ParticleSystem burstRocks;

    private Animator animator;
    private AudioSource audioSource;
    private int animationHurtTrigger;
    private int animationDeadTrigger;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        animationHurtTrigger = Animator.StringToHash("Hurt");
        animationDeadTrigger = Animator.StringToHash("Dead");
    }

    private void Update()
    {
        CheckIsDead();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Hurt(1, FindObjectOfType<CharacterAttack>().transform);
        }
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        Vector2 vector = attackPosition.position - transform.position;
        if (vector.x > 0)
        {
            leftParticle.Play();
        }
        else
        {
            rightParticle.Play();
        }
        SpawnCoins();
        animator.SetTrigger(animationHurtTrigger);
    }

    protected override void Dead()
    {
        base.Dead();
        leftParticle.Play();
        rightParticle.Play();
        upParticle.Play();
        burstRocks.Play();
        animator.SetTrigger(animationDeadTrigger);
    }

    public void PlayHurtAudio(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    private void SpawnCoins()
    {
        int randomCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < randomCount; i++)
        {
            GameObject geo = Instantiate(coin, transform.position, Quaternion.identity, transform) as GameObject;
            Vector2 force = new Vector2(Random.Range(-maxBumpHorizontalForce, maxBumpHorizontalForce), Random.Range(minBumpVerticalForce, maxBumpVerticalForce));
            geo.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Force);
        }
    }
}
