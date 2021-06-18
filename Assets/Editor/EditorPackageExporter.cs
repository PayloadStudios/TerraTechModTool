using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorPackageExporter : EditorWindow
{
	[MenuItem("TerraTech Mod Tool/Package Export")]
	static void Open()
	{
		// Get existing open window or if none, make a new one:
		EditorPackageExporter window = GetWindow<EditorPackageExporter>("Package Export");
		window.Show();
	}

	void OnGUI()
	{
		if(GUILayout.Button("ExportPackage"))
		{
			List<string> assetPaths = new List<string>();

			AddAllFiles($"{ModUtils.AssetsDir}/Presets", assetPaths);
			AddAllFiles($"{ModUtils.AssetsDir}/SampleAssets", assetPaths);
			AddAllFiles($"{ModUtils.AssetsDir}/Scripts", assetPaths);
			AddAllFiles($"{ModUtils.AssetsDir}/Shaders", assetPaths);
			AddAllFiles($"{ModUtils.AssetsDir}/FixedAssets", assetPaths);

			AssetDatabase.ExportPackage(assetPaths.ToArray(), "TerraTechModTool.unitypackage");
		}
	}

	void AddAllFiles(string path, List<string> assetPaths)
	{
		foreach(string subpath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
		{
			if(!subpath.EndsWith(".meta"))
				assetPaths.Add(subpath);
		}

	}
}
