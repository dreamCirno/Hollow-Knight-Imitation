using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    GameSceneManager gameSceneManager;

    private void Start()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
    }

    public void DelayLoadNextScene()
    {
        gameSceneManager.DelayLoadNextScene();
    }
}
