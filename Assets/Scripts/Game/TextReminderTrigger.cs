using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextReminderTrigger : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private string textString;
    [SerializeField] private Color hideColor;
    [SerializeField] private Color showColor;
    [SerializeField] private bool detectTrigger;
    bool isTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!detectTrigger)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector") && !isTrigger)
        {
            isTrigger = true;
            DisplayText();
        }
    }

    public void DisplayText()
    {
        StartCoroutine(DelayDisplayText());
    }

    IEnumerator DelayDisplayText()
    {
        text.text = textString;
        text.color = hideColor;
        float t = 0;
        do
        {
            t += 0.025f;
            text.color = Color.Lerp(text.color, showColor, t);
            yield return new WaitForSeconds(0.1f);
        }
        while (text.color != showColor);

        yield return new WaitForSeconds(3f);

        t = 0;
        do
        {
            t += 0.025f;
            text.color = Color.Lerp(text.color, hideColor, t);
            yield return new WaitForSeconds(0.1f);
        }
        while (text.color != hideColor);
    }
}
