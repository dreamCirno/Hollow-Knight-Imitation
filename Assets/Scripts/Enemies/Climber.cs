using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climber : Enemy
{
    [SerializeField] float idleTime = 0.5f;
    [SerializeField] int deadForce;
    [SerializeField] AudioClip enemyDeathSword;
    [SerializeField] Animator enemy;

    private Rigidbody2D rb;
    private AudioSource audioPlayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioPlayer = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckIsDead();
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        StartCoroutine(DelayHurt());
    }

    IEnumerator DelayHurt()
    {
        animator.speed = 0.0f;
        yield return new WaitForSeconds(idleTime);
        animator.speed = 1.0f;
    }

    protected override void Dead()
    {
        base.Dead();
        StartCoroutine(DelayDead());
    }

    IEnumerator DelayDead()
    {
        audioPlayer.PlayOneShot(enemyDeathSword);
        animator.enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Vector3 diff = (GameObject.FindWithTag("Player").transform.position - transform.position).normalized;
        rb.velocity = Vector2.zero;
        if (diff.x > 0)
        {
            rb.AddForce(Vector2.left * deadForce);
        }
        else if (diff.x < 0)
        {
            rb.AddForce(Vector2.right * deadForce);
        }
        if (animator != null)
        {
            enemy.SetTrigger("Dead");
        }
        yield return new WaitForSeconds(0.2f);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
    }
}