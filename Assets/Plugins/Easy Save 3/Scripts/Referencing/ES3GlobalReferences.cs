using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ES3Internal
{
    public class ES3GlobalReferences : ScriptableObject
    {
        private const string globalReferencesPath = "ES3/ES3GlobalReferences";

        public ES3RefIdDictionary refId = new ES3RefIdDictionary();

        private static ES3GlobalReferences _globalReferences = null;
        public static ES3GlobalReferences Instance
        {
            get
            {
                if (!ES3Settings.defaultSettingsScriptableObject.useGlobalReferences)
                    return null;

                if (_globalReferences == null)
                {
                    _globalReferences = Resources.Load<ES3GlobalReferences>(globalReferencesPath);
#if UNITY_EDITOR
                    if (_globalReferences == null)
                    {
                        _globalReferences = ScriptableObject.CreateInstance<ES3GlobalReferences>();

                        // If this is the version being submitted to the Asset Store, don't include ES3Defaults.
                        if (Application.productName.Contains("ES3 Release"))
                        {
                            Debug.Log("This has been identified as a release build as the title contains 'ES3 Release', so ES3GlobalReferences will not be created.");
                            return _globalReferences;
                        }

                        UnityEditor.AssetDatabase.CreateAsset(_globalReferences, PathToGlobalReferences());
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
#endif
                }
                return _globalReferences;
            }
        }

        public long Get(UnityEngine.Object obj)
        {
            if (obj == null)
                return -1;

            long id;
            if (!refId.TryGetValue(obj, out id))
                return -1;
            return id;
        }

        public UnityEngine.Object Get(long id)
        {
            foreach(var kvp in refId)
                if (kvp.Value == id)
                    return kvp.Key;
            return null;
        }

#if UNITY_EDITOR
        public long GetOrAdd(UnityEngine.Object obj)
        {
            if (Application.isPlaying)
            {
                ES3Debug.LogError("GetOrAdd can only be called in the Editor, not during runtime");
                return -1;
            }

            var id = Get(obj);

            if (id == -1)
            {
                if (!UnityEditor.AssetDatabase.Contains(obj))
                    return -1;
                id = ES3ReferenceMgrBase.GetNewRefID();
                refId.Add(obj, id);

                UnityEditor.EditorUtility.SetDirty(this);
            }

            return id;
        }

        private static string PathToGlobalReferences()
        {
            return ES3Settings.PathToEasySaveFolder() + "Resources/" + globalReferencesPath +".asset";
        }
#endif
    }
}
