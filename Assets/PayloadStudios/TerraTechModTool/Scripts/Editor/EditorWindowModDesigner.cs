using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorWindowModDesigner : EditorWindow 
{
	[MenuItem("Tools/TerraTech Mod Designer")]
	static void Open()
	{
		// Get existing open window or if none, make a new one:
		EditorWindowModDesigner window = GetWindow<EditorWindowModDesigner>("Mod Designer");
		window.ScanForExistingMods();
		window.Show();
	}

	public static readonly string[] kVanillaCorps =
	{
		"GSO",
		"GC",
		"VEN",
		"HE",
		"BF",
		"SJ",
		"EXP",
	};

	private static Texture[] s_CorpIcons = null;
	private static Texture[] s_VanillaCorpIcons = null;
	private static string[] s_CorpNames = null;
	private static Material[][] s_CorpMaterials = null;

	public static Texture[] VanillaCorpIcons
	{
		get
		{
			if (s_VanillaCorpIcons == null)
			{
				EditorWindowModDesigner inst = GetWindow<EditorWindowModDesigner>("Mod Designer");
				inst.RefreshCorpList();
			}

			return s_VanillaCorpIcons;
		}
	}

	public static Texture[] AvailableCorpIcons
	{
		get
		{
			if(s_CorpIcons == null)
			{
				EditorWindowModDesigner inst = GetWindow<EditorWindowModDesigner>("Mod Designer");
				inst.RefreshCorpList();
			}

			return s_CorpIcons;
		}
	}

	public static string[] AvailableCorps
	{
		get
		{
			if(s_CorpNames == null)
			{
				EditorWindowModDesigner inst = GetWindow<EditorWindowModDesigner>("Mod Designer");
				inst.RefreshCorpList();
			}
			return s_CorpNames;
		}
	}

	public static TextureSlot GetSlotFromMaterialName(string mat)
	{
		if (mat.EndsWith(" (Instance)"))
		{
			mat = mat.Substring(0, mat.Length - " (Instance)".Length);
		}
		foreach(Material[] materialSlots in s_CorpMaterials)
		{
			for(int i = 0; i < (int)TextureSlot.NUM_TEXTURE_SLOTS; i++)
			{
				if (materialSlots[i] != null && materialSlots[i].name == mat)
					return (TextureSlot)i;
			}
		}
		Debug.LogError($"Could not find TextureSlot corresponding to material {mat}");
		return TextureSlot.Main;
	}

	public static TextureSlot[] GetAvailableSlotsForCorp(string corp)
	{
		for(int i = 0; i < s_CorpNames.Length; i++)
		{
			if(s_CorpNames[i] == corp)
			{
				List<TextureSlot> available = new List<TextureSlot>((int)TextureSlot.NUM_TEXTURE_SLOTS);

				for(int j = 0; j < (int)TextureSlot.NUM_TEXTURE_SLOTS; j++)
				{
					if (s_CorpMaterials[i][j] != null)
						available.Add((TextureSlot)j);
				}

				return available.ToArray();
			}
		}

		Debug.LogError($"Could not find number of texture slots available for corp {corp}");
		return new TextureSlot[0];
	}

	public static Material GetCorpMaterial(string corp, TextureSlot slot)
	{
		if(s_CorpMaterials == null)
		{
			EditorWindowModDesigner inst = GetWindow<EditorWindowModDesigner>("Mod Designer");
			inst.RefreshCorpList();
		}
		for(int i = 0; i < s_CorpNames.Length; i++)
		{
			if (corp == s_CorpNames[i])
				return s_CorpMaterials[i][(int)slot];
		}
		Debug.LogError($"Could not find corp material for {corp}");
		return null;
	}


	// Initialization, system data


	// The list of mods we have available
	Dictionary<string, ModContents> m_Mods = new Dictionary<string, ModContents>(); 
	List<string> m_ModNames = new List<string>();

	// Editor GUI bits
	private string m_NewModName = "Untitled Mod";
	private int m_SelectedModIndex = -1;
	private string m_TerraTechInstallDir = "Unset";
	private Editor m_ContentsEditor;
	private bool m_Inited = false;
	private int m_EditorTab = 0;
	private string[] m_EditorTabNames = new string[] { "Edit Content", "Local Export", "Steam Upload" };
	private Vector2 m_ScrollPosition;
	private bool m_SetTags = true;

	private void Awake()
	{
	}

	void FindSteamInstallDirectory()
	{
		m_TerraTechInstallDir = "C:\\Program Files (x86)\\Steam\\Steamapps\\common\\TerraTech";
		string steamInstallDir = "";

		// Try to auto-find
		Microsoft.Win32.RegistryKey key32 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\VALVE\\");
		Microsoft.Win32.RegistryKey key64 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\VALVE\\");
		if (key64.ToString() == null || key64.ToString() == "")
		{
			foreach (string k32subKey in key32.GetSubKeyNames())
			{
				using (Microsoft.Win32.RegistryKey subKey = key32.OpenSubKey(k32subKey))
				{
					steamInstallDir = subKey.GetValue("InstallPath").ToString();
				}
			}
		}
		else
		{
			foreach (string k64subKey in key64.GetSubKeyNames())
			{
				using (Microsoft.Win32.RegistryKey subKey = key64.OpenSubKey(k64subKey))
				{
					steamInstallDir = subKey.GetValue("InstallPath").ToString();
				}
			}
		}

		if(steamInstallDir != null && steamInstallDir != "")
		{
			if (!SearchForTerraTechIn(steamInstallDir))
			{
				// If TT was not in the default install directory, try other library folders
				string configPath = steamInstallDir + "/steamapps/libraryfolders.vdf";
				string driveRegex = @"[A-Z]:\\";
				if (File.Exists(configPath))
				{
					string[] configLines = File.ReadAllLines(configPath);
					foreach (var item in configLines)
					{
						Match match = Regex.Match(item, driveRegex);
						if (item != string.Empty && match.Success)
						{
							string matched = match.ToString();
							string item2 = item.Substring(item.IndexOf(matched));
							item2 = item2.Replace("\\\\", "\\");
							item2 = item2.Replace("\\\\", "\\");
							item2 = item2.Replace("\"", "");

							if (SearchForTerraTechIn(item2))
								break;
						}
					}
				}
			}
		}
	}

	bool SearchForTerraTechIn(string location)
	{
		if (File.Exists($"{location}\\steamapps\\appmanifest_285920.acf"))
		{
			m_TerraTechInstallDir = $"{location}\\steamapps\\common\\TerraTech";
			return true;
		}
		return false;
	}

	void OnGUI()
	{
		if (!m_Inited)
		{
			ScanForExistingMods();

			GameObject preview = GameObject.Find($"SkinPreview_GSO");
			if (preview == null)
			{
				Debug.LogWarning($"Could not locate SkinPreview object, attempting to open BlockPreviews scene");
				EditorSceneManager.OpenScene($"{ModUtils.AssetsDir}/FixedAssets/BlockPreviews.unity");
			}

			Preset.SetAsDefault(AssetDatabase.LoadAssetAtPath<Preset>($"{ModUtils.AssetsDir}/Presets/Default.preset"));
			Preset.SetAsDefault(AssetDatabase.LoadAssetAtPath<Preset>($"{ModUtils.AssetsDir}/Presets/FBXImporter.preset"));

			m_Inited = true;
		}

		GUILayout.BeginHorizontal();
		GUILayout.Label("Helpful Resources:");
		if (GUILayout.Button("Mod Support Wiki"))
		{
			Application.OpenURL("https://terratech.gamepedia.com/Official_TerraTech_Mod_Support");
		}
		if (GUILayout.Button("Forum"))
		{
			Application.OpenURL("https://forum.terratechgame.com/index.php?forums/official-mods.52/");
		}
		if (GUILayout.Button("Blockpedia"))
		{
			Application.OpenURL("https://terratechgame.com/blockpedia.php");
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Create New Mod:");
		m_NewModName = GUILayout.TextField(m_NewModName);
		if (GUILayout.Button("+", new GUILayoutOption[] { GUILayout.MaxWidth(20) }))
		{
			CreateNewMod();
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.Separator();

		//int newModIndex = GUILayout.SelectionGrid(m_SelectedModIndex, m_ModNames.ToArray(), 3);
		Rect contentRect = (Rect)EditorGUILayout.BeginVertical();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		contentRect.xMin += 5;
		contentRect.xMax -= 5;
		int newModIndex = EditorGUI.Popup(contentRect, "Select Mod:", m_SelectedModIndex, m_ModNames.ToArray());
		EditorGUILayout.EndVertical();
		if (newModIndex != m_SelectedModIndex)
		{
			m_SelectedModIndex = newModIndex;
		}


		// Panel for mod settings
		EditorGUI.BeginDisabledGroup(m_SelectedModIndex == -1);
		{
			if (m_SelectedModIndex == -1)
			{
				GUILayout.Label("No mod selected");
			}
			else if (m_SelectedModIndex < m_ModNames.Count)
			{
				string modName = m_ModNames[m_SelectedModIndex];
				ModContents mod;
				if (m_Mods.TryGetValue(modName, out mod))
				{
					EditorGUILayout.Separator();
					GUILayout.Label($"Editing {modName}", EditorGUITT.boldText);

					SetEditorTarget(mod);

					m_EditorTab = GUILayout.Toolbar(m_EditorTab, m_EditorTabNames);
					bool valid = DrawVerificationSection(mod);
					EditorGUILayout.Separator();

					switch (m_EditorTab)
					{
						case 0: // Edit
						{
							EditorGUITT.HorizontalLine(Color.black);
							m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.box);
							if (m_ContentsEditor != null)
							{
								m_ContentsEditor.OnInspectorGUI();
							}
							EditorGUILayout.EndScrollView();
							break;
						}
						case 1: // Local Export
						{
							// If we have failed verification, disable export options
							EditorGUI.BeginDisabledGroup(!valid);
							{
								// Local export section
								EditorGUILayout.Separator();
								EditorGUITT.HorizontalLine(Color.black);
								GUILayout.Label($"Local Export", EditorGUITT.boldText);
								{
									if(m_TerraTechInstallDir == "Unset")
									{
										FindSteamInstallDirectory();
									}

									//GUILayout.BeginHorizontal();
									if (GUILayout.Button("Select Local Game Folder"))
									{
										m_TerraTechInstallDir = EditorUtility.OpenFolderPanel("Select TerraTech install directory (should be in Steam/steamapps/common/)", m_TerraTechInstallDir, "");
									}
									m_TerraTechInstallDir = GUILayout.TextField(m_TerraTechInstallDir);
									if (m_TerraTechInstallDir.EndsWith("LocalMods"))
										m_TerraTechInstallDir = m_TerraTechInstallDir.Substring(0, m_TerraTechInstallDir.Length - "LocalMods".Length);
									if (m_TerraTechInstallDir.EndsWith("LocalMods/"))
										m_TerraTechInstallDir = m_TerraTechInstallDir.Substring(0, m_TerraTechInstallDir.Length - "LocalMods/".Length);
									//GUILayout.EndHorizontal();
								}
								if (GUILayout.Button("Export"))
								{
									ExportLocally(mod);
								}
							}
							EditorGUI.EndDisabledGroup();
							break;
						}
						case 2: // Steam Workshop Upload
						{
							// If we have failed verification, disable export options
							EditorGUI.BeginDisabledGroup(!valid);
							{
								// Steam Workshop section
								EditorGUILayout.Separator();
								EditorGUITT.HorizontalLine(Color.black);
								GUILayout.Label($"Steam Workshop Export", EditorGUITT.boldText);
								// Register button - only works if we don't have a SteamID
								// TODO: Re-assign ID in the case of deletion from Steam end?
								EditorGUI.BeginDisabledGroup(mod.CanPublish());
								{
									if (GUILayout.Button("Register Steam Workshop Item"))
									{
										RegisterMod(mod);
									}
								}
								EditorGUI.EndDisabledGroup();

								GUILayout.Label($"Workshop ID:{mod.m_WorkshopId}");
								if(GUILayout.Button("Open Workshop Page"))
								{
									Application.OpenURL($"steam://url/CommunityFilePage/{mod.m_WorkshopId}");
								}

								// Publish button - only works if we have a SteamID
								EditorGUI.BeginDisabledGroup(!mod.CanPublish());
								{
									m_SetTags = GUILayout.Toggle(m_SetTags, "Set Workshop Tags (This will override custom tags applied in Workshop)");
									GUILayout.Label("Enter changenotes for Workshop update:");
									mod.m_Changenotes = GUILayout.TextArea(mod.m_Changenotes);
									if (GUILayout.Button("Publish Mod To Steam"))
									{
										PublishMod(mod);
									}
								}
								EditorGUI.EndDisabledGroup();
							}
							EditorGUI.EndDisabledGroup();

							if(SteamWorkshopUploader.IsSteamInited())
							{
								GUILayout.Label("WARNING: Steam is now connected to Unity.");
								GUILayout.Label("You will need to close Unity to run TerraTech.");
							}

							break;
						}
					}
				}
				else
				{
					ScanForExistingMods();
				}
			}
			else
			{
				Debug.LogError("Invalid selected mod index");
				m_SelectedModIndex = -1;
				SetEditorTarget(null);
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	private bool DrawVerificationSection(ModContents mod)
	{
		// Verification section
		int numAssets = 0;
		int numVerifiedAssets = 0;
		foreach (ModdedAsset asset in mod)
		{
			numAssets++;
			if (asset.VerifyAsset() == null)
				numVerifiedAssets++;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(numVerifiedAssets == numAssets ? EditorGUITT.Tick : EditorGUITT.Cross, EditorGUITT.tinyButton);
		GUILayout.Label($"{numVerifiedAssets}/{numAssets} assets verified and ready to export.");
		GUILayout.EndHorizontal();

		if(File.GetLastWriteTime(ModUtils.k_ModPreviewTemplate) == File.GetLastWriteTime($"{mod.WorkingDir}/preview.png"))
		{
			GUILayout.Label("You have not edited the mod preview icon that will appear on the workshop", EditorGUITT.boldText);
			return false;
		}

		if (Application.unityVersion != "2018.4.13f1")
		{
			GUILayout.Label("Incorrect Unity Version! Please reinstall with version 2018.4.13f1", EditorGUITT.boldText);
			return false;
		}

		return numVerifiedAssets == numAssets;
	}

	private void SetEditorTarget(UnityEngine.Object target)
	{
		if (target != null)
		{
			if (m_ContentsEditor == null)
			{
				m_ContentsEditor = Editor.CreateEditor(target);
			}
			else if (m_ContentsEditor.target != target)
			{
				DestroyImmediate(m_ContentsEditor);
				m_ContentsEditor = Editor.CreateEditor(target);
			}
		}
		else if (m_ContentsEditor != null)
		{
			DestroyImmediate(m_ContentsEditor);
			m_ContentsEditor = null;
		}
	}

	public void Update()
	{
		SteamWorkshopUploader.Update();
	}

	// Create a new mod with a contents object
	void CreateNewMod()
	{
		DirectoryInfo modDirectory = new DirectoryInfo($"{ModUtils.k_ModsDirectory}/{m_NewModName}");
		if(modDirectory.Exists)
		{
			Debug.LogError($"Mod {m_NewModName} already exists");
		}
		else
		{
			// Create directory and subfolders for assets
			modDirectory.Create();
			Directory.CreateDirectory($"{ModUtils.k_ModsDirectory}/{m_NewModName}/Skins");
			Directory.CreateDirectory($"{ModUtils.k_ModsDirectory}/{m_NewModName}/Corps");
			Directory.CreateDirectory($"{ModUtils.k_ModsDirectory}/{m_NewModName}/Blocks");

			// Create ModContents object
			string contentsPath = $"{ModUtils.k_ModsDirectory}/{m_NewModName}/Contents.asset";
			ModContents contents = CreateInstance<ModContents>();
			contents.ModName = m_NewModName;
			AssetDatabase.CreateAsset(contents, contentsPath);
			AssetDatabase.SaveAssets();
			contents = AssetDatabase.LoadAssetAtPath<ModContents>(contentsPath);

			if (File.Exists(ModUtils.k_ModPreviewTemplate))
			{
				File.Copy(ModUtils.k_ModPreviewTemplate, $"{ModUtils.k_ModsDirectory}/{m_NewModName}/preview.png");
			}

			// Now refresh the asset database so the modder can start working with the created files
			//AssetDatabase.Refresh();
			AssetImportCatcher.ManualAssetRefresh();
			// The asset database refresh will trigger a ScanForExistingMods
		}
	}

	// Check all our asset folders for a Contents asset, which indicates that that folder is an existing mod project
	public void ScanForExistingMods()
	{
		string log = "";
		m_Mods.Clear();
		m_ModNames.Clear();
		if (Directory.Exists(ModUtils.k_ModsDirectory))
		{
			foreach (DirectoryInfo dir in new DirectoryInfo(ModUtils.k_ModsDirectory).GetDirectories())
			{
				string modName = dir.Name;
				// And go find the contents asset
				ModContents contents = AssetDatabase.LoadAssetAtPath<ModContents>($"{ModUtils.k_ModsDirectory}/{modName}/Contents.asset");
				if (contents != null)
				{
					if (log == null)
						log = $"{modName}";
					else log += $", {modName}";

					m_Mods[modName] = contents;
					contents.ModName = modName;
					contents.VerifyAssets();
					m_ModNames.Add(modName);
				}
				else
				{
					Debug.LogError($"Could not locate contents for mod {modName}");
				}
			}
		}
		else
		{
			Directory.CreateDirectory(ModUtils.k_ModsDirectory);
		}

		RefreshCorpList();

		Debug.Log($"Mod search found {m_Mods.Count} mod(s): {log}");
	}

	public void RefreshCorpList()
	{
		List<string> corpNames = new List<string>(kVanillaCorps);
		List<Texture> corpIcons = new List<Texture>();
		List<Texture> vanillaCorpIcons = new List<Texture>(kVanillaCorps.Length);
		List<Material[]> corpMats = new List<Material[]>();
		foreach (string corpName in kVanillaCorps)
		{
			Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ModUtils.AssetsDir}/FixedAssets/Corporation/Corp_Icon_{corpName}.png");
			vanillaCorpIcons.Add(tex);
			corpIcons.Add(tex);
			Material[] materialSlots = new Material[(int)TextureSlot.NUM_TEXTURE_SLOTS];

			// Load material arrays with base texture and wheel texture if we have one
			materialSlots[0] = AssetDatabase.LoadAssetAtPath<Material>($"{ ModUtils.AssetsDir}/FixedAssets/{corpName}/TankBlockMaterial_{corpName}.mat");
			if(corpName == "GSO" || corpName == "GC")
				materialSlots[1] = AssetDatabase.LoadAssetAtPath<Material>($"{ ModUtils.AssetsDir}/FixedAssets/{corpName}/{corpName}_TankTrack.mat");

			corpMats.Add(materialSlots);
		}
		foreach (ModContents contents in m_Mods.Values)
		{
			foreach (ModdedCorpDefinition corp in contents.m_Corps)
			{
				corpNames.Add(corp.m_ShortName);
				corpIcons.Add(corp.m_Icon);

				if (corp.m_DefaultSkinSlots != null)
				{
					Material[] materialSlots = new Material[(int)TextureSlot.NUM_TEXTURE_SLOTS];
					for(int i = 0; i < (int)TextureSlot.NUM_TEXTURE_SLOTS; i++)
					{
						if (corp.m_DefaultSkinSlots != null && corp.m_DefaultSkinSlots.Length > i && corp.m_DefaultSkinSlots[i] != null)
						{
							materialSlots[i] = new Material(Shader.Find("StandardTankBlock"));
							materialSlots[i].mainTexture = corp.m_DefaultSkinSlots[i].m_Albedo;
						}
					}
					corpMats.Add(materialSlots);
				}
				else
				{
					corpMats.Add(new Material[(int)TextureSlot.NUM_TEXTURE_SLOTS]);
				}
			}
		}
		s_CorpNames = corpNames.ToArray();
		s_VanillaCorpIcons = vanillaCorpIcons.ToArray();
		s_CorpIcons = corpIcons.ToArray();
		s_CorpMaterials = corpMats.ToArray();
	}

	bool RegisterMod(ModContents mod)
	{
		return SteamWorkshopUploader.AssignWorkshopID(mod);
	}

	const string k_BundleTempDir = "Temp/Bundles";

	bool CreateAssetBundle(ModContents mod, string assetBundleName)
	{
		bool succcess = false;

		mod.GenerateAssets();

		AssetImportCatcher.ManualAssetRefresh();

		// Pre-Step: Verify mod contents is valid
		int numInvalidAssets = mod.VerifyAssets(forBundle: true);

		if (numInvalidAssets > 0)
		{
			EditorUtility.DisplayDialog("Mod Verification Failed", $"{mod.ModName} failed verification with {numInvalidAssets} invalid assets. Please check the console (Ctrl+Shift+C) for more info", "OK");
		}
		else
		{
			// Step 1: Put mod assets into an asset bundle
			DirectoryInfo modDir = new DirectoryInfo($"{ModUtils.k_ModsDirectory}/{mod.ModName}");

			AssetImporter contentsImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mod));
			if (contentsImporter)
			{
				contentsImporter.assetBundleName = assetBundleName;
			}
			else
			{
				Debug.LogWarning($"Contents not valid to add to bundle for {mod.ModName}");
			}

			// Step 2: Build mod into an asset bundle
			AssetBundleBuild build = new AssetBundleBuild()
			{
				assetBundleName = assetBundleName,
				assetBundleVariant = "",
				assetNames = new string[] { $"{ModUtils.k_ModsDirectory}/{mod.ModName}/Contents.asset" }, //AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName),
				addressableNames = null,
			};

			try
			{
				// Ensure all the directories we need exist
				Directory.CreateDirectory(k_BundleTempDir);
				Directory.CreateDirectory(mod.OutputDir);

				// Build the bundle
				BuildTarget target;
				switch(Application.platform)
				{
					case RuntimePlatform.LinuxEditor:
						target = BuildTarget.StandaloneLinux64;
						break;
					case RuntimePlatform.OSXEditor:
						target = BuildTarget.StandaloneOSX;
						break;
					case RuntimePlatform.WindowsEditor:
						target = BuildTarget.StandaloneWindows64;
						break;
					default:
						Debug.LogError($"Did not recognize current platform {Application.platform}. Will attempt to build for Windows");
						target = BuildTarget.StandaloneWindows64;
						break;
				}
				AssetBundleManifest manifest = 
					BuildPipeline.BuildAssetBundles(k_BundleTempDir, 
					new AssetBundleBuild[] { build },
					BuildAssetBundleOptions.ForceRebuildAssetBundle, 
					target);

				// Copy to the output directory
				File.Copy($"{k_BundleTempDir}/{assetBundleName}", $"{mod.OutputDir}/{assetBundleName}", true);
				//File.Copy($"{k_BundleTempDir}/{assetBundleName}.manifest",  $"{mod.OutputDir}/{assetBundleName}.manifest", true); // I don't think we need the manifest for end users
				if (File.Exists($"{mod.WorkingDir}/preview.png"))
				{
					File.Copy($"{mod.WorkingDir}/preview.png", $"{mod.OutputDir}/preview.png", true);
				}

				// Copy dll if we have one
				if (File.Exists($"{mod.WorkingDir}/{mod.ModName}.dll"))
				{
					File.Copy($"{mod.WorkingDir}/{mod.ModName}.dll", $"{mod.OutputDir}/{mod.ModName}.dll", true);
				}

				succcess = true;

				Debug.Log($"Successfully created bundle for {mod.name} at {mod.OutputDir}");
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to create bundle for {mod.name} due to: {e.StackTrace}");
			}
		}

		// If we cleared some assets because they were invalid, re-add them to the mod
		mod.VerifyAssets(forBundle: false);

		return succcess;
	}

	void CopyHotswapJSON(ModContents mod)
	{
		string jsonOutputFolder = $"{mod.OutputDir}/BlockJSON";
		if (!Directory.Exists(jsonOutputFolder))
			Directory.CreateDirectory(jsonOutputFolder);

		foreach (ModdedBlockDefinition block in mod.m_Blocks)
		{
			if(block.m_Json != null)
			{
				string path = AssetDatabase.GetAssetPath(block);
				string pathNoExt = path.Substring(0, path.LastIndexOf('.'));
				string jsonPath = $"{pathNoExt}.json";

				File.Copy(jsonPath, $"{jsonOutputFolder}/{block.name}.json", true);
			}
		}
	}

	[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
	void ExportLocally(ModContents mod)
	{
		string assetBundleName = mod.ModName + "_bundle";

		// Bundle mod
		if (CreateAssetBundle(mod, assetBundleName))
		{
			CopyHotswapJSON(mod);

			string localModsRoot = $"{m_TerraTechInstallDir}/LocalMods/";
			string localModDir = $"{m_TerraTechInstallDir}/LocalMods/{mod.ModName}";
			try
			{
				if (!Directory.Exists(localModsRoot))
					Directory.CreateDirectory(localModsRoot);

				if (Directory.Exists(localModDir))
					Directory.Delete(localModDir, true);

				Directory.Move($"{mod.OutputDir}", $"{localModDir}");

				Debug.Log($"Copied {mod.ModName} asset bundles to LocalMods folder.");
			}
			catch (Exception)
			{
				Debug.LogError($"Mod Designer failed to copy mod into TT directory. Output is at {mod.OutputDir} and needs to be manually dragged to {localModDir}");
				if (EditorUtility.DisplayDialog("Manual Move", $"TerraTech is installed in a administrator locked folder, you need to manually drag the folder {mod.ModName} from {ModContents.k_OutputDir} to {localModsRoot}", "OK"))
				{
					System.Diagnostics.Process.Start($"{new DirectoryInfo(mod.OutputDir).FullName}");
					System.Diagnostics.Process.Start($"{new DirectoryInfo(localModsRoot).FullName}");
				}
			}
		}
	}

	void PublishMod(ModContents mod)
	{
		string assetBundleName = mod.ModName + "_bundle";

		// Bundle mod
		CreateAssetBundle(mod, assetBundleName);

		// Send that mod to Steam Workshop
		SteamWorkshopUploader.PublishAssetBundleToWorkshop(mod, mod.OutputDir, mod.m_Changenotes, m_SetTags);

		Application.OpenURL($"steam://url/CommunityFilePage/{mod.m_WorkshopId}");
	}
}
