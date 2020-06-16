using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlots : MonoBehaviour
{
    public GameObject[] slots;
    public GameObject[] texts;

    private void Start()
    {
        GetSaveFile();
    }

    public void GetSaveFile()
    {
        string name = PlayerPrefs.GetString("SaveFile");
        string[] indexs = name.Split('|');

        for (int i = 0; i < 3; i++)
        {
            slots[i].SetActive(false);
            texts[i].SetActive(true);
        }
        for (int i = 0; i < indexs.Length; i++)
        {
            if (i + "" == indexs[i])
            {
                slots[i].SetActive(true);
                texts[i].SetActive(false);
            }
        }
    }
}
