using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AnyKeyTip : MonoBehaviour
{
    public int counter;

    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private VideoPlayer prologue;
    [SerializeField] private VideoPlayer intro;
    [SerializeField] private bool isCrossing;

    public Animator animator;
    public AudioSource audioSource;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isCrossing)
            {
                Display();
            }
            else
            {
                DisplayLate();
            }
        }
    }

    public void Display()
    {
        StopAllCoroutines();
        switch (counter)
        {
            case 0:
                ResetAnyKeyTip();
                animator.Play("Enter", 0, 720);
                audioSource.Stop();
                break;
            case 1:
                ResetAnyKeyTip();
                prologue.frame = (long)prologue.frameCount;
                break;
            case 2:
                ResetAnyKeyTip();
                intro.frame = (long)intro.frameCount;
                break;
        }
    }

    public void DisplayLate()
    {
        StartCoroutine(DelayDisplay());
    }

    IEnumerator DelayDisplay()
    {
        isCrossing = true;
        while (textCanvasGroup.alpha != 1)
        {
            textCanvasGroup.alpha += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResetAnyKeyTip()
    {
        isCrossing = false;
        textCanvasGroup.alpha = 0;
    }
}
