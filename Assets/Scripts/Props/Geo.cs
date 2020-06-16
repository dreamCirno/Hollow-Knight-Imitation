using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geo : MonoBehaviour
{
    [SerializeField] AudioClip[] geoHitGrounds;
    AudioSource audioSource;

    public bool isGrounded;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isGrounded && (collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Terrain")))
        {
            isGrounded = true;
            int index = Random.Range(0, geoHitGrounds.Length);
            audioSource.PlayOneShot(geoHitGrounds[index]);
        }
    }
}
