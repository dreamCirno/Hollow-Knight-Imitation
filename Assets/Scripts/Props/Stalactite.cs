using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalactite : Breakable
{
    SpriteRenderer sprite;
    public GameObject particleRocks;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public CharacterController2D character;
    public Rigidbody2D rigidbody2d;
    public bool canHurt;

    private Collider2D col;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        character = FindObjectOfType<CharacterController2D>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        canHurt = true;
    }

    private void Update()
    {
        CheckIsDead();
    }

    protected override void Dead()
    {
        base.Dead();
        audioSource.PlayOneShot(audioClips[1]);
        sprite.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        particleRocks.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canHurt && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            canHurt = false;
            // 无敌状态，屏蔽碰撞执行语句
            StartCoroutine(character.TakeDamage());
            FindObjectOfType<HitPause>().Stop(0.5f);
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            canHurt = false;
            rigidbody2d.bodyType = RigidbodyType2D.Static;
            col.isTrigger = true;
            audioSource.PlayOneShot(audioClips[0]);
        }
    }
}
