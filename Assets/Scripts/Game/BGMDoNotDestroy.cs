using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMDoNotDestroy : MonoBehaviour
{
    AudioSource bgm;

    private void Awake()
    {
        bgm = GetComponent<AudioSource>();
    }

    public void Play()
    {
        bgm.Play();
    }
}
