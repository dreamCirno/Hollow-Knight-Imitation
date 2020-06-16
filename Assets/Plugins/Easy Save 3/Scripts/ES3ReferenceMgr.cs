using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ES3Internal;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using System;
#endif

public class ES3ReferenceMgr : ES3ReferenceMgrBase, ISerializationCallbackReceiver 
{
	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		// This is called before building or when things are being serialised before pressing play.
		#endif
	}

	public void OnAfterDeserialize(){}

	#if UNITY_EDITOR

	public void RefreshDependencies()
	{
        // This will get the dependencies for all GameObjects and Components from the active scene.
        AddDependencies(SceneManager.GetActiveScene().GetRootGameObjects());
        AddPrefabsToManager();
        RemoveNullValues();
    }

    public void Optimize()
    {
        var dependencies = CollectDependencies(SceneManager.GetActiveScene().GetRootGameObjects());
        var notDependenciesOfScene = new HashSet<UnityEngine.Object>();

        foreach (var kvp in idRef)
            if (!dependencies.Contains(kvp.Value))
                notDependenciesOfScene.Add(kvp.Value);

        foreach (var obj in notDependenciesOfScene)
            Remove(obj);
    }

	public void AddDependencies(UnityEngine.Object[] objs, float timeoutSecs=1f)
	{
		var startTime = Time.realtimeSinceStartup;

        for(int i=0; i<objs.Length; i++)
        {
            var obj = objs[i];

            // If it's longer than the timeout period, show a progress bar so people can cancel if necessary.
            if (Time.realtimeSinceStartup - startTime > timeoutSecs)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Updating references", "Easy Save is updating references for this scene.", i / objs.Length))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Reference updating cancelled", "Reference updating was cancelled.\nTo continue updating references, go to Window > Easy Save 3 > References, and press Refresh references", "Ok");
                    return;
                }
            }
        	
    	    var dependencies = CollectDependencies(obj);
    
    		foreach(var dependency in dependencies)
    			if(dependency != null && CanBeSaved(dependency))
    			    Add(dependency);
        }
        EditorUtility.ClearProgressBar();
        Undo.RecordObject(this, "Update Easy Save 3 Reference List");
	}

    public void AddDependencies(UnityEngine.Object obj, float timeoutSecs = 1f)
    {
        AddDependencies(new UnityEngine.Object[] { obj }, timeoutSecs);
    }


    public void GeneratePrefabReferences()
	{
		AddPrefabsToManager();
		foreach(var es3Prefab in prefabs)
			es3Prefab.GeneratePrefabReferences();
    }
    
    public void AddPrefabsToManager()
	{
		if(this.prefabs.RemoveAll(item => item == null) > 0)
			Undo.RecordObject(this, "Update Easy Save 3 Reference List");

		foreach(var es3Prefab in Resources.FindObjectsOfTypeAll<ES3Prefab>())
		{
            try
            {
                if (es3Prefab != null && EditorUtility.IsPersistent(es3Prefab) && GetPrefab(es3Prefab, true) == -1)
                {
                    AddPrefab(es3Prefab);
                    Undo.RecordObject(this, "Update Easy Save 3 Reference List");
                }
            }
            catch { }
		}
	}

    public static bool CanBeSaved(UnityEngine.Object obj)
	{
        if (obj == null)
            return true;

        var type = obj.GetType();

        // Check if any of the hide flags determine that it should not be saved.
        if ((((obj.hideFlags & HideFlags.DontSave) == HideFlags.DontSave) || 
		     ((obj.hideFlags & HideFlags.DontSaveInBuild) == HideFlags.DontSaveInBuild) ||
		     ((obj.hideFlags & HideFlags.DontSaveInEditor) == HideFlags.DontSaveInEditor) ||
		     ((obj.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)))
		{
			// Meshes are marked with HideAndDontSave, but shouldn't be ignored.
			if(type == typeof(Mesh) || type == typeof(Material))
				return true;
        }

        // Exclude the Easy Save 3 Manager, and all components attached to it.
        if (obj.name == "Easy Save 3 Manager")
            return false;

        return true;
	}

	#endif
}
