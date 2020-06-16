using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TutePole : Breakable
{
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] List<GameObject> readyEnableList;

    protected AudioSource audioPlayer;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        audioPlayer = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckIsDead();
    }

    protected override void Dead()
    {
        base.Dead();
        PlayAudio();
        sprite.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        foreach (GameObject o in readyEnableList)
        {
            o.SetActive(true);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Hurt(1);
        }
    }

    protected virtual void PlayAudio()
    {
        audioPlayer.Play();
    }
}
