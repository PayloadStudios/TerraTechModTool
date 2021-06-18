using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;


public class AssetImportCatcher : AssetPostprocessor
{
	private static bool sShouldPostprocess = true;

	public static void ManualSaveAssets()
	{
		sShouldPostprocess = false;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		sShouldPostprocess = true;
	}

	public static void ManualAssetRefresh()
	{
		sShouldPostprocess = false;
		AssetDatabase.Refresh();
		sShouldPostprocess = true;
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		if (!sShouldPostprocess)
			return;

		EditorWindowModDesigner window = EditorWindow.GetWindow<EditorWindowModDesigner>("Mod Designer");
		window.ScanForExistingMods();
	}
}