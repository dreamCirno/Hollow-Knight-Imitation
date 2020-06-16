using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private bool isEnding;
    bool isTrigger;
    CrossFader crossFader;

    private void Awake()
    {
        crossFader = FindObjectOfType<CrossFader>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTrigger && collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
        {
            isTrigger = true;

            StartCoroutine(LoadAsyncNextScene());
        }
    }

    IEnumerator LoadAsyncNextScene()
    {
        crossFader.FadeOut();
        yield return new WaitForSeconds(1);

        AsyncOperation asyncLoad;
        if (isEnding)
        {
            asyncLoad = SceneManager.LoadSceneAsync("Ending");
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
        do
        {
            yield return null;
        } while (!asyncLoad.isDone);
    }


}
