using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProCamera2DController : MonoBehaviour
{
    Transform player;
    ProCamera2D camera2d;
    SoulOrb soulOrb;

    private void Awake()
    {
        camera2d = GetComponent<ProCamera2D>();
        GameObject[] gameObjects = DontdestroyOnLoadAccessor.Instance.GetAllRootsOfDontDestroyOnLoad();
        foreach (GameObject o in gameObjects)
        {
            if (o.CompareTag("Player"))
            {
                player = o.transform;
                break;
            }
        }
        soulOrb = FindObjectOfType<SoulOrb>();
    }

    private void Start()
    {
        soulOrb.ShowSoulOrb();
        camera2d.AddCameraTarget(player, 1, 1);
    }
}
