using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public float transitionTime = 1f;

    private CrossFader crossFader;

    private void Awake()
    {
        crossFader = FindObjectOfType<CrossFader>();
    }

    private void Start()
    {
        crossFader.FadeIn();
    }

    public void LoadNextScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void DelayLoadNextScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        crossFader.FadeOut();
        // Wait
        yield return new WaitForSeconds(transitionTime);
        // Load Scene
        SceneManager.LoadSceneAsync(levelIndex);
    }
}
