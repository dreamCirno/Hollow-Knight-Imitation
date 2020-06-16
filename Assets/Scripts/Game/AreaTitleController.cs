using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaTitleController : MonoBehaviour
{
    [Header("Require Scripts")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Text subText;
    [SerializeField] Text mainText;
    [SerializeField] BGMDoNotDestroy bgm;
    [Header("Attrs")]
    [SerializeField] string subTitle;
    [SerializeField] string mainTitle;
    [Range(0, 1)]
    [SerializeField] float speed;
    bool isTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTrigger && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            isTrigger = true;
            StartCoroutine(DelayDisplayAreaTitle());

            if (bgm != null)
            {
                bgm.Play();
            }
        }
    }

    IEnumerator DelayDisplayAreaTitle()
    {
        subText.text = subTitle;
        mainText.text = mainTitle;

        canvasGroup.alpha = 0;

        do
        {
            canvasGroup.alpha += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        while (canvasGroup.alpha != 1);

        yield return new WaitForSeconds(3f);

        do
        {
            canvasGroup.alpha -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        while (canvasGroup.alpha != 0);
    }
}
