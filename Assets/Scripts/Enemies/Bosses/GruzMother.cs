using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruzMother : Enemy
{
    [Header("Attrs")]
    public float speed = 1f;
    [SerializeField] private EnemyState currentState;
    [SerializeField] private bool isHitWall, isDesc;
    [SerializeField] private Transform[] crashPoints;
    [SerializeField] private Transform deathCrashPoint;
    [SerializeField]
    private int currentCrashPoint, crashCount, maxCrashCount;
    [SerializeField] private GameObject coliisionEffect;
    [SerializeField] private GameObject flySpawn;
    [SerializeField] private float deathGravity;
    [SerializeField] private float deathUpForce;
    [Header("Particle System")]
    [SerializeField] private ParticleSystem snore;
    [SerializeField] private ParticleSystem gushing;
    [SerializeField] private ParticleSystem boom;
    [Header("Audio Clips")]
    [SerializeField] private AudioClip enemyDamage;
    [SerializeField] private AudioClip snoreStartle;
    [SerializeField] private AudioClip wallHit;
    [SerializeField] private AudioClip bossDefeat;
    [SerializeField] private AudioClip bossExplore;
    [SerializeField] private AudioClip bossFinalHit;
    [SerializeField] private AudioClip bossGushing;
    [Header("Requirement")]
    [SerializeField] private AudioSource mainSource;
    [SerializeField] private AudioSource effectAudio;
    [SerializeField] private AudioSource snoreAudio;
    [SerializeField] private AudioSource attackBGM;
    [SerializeField] private BattleGate leftBattleGate;
    [SerializeField] private BattleGate rightBattleGate;
    private PolyNavAgent agent
    {
        get { return _agent != null ? _agent : _agent = GetComponent<PolyNavAgent>(); }
    }
    private PolyNavAgent _agent;
    private Transform player;
    private Rigidbody2D rb;
    private HitEffect hit;
    private int animationScaredTrigger;
    private bool deathCrashed;
    private bool keepFadeIn;
    private bool keepFadeOut;

    public enum EnemyState
    {
        SLEEP, FLY, ATTACK_READY, ATTACK_PATHFINDING, ATTACK, CRASH, DEAD,
    }

    private void Start()
    {
        animationScaredTrigger = Animator.StringToHash("Scared");

        rb = GetComponent<Rigidbody2D>();
        hit = GetComponentInChildren<HitEffect>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable()
    {
        agent.OnDestinationReached += WhenDesinationReached;
    }


    private void OnDisable()
    {
        agent.OnDestinationReached -= WhenDesinationReached;
    }

    private void Update()
    {
        if (isDead)
            return;
        CheckIsDead();
        UpdateDirection();
    }

    protected override void UpdateDirection()
    {
        if (currentState == EnemyState.FLY)
        {
            if (transform.localScale.x < 0)
            {
                isFacingLeft = true;
            }
            else if (transform.localScale.x > 0)
            {
                isFacingLeft = false;
            }
            if (player.position.x > transform.position.x && isFacingLeft)
            {
                Flip();
            }
            else if (player.position.x < transform.position.x && !isFacingLeft)
            {
                Flip();
            }
        }
    }

    public void SwitchState(EnemyState state)
    {
        switch (currentState)
        {
            case EnemyState.SLEEP:
                ExitSleepState();
                break;
            case EnemyState.FLY:
                ExitFlyState();
                break;
            case EnemyState.ATTACK_READY:
                ExitAttackReadyState();
                break;
            case EnemyState.ATTACK_PATHFINDING:
                ExitAttackPathFindingState();
                break;
            case EnemyState.ATTACK:
                ExitAttackState();
                break;
            case EnemyState.CRASH:
                ExitCrashState();
                break;
            case EnemyState.DEAD:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case EnemyState.SLEEP:
                EnterSleepState();
                break;
            case EnemyState.FLY:
                EnterFlyState();
                break;
            case EnemyState.ATTACK_READY:
                EnterAttackReadyState();
                break;
            case EnemyState.ATTACK_PATHFINDING:
                EnterAttackPathFindingState();
                break;
            case EnemyState.ATTACK:
                EnterAttackState();
                break;
            case EnemyState.CRASH:
                EnterCrashState();
                break;
            case EnemyState.DEAD:
                EnterDeadState();
                break;
        }

        currentState = state;
    }

    private void EnterSleepState()
    {

    }

    private void ExitSleepState()
    {
        LockGates();
        GetComponent<TextReminderTrigger>().DisplayText();
        snore.Stop();
        snoreAudio.Stop();
        GetComponent<Collider2D>().isTrigger = false;
        effectAudio.PlayOneShot(snoreStartle);
        attackBGM.Play();
        animator.SetTrigger(animationScaredTrigger);

        Vector3 destination = player.transform.position + Vector3.up * 6;
        agent.SetDestination(destination);
    }

    private void EnterFlyState()
    {
        agent.maxSpeed = 3.5f;
        agent.SetDestination(player.position + Vector3.up * 3);
    }

    private void ExitFlyState()
    {

    }

    private void EnterAttackReadyState()
    {
        animator.SetTrigger("AttackReady");
        float minDistance = 0;
        for (int i = 0; i < crashPoints.Length; i++)
        {
            if (i % 2 == 0)
            {
                float distance = Vector2.Distance(transform.position, crashPoints[i].position);
                if (minDistance == 0 || distance < minDistance)
                {
                    minDistance = distance;
                    currentCrashPoint = i;
                }
            }
        }

        // 冲撞次数归零
        crashCount = 0;
        // 冲撞计数
        crashCount++;
        // 设定目的地
        agent.SetDestination(crashPoints[currentCrashPoint].position);
        agent.maxSpeed = 50;
        // 根据角色横轴相对方位，决定冲撞顺序
        if (player.position.x > transform.position.x)
        {
            isDesc = false;
        }
        else
        {
            isDesc = true;
        }
    }

    private void ExitAttackReadyState()
    {
        if (currentState == EnemyState.ATTACK_READY)
        {
            agent.Stop();
        }
    }

    private void EnterAttackPathFindingState()
    {
        crashCount++;
        if (crashCount > maxCrashCount)
        {
            SwitchState(EnemyState.FLY);
            return;
        }
        // 根据顺序或倒序决定下次冲撞点
        if (isDesc)
        {
            currentCrashPoint--;
        }
        else
        {
            currentCrashPoint++;
        }
        if (!isDesc && currentCrashPoint >= crashPoints.Length)
        {
            isDesc = true;
            currentCrashPoint = crashPoints.Length - 2;
        }
        else if (isDesc && currentCrashPoint < 0)
        {
            isDesc = false;
            currentCrashPoint = 1;
        }
        agent.SetDestination(crashPoints[currentCrashPoint].position);
    }

    private void ExitAttackPathFindingState()
    {

    }

    private void EnterAttackState()
    {
        if (currentCrashPoint % 2 == 0)
        {
            coliisionEffect.transform.localRotation = Quaternion.Euler(-180, 0, 0);
        }
        else
        {
            coliisionEffect.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        animator.SetTrigger("Attack");
        effectAudio.PlayOneShot(wallHit);
        // 抖动相机
        CameraShake();
        isHitWall = true;
    }

    private void ExitAttackState()
    {

    }

    private void EnterCrashState()
    {

    }

    private void ExitCrashState()
    {

    }

    private void EnterDeadState()
    {
        GetComponent<PolyNavAgent>().enabled = false;
        hit.PlayHitAnimation();
        animator.SetTrigger("Dead");
        effectAudio.PlayOneShot(bossFinalHit);
    }

    private void ExitDeadState()
    {

    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        if (currentState == EnemyState.SLEEP)
        {
            SwitchState(EnemyState.FLY);
        }
        hit.PlayHitAnimation();
        effectAudio.PlayOneShot(enemyDamage);
    }

    public void LookAtPlayer()
    {
        if (player.position.x > transform.position.x && isFacingLeft)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && !isFacingLeft)
        {
            Flip();
        }
    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x = vector.x * -1;
        transform.localScale = vector;
    }

    private void WhenDesinationReached()
    {
        switch (currentState)
        {
            case EnemyState.FLY:
                SwitchState(EnemyState.ATTACK_READY);
                break;
            case EnemyState.ATTACK_READY:
            case EnemyState.ATTACK_PATHFINDING:
                SwitchState(EnemyState.ATTACK);
                break;
        }
    }

    public void PlaySnoreEffect()
    {
        snore.Play();
    }

    public void StopSnoreEffect()
    {
        snore.Stop();
    }

    public bool CanResumeCrash()
    {
        if (crashCount >= maxCrashCount)
            return false;
        else
            return true;
    }

    public void CameraShake()
    {
        // 相机震动
        var shakePreset = ProCamera2DShake.Instance.ShakePresets[2];
        ProCamera2DShake.Instance.Shake(shakePreset);
    }

    public void StartCameraConstanceShake()
    {
        var shakePreset = ProCamera2DShake.Instance.ConstantShakePresets[0];
        ProCamera2DShake.Instance.ConstantShake(shakePreset);
    }

    public void StopCameraConstanceShake()
    {
        var shakePreset = ProCamera2DShake.Instance.ConstantShakePresets[0];
        ProCamera2DShake.Instance.StopConstantShaking();
    }

    public void RestoreGravity()
    {
        rb.gravityScale = deathGravity;
    }

    protected override void Dead()
    {
        isDead = true;
        SwitchState(EnemyState.DEAD);
    }
    public void PlayDeathVoice()
    {
        StartCoroutine(FadeOut(0.2f));
        effectAudio.PlayOneShot(bossGushing);
    }

    public void PlayBossDefeatVoice()
    {
        mainSource.PlayOneShot(bossDefeat);
        mainSource.PlayOneShot(bossExplore);
    }

    IEnumerator FadeOut(float speed)
    {
        keepFadeIn = false;
        keepFadeOut = true;

        float audioVolume = attackBGM.volume;

        while (attackBGM.volume >= 0 && keepFadeOut)
        {
            audioVolume -= speed;
            attackBGM.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void AddDeathForce()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * deathUpForce);
    }

    public void LockGates()
    {
        leftBattleGate.Lock();
        rightBattleGate.Lock();
    }

    public void UnlockGates()
    {
        leftBattleGate.Unlock();
        rightBattleGate.Unlock();
    }

    public void PlayGushingEffect()
    {
        gushing.Play();
    }

    public void StopGushingEffect()
    {
        gushing.Stop();
    }

    public void PlayConstantBoomEffect()
    {
        var main = boom.main;
        main.loop = true;
        boom.Play();
    }

    public void PlayBoomEffect()
    {
        var main = boom.main;
        main.loop = false;
        boom.Play();
    }

    public void StopBoomEffect()
    {
        boom.Stop();
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        mainSource.PlayOneShot(audioClip);
    }

    protected override void DetectCollisionEnter2D(Collision2D collision)
    {
        // 如果碰撞到敌人
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            // 无敌状态，屏蔽碰撞执行语句
            StartCoroutine(character.TakeDamage(this));
            FindObjectOfType<HitPause>().Stop(0.5f);
        }
        if (isDead && (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Terrain Detector")))
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            GetComponent<Collider2D>().enabled = false;
            animator.SetTrigger("DeadOnGround");
        }
    }

    public void SpawnFlys()
    {
        flySpawn.SetActive(true);
    }
}
