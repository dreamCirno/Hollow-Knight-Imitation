using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    [SerializeField] AudioClip hited;
    [SerializeField] AudioClip enemyDeathSword;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float hurtForce = 5f;
    [SerializeField] float deadForce = 300f;
    [SerializeField] Collider2D facingDetector;
    [SerializeField] ContactFilter2D contactFilter;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float circleRadius;

    HitEffect hit;
    AudioSource audioPlayer;
    Rigidbody2D rb;
    bool forceMovement = true;
    bool isGrounded;

    private void Start()
    {
        canMove = true;

        rb = GetComponent<Rigidbody2D>();
        audioPlayer = GetComponent<AudioSource>();
        hit = GetComponentInChildren<HitEffect>();
    }

    private void Update()
    {
        CheckIsDead();
        FacingDetect();
        UpdateDirection();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    protected override void UpdateDirection()
    {
        if (transform.lossyScale.x == 1)
        {
            isFacingLeft = false;
        }
        else if (transform.lossyScale.x == -1)
        {
            isFacingLeft = true;
        }
    }

    private void FacingDetect()
    {
        if (isDead)
            return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.transform.position, circleRadius, groundLayer);
        if (!isGrounded)
        {
            Flip();
        }
        else
        {
            int count = Physics2D.OverlapCollider(facingDetector, contactFilter, new List<Collider2D>());
            if (count > 0)
            {
                Flip();
            }
        }
    }

    private void Movement()
    {
        if (!isDead && forceMovement)
        {
            if (canMove)
            {
                if (isFacingLeft)
                {
                    rb.velocity = Vector2.left * movementSpeed;
                }
                else
                {
                    rb.velocity = Vector2.right * movementSpeed;
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x = vector.x * -1;
        transform.localScale = vector;
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        // 受伤动画
        Vector2 vector = transform.position - attackPosition.position;
        StartCoroutine(DelayHurt(vector));
        hit.PlayHitAnimation();
    }

    IEnumerator DelayHurt(Vector2 vector)
    {
        // 播放打击声音
        audioPlayer.PlayOneShot(hited);
        rb.velocity = Vector2.zero;
        forceMovement = false;
        if (vector.x > 0)
        {
            rb.AddForce(new Vector2(hurtForce, 0));
        }
        else
        {
            rb.AddForce(new Vector2(-hurtForce, 0));
        }
        yield return new WaitForSeconds(0.3f);
        forceMovement = true;
    }

    protected override void Dead()
    {
        base.Dead();
        StartCoroutine(DelayDead());
    }

    IEnumerator DelayDead()
    {
        audioPlayer.PlayOneShot(enemyDeathSword);
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
            animator.SetTrigger("Dead");
        }
        yield return new WaitForSeconds(0.2f);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
    }

    public void StopAudioSource()
    {
        audioPlayer.clip = null;
    }
}
