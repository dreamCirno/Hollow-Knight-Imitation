using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ES3Internal;

namespace ES3Editor
{
	public class AddES3Prefab : Editor 
	{
		[MenuItem("GameObject/Easy Save 3/Enable Easy Save for Prefab", false, 1001)]
		[MenuItem("Assets/Easy Save 3/Enable Easy Save for Prefab", false, 1001)]
		public static void Enable()
		{
			var go = Selection.activeGameObject;

			#if UNITY_2018_3_OR_NEWER
			if(PrefabUtility.GetPrefabInstanceStatus(go) != PrefabInstanceStatus.NotAPrefab)
			{
				go = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(go);
				if(go == null)
					return;
			}
			#else
			if(PrefabUtility.GetPrefabType(go) != PrefabType.Prefab)
			{
				go = (GameObject)PrefabUtility.GetPrefabParent(go);
				if(go == null)
					return;
			}
			#endif
		
			var es3Prefab = Undo.AddComponent<ES3Prefab>(go);

            if (ES3ReferenceMgr.Current != null)
            {
                ES3ReferenceMgr.Current.AddPrefab(es3Prefab);
                EditorUtility.SetDirty(ES3ReferenceMgr.Current);
            }
		}

		[MenuItem("GameObject/Easy Save 3/Enable Easy Save for Prefab", true, 1001)]
		[MenuItem("Assets/Easy Save 3/Enable Easy Save for Prefab", true, 1001)]
		public static bool Validate()
		{
			var go = Selection.activeGameObject;
			if(go == null)
				return false;
			if(go.GetComponent<ES3Prefab>() != null)
				return false;
			return true;
		}
	}
}
