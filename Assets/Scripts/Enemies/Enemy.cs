using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Enemy : Breakable
{
    protected Animator animator;
    protected CharacterController2D character;

    [Header("Coin Attr")]
    [SerializeField] protected GameObject coin;
    [SerializeField] protected int minSpawnCount = 1;
    [SerializeField] protected int maxSpawnCount = 4;
    [SerializeField] protected float maxBumpHorizontalForce = 400;
    [SerializeField] protected float minBumpVerticalForce = 600;
    [SerializeField] protected float maxBumpVerticalForce = 800;

    public bool isFacingLeft;
    protected bool canMove;

    private int animatorDeadBool;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        character = FindObjectOfType<CharacterController2D>();
    }

    protected virtual void UpdateDirection()
    {
        if (transform.localScale.x == 1)
        {
            isFacingLeft = true;
        }
        else if (transform.localScale.x == -1)
        {
            isFacingLeft = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DetectCollisionEnter2D(collision);
    }

    protected virtual void DetectCollisionEnter2D(Collision2D collision)
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
        }
    }

    protected override void Dead()
    {
        base.Dead();
        SpawnCoins();
        canMove = false;
    }

    public virtual void SpawnCoins()
    {
        int randomCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < randomCount; i++)
        {
            GameObject geo = Instantiate(coin, transform.position, Quaternion.identity, transform.parent) as GameObject;
            Vector2 force = new Vector2(Random.Range(-maxBumpHorizontalForce, maxBumpHorizontalForce), Random.Range(minBumpVerticalForce, maxBumpVerticalForce));
            geo.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Force);
        }
    }
}
