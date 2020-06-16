using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletTop : MonoBehaviour
{
    public Animator animator;
    public bool isInteractive;

    private void Update()
    {
        if (isInteractive && Input.GetKeyUp(KeyCode.UpArrow))
        {
            animator.SetTrigger("Active");
        }
        if (isInteractive && Input.GetKeyUp(KeyCode.Escape))
        {
            animator.SetTrigger("Disactive");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isInteractive && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            isInteractive = true;
        }
    }
}
