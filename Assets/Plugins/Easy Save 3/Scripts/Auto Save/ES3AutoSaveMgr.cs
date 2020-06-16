using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ES3AutoSaveMgr : MonoBehaviour, ISerializationCallbackReceiver 
{
	public static ES3AutoSaveMgr Instance
	{
		get
		{
			if (_instance != null)
				return _instance;
			return (_instance = GameObject.FindObjectOfType<ES3AutoSaveMgr>());
		}
	}
	public static ES3AutoSaveMgr _instance = null;

	public enum LoadEvent { None, Awake, Start }
	public enum SaveEvent { None, OnApplicationQuit, OnApplicationPause }

	public string key = System.Guid.NewGuid().ToString();
	public SaveEvent saveEvent = SaveEvent.OnApplicationQuit;
	public LoadEvent loadEvent = LoadEvent.Awake;
	public ES3SerializableSettings settings = null;

	public HashSet<ES3AutoSave> autoSaves = new HashSet<ES3AutoSave>();

	public void Save()
	{
        if (autoSaves == null || autoSaves.Count == 0)
        {
            ES3.DeleteKey(key, settings);
            return;
        }

        var gameObjects = new List<GameObject>();
        foreach (var autoSave in autoSaves)
            // If the ES3AutoSave component is disabled, don't save it.
            if (autoSave.enabled)
                gameObjects.Add(autoSave.gameObject);

		ES3.Save<GameObject[]>(key, gameObjects.ToArray(), settings);
	}

	public void Load()
	{
		ES3.Load<GameObject[]>(key, new GameObject[0], settings);
	}

	void Start()
	{
		if(loadEvent == LoadEvent.Start)
			Load();
	}

    public void Awake()
    {
        autoSaves = new HashSet<ES3AutoSave>();

        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            autoSaves.UnionWith(go.GetComponentsInChildren<ES3AutoSave>(true));

        _instance = this;

        if (loadEvent == LoadEvent.Awake)
            Load();
    }

    void OnApplicationQuit()
	{
		if(saveEvent == SaveEvent.OnApplicationQuit)
			Save();
	}

	void OnApplicationPause(bool paused)
	{
		if(	(saveEvent == SaveEvent.OnApplicationPause || 
			(Application.isMobilePlatform && saveEvent == SaveEvent.OnApplicationQuit)) && paused)
			Save();
	}

	/* Register an ES3AutoSave with the ES3AutoSaveMgr, if there is one */
	public static void AddAutoSave(ES3AutoSave autoSave)
	{
		if(ES3AutoSaveMgr.Instance != null)
			ES3AutoSaveMgr.Instance.autoSaves.Add(autoSave);
	}

	/* Remove an ES3AutoSave from the ES3AutoSaveMgr, for example if it's GameObject has been destroyed */
	public static void RemoveAutoSave(ES3AutoSave autoSave)
	{
		if(ES3AutoSaveMgr.Instance != null)
			ES3AutoSaveMgr.Instance.autoSaves.Remove(autoSave);
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
        // If the default settings have not yet been set, set them.
        if (settings == null || settings.bufferSize == 0)
        {
            settings = new ES3SerializableSettings(true);
            settings.path = "AutoSave.es3";
        }
		#endif
	}

	public void OnAfterDeserialize(){}
}
