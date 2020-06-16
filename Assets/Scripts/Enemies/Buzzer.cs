using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Buzzer : Enemy
{
    public float movementSpeed;

    [Header("Audio Clip")]
    [SerializeField] AudioClip[] startles;
    [SerializeField] AudioClip enemyDeathSword;
    [SerializeField] private EnemyState currentState;
    [SerializeField] private float maxChaseDistance;
    [SerializeField] private float hurtForce, deadForce;
    private PolyNavAgent agent
    {
        get { return _agent != null ? _agent : _agent = GetComponent<PolyNavAgent>(); }
    }
    private PolyNavAgent _agent;
    private Transform player;
    private Vector3 originPosition;
    private AudioSource audioSource;
    private HitEffect hit;
    private Rigidbody2D rb;
    public enum EnemyState
    {
        IDLE, SCARED, PATHFINDING, HURT, DEAD,
    }
    private void Start()
    {
        originPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        hit = GetComponentInChildren<HitEffect>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable()
    {
        agent.OnDestinationInvalid += CancelPathFinding;
    }

    private void OnDisable()
    {
        agent.OnDestinationInvalid -= CancelPathFinding;
    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x = vector.x * -1;
        transform.localScale = vector;
    }

    private void Update()
    {
        if (isDead)
            return;
        CheckIsDead();
        UpdateDirection();
        UpdateStatements();
    }

    protected override void UpdateDirection()
    {
        if (transform.localScale.x == 1)
        {
            isFacingLeft = false;
        }
        else if (transform.localScale.x == -1)
        {
            isFacingLeft = true;
        }
    }

    private void UpdateStatements()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
            case EnemyState.PATHFINDING:
                EnterPathFindingState();
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
            case EnemyState.SCARED:
                ExitScaredState();
                break;
            case EnemyState.PATHFINDING:
                ExitPathFindingState();
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
            case EnemyState.SCARED:
                EnterScaredState();
                break;
            case EnemyState.PATHFINDING:
                EnterPathFindingState();
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
    private void EnterScaredState()
    {
        PlayStartleAudioClip();
        animator.SetTrigger("Scared");
    }
    private void ExitScaredState()
    {

    }
    private void EnterPathFindingState()
    {
        if (isDead)
            return;
        if (player.position.x - transform.position.x > 0 && isFacingLeft)
        {
            Flip();
        }
        else if (player.position.x - transform.position.x < 0 && !isFacingLeft)
        {
            Flip();
        }
        if (agent.remainingDistance < maxChaseDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            SwitchState(EnemyState.IDLE);
        }
    }
    private void ExitPathFindingState()
    {
        if (isDead)
            return;
        animator.SetTrigger("Idle");
        agent.SetDestination(originPosition);
    }
    private void EnterHurtState()
    {
        hit.PlayHitAnimation();
        // 判断角色位置
        Vector2 vector = transform.position - player.position;
        animator.SetTrigger("Hurt");
        rb.velocity = Vector2.zero;
        if (vector.x > 0)
        {
            rb.AddForce(new Vector2(hurtForce, 0));
        }
        else
        {
            rb.AddForce(new Vector2(-hurtForce, 0));
        }
    }
    private void ExitHurtState()
    {

    }
    private void EnterDeadState()
    {
        GetComponent<PolyNavAgent>().enabled = false;
        hit.PlayHitAnimation();
        audioSource.PlayOneShot(enemyDeathSword);
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
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector") && currentState == EnemyState.IDLE)
        {
            SwitchState(EnemyState.SCARED);
        }
    }
    private void PlayStartleAudioClip()
    {
        int random = Random.Range(0, startles.Length);
        audioSource.PlayOneShot(startles[random]);
    }

    private void CancelPathFinding()
    {
        agent.Stop();
        SwitchState(EnemyState.IDLE);
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
}
