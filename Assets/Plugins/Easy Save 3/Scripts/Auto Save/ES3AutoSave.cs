using UnityEngine;
using System.Collections.Generic;

public class ES3AutoSave : MonoBehaviour
{
	public bool saveChildren = false;
	private bool isQuitting = false;

    [HideInInspector]
    public List<Component> componentsToSave = new List<Component>();

    public void Awake()
    {
        if (ES3AutoSaveMgr.Instance == null)
            ES3Internal.ES3Debug.LogWarning("<b>No GameObjects in this scene will be autosaved</b> because there is no Easy Save 3 Manager. To add a manager to this scene, exit playmode and go to Assets > Easy Save 3 > Add Manager to Scene.", this);
        else
            ES3AutoSaveMgr.AddAutoSave(this);
    }

    public void OnApplicationQuit()
	{
		isQuitting = true;
	}

	public void OnDestroy()
	{
		// If this is being destroyed, but not because the application is quitting,
		// remove the AutoSave from the manager.
		if(!isQuitting)
			ES3AutoSaveMgr.RemoveAutoSave (this);
	}
}