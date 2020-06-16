using System.IO;
using System;
using UnityEngine;

namespace ES3Internal
{
	public static class ES3IO
	{
        public static readonly string persistentDataPath = Application.persistentDataPath;

		public enum ES3FileMode {Read, Write, Append}

		public static DateTime GetTimestamp(string filePath)
		{
			if(!FileExists(filePath))
				return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			return File.GetLastWriteTime(filePath).ToUniversalTime();
		}

		public static string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		public static void DeleteFile(string filePath)
		{ 
			if(FileExists(filePath))
				File.Delete(filePath);	
		}

		public static bool FileExists(string filePath) { return File.Exists(filePath); }
		public static void MoveFile(string sourcePath, string destPath) { File.Move(sourcePath, destPath); }
		public static void CopyFile(string sourcePath, string destPath) { File.Copy(sourcePath, destPath); }

		public static void MoveDirectory(string sourcePath, string destPath) { Directory.Move(sourcePath, destPath); }
		public static void CreateDirectory(string directoryPath){ Directory.CreateDirectory(directoryPath); }
		public static bool DirectoryExists(string directoryPath) { return Directory.Exists(directoryPath); }

		/*
		 * 	Given a path, it returns the directory that path points to.
		 * 	eg. "C:/myFolder/thisFolder/myFile.txt" will return "C:/myFolder/thisFolder".
		 */
		public static string GetDirectoryPath(string path, char seperator = '/')
		{ 
			//return Path.GetDirectoryName(path);
			// Path.GetDirectoryName turns forward slashes to backslashes in some cases on Windows, which is why
			// Substring is used instead.
			char slashChar = UsesForwardSlash(path) ? '/' : '\\';
		
			int slash = path.LastIndexOf(slashChar);
			// Ignore trailing slash if necessary.
			if(slash == (path.Length - 1))
				slash = path.Substring(0, slash).LastIndexOf(slashChar);
			if(slash == -1)
				ES3Debug.LogError("Path provided is not a directory path as it contains no slashes.");
			return path.Substring(0, slash);
		}
		
		public static bool UsesForwardSlash(string path)
		{
			if(path.Contains("/"))
				return true;
			return false;
		}
		
		// Takes a directory path and a file or directory name and combines them into a single path.
		public static string CombinePathAndFilename(string directoryPath, string fileOrDirectoryName)
		{
			if(directoryPath[directoryPath.Length-1] != '/' && directoryPath[directoryPath.Length-1] != '\\')
				directoryPath += '/';
			return directoryPath + fileOrDirectoryName;
		}
			
		public static string[] GetDirectories(string path, bool getFullPaths = true)
		{
			var paths = Directory.GetDirectories(path);
			for (int i = 0; i < paths.Length; i++) 
			{
				if(!getFullPaths)
					paths[i] = Path.GetFileName(paths[i]);
				// GetDirectories sometimes returns backslashes, so we need to convert them to
				// forward slashes.
				paths[i].Replace ("\\", "/");
			}
			return paths;
		}

		public static void DeleteDirectory(string directoryPath)
		{
			if(DirectoryExists(directoryPath))
				Directory.Delete( directoryPath, true ); 
		}

		public static string[] GetFiles(string path, bool getFullPaths = true)
		{
			var paths = Directory.GetFiles(path);
			if(!getFullPaths)
			{
				for(int i=0; i<paths.Length; i++)
					paths[i] = Path.GetFileName(paths[i]);
			}
			return paths;
		}

		public static byte[] ReadAllBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			File.WriteAllBytes(path, bytes);
		}

		public static void CommitBackup(ES3Settings settings)
		{
			if(settings.location == ES3.Location.File)
			{
				// Delete the old file before overwriting it.
				DeleteFile(settings.FullPath);
				// Rename temporary file to new file.
				MoveFile(settings.FullPath + ES3.temporaryFileSuffix, settings.FullPath);
			}
			else if(settings.location == ES3.Location.PlayerPrefs)
			{
				PlayerPrefs.SetString(settings.FullPath, PlayerPrefs.GetString(settings.FullPath + ES3.temporaryFileSuffix));
				PlayerPrefs.DeleteKey(settings.FullPath + ES3.temporaryFileSuffix);
				PlayerPrefs.Save();
			}
		}
	}
}