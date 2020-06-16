using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{

    public void SelectSaveFile(int i)
    {
        ES3AutoSaveMgr.Instance.settings.path = string.Format("HWSave{0}.es3", i);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F5))
        {
            Save();
        }
        if (Input.GetKeyUp(KeyCode.F6))
        {
            Load();
        }
    }

    public void Save()
    {
        ES3AutoSaveMgr.Instance.Save();
    }

    public void Load()
    {
        ES3AutoSaveMgr.Instance.Load();
    }
}
