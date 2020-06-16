using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulOrb : MonoBehaviour
{
    HealthUI healthUI;
    Animator animator;

    private void Awake()
    {
        healthUI = FindObjectOfType<HealthUI>();
        animator = GetComponent<Animator>();
    }

    public void DelayShowOrb(float delay)
    {
        StartCoroutine(IShowOrb(delay));
    }

    IEnumerator IShowOrb(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowSoulOrb();
    }

    public void ShowSoulOrb()
    {
        animator.SetTrigger("Respawn");
    }

    public void HideSoulOrb()
    {
        animator.SetTrigger("Hide");
    }

    public void ShowHealthItems()
    {
        StartCoroutine(healthUI.ShowHealthItems());
    }

    public void HideHealthItems()
    {
        healthUI.HideHealthItems();
    }
}
