using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestoryGameObject : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
