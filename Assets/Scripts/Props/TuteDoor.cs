using System.Collections.Generic;
using UnityEngine;

public class TuteDoor : Breakable
{
    [SerializeField] protected SpriteRenderer sprite;
    [SerializeField] List<GameObject> readyEnableList;
    [SerializeField] List<GameObject> readyDisableList;
    [SerializeField] AudioSource audioPlayer;

    private void Update()
    {
        CheckIsDead();
    }

    protected override void Dead()
    {
        base.Dead();
        audioPlayer.Play();
        sprite.enabled = false;
        foreach (GameObject o in readyDisableList)
        {
            o.SetActive(false);
        }
        foreach (GameObject o in readyEnableList)
        {
            o.SetActive(true);
        }
    }
}

    