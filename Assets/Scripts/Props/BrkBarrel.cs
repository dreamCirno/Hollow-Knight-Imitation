using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrkBarrel : TutePole
{
    [SerializeField] AudioClip[] barrelDeath;

    protected override void PlayAudio()
    {
        int r = Random.Range(0, barrelDeath.Length);
        audioPlayer.clip = barrelDeath[r];
        base.PlayAudio();
    }
}
