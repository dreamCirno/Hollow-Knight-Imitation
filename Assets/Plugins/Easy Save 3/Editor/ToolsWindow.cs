using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ES3Editor
{
	public class ToolsWindow : SubWindow
	{
		public ToolsWindow(EditorWindow window) : base("Tools", window){}

		public override void OnGUI()
		{
			var style = EditorStyle.Get;

			EditorGUILayout.BeginHorizontal(style.area);

			if(GUILayout.Button("Open Persistent Data Path"))
				OSFileBrowser.Open(Application.persistentDataPath);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(style.area);

			if(GUILayout.Button("Clear Persistent Data Path"))
			{
				if(EditorUtility.DisplayDialog("Clear Persistent Data Path", "Are you sure you wish to clear the persistent data path?\n This action cannot be reversed.", "Clear", "Cancel"))
				{
					System.IO.DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);

					foreach (FileInfo file in di.GetFiles())
						file.Delete(); 
					foreach (DirectoryInfo dir in di.GetDirectories())
						dir.Delete(true); 
				}
			}

			if(GUILayout.Button("Clear PlayerPrefs"))
				if(EditorUtility.DisplayDialog("Clear PlayerPrefs", "Are you sure you wish to clear PlayerPrefs?\nThis action cannot be reversed.", "Clear", "Cancel"))
					PlayerPrefs.DeleteAll();

			EditorGUILayout.EndHorizontal();
		}
	}

	public static class OSFileBrowser
	{
		public static bool IsInMacOS
		{
			get
			{
				return UnityEngine.SystemInfo.operatingSystem.IndexOf("Mac OS") != -1;
			}
		}

		public static bool IsInWinOS
		{
			get
			{
				return UnityEngine.SystemInfo.operatingSystem.IndexOf("Windows") != -1;
			}
		}

		public static void OpenInMac(string path)
		{
			bool openInsidesOfFolder = false;

			// try mac
			string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

			if ( System.IO.Directory.Exists(macPath) ) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}

			if ( !macPath.StartsWith("\"") )
			{
				macPath = "\"" + macPath;
			}

			if ( !macPath.EndsWith("\"") )
			{
				macPath = macPath + "\"";
			}

			string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

			try
			{
				System.Diagnostics.Process.Start("open", arguments);
			}
			catch ( System.ComponentModel.Win32Exception e )
			{
				// tried to open mac finder in windows
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}

		public static void OpenInWin(string path)
		{
			bool openInsidesOfFolder = false;

			// try windows
			string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

			if ( System.IO.Directory.Exists(winPath) ) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}

			try
			{
				System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
			}
			catch ( System.ComponentModel.Win32Exception e )
			{
				// tried to open win explorer in mac
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}

		public static void Open(string path)
		{
			if ( IsInWinOS )
			{
				OpenInWin(path);
			}
			else if ( IsInMacOS )
			{
				OpenInMac(path);
			}
			else // couldn't determine OS
			{
				OpenInWin(path);
				OpenInMac(path);
			}
		}
	}
}
