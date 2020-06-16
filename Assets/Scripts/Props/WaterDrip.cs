using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrip : MonoBehaviour
{
    [SerializeField] private AudioClip[] waterDrips;
    AudioSource audioSource;
    Animator animator;
    Vector3 originPos;
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        originPos = transform.position;
    }

    public void Reset()
    {
        rb.bodyType = RigidbodyType2D.Static;
        transform.position = originPos;
    }

    public void Drip()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
        {
            PlayRandomWaterDripAudioClip();
            animator.SetTrigger("OnGround");
        }
    }

    private void PlayRandomWaterDripAudioClip()
    {
        if (waterDrips.Length > 0)
        {
            int r = Random.Range(0, waterDrips.Length);
            audioSource.PlayOneShot(waterDrips[r]);
        }
    }

}
