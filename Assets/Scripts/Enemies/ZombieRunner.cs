using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieRunner : Enemy
{
    [SerializeField] private BoxCollider2D attackRange;
    [SerializeField] float groundCheckDistance, wallCheckDistance, normalSpeed, madSpeed;
    [SerializeField] private Transform groundCheck, wallCheck;
    [SerializeField] private LayerMask whatIsGrounded;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private EnemyState currentState;
    [SerializeField] float hurtForce = 400f, deadForce = 500f;
    [Header("Audio Clip")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] AudioClip[] zombieChases;
    [SerializeField] AudioClip hited;
    private Transform player;
    private Vector2 movement, endPos;
    private HitEffect hit;
    private bool groundDetected, wallDetected, isAttack, isHurt;

    public enum EnemyState
    {
        IDLE, MOVEMENT, ATTACK_READY, ATTACK, HURT, DEAD
    }


    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hit = GetComponentInChildren<HitEffect>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (isDead)
            return;
        CheckIsDead();
        Detect();
        UpdateDirection();
        UpdateStatements();
    }

    private void UpdateStatements()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
            case EnemyState.MOVEMENT:
                EnterMovementState();
                break;
            case EnemyState.ATTACK:
                EnterAttackState();
                break;
            case EnemyState.DEAD:
                EnterDeadState();
                break;
        }
    }

    protected override void UpdateDirection()
    {
        if (transform.localScale.x == -1)
        {
            isFacingLeft = true;
        }
        else if (transform.localScale.x == 1)
        {
            isFacingLeft = false;
        }
    }

    private void Detect()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGrounded);
        wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.left, wallCheckDistance, whatIsWall);
    }

    private void UpdateAnimatorStatement()
    {
        animator.SetBool("Attack", isAttack);
    }

    private void EnterIdleState()
    {

    }
    private void ExitIdleState()
    {

    }

    private void EnterMovementState()
    {
        if (!groundDetected || wallDetected)
        {
            // Flip
            Flip();
        }
        else
        {
            // Move
            movement.Set((isFacingLeft ? -1 : 1) * normalSpeed, rb.velocity.y);
            rb.velocity = movement;
        }
    }
    private void ExitMovementState()
    {

    }

    private void EnterAttackReadyState()
    {
        rb.velocity = Vector2.zero;
        isAttack = true;
        animator.SetTrigger("AttackReady");
        animator.SetBool("Attack", isAttack);
        if (player.position.x - transform.position.x > 0 && isFacingLeft)
        {
            Flip();
        }
        else if (player.position.x - transform.position.x < 0 && !isFacingLeft)
        {
            Flip();
        }
    }

    private void ExitAttackReadyState()
    {

    }

    private void EnterAttackState()
    {
        if (!groundDetected || wallDetected)
        {
            // Flip
            Flip();
            SwitchState(EnemyState.MOVEMENT);
        }
        else
        {
            animator.SetBool("Attack", isAttack);
            // Move
            movement.Set((isFacingLeft ? -1 : 1) * madSpeed, rb.velocity.y);
            rb.velocity = movement;
        }
    }
    private void ExitAttackState()
    {
        isAttack = false;
        animator.SetBool("Attack", isAttack);
    }
    private void EnterHurtState()
    {
        isHurt = true;
        isAttack = false;
        animator.SetBool("Attack", isAttack);
        audioSource.PlayOneShot(hited);
        hit.PlayHitAnimation();
        // 判断角色位置
        Vector2 vector = transform.position - player.position;
        rb.velocity = Vector2.zero;
        if (vector.x > 0)
        {
            rb.AddForce(new Vector2(hurtForce, 0));
        }
        else
        {
            rb.AddForce(new Vector2(-hurtForce, 0));
        }
        SwitchState(EnemyState.ATTACK_READY);
    }
    private void ExitHurtState()
    {
        isHurt = false;
    }

    private void EnterDeadState()
    {
        StartCoroutine(DelayDead());
    }
    private void ExitDeadState()
    {

    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x = vector.x * -1;
        transform.localScale = vector;
    }

    public void SwitchState(EnemyState state)
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                ExitIdleState();
                break;
            case EnemyState.MOVEMENT:
                ExitMovementState();
                break;
            case EnemyState.ATTACK_READY:
                ExitAttackReadyState();
                break;
            case EnemyState.ATTACK:
                ExitAttackState();
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
            case EnemyState.MOVEMENT:
                EnterMovementState();
                break;
            case EnemyState.ATTACK_READY:
                EnterAttackReadyState();
                break;
            case EnemyState.ATTACK:
                EnterAttackState();
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x - wallCheckDistance, wallCheck.position.y));
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector") && !isAttack && !isHurt)
        {
            float movement = attackRange.bounds.size.x / 2;
            if (player.position.x > 0)
            {
                endPos.Set(transform.position.x + movement, transform.position.y);
            }
            else
            {
                endPos.Set(transform.position.x - movement, transform.position.y);
            }
            SwitchState(EnemyState.ATTACK_READY);
        }
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        CheckIsDead();
        if (!isDead)
            SwitchState(EnemyState.HURT);
    }
    protected override void Dead()
    {
        base.Dead();
        SwitchState(EnemyState.DEAD);
    }

    IEnumerator DelayDead()
    {
        hit.PlayHitAnimation();
        Vector3 diff = (player.position - transform.position).normalized;
        rb.velocity = Vector2.zero;
        if (diff.x > 0)
        {
            rb.AddForce(Vector2.left * deadForce);
        }
        else if (diff.x < 0)
        {
            rb.AddForce(Vector2.right * deadForce);
        }
        animator.SetTrigger("Dead");
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayZombieChase()
    {
        int count = zombieChases.Length;
        int r = UnityEngine.Random.Range(0, count);
        audioSource.PlayOneShot(zombieChases[r]);
    }
}
