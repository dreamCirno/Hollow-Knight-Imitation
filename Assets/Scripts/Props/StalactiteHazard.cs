using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StalactiteHazard : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    public GameObject embedded;
    public GameObject dustStalactite;
    public GameObject dustTrail;
    public GameObject dustFall;

    private bool isTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTrigger && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            StartCoroutine(DelayFall());
        }
    }

    IEnumerator DelayFall()
    {
        audioSource.Play();
        isTrigger = true;
        dustTrail.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.enabled = false;
        audioSource.Stop();
        dustTrail.SetActive(false);
        dustStalactite.SetActive(true);
        embedded.SetActive(true);
        dustFall.SetActive(true);
    }
}
