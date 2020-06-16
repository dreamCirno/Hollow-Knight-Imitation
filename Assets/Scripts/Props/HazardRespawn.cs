using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HazardRespawn : MonoBehaviour
{
    public HazardRespawnTrigger hazardRespawnTrigger;
    public Transform respawnPos;
    [SerializeField] private bool needToReload;

    private CharacterController2D character;
    private CharacterData data;
    private CrossFader crossFader;
    private GameManager gameManager;
    private SoulOrb soulOrb;

    private void Start()
    {
        character = FindObjectOfType<CharacterController2D>();
        data = FindObjectOfType<CharacterData>();
        crossFader = FindObjectOfType<CrossFader>();
        gameManager = FindObjectOfType<GameManager>();
        soulOrb = FindObjectOfType<SoulOrb>();
    }
    public void Respawn()
    {
        if (needToReload)
        {
            StartCoroutine(ReloadAsyncScene());
        }
        else
        {
            StartCoroutine(DelayRespawn());
        }
    }

    IEnumerator ReloadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        do
        {
            yield return null;
        } while (!asyncLoad.isDone);
    }

    public void Respawn(CaveSpikes caveSpikes)
    {
        StartCoroutine(DelayRespawn(caveSpikes));
    }

    public void BackToAlivePoint(CaveSpikes caveSpikes)
    {
        StartCoroutine(DelayBackToAlivePoint(caveSpikes));
    }

    IEnumerator DelayRespawn()
    {
        gameManager.SetEnableInput(false);
        crossFader.FadeOut();
        yield return new WaitForSeconds(2f);
        if (data.GetDeadStatement())
        {
            soulOrb.HideSoulOrb();
            soulOrb.HideHealthItems();
            character.transform.position = respawnPos.position;
            data.SetRespawnData(5);
            crossFader.FadeIn();
            character.PlayRespawnAnimation();
        }
        yield return new WaitForSeconds(3f);
        soulOrb.ShowSoulOrb();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"), LayerMask.NameToLayer("Enemy Detector"), false);
    }

    IEnumerator DelayRespawn(CaveSpikes caveSpikes)
    {
        gameManager.SetEnableInput(false);
        crossFader.FadeOut();
        yield return new WaitForSeconds(2f);
        if (data.GetDeadStatement())
        {
            soulOrb.HideSoulOrb();
            soulOrb.HideHealthItems();
            character.transform.position = respawnPos.position;
            data.SetRespawnData(5);
            crossFader.FadeIn();
            character.PlayRespawnAnimation();
        }
        yield return new WaitForSeconds(3f);
        caveSpikes.isTrigger = false;
        soulOrb.ShowSoulOrb();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"), LayerMask.NameToLayer("Enemy Detector"), false);
    }

    IEnumerator DelayBackToAlivePoint(CaveSpikes caveSpikes)
    {
        gameManager.SetEnableInput(false);
        crossFader.FadeOut();
        yield return new WaitForSeconds(2f);
        character.transform.position = respawnPos.position;
        caveSpikes.isTrigger = false;
        crossFader.FadeIn();
        character.PlayRespawnAnimation();
    }
}
