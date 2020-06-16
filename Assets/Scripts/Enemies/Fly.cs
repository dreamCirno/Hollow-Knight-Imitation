using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Enemy
{
    public enum EnemyState
    {
        IDLE, HURT, DEAD,
    }

    [SerializeField] private float movementHorizontalSpeed, movementVerticalSpeed;
    [SerializeField] private float flipIntervalTime;
    [SerializeField] private EnemyState currentState;
    [SerializeField] AudioClip enemyDamage;
    [SerializeField] AudioClip enemyDeathSword;
    [SerializeField] private float hurtForce, deadForce;

    private Transform player;
    private Rigidbody2D rb;
    private AudioSource audioPlayer;
    private HitEffect hit;
    private float lastFlipTime;

    private void Start()
    {
        canMove = true;

        rb = GetComponent<Rigidbody2D>();
        audioPlayer = GetComponent<AudioSource>();
        hit = GetComponentInChildren<HitEffect>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (isDead)
            return;
        CheckIsDead();
        UpdateDirection();
        UpdateStatements();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void UpdateStatements()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
        }
    }

    public void SwitchState(EnemyState state)
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                ExitIdleState();
                break;
            case EnemyState.HURT:
                ExitHurtState();
                break;
            case EnemyState.DEAD:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
            case EnemyState.HURT:
                EnterHurtState();
                break;
            case EnemyState.DEAD:
                EnterDeadState();
                break;
        }

        currentState = state;
    }

    private void EnterIdleState()
    {

    }
    private void ExitIdleState()
    {

    }
    private void EnterHurtState()
    {
        hit.PlayHitAnimation();
        // 播放受伤音效
        audioPlayer.PlayOneShot(enemyDamage);
        SwitchState(EnemyState.IDLE);
    }
    private void ExitHurtState()
    {

    }
    private void EnterDeadState()
    {
        hit.PlayHitAnimation();
        audioPlayer.PlayOneShot(enemyDeathSword);
        Vector3 diff = (player.position - transform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 3;
        if (diff.x > 0)
        {
            rb.AddForce(Vector2.left * deadForce);
        }
        else if (diff.x < 0)
        {
            rb.AddForce(Vector2.right * deadForce);
        }
        animator.SetTrigger("Dead");
        Destroy(gameObject, 3f);
    }
    private void ExitDeadState()
    {

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time > lastFlipTime + flipIntervalTime)
        {
            lastFlipTime = Time.time;

            if (collision.contacts[0].normal.y != 0)
            {
                movementVerticalSpeed *= -1;
            }
            else if (collision.contacts[0].normal == Vector2.right || collision.contacts[0].normal == Vector2.left)
            {
                Flip();
            }
        }
    }

    private void Movement()
    {
        if (canMove)
        {
            rb.velocity = new Vector2(movementHorizontalSpeed, movementVerticalSpeed);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x *= -1;
        transform.localScale = vector;
    }

    protected override void UpdateDirection()
    {
        if (transform.lossyScale.x > 0 && movementHorizontalSpeed > 0)
        {
            isFacingLeft = false;
            movementHorizontalSpeed = Math.Abs(movementHorizontalSpeed) * -1;
        }
        else if (transform.lossyScale.x < 0 && movementHorizontalSpeed < 0)
        {
            isFacingLeft = true;
            movementHorizontalSpeed = Math.Abs(movementHorizontalSpeed);
        }
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        EnterHurtState();
    }

    protected override void Dead()
    {
        base.Dead();
        SwitchState(EnemyState.DEAD);
    }

    public bool GetDeadStatment()
    {
        return isDead;
    }
}
