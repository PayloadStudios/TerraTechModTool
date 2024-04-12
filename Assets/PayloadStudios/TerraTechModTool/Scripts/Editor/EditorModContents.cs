using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ModContents))]
public class EditorModContents : Editor
{
	enum ContentTypeTab
	{
		Skins,
		Blocks,
		Corporations,
		Advanced,
	}

	readonly string[] k_ContentTypeNames = new string[]
	{
		"Skins",
		"Blocks",
		"Corporations",
		"Advanced",
	};

	private int m_SelectedContentTypeTab;
	private Editor m_CurrentSelectedEditor;
	private string m_NewContentName = "Untitled Skin";

	private void Awake()
	{

	}

	public override void OnInspectorGUI()
	{
		ModContents contents = (ModContents)target;
		string contentsPath = AssetDatabase.GetAssetPath(contents);
		contentsPath = contentsPath.Replace("/Contents.asset", "");

		int newSelection = GUILayout.Toolbar(m_SelectedContentTypeTab, k_ContentTypeNames);
		if (newSelection != m_SelectedContentTypeTab)
		{
			m_SelectedSkin = null;
			if (m_CurrentSelectedEditor != null)
			{
				DestroyImmediate(m_CurrentSelectedEditor);
				m_CurrentSelectedEditor = null;
			}
			switch ((ContentTypeTab)newSelection)
			{
				case ContentTypeTab.Blocks: m_NewContentName = "Untitled Block"; break;
				case ContentTypeTab.Skins: m_NewContentName = "Untitled Skin"; break;
				case ContentTypeTab.Corporations: m_NewContentName = "Untitled Corp"; break;
				case ContentTypeTab.Advanced: m_NewContentName = "Untitled Asset"; break;
			}
		}
		m_SelectedContentTypeTab = newSelection;

		switch ((ContentTypeTab)m_SelectedContentTypeTab)
		{
			case ContentTypeTab.Skins:
			{
				DrawSkinsTab(contentsPath, contents);
				break;
			}
			case ContentTypeTab.Blocks:
			{
				DrawBlocksTab(contentsPath, contents);
				break;
			}
			case ContentTypeTab.Corporations:
			{
				DrawCorpsTab(contentsPath, contents);
				break;
			}
			case ContentTypeTab.Advanced:
			{
				DrawAdvancedTab(contentsPath, contents);
				break;
			}
			default:
			{
				GUILayout.Label("Unknown asset type!");
				break;
			}
		}

		if (m_CurrentSelectedEditor != null)
		{
			m_CurrentSelectedEditor.OnInspectorGUI();
		}
	}

	// --------------------------------------------------------------------------------
	//  SKIN TAB
	// --------------------------------------------------------------------------------
	private ModdedSkinDefinition m_SelectedSkin;

	private void DrawSkinsTab(string contentsPath, ModContents contents)
	{
		// Header
		ModdedSkinDefinition skinToDelete = null;

		// New skin button
		GUILayout.BeginHorizontal();
		GUILayout.Label("Create New Skin:");
		m_NewContentName = GUILayout.TextField(m_NewContentName);
		if (GUILayout.Button("Create", GUILayout.MaxWidth(100f)))
		{
			string skinPath = $"{contentsPath}/Skins/{m_NewContentName}.asset";
			ModdedSkinDefinition skin = CreateInstance<ModdedSkinDefinition>();
			AssetDatabase.CreateAsset(skin, skinPath);
			AssetImportCatcher.ManualSaveAssets();
		}
		GUILayout.EndHorizontal();

		EditorGUITT.HorizontalLine(Color.black);

		// List each skin
		foreach (ModdedSkinDefinition skin in contents.m_Skins)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(skin.name, (m_SelectedSkin == skin ? EditorGUITT.boldText : GUI.skin.label));
			EditorGUI.BeginDisabledGroup(m_SelectedSkin == skin);
			GUILayout.Label(skin.VerifyAsset() == null ? EditorGUITT.Tick : EditorGUITT.Cross, EditorGUITT.tinyButton);
			if (GUILayout.Button("Edit", EditorGUITT.fixedSizeButton50))
			{
				m_SelectedSkin = skin;
			}
			EditorGUI.EndDisabledGroup();
			if (GUILayout.Button("Delete", EditorGUITT.fixedSizeButton50))
			{
				skinToDelete = skin;
			}
			GUILayout.EndHorizontal();
		}
		if (skinToDelete != null)
		{
			if (m_SelectedSkin == skinToDelete)
			{
				m_SelectedSkin = null;
			}
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(skinToDelete));
			contents.m_Skins.Remove(skinToDelete);
		}

		EditorGUITT.HorizontalLine(Color.black);
		EditorGUITT.HorizontalLine(Color.black);

		// Show panel for skin editing
		SetEditorTarget(m_SelectedSkin);
	}

	// --------------------------------------------------------------------------------
	//  BLOCKS TAB
	// --------------------------------------------------------------------------------
	private ModdedBlockDefinition m_SelectedBlock;

	public void DrawBlocksTab(string contentsPath, ModContents contents)
	{
		// Header
		ModdedBlockDefinition blockToDelete = null;

		// New block button
		GUILayout.BeginHorizontal();
		GUILayout.Label("Create New Block:");
		m_NewContentName = GUILayout.TextField(m_NewContentName);
		if (GUILayout.Button("Create", GUILayout.MaxWidth(100f)))
		{
			string blockPath = $"{contentsPath}/Blocks/{m_NewContentName}.asset";
			ModdedBlockDefinition block = CreateInstance<ModdedBlockDefinition>();
			AssetDatabase.CreateAsset(block, blockPath);
			AssetImportCatcher.ManualSaveAssets();
		}
		GUILayout.EndHorizontal();

		EditorGUITT.HorizontalLine(Color.black);

		// List each block
		foreach (ModdedBlockDefinition block in contents.m_Blocks)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(block.name, (m_SelectedBlock == block ? EditorGUITT.boldText : GUI.skin.label));
			EditorGUI.BeginDisabledGroup(m_SelectedBlock == block);
			GUILayout.Label(block.VerifyAsset() == null ? EditorGUITT.Tick : EditorGUITT.Cross, EditorGUITT.tinyButton);
			if (GUILayout.Button("Edit", EditorGUITT.fixedSizeButton50))
			{
				m_SelectedBlock = block;
			}
			EditorGUI.EndDisabledGroup();
			if (GUILayout.Button("Delete", EditorGUITT.fixedSizeButton50))
			{
				blockToDelete = block;
			}
			GUILayout.EndHorizontal();
		}
		if (blockToDelete != null)
		{
			if (m_SelectedBlock == blockToDelete)
			{
				m_SelectedBlock = null;
			}
			if (blockToDelete.m_Json != null)
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(blockToDelete.m_Json));
			}
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(blockToDelete));
			contents.m_Blocks.Remove(blockToDelete);
		}

		EditorGUITT.HorizontalLine(Color.black);
		EditorGUITT.HorizontalLine(Color.black);

		// Show panel for corp editing
		SetEditorTarget(m_SelectedBlock);
	}

	// --------------------------------------------------------------------------------
	//  CORPORATIONS TAB
	// --------------------------------------------------------------------------------
	private ModdedCorpDefinition m_SelectedCorp;

	public void DrawCorpsTab(string contentsPath, ModContents contents)
	{
		// Header
		ModdedCorpDefinition corpToDelete = null;

		// New corps button
		GUILayout.BeginHorizontal();
		GUILayout.Label("Create New Corp:");
		m_NewContentName = GUILayout.TextField(m_NewContentName);
		if (GUILayout.Button("Create", GUILayout.MaxWidth(100f)))
		{
			string corpPath = $"{contentsPath}/Corps/{m_NewContentName}.asset";
			ModdedCorpDefinition corp = CreateInstance<ModdedCorpDefinition>();
			AssetDatabase.CreateAsset(corp, corpPath);
			AssetImportCatcher.ManualSaveAssets();
		}
		GUILayout.EndHorizontal();

		EditorGUITT.HorizontalLine(Color.black);

		// List each corp
		foreach (ModdedCorpDefinition corp in contents.m_Corps)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(corp.name, (m_SelectedCorp == corp ? EditorGUITT.boldText : GUI.skin.label));
			EditorGUI.BeginDisabledGroup(m_SelectedCorp == corp);
			GUILayout.Label(corp.VerifyAsset() == null ? EditorGUITT.Tick : EditorGUITT.Cross, EditorGUITT.tinyButton);
			if (GUILayout.Button("Edit", EditorGUITT.fixedSizeButton50))
			{
				m_SelectedCorp = corp;
			}
			if (GUILayout.Button("Delete", EditorGUITT.fixedSizeButton50))
			{
				corpToDelete = corp;
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}
		if (corpToDelete != null)
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(corpToDelete));
			contents.m_Corps.Remove(corpToDelete);
		}

		EditorGUITT.HorizontalLine(Color.black);
		EditorGUITT.HorizontalLine(Color.black);

		// Show panel for corp editing
		SetEditorTarget(m_SelectedCorp);
	}

	// --------------------------------------------------------------------------------
	//  ADVANCED TAB
	// --------------------------------------------------------------------------------
	private void DrawAdvancedTab(string contentsPath, ModContents contents)
	{
		EditorStyles.label.wordWrap = true;
		EditorGUILayout.LabelField("This is for advanced mods that add scripts or blocks for use with the Unofficial Block Loader. If your mod requires any extra assets, list them here and they will be bundled for use in-game.");

		EditorGUILayout.LabelField("You do not need to list any skin, block or corporation assets here unless you are using the Unofficial Block Loader.");
		if (GUILayout.Button("Import additional assets from \"Blocks\" folder"))
		{
			ImportAssets(contentsPath, contents);
		}
		if (GUILayout.Button("Import Nuterra Blocks from \"Blocks\" folder"))
		{
			ImportBlocks(contentsPath, contents);
		}
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AdditionalAssets"), true);
		serializedObject.ApplyModifiedProperties();
	}

	private void SetEditorTarget(UnityEngine.Object target)
	{
		if (target != null)
		{
			if (m_CurrentSelectedEditor == null)
			{
				m_CurrentSelectedEditor = CreateEditor(target);
			}
			else if (m_CurrentSelectedEditor.target != target)
			{
				DestroyImmediate(m_CurrentSelectedEditor);
				m_CurrentSelectedEditor = CreateEditor(target);
			}
		}
		else if (m_CurrentSelectedEditor != null)
		{
			DestroyImmediate(m_CurrentSelectedEditor);
			m_CurrentSelectedEditor = null;
		}
	}

	public override bool HasPreviewGUI()
	{
		return m_SelectedContentTypeTab == (int)ContentTypeTab.Skins;
	}

	public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
	{
		switch ((ContentTypeTab)m_SelectedContentTypeTab)
		{
			case ContentTypeTab.Skins:
			{
				if (m_CurrentSelectedEditor != null)
				{
					m_CurrentSelectedEditor.OnInteractivePreviewGUI(r, background);
				}
				break;
			}
			default:
			{
				// Invalid preview?
				break;
			}
		}
	}

	private void ImportAssets(string contentsPath, ModContents contents)
	{
		string blocksPath = $"{contentsPath}/Blocks";

		string[] folders = new string[] { blocksPath };

		foreach (UnityEngine.Object item in AssetDatabase.FindAssets("t:Mesh t:Texture", folders).Select(GUID => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(GUID))))
		{
			if (!contents.m_AdditionalAssets.Contains(item))
			{
				contents.m_AdditionalAssets.Add(item);
			}
		}

		AssetImportCatcher.ManualSaveAssets();
	}

	private static string StripComments(string input)
	{
		// JavaScriptSerializer doesn't accept commented-out JSON,
		// so we'll strip them out ourselves;
		input = Regex.Replace(input, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes line comments like this
		input = Regex.Replace(input, @"/\*(\s|\S)*?\*/", "", RegexOptions.Multiline); /* comments like this */
		input = Regex.Replace(input, @"([,\[\{\]\}\." + Regex.Escape("\"") + @"0-9]|null)\s*//[^\n]*\n", "$1\n", RegexOptions.Multiline);    // Removes mixed JSON comments
		input = Regex.Replace(input, @",\s*([\}\]])", "\n$1", RegexOptions.Multiline);  // remove trailing ,
		return input.Replace("JSONBLOCK", "Deserializer");
	}

	private static readonly Regex getGrade = new Regex("\"Grade\":\\s*([0-9])");
	private static readonly Regex getFaction = new Regex("\"Faction\":\\s*([0-9])");
	private static readonly Regex getCategory = new Regex("\"Category\":\\s*([0-9])");

	private void ImportBlocks(string contentsPath, ModContents contents)
	{
		string blocksPath = $"{contentsPath}/Blocks";

		string[] folders = new string[] { blocksPath };

		string rootPath = Application.dataPath.Replace("Assets", blocksPath);

		foreach (string file in Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories))
		{
			string text = File.ReadAllText(file);
			if (!text.Contains("\"NuterraBlock\""))
			{
				string formattedText = StripComments(text);
				Match grade = getGrade.Match(formattedText);

				StringBuilder sb = new StringBuilder(formattedText);
				if (grade != null && grade.Groups != null && grade.Groups.Count >= 2)
				{
					Group gradeGroup = grade.Groups[1];
					int indexed0 = int.Parse(gradeGroup.Value);
					sb[gradeGroup.Index] = (indexed0 + 1).ToString()[0];
				}

				sb.Insert(0, "{\n\t\"NuterraBlock\":\n");
				sb.Append("}");
				text = sb.ToString();

				File.WriteAllText(file, text);
			}

			// See if we can find the name
			string displayName = file;
			Match match = Regex.Match(text, "\"Name\":\\s*?\"(.*?)\"");
			if (match != null && match.Groups != null && match.Groups.Count >= 2)
			{
				displayName = match.Groups[1].Value;
			}

			// Get block JSON and asset
			string blockJSONPath = file.Replace(Application.dataPath, "Assets");
			string blockAssetPath = blockJSONPath.Replace(".json", ".asset");
			ModdedBlockDefinition blockAsset = AssetDatabase.LoadAssetAtPath<ModdedBlockDefinition>(blockAssetPath);

			if (!blockAsset)
			{
				blockAsset = CreateInstance<ModdedBlockDefinition>();
				AssetDatabase.CreateAsset(blockAsset, blockAssetPath);

				blockAsset.m_BlockDisplayName = displayName;

				Match faction = getFaction.Match(text);
				if (faction != null && faction.Groups != null && faction.Groups.Count >- 2)
                {
					int factionInd = int.Parse(faction.Groups[1].Value);
					string factionStr = "GSO";
					switch (factionInd)
                    {
						case 1:
							factionStr = "GSO";
							break;
						case 2:
							factionStr = "GC";
							break;
						case 3:
							factionStr = "EXP";
							break;
						case 4:
							factionStr = "VEN";
							break;
						case 5:
							factionStr = "HE";
							break;
						case 6:
							factionStr = "SPE";
							break;
						default:
							factionStr = "GSO";
							break;
                    }
					blockAsset.m_Corporation = factionStr;
				}

				Match category = getCategory.Match(text);
				if (category != null && category.Groups != null && category.Groups.Count > -2)
				{
					int categoryInd = int.Parse(category.Groups[1].Value);
					blockAsset.m_Category = (BlockCategories) categoryInd;
				}

				blockAsset.m_Icon = new Texture2D(512, 512);

				TextAsset JSON = AssetDatabase.LoadAssetAtPath<TextAsset>(blockJSONPath);
				blockAsset.m_Json = JSON;

				string blockPrefabPath = blockAssetPath.Replace(".asset", "_Prefab.prefab");

				// Create a new copy of the prefab, save it, then destroy it
				GameObject prefab = new GameObject($"{blockAsset.name}_Prefab");
				prefab.AddComponent<TankBlockTemplate>();
				prefab.AddComponent<MeshFilter>();
				prefab.AddComponent<MeshRenderer>();
				prefab.AddComponent<MeshRendererTemplate>();
				prefab.AddComponent<BoxCollider>();
				blockAsset.m_PhysicalPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, blockPrefabPath).GetComponent<TankBlockTemplate>();
				DestroyImmediate(prefab);
				EditorUtility.SetDirty(target);
			}
		}

		AssetImportCatcher.ManualSaveAssets();
	}
}
