#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
#endif

// Has to be outside the Editor namespace because otherwise #if UNITY_EDITOR code outside of the Editor namespace can't use it
public static class ModUtils
{
#if UNITY_EDITOR
	private static string kAssetsDir = null;
	public static string AssetsDir
	{
		get
		{
			if (kAssetsDir == null)
			{
				if (AssetDatabase.LoadAssetAtPath<Preset>("Assets/Presets/Albedo.preset") == null)
				{
					// Preset not present, we must be in the pre-exported version
					kAssetsDir = "Assets/PayloadStudios/TerraTechModTool";
				}
				else
				{
					kAssetsDir = "Assets";
				}
			}

			return kAssetsDir;
		}
	}

	public static string k_ModsDirectory { get { return "Assets/Mods"; } } // Does not move with implementation
	public static string k_ModPreviewTemplate { get { return $"{AssetsDir}/SampleAssets/preview.png"; } }

	// To be populated by the editor window or ManMods in game.
	public static List<ModdedCorpDefinition> m_ModdedCorps = new List<ModdedCorpDefinition>();
#endif

	public static string CreateCompoundId(string modId, string assetId)
	{
		return $"{modId}:{assetId}";
	}

	public static bool IsValidModId(string test)
	{
		if (test.Contains(":"))
			return false;

		return true;
	}

	public static bool IsValidCompoundId(string test)
	{
		string[] parts = test.Split(':');
		if (parts.Length != 2)
			return false;

		return true;
	}

	public static bool IsVanillaCorp(string corpID)
	{
		switch(corpID)
		{
			case "GSO":
			case "GC":
			case "VEN":
			case "HE":
			case "BF":
			case "EXP":
				return true;
			default:
				return false;
		}
	}

	public static string GetModFromCompoundId(string compoundId)
	{
		int index = compoundId.IndexOf(':');
		return compoundId.Substring(0, index);
	}

	public static string GetAssetFromCompoundId(string compoundId)
	{
		int index = compoundId.IndexOf(':');
		return compoundId.Substring(index + 1);
	}

	public static bool SplitCompoundId(string compoundId, out string modId, out string assetId)
	{
		int index = compoundId.IndexOf(':');
		modId = compoundId.Substring(0, index);
		assetId = compoundId.Substring(index + 1);
		return true;
	}
}
