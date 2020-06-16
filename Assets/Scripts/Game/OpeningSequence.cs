using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OpeningSequence : MonoBehaviour
{
    [SerializeField] private VideoPlayer prologue;
    [SerializeField] private VideoPlayer intro;

    private AnyKeyTip anyKeyTip;
    private CrossFader crossFader;

    private void Awake()
    {
        anyKeyTip = FindObjectOfType<AnyKeyTip>();
        crossFader = FindObjectOfType<CrossFader>();
    }

    private void Start()
    {
        prologue.loopPointReached += ProloguePlayer_loopPointReached;
        intro.loopPointReached += IntroPlayer_loopPointReached;
    }

    public void PlayPrologue()
    {
        ResetAnyKeyTip();
        prologue.Play();
    }

    private void ProloguePlayer_loopPointReached(VideoPlayer source)
    {
        UpdateAnyKeyTipCounter();
        ResetAnyKeyTip();
        intro.Play();
    }

    private void IntroPlayer_loopPointReached(VideoPlayer source)
    {
        UpdateAnyKeyTipCounter();
        ResetAnyKeyTip();
        crossFader.FadeIn();
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadSceneAsync(1);
        }
    }

    public void UpdateAnyKeyTipCounter()
    {
        anyKeyTip.counter++;
    }

    public void ResetAnyKeyTip()
    {
        anyKeyTip.ResetAnyKeyTip();
    }

}
