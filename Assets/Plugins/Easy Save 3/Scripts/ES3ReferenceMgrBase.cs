using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ES3Internal
{
	[System.Serializable]
	[DisallowMultipleComponent]
	public abstract class ES3ReferenceMgrBase : MonoBehaviour
	{
		public const string referencePropertyName = "_ES3Ref";
		private static ES3ReferenceMgrBase _current = null;
#if UNITY_EDITOR
        private static int CollectDependenciesDepth = 2;
#endif

        private static System.Random rng;

		[HideInInspector]
		public bool openPrefabs = false; // Whether the prefab list should be open in the Editor.

        public List<ES3Prefab> prefabs = new List<ES3Prefab>();

        public static ES3ReferenceMgrBase Current
		{
			get
			{
				if(_current == null)
				{
					var mgrs = UnityEngine.Object.FindObjectsOfType<ES3ReferenceMgrBase>();
					if(mgrs.Length == 1)
						_current = mgrs[0];
					else if(mgrs.Length > 1)
						throw new InvalidOperationException("There is more than one ES3ReferenceMgr in this scene, but there must only be one.");
				}
				return _current; 
			}
		}

		public bool IsInitialised{ get{ return idRef.Count > 0; } }

		[SerializeField]
		public ES3IdRefDictionary idRef = new ES3IdRefDictionary();
		private ES3RefIdDictionary _refId = null;

		public ES3RefIdDictionary refId
		{
			get
			{
				if(_refId == null)
				{
					_refId = new ES3RefIdDictionary();
					// Populate the reverse dictionary with the items from the normal dictionary.
					foreach (var kvp in idRef)
						if(kvp.Value != null)
							_refId [kvp.Value] = kvp.Key;
				}
				return _refId;
			}
			set
			{
				_refId = value;
			}
		}

        public ES3GlobalReferences _globalReferences = null;
        public ES3GlobalReferences GlobalReferences
        {
            get
            {
                if (_globalReferences == null)
                    _globalReferences = ES3GlobalReferences.Instance;
                return _globalReferences;
            }
        }

        public void Awake()
		{
			if(_current != null && _current != this)
			{
				_current.Merge(this);
				if(gameObject.name.Contains("Easy Save 3 Manager"))
					Destroy(this.gameObject);
				else
					Destroy(this);
			}
			else
				_current = this;
		}

		// Merges two managers, not allowing any clashes of IDs
		public void Merge(ES3ReferenceMgrBase otherMgr)
		{
			foreach(var kvp in otherMgr.idRef)
			{
				// Check for duplicate keys with different values.
				UnityEngine.Object value;
				if(idRef.TryGetValue(kvp.Key, out value))
				{
					if(value != kvp.Value)
						throw new ArgumentException ("Attempting to merge two ES3 Reference Managers, but they contain duplicate IDs. If you've made a copy of a scene and you're trying to load it additively into another scene, generate new reference IDs by going to Assets > Easy Save 3 > Generate New Reference IDs for Scene. Alternatively, remove the Easy Save 3 Manager from the scene if you do not intend on saving any data from it.");
				}
				else
					Add(kvp.Value, kvp.Key);
			}
		}

		public long Get(UnityEngine.Object obj)
		{
			long id;
			if(!refId.TryGetValue(obj, out id))
				return -1;
			return id;
		}

        internal UnityEngine.Object Get(long id, Type type)
        {
            if (id == -1)
                return null;
            UnityEngine.Object obj;
            if (!idRef.TryGetValue(id, out obj))
            {
                if (GlobalReferences != null)
                {
                    var globalRef = GlobalReferences.Get(id);
                    if (globalRef != null)
                        return globalRef;
                }

                ES3Internal.ES3Debug.LogWarning("Reference for "+type+" with ID " + id + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene. If you are loading objects dynamically, this warning is expected and can be ignored.", this);
                return null;
            }
            return obj;
        }

        public UnityEngine.Object Get(long id, bool suppressWarnings=false)
		{
			if(id == -1)
				return null;
			UnityEngine.Object obj;
            if (!idRef.TryGetValue(id, out obj))
            {
                if (GlobalReferences != null)
                {
                    var globalRef = GlobalReferences.Get(id);
                    if (globalRef != null)
                        return globalRef;
                }

                if (!suppressWarnings) ES3Internal.ES3Debug.LogWarning("Reference for property ID "+id+" could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene. If you are loading objects dynamically, this warning is expected and can be ignored.", this);
                return null;
            }
			return obj;
		}

		public ES3Prefab GetPrefab(long id, bool suppressWarnings = false)
		{
			for(int i=0; i<prefabs.Count; i++)
				if(prefabs[i] != null && prefabs[i].prefabId == id)
					return prefabs[i];
            if (!suppressWarnings) ES3Internal.ES3Debug.LogWarning("Prefab with ID " + id + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene.", this);
            return null;
		}

		public long GetPrefab(ES3Prefab prefab, bool suppressWarnings = false)
		{
			for(int i=0; i<prefabs.Count; i++)
				if(prefabs[i] == prefab)
					return prefabs[i].prefabId;
            if (!suppressWarnings) ES3Internal.ES3Debug.LogWarning("Prefab with name " + prefab.name + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene.", prefab);
            return -1;
		}

        public long Add(UnityEngine.Object obj)
		{
			long id; 
			// If it already exists in the list, do nothing.
			if(refId.TryGetValue(obj, out id))
				return id;
#if UNITY_EDITOR
            if(!Application.isPlaying && GlobalReferences != null)
            {
                id = GlobalReferences.GetOrAdd(obj);
                if(id != -1)
                {
                    Add(obj, id);
                    return id;
                }
            }
#endif
            // Add the reference to the Dictionary.
            id = GetNewRefID();
			Add(obj, id);
			return id;
		}

		public void Add(UnityEngine.Object obj, long id)
		{
			// If the ID is -1, auto-generate an ID.
			if(id == -1)
				id = GetNewRefID();
			// Add the reference to the Dictionary.
			idRef[id] = obj;
			refId [obj] = id;
		}

		public void AddPrefab(ES3Prefab prefab)
		{
			if(!prefabs.Contains(prefab))
				prefabs.Add(prefab);
		}

		public void Remove(UnityEngine.Object obj)
		{	
			refId.Remove(obj);
            // There may be multiple references with the same ID, so remove them all.
            foreach (var item in idRef.Where(kvp => kvp.Value == obj).ToList())
                idRef.Remove(item.Key);
		}

		public void Remove(long referenceID)
		{
			idRef.Remove(referenceID);
            // There may be multiple references with the same ID, so remove them all.
            foreach (var item in refId.Where(kvp => kvp.Value == referenceID).ToList())
                refId.Remove(item.Key);
        }

		public void RemoveNullValues()
		{
			var nullKeys = idRef.Where(pair => pair.Value == null)
								.Select(pair => pair.Key).ToList();
			foreach (var key in nullKeys)
				idRef.Remove(key);
		}

		public void Clear()
		{
			refId.Clear();
			idRef.Clear();
		}

		public bool Contains(UnityEngine.Object obj)
		{
			return refId.ContainsKey(obj);
		}

		public bool Contains(long referenceID)
		{
			return idRef.ContainsKey(referenceID);
		}

		public void ChangeId(long oldId, long newId)
		{
			idRef.ChangeKey(oldId, newId);
			// Empty the refId so it has to be refreshed.
			refId = null;
		}

		internal static long GetNewRefID()
		{
			if(rng == null)
				rng = new System.Random();

			byte[] buf = new byte[8];
			rng.NextBytes(buf);
			long longRand = BitConverter.ToInt64(buf, 0);

			return (System.Math.Abs(longRand % (long.MaxValue - 0)) + 0);
		}

#if UNITY_EDITOR
        /*
         * Collects all top-level dependencies of an object.
         * For GameObjects, it will traverse all children.
         * For Components or ScriptableObjects, it will get all serialisable UnityEngine.Object fields/properties as dependencies.
         */
        public static HashSet<UnityEngine.Object> CollectDependencies(UnityEngine.Object[] objs, HashSet<UnityEngine.Object> dependencies = null)
        {
            if (dependencies == null)
                dependencies = new HashSet<UnityEngine.Object>(objs);
            else
                dependencies.UnionWith(objs);

            foreach (var obj in objs)
            {
                if (obj == null)
                    continue;

                var type = obj.GetType();
                // Skip types which don't need processing
                if (type == typeof(ES3ReferenceMgr) || type == typeof(ES3Prefab) || type == typeof(ES3AutoSaveMgr) || type == typeof(ES3AutoSave) || type == typeof(ES3InspectorInfo))
                    continue;

                // If it's a GameObject, get the GameObject's Components and collect their dependencies.
                if (type == typeof(GameObject))
                {
                    var go = (GameObject)obj;
                    // Get the dependencies of each Component in the GameObject.
                    CollectDependencies(go.GetComponents<Component>(), dependencies);
                    // Get the dependencies of each child in the GameObject.
                    foreach(Transform child in go.transform)
                       CollectDependencies( child.gameObject, dependencies);
                }
                // Else if it's a Component or ScriptableObject, add the values of any UnityEngine.Object fields as dependencies.
                else
                    CollectDependenciesFromFields(obj, dependencies, CollectDependenciesDepth);
            }

            return dependencies;
        }

        public static HashSet<UnityEngine.Object> CollectDependencies(UnityEngine.Object obj, HashSet<UnityEngine.Object> dependencies = null)
        {
            return CollectDependencies(new UnityEngine.Object[] { obj}, dependencies);
        }

        private static void CollectDependenciesFromFields(UnityEngine.Object obj, HashSet<UnityEngine.Object> dependencies, int depth)
        {
            if (depth == 0)
                return;

            var so = new UnityEditor.SerializedObject(obj);
            if (so == null)
                return;

            var property = so.GetIterator();
            if (property == null)
                return;

            // Iterate through each of this object's properties.
            while (property.NextVisible(true))
            {
                try
                {
                    // If it's an array which contains UnityEngine.Objects, add them as dependencies.
                    if (property.isArray)
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            var element = property.GetArrayElementAtIndex(i);
                            // If the array contains UnityEngine.Object types, add them to the dependencies.
                            if (element.propertyType == UnityEditor.SerializedPropertyType.ObjectReference && element.objectReferenceValue != null)
                            {
                                dependencies.Add(element.objectReferenceValue);
                                CollectDependenciesFromFields(element.objectReferenceValue, dependencies, depth - 1);
                            }
                            // Otherwise this array does not contain UnityEngine.Object types, so we should stop.
                            else
                                break;
                        }
                    }
                    // Else if it's a normal UnityEngine.Object field, add it.
                    else if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
                    {
                        dependencies.Add(property.objectReferenceValue);
                        CollectDependenciesFromFields(property.objectReferenceValue, dependencies, depth - 1);
                    }
                }
                catch { }
            }
        }
#endif
    }

	[System.Serializable]
	public class ES3IdRefDictionary : ES3SerializableDictionary<long, UnityEngine.Object>
	{
		protected override bool KeysAreEqual(long a, long b)
		{
			return a == b;
		}

		protected override bool ValuesAreEqual(UnityEngine.Object a, UnityEngine.Object b)
		{
			return a == b;
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	[System.Serializable]
	public class ES3RefIdDictionary : ES3SerializableDictionary<UnityEngine.Object, long>
	{
		protected override bool KeysAreEqual(UnityEngine.Object a, UnityEngine.Object b)
		{
			return a == b;
		}

		protected override bool ValuesAreEqual(long a, long b)
		{
			return a == b;
		}
	}
}