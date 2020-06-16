using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    readonly Vector3 flippedScale = new Vector3(-1, 1, 1);

    private Rigidbody2D controllerRigidbody;

    [Header("依赖脚本")]
    [SerializeField] Animator animator = null;
    [SerializeField] CharacterAudio audioEffectPlayer = null;
    [SerializeField] CharacterAttack attacker = null;
    [SerializeField] CharacterEffect effecter = null;
    [SerializeField] CharacterData data = null;
    [SerializeField] AudioSource audioMusicPlayer = null;
    [SerializeField] GameManager gameManager = null;

    [Header("移动参数")]
    [SerializeField] float maxSpeed = 0.0f;
    [SerializeField] float jumpForce = 0.0f;
    [SerializeField] float wallJumpForce = 0.0f;
    [SerializeField] float wallReactingForce = 0.0f;
    [SerializeField] float recoilForce = 0.0f;
    [SerializeField] float downRecoilForce = 0.0f;
    [SerializeField] float hurtForce = 0.0f;
    [SerializeField] float maxGravityVelocity = 10.0f;
    [SerializeField] float jumpGravityScale = 1.0f;
    [SerializeField] float fallGravityScale = 1.0f;
    [SerializeField] float slidingGravityScale = 1.0f;
    [SerializeField] float groundedGravityScale = 1.0f;

    [Header("层级")]
    [SerializeField] LayerMask whatIsOnGround;

    private Vector2 vectorInput;
    private bool jumpInput;
    private bool enableGravity;
    private int jumpCount;

    [Header("战斗参数")]
    [Tooltip("连击时间")]
    [SerializeField] float maxComboDelay = 0.4f;
    [Tooltip("攻击按键间隔时间")]
    [SerializeField] float slashIntervalTime = 0.2f;

    [Header("攻击数值参数")]
    [SerializeField] int slashDamage;

    private int slashCount;
    private float lastSlashTime;

    private bool isOnGround;
    private bool isFacingLeft;
    private bool isJumping;
    private bool isSliding;
    private bool isFalling;

    [Header("其他参数")]
    [SerializeField] private bool firstLanding;

    private int animatorFristLandingBool;
    private int animatorGroundedBool;
    private int animatorSlidingBool;
    private int animatorMovementSpeed;
    private int animatorVelocitySpeed;
    private int animatorJumpTrigger;
    private int animatorDoubleJumpTrigger;
    private int animatorSlideJumpTrigger;
    private int animatorTurnTrigger;
    private int animatorRespawnTrigger;

    private float counter;

    public bool canMove { get; set; }

    #region Callback Function

    private void OnEnable()
    {
        InputManager.InputControl.GamePlayer.Movement.performed += ctx => vectorInput = ctx.ReadValue<Vector2>();
        InputManager.InputControl.GamePlayer.Jump.started += Jump_started;
        InputManager.InputControl.GamePlayer.Jump.performed += Jump_performed;
        InputManager.InputControl.GamePlayer.Jump.canceled += Jump_canceled;
        InputManager.InputControl.GamePlayer.Attack.started += Attack_started;
        InputManager.InputControl.GamePlayer.Attack.performed += Attack_performed;
        InputManager.InputControl.GamePlayer.Attack.canceled += Attack_canceled;
    }

    private void OnDisable()
    {
        InputManager.InputControl.GamePlayer.Jump.started -= Jump_started;
        InputManager.InputControl.GamePlayer.Jump.performed -= Jump_performed;
        InputManager.InputControl.GamePlayer.Jump.canceled -= Jump_canceled;
        InputManager.InputControl.GamePlayer.Attack.started -= Attack_started;
        InputManager.InputControl.GamePlayer.Attack.performed -= Attack_performed;
        InputManager.InputControl.GamePlayer.Attack.canceled -= Attack_canceled;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        controllerRigidbody = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
        data = GetComponent<CharacterData>();

        animatorGroundedBool = Animator.StringToHash("Grounded");
        animatorSlidingBool = Animator.StringToHash("Sliding");
        animatorMovementSpeed = Animator.StringToHash("Movement");
        animatorVelocitySpeed = Animator.StringToHash("Velocity");
        animatorJumpTrigger = Animator.StringToHash("Jump");
        animatorDoubleJumpTrigger = Animator.StringToHash("DoubleJump");
        animatorSlideJumpTrigger = Animator.StringToHash("SlideJump");
        animatorTurnTrigger = Animator.StringToHash("Turn");
        animatorRespawnTrigger = Animator.StringToHash("Respawn");
        animatorFristLandingBool = Animator.StringToHash("FirstLanding");

        canMove = true;
        enableGravity = true;
        animator.SetBool(animatorFristLandingBool, firstLanding);
        if (firstLanding)
        {
            FindObjectOfType<SoulOrb>().DelayShowOrb(0);
        }
    }

    private void Update()
    {
        ResetComboTimer();
    }

    void FixedUpdate()
    {
        UpdateVelocity();
        UpdateDirection();
        UpdateJump();
        UpdateGravityScale();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGrounding(collision, false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGrounding(collision, false);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UpdateGrounding(collision, true);
    }

    #endregion

    #region Movement

    private void Jump_started(InputAction.CallbackContext context)
    {
        if (data.GetDeadStatement())
            return;
        if (isSliding && !isOnGround)
        {
            StartCoroutine(GrabWallJump());
        }
        else
        {
            if (!gameManager.IsEnableInput())
                return;
            counter = Time.time;
            if (jumpCount <= 1)
            {
                ++jumpCount;
                if (jumpCount == 1)
                {
                    // Set animator
                    animator.SetTrigger(animatorJumpTrigger);
                    // Play audio
                    audioEffectPlayer.Play(CharacterAudio.AudioType.Jump, true);
                }
                else if (jumpCount == 2)
                {
                    animator.SetTrigger(animatorDoubleJumpTrigger);
                    effecter.DoEffect(CharacterEffect.EffectType.DoubleJump, true);
                    // Play audio
                    audioEffectPlayer.Play(CharacterAudio.AudioType.HeroWings, true);
                }
                else
                {
                    return;
                }
                // 跳跃键被按下
                jumpInput = true;
            }
        }
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        JumpCancel();
    }

    private void Jump_canceled(InputAction.CallbackContext context)
    {
        JumpCancel();
    }

    private void JumpCancel()
    {
        jumpInput = false;
        isJumping = false;
        if (jumpCount == 1)
        {
            animator.ResetTrigger(animatorJumpTrigger);
        }
        else if (jumpCount == 2)
        {
            animator.ResetTrigger(animatorDoubleJumpTrigger);
        }
    }

    private void UpdateGrounding(Collision2D collision, bool exitState)
    {
        if (exitState)
        {
            if ((collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain")))
            {
                isOnGround = false;
                isSliding = false;
            }
        }
        else
        {
            // 如果下方碰撞到地形，则跳跃已完成，人物已在地面上
            if ((collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
                && collision.contacts[0].normal == Vector2.up
                && !isOnGround)
            {
                isOnGround = true;
                // Reset jumping flags
                isJumping = false;
                isFalling = false;
                effecter.DoEffect(CharacterEffect.EffectType.FallTrail, true);
            }
            // 如果上方碰撞到地形，则取消长按跳跃
            else if ((collision.gameObject.layer == LayerMask.NameToLayer("Terrain")
                || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
                && collision.contacts[0].normal == Vector2.down && isJumping)
            {
                JumpCancel();
            }
        }
        animator.SetBool(animatorGroundedBool, isOnGround);
    }

    private void UpdateVelocity()
    {
        if (!data.GetDeadStatement())
        {
            Vector2 velocity = controllerRigidbody.velocity;
            if (isSliding && vectorInput.x != 0)
            {
                velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity / 2, maxGravityVelocity / 2);
            }
            else
            {
                velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity, maxGravityVelocity);
            }
            animator.SetFloat(animatorVelocitySpeed, controllerRigidbody.velocity.y);

            if (canMove && gameManager.IsEnableInput())
            {
                controllerRigidbody.velocity = new Vector2(vectorInput.x * maxSpeed, velocity.y);
                animator.SetInteger(animatorMovementSpeed, (int)vectorInput.x);
            }
        }
        else
        {
            Vector2 velocity = controllerRigidbody.velocity;
            velocity.x = 0;
            velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity, maxGravityVelocity);
            controllerRigidbody.velocity = velocity;
        }
    }

    private void UpdateJump()
    {
        // Set falling flag
        if (isJumping && controllerRigidbody.velocity.y < 0)
            isFalling = true;

        // Jump
        if (jumpInput && gameManager.IsEnableInput())
        {
            // Jump using impulse force
            controllerRigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

            // Set jumping flag
            isJumping = true;

            effecter.DoEffect(CharacterEffect.EffectType.FallTrail, false);
        }

        if (isOnGround && !isJumping && jumpCount != 0)
        {
            jumpCount = 0;
            counter = Time.time - counter;
        }
    }

    private void UpdateDirection()
    {
        if (canMove && !data.GetDeadStatement())
        {
            // Use scale to flip character depending on direction
            if (controllerRigidbody.velocity.x > 1 && isFacingLeft)
            {
                isFacingLeft = false;
                transform.localScale = flippedScale;
            }
            else if (controllerRigidbody.velocity.x < -1 && !isFacingLeft)
            {
                isFacingLeft = true;
                transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateGravityScale()
    {
        // Use grounded gravity scale by default.
        var gravityScale = groundedGravityScale;

        if (!isOnGround)
        {
            if (isSliding && vectorInput.x != 0)
            {
                gravityScale = slidingGravityScale;
            }
            else
            {
                // If not grounded then set the gravity scale according to upwards (jump) or downwards 
                // (falling) motion.
                gravityScale = controllerRigidbody.velocity.y > 0.0f ? jumpGravityScale : fallGravityScale;
            }
        }

        if (!enableGravity)
        {
            gravityScale = 0;
        }

        controllerRigidbody.gravityScale = gravityScale;
    }

    IEnumerator GrabWallJump()
    {
        gameManager.SetEnableInput(false);
        enableGravity = false;
        animator.SetTrigger(animatorSlideJumpTrigger);
        controllerRigidbody.velocity = new Vector2(transform.lossyScale.x * 
            wallReactingForce, wallJumpForce);
        yield return new WaitForSeconds(0.15f);
        enableGravity = true;
        gameManager.SetEnableInput(true);
        animator.ResetTrigger(animatorSlideJumpTrigger);
    }

    public void StopHorizontalMovement()
    {
        Vector2 velocity = controllerRigidbody.velocity;
        velocity.x = 0;
        controllerRigidbody.velocity = velocity;
        animator.SetInteger(animatorMovementSpeed, 0);
    }

    public void StopInput()
    {
        gameManager.SetEnableInput(false);
        StopHorizontalMovement();
    }

    public void ResumeInput()
    {
        gameManager.SetEnableInput(true);
    }
    #endregion

    #region Combat

    private void Attack_started(InputAction.CallbackContext context)
    {
        if (gameManager.IsEnableInput() && !data.GetDeadStatement())
            if (Time.time >= lastSlashTime + slashIntervalTime)
            {
                lastSlashTime = Time.time;
                if (vectorInput.y > 0)
                {
                    SlashAndDetect(CharacterAttack.AttackType.UpSlash);
                    animator.Play("UpSlash");
                }
                else if (!isOnGround && vectorInput.y < 0)
                {
                    SlashAndDetect(CharacterAttack.AttackType.DownSlash);
                    animator.Play("DownSlash");
                }
                else
                {
                    // 如果垂直方向键没有被按下
                    slashCount++;
                    switch (slashCount)
                    {
                        case 1:
                            SlashAndDetect(CharacterAttack.AttackType.Slash);
                            animator.Play("Slash");
                            break;
                        case 2:
                            SlashAndDetect(CharacterAttack.AttackType.AltSlash);
                            animator.Play("AltSlash");
                            slashCount = 0;
                            break;
                    }
                }
            }
    }

    private void Attack_performed(InputAction.CallbackContext context)
    {

    }

    private void Attack_canceled(InputAction.CallbackContext context)
    {

    }

    private void ResetComboTimer()
    {
        if (Time.time >= lastSlashTime + maxComboDelay && slashCount != 0)
        {
            slashCount = 0;
        }
    }

    /// <summary>
    /// 检测范围并攻击
    /// </summary>
    private void SlashAndDetect(CharacterAttack.AttackType attackType)
    {
        List<Collider2D> colliders = new List<Collider2D>();
        attacker.Play(attackType, ref colliders);
        bool hasEnemy = false;
        bool hasDamageAll = false;
        // 检测是否攻击到敌人
        foreach (Collider2D c in colliders)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"))
            {
                hasEnemy = true;
                break;
            }
        }
        // 检测是否攻击到陷阱
        foreach (Collider2D c in colliders)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Damage All"))
            {
                hasDamageAll = true;
                break;
            }
        }
        if (hasEnemy)
        {
            if (attackType == CharacterAttack.AttackType.DownSlash)
            {
                AddDownRecoilForce();
            }
            else
            {
                StartCoroutine(AddRecoilForce());
            }
        }
        if (hasDamageAll)
        {
            if (attackType == CharacterAttack.AttackType.DownSlash)
            {
                audioEffectPlayer.PlayOneShot(CharacterAudio.AudioClipType.SwordHitReject);
                AddDownRecoilForce();
            }
        }
        foreach (Collider2D col in colliders)
        {
            Breakable breakable = col.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable.Hurt(slashDamage, transform);
            }
        }
    }

    public void AddDownRecoilForce()
    {
        controllerRigidbody.velocity.Set(controllerRigidbody.velocity.x, 0);
        controllerRigidbody.AddForce(Vector2.up * downRecoilForce, ForceMode2D.Force);
    }

    IEnumerator AddRecoilForce()
    {
        canMove = false;
        if (isFacingLeft)
        {
            controllerRigidbody.AddForce(Vector2.right * recoilForce, ForceMode2D.Force);
        }
        else
        {
            controllerRigidbody.AddForce(Vector2.left * recoilForce, ForceMode2D.Force);
        }
        yield return new WaitForSeconds(0.2f);
        canMove = true;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    public IEnumerator TakeDamage(Enemy enemy)
    {
        ProCamera2DShake.Instance.Shake(ProCamera2DShake.Instance.ShakePresets[3]);
        gameManager.SetEnableInput(false);
        audioEffectPlayer.Play(CharacterAudio.AudioType.HeroDamage, true);
        FindObjectOfType<HealthUI>().Hurt();
        if (!data.GetDeadStatement())
        {
            StartCoroutine(FindObjectOfType<Invincibility>().SetInvincibility());
            if (isFacingLeft)
            {
                controllerRigidbody.velocity = new Vector2(-1f, 1f) * hurtForce;
            }
            else
            {
                controllerRigidbody.velocity = new Vector2(1f, 1f) * hurtForce;
            }
        }
        animator.Play("Damage");
        yield return null;
    }

    public IEnumerator TakeDamage()
    {
        ProCamera2DShake.Instance.Shake(ProCamera2DShake.Instance.ShakePresets[3]);
        gameManager.SetEnableInput(false);
        audioEffectPlayer.Play(CharacterAudio.AudioType.HeroDamage, true);
        FindObjectOfType<HealthUI>().Hurt();
        if (!data.GetDeadStatement())
        {
            StartCoroutine(FindObjectOfType<Invincibility>().SetInvincibility());
            if (isFacingLeft)
            {
                controllerRigidbody.velocity = new Vector2(-1f, 1f) * hurtForce;
            }
            else
            {
                controllerRigidbody.velocity = new Vector2(1f, 1f) * hurtForce;
            }
        }
        animator.Play("Damage");
        yield return null;
    }

    #endregion

    #region Others

    public void FirstLand()
    {
        StopInput();
        effecter.DoEffect(CharacterEffect.EffectType.BurstRocks, true);
    }
    public void HardLand()
    {
        StopInput();
    }
    public void StartShake()
    {
        var shakePreset = ProCamera2DShake.Instance.ShakePresets[0];
        ProCamera2DShake.Instance.Shake(shakePreset);
    }

    public void StopShake()
    {
        ProCamera2DShake.Instance.StopShaking();
    }

    public void PlayHitParticles()
    {
        effecter.DoEffect(CharacterEffect.EffectType.HitLeft, true);
        effecter.DoEffect(CharacterEffect.EffectType.HitRight, true);
    }

    public void PlayAshParticles()
    {
        effecter.DoEffect(CharacterEffect.EffectType.AshLeft, true);
        effecter.DoEffect(CharacterEffect.EffectType.AshRight, true);
    }

    public void PlayShadeParticle()
    {
        effecter.DoEffect(CharacterEffect.EffectType.Shade, true);
    }

    public void PlayRespawnAnimation()
    {
        animator.SetTrigger(animatorRespawnTrigger);
    }

    public bool GetIsOnGround()
    {
        return isOnGround;
    }

    public void PlayMusicAudioClip(AudioClip audioClip)
    {
        audioMusicPlayer.PlayOneShot(audioClip);
    }

    public void ResetFallDistance()
    {
        animator.GetBehaviour<FallingBehaviour>().ResetAllParams();
    }

    public void SlideWall_ResetJumpCount()
    {
        jumpCount = 1;
    }

    public void SetIsSliding(bool state)
    {
        isSliding = state;
        if (!data.GetDeadStatement())
        {
            animator.SetBool(animatorSlidingBool, isSliding);
        }
    }

    public void SetIsOnGrounded(bool state)
    {
        isOnGround = state;
        if (!data.GetDeadStatement())
        {
            animator.SetBool(animatorGroundedBool, isOnGround);
        }
    }
    #endregion
}
