using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using ES3Internal;

public class ES3EditorUtility : Editor 
{
	public static void DisplayLink(string label, string url)
	{
		var style = ES3Editor.EditorStyle.Get;
		if(GUILayout.Button(label, style.link))
			Application.OpenURL(url);

		var buttonRect = GUILayoutUtility.GetLastRect();
		buttonRect.width = style.link.CalcSize(new GUIContent(label)).x;

		EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);	
	}

	public static bool IsPrefabInAssets(UnityEngine.Object obj)
	{
		#if UNITY_2018_3_OR_NEWER
		return PrefabUtility.IsPartOfPrefabAsset(obj);
		#else
		return (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab);
		#endif
	}

    /* 
     * Gets all children and components from a GameObject or GameObjects.
     * We create our own method for this because EditorUtility.CollectDeepHierarchy isn't thread safe in the Editor.
     */
    public static IEnumerable<UnityEngine.Object> CollectDeepHierarchy(IEnumerable<GameObject> gos)
    {
        var deepHierarchy = new HashSet<UnityEngine.Object>();
        foreach (var go in gos)
        {
            deepHierarchy.Add(go);
            deepHierarchy.UnionWith(go.GetComponents<Component>());
            foreach (Transform t in go.transform)
                deepHierarchy.UnionWith( CollectDeepHierarchy( new GameObject[] { t.gameObject } ) );
        }
        return deepHierarchy;
    }
}
