using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ES3Internal;


/*
 * ---- How Postprocessing works for the reference manager ----
 * - When the manager is first added to the scene, all top-level dependencies are added to the manager (AddManagerToScene).
 * - When the manager is first added to the scene, all prefabs with ES3Prefab components are added to the manager (AddManagerToScene).
 * - All GameObjects and Components in the scene are added to the reference manager when we enter Playmode or the scene is saved (PlayModeStateChanged, OnWillSaveAssets -> AddGameObjectsAndComponentstoManager).
 * - When a UnityEngine.Object field of a Component is modified, the new UnityEngine.Object reference is added to the reference manager (PostProcessModifications)
 * - All prefabs with ES3Prefab Components are added to the reference manager when we enter Playmode or the scene is saved (PlayModeStateChanged, OnWillSaveAssets -> AddGameObjectsAndComponentstoManager).
 * - Local references for prefabs are processed whenever a prefab with an ES3Prefab Component is deselected (SelectionChanged -> ProcessGameObject)
 */
[InitializeOnLoad]
public class ES3Postprocessor : UnityEditor.AssetModificationProcessor
{
	public static ES3ReferenceMgr _refMgr;
	public static ES3ReferenceMgr refMgr
	{
		get
		{
			if(ES3Settings.defaultSettingsScriptableObject.addMgrToSceneAutomatically && _refMgr == null)
				AddManagerToScene();
			return _refMgr;
		}
	}
	
	public static ES3AutoSaveMgr _autoSaveMgr;
	public static ES3AutoSaveMgr autoSaveMgr
	{
		get{ if(_autoSaveMgr != null) return _autoSaveMgr; if(refMgr == null) return null; return refMgr.gameObject.GetComponent<ES3AutoSaveMgr>(); }
	}

	public static GameObject lastSelected = null;
	
	public static Queue<GameObject> referenceQueue = new Queue<GameObject>();


	// This constructor is also called once when playmode is activated and whenever recompilation happens
    // because we have the [InitializeOnLoad] attribute assigned to the class.
	static ES3Postprocessor()
	{
		// Open the Easy Save 3 window the first time ES3 is installed.
		ES3Editor.ES3Window.OpenEditorWindowOnStart();

		EditorApplication.update += Update;
		Selection.selectionChanged += SelectionChanged;
        Undo.postprocessModifications += PostProcessModifications;

#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif

        //CreateDefaultSettingsIfNotExist();
    }

#region Reference Updating

#if UNITY_2017_2_OR_NEWER
    public static void PlayModeStateChanged(PlayModeStateChange state)

    {
        // Add all GameObjects and Components to the reference manager before we enter play mode.
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            //AddGameObjectsAndComponentsToManager();
            if (refMgr != null)
                refMgr.RefreshDependencies();
            UpdateAssembliesContainingES3Types();
        }
    }
#else
    public static void PlaymodeStateChanged()
    {
        // Add all GameObjects and Components to the reference manager before we enter play mode.
        if (!EditorApplication.isPlaying)
            AddGameObjectsAndComponentsToManager();
    }
#endif


    public static string[] OnWillSaveAssets(string[] paths)
    {
        // Add all GameObjects and Components to the reference manager whenever we save the scene.
        if (ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
            //AddGameObjectsAndComponentsToManager();
            if (refMgr != null)
                refMgr.RefreshDependencies();
        UpdateAssembliesContainingES3Types();
        return paths;
    }

    static UndoPropertyModification[] PostProcessModifications(UndoPropertyModification[] modifications)
    {
        if (refMgr != null)
            // For each property which has had an Undo registered ...
            foreach (var mod in modifications)
                // If this property change is an object reference, and the Component this change has been made to is in the scene, not in Assets ...
                if (mod.currentValue != null && mod.currentValue.objectReference != null && mod.currentValue.target != null && !AssetDatabase.Contains(mod.currentValue.target))
                    // If this object reference can be saved ...
                    if(ES3ReferenceMgr.CanBeSaved(mod.currentValue.objectReference))
                        // Add it to the reference manager
                        refMgr.Add(mod.currentValue.objectReference);
        return modifications;
    }

    static void AddGameObjectsAndComponentsToManager()
    {
        if (refMgr != null)
        {
            foreach (var obj in EditorUtility.CollectDeepHierarchy(SceneManager.GetActiveScene().GetRootGameObjects()))
            {
                try
                {
                    // If this object can be saved, add it to the reference manager.
                    if (obj != null && ES3ReferenceMgr.CanBeSaved(obj))
                        refMgr.Add(obj);
                }
                catch { }
            }

            refMgr.AddPrefabsToManager();
            refMgr.RemoveNullValues();
        }
    }

#endregion

    static void OnWillCreateAsset(string assetName)
    {
    }

    static void SelectionChanged()
	{
		if(EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
			return;

        if (!ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
            return;

        try
        {
            var selected = Selection.activeGameObject;

            // If we just deselected a prefab, process its references.
            if (lastSelected != null && ES3EditorUtility.IsPrefabInAssets(selected))
                ProcessGameObject(lastSelected);

            lastSelected = selected;
        }
        catch{}
	}
	
	static void Update()
	{
        if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
			return;

        if (refMgr == null) { /* We call refMgr so that it is automatically created if it doesn't already exist */ }

        /*try
        {
            // If the last selected GameObject hasn't been deselected, process it as if it had been deselected.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ProcessGameObject(lastSelected);
                lastSelected = null;
                return;
            }

            var timeStarted = Time.realtimeSinceStartup;

            //Ensure that the following code is always last in the Update() routine

            if (defaultSettings.autoUpdateReferences && refMgr != null)
            {
                while (referenceQueue.Count > 0)
                {
                    if (Time.realtimeSinceStartup - timeStarted > 0.02f)
                        return;
                    refMgr.AddDependencies(new UnityEngine.Object[] { referenceQueue.Dequeue() });
                }
            }
        }
        catch{}*/
	}

    private static void UpdateAssembliesContainingES3Types()
    {
#if UNITY_2017_3_OR_NEWER
        var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
        var defaults = ES3Settings.defaultSettingsScriptableObject;
        var assemblyNames = new List<string>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var name = assembly.name;
                var substr = name.Length >= 5 ? name.Substring(0, 5) : "";

                if (substr != "Unity" && substr != "com.u" && !name.Contains("-Editor"))
                    assemblyNames.Add(name);
            }
            catch { }
        }

        defaults.settings.assemblyNames = assemblyNames.ToArray();
        EditorUtility.SetDirty(defaults);
#endif
    }

	private static void ProcessGameObject(GameObject go)
	{
		if(go == null) return;
		
		if(ES3EditorUtility.IsPrefabInAssets(go))
		{
			var es3Prefab = go.GetComponent<ES3Prefab>();
			if(es3Prefab != null)
				es3Prefab.GeneratePrefabReferences();
		}
		else if(refMgr != null)
			refMgr.AddDependencies(go);
	}

    public static GameObject AddManagerToScene()
	{
		if(_refMgr != null)
			return _refMgr.gameObject;
		
		var mgr = GameObject.Find("Easy Save 3 Manager");

		if(mgr == null)
		{
			mgr = new GameObject("Easy Save 3 Manager");
			var inspectorInfo = mgr.AddComponent<ES3InspectorInfo>();
			inspectorInfo.message = "The Easy Save 3 Manager is required in any scenes which use Easy Save, and is automatically added to your scene when you enter Play mode.\n\nTo stop this from automatically being added to your scene, go to 'Window > Easy Save 3 > Settings' and deselect the 'Auto Add Manager to Scene' checkbox.";

			_refMgr = mgr.AddComponent<ES3ReferenceMgr>();
			_autoSaveMgr = mgr.AddComponent<ES3AutoSaveMgr>();
			
			referenceQueue = new Queue<GameObject>(EditorSceneManager.GetActiveScene().GetRootGameObjects());

            _refMgr.RefreshDependencies();
			_refMgr.GeneratePrefabReferences();

			Undo.RegisterCreatedObjectUndo(mgr, "Enabled Easy Save for Scene");

		}
		else
		{
			_refMgr = mgr.GetComponent<ES3ReferenceMgr>();
			if(_refMgr == null)
			{
				_refMgr = mgr.AddComponent<ES3ReferenceMgr>();
				Undo.RegisterCreatedObjectUndo(_refMgr, "Enabled Easy Save for Scene");
			}

			_autoSaveMgr = mgr.GetComponent<ES3AutoSaveMgr>();
			if(_autoSaveMgr == null)
			{
				_autoSaveMgr = mgr.AddComponent<ES3AutoSaveMgr>();
				Undo.RegisterCreatedObjectUndo(_autoSaveMgr, "Enabled Easy Save for Scene");
			}
		}
		return mgr;
	}
}