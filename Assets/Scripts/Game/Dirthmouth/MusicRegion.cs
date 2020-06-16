using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicRegion : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float bgmChangeSpeed;
    [SerializeField] AudioSource dirtmouth;

    private bool keepFadeIn, keepFadeOut;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        FadeInCaller(bgmChangeSpeed);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        FadeoutCaller(bgmChangeSpeed);
    }

    public void FadeInCaller(float speed)
    {
        StartCoroutine(FadeIn(speed));
    }
    public void FadeoutCaller(float speed)
    {
        StartCoroutine(FadeOut(speed));
    }

    IEnumerator FadeIn(float speed)
    {
        keepFadeIn = true;
        keepFadeOut = false;

        if (!dirtmouth.isPlaying)
            dirtmouth.Play();
        float audioVolume = dirtmouth.volume;

        while (dirtmouth.volume < 1 && keepFadeIn)
        {
            audioVolume += speed;
            dirtmouth.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator FadeOut(float speed)
    {
        keepFadeIn = false;
        keepFadeOut = true;

        float audioVolume = dirtmouth.volume;

        while (dirtmouth.volume >= 0 && keepFadeOut)
        {
            audioVolume -= speed;
            dirtmouth.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
