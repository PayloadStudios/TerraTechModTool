using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEditor;
using System.IO;

// -----------------------------------------------------------------
// <SHARED CLASS> This is shared between TerraTech and TTModDesigner
// -----------------------------------------------------------------
public class ModContents : ScriptableObject
{
	public override string ToString() { return ModName; }

	public string ModName = "";
	public ModBase Script = null;
	public PublishedFileId_t m_WorkshopId = PublishedFileId_t.Invalid;
	public List<ModdedSkinDefinition> m_Skins = new List<ModdedSkinDefinition>();
	public List<ModdedCorpDefinition> m_Corps = new List<ModdedCorpDefinition>();
	public List<ModdedBlockDefinition> m_Blocks = new List<ModdedBlockDefinition>();
	public List<Object> m_AdditionalAssets = new List<Object>();

	public Object FindAsset(string id)
	{
		if (m_AdditionalAssets != null)
		{
			foreach (Object obj in m_AdditionalAssets)
			{
				if (obj.name == id)
					return obj;
			}
		}
		return null;
	}

	// TerraTechModTool only - this data is not exported
#if UNITY_EDITOR
	public bool CanPublish() { return m_WorkshopId != PublishedFileId_t.Invalid; }
	public void SetWorkshopID(PublishedFileId_t id)
	{
		m_WorkshopId = id;
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();
	}

	public IEnumerator<ModdedAsset> GetEnumerator()
	{
		foreach (ModdedSkinDefinition skin in m_Skins)
			yield return skin;
		foreach (ModdedCorpDefinition corp in m_Corps)
			yield return corp;
		foreach (ModdedBlockDefinition block in m_Blocks)
			yield return block;
	}

	public const string k_OutputDir = "SteamOutput";

	public string WorkingDir { get { return $"{ ModUtils.k_ModsDirectory}/{ModName}"; } }
	public string OutputDir { get { return $"{k_OutputDir}/{ModName}"; } }

	public string m_Changenotes = "Initial release";

	public void GenerateAssets()
	{
		for (int i = m_Skins.Count - 1; i >= 0; i--)
		{
			m_Skins[i].GenerateCombinedTextureIfRequired($"{WorkingDir}/Skins/{m_Skins[i].m_SkinDisplayName}");
		}
	}

	// Return the number of invalid assets
	public int VerifyAssets(bool forBundle = false)
	{
		int numInvalid = 0;

		// Check for new skins
		foreach (string path in Directory.EnumerateFiles(WorkingDir, "*", SearchOption.AllDirectories))
		{
			Object item = AssetDatabase.LoadMainAssetAtPath(path);
			if (item is ModdedSkinDefinition)
			{
				ModdedSkinDefinition skin = (ModdedSkinDefinition)item;
				if (!m_Skins.Contains(skin) && !skin.m_IsCorpDefault)
				{
					Debug.LogWarning($"Found skin {item.name} at path {path} in mod {ModName} that was not listed in the contents. Adding now.");
					m_Skins.Add(skin);
				}
			}
			else if(item is ModdedCorpDefinition)
			{
				ModdedCorpDefinition corp = (ModdedCorpDefinition)item;
				if(!m_Corps.Contains(corp))
				{
					Debug.LogWarning($"Found corp {item.name} at path {path} in mod {ModName} that was not listed in the contents. Adding now.");
					m_Corps.Add(corp);
				}
			}
			else if (item is ModdedBlockDefinition)
			{
				ModdedBlockDefinition block = (ModdedBlockDefinition)item;
				if (!m_Blocks.Contains(block))
				{
					Debug.LogWarning($"Found block {item.name} at path {path} in mod {ModName} that was not listed in the contents. Adding now.");
					m_Blocks.Add(block);
				}
			}
		}		

		for (int i = m_Skins.Count - 1; i >= 0; i--)
		{
			if(VerifyAsset(m_Skins[i], forBundle, (asset) => { return !asset.m_IsCorpDefault; }))
			{
				m_Skins.RemoveAt(i);
				numInvalid++;
			}
		}

		for (int i = m_Corps.Count - 1; i >= 0; i--)
		{
			if (VerifyAsset(m_Corps[i], forBundle, (asset) => { return true; }))
			{
				m_Corps.RemoveAt(i);
				numInvalid++;
			}
		}

		for (int i = m_Blocks.Count - 1; i >= 0; i--)
		{
			if (m_Blocks[i] != null)
			{
				m_Blocks[i].StripPreviews();
				if (VerifyAsset(m_Blocks[i], forBundle, (asset) => { return true; }))
				{
					m_Blocks.RemoveAt(i);
					numInvalid++;
				}
			}
			else
			{
				m_Blocks.RemoveAt(i);
				numInvalid++;
			}
		}

		return numInvalid;
	}

	delegate bool CustomVerifyFunc<T>(T t) where T : ModdedAsset;

	bool VerifyAsset<T>(T asset, bool forBundle, CustomVerifyFunc<T> customVerifyFunc) where T : ModdedAsset
	{
		// Then verify that all our current assets are 
		// a) still present on disk
		// b) (if we're exporting a bundle) completely valid
		bool shouldRemove = false;
		if (asset == null)
		{
			Debug.LogWarning($"An asset in mod {ModName} is no longer present on disk. Removing from contents.");
			shouldRemove = true;
		}
		else if(!customVerifyFunc(asset))
		{
			shouldRemove = true;
		}
		else
		{
			string path = AssetDatabase.GetAssetPath(asset);
			if (path == null || path.Length == 0)
			{
				Debug.LogWarning($"Asset {asset.name} of mod {ModName} is no longer present on disk. Removing from contents.");
				shouldRemove = true;
			}
			else
			{
				// When adding to a bundle for export, we need to check a few more things.
				if (forBundle)
				{
					string errorString = asset.VerifyAsset();
					if (errorString != null)
					{
						Debug.LogWarning($"Asset {asset.name} of mod {ModName} failed verification: {errorString}");
						shouldRemove = true;
					}
				}
			}
		}
		return shouldRemove;
	}
#endif
}