using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModdedCorpDefinition))]
public class EditorModdedCorpDefinition : Editor
{
	private ModdedCorpDefinition m_SelectedCorp = null;
	private Editor m_BaseSkinEditor;
	private TextureSlot m_SelectedSlot = TextureSlot.Main;
	private int m_SelectedRewardCorp = -1;


	public override void OnInspectorGUI()
	{
		m_SelectedCorp = (ModdedCorpDefinition)target;

		if (m_SelectedCorp != null)
		{
			// Strip path to containing folder
			string corpPath = AssetDatabase.GetAssetPath(m_SelectedCorp);
			corpPath = corpPath.Substring(0, corpPath.LastIndexOf('/'));

			bool dirty = false;

			GUILayout.Label($"Currently Editing {m_SelectedCorp.name}", EditorGUITT.boldText);

			// Short name
			GUILayout.BeginHorizontal();
			GUILayout.Label("Short Name:");
			string oldShortName = m_SelectedCorp.m_ShortName;
			string shortName = GUILayout.TextField(oldShortName);
			if(shortName != oldShortName)
			{
				m_SelectedCorp.m_ShortName = shortName;
				dirty = true;
			}
			GUILayout.EndHorizontal();

			//Display name
			GUILayout.BeginHorizontal();
			GUILayout.Label("Display Name:");
			string displayName = GUILayout.TextField(m_SelectedCorp.m_DisplayName);
			if(displayName != m_SelectedCorp.m_DisplayName)
			{
				dirty = true;
				m_SelectedCorp.m_DisplayName = displayName;
			}
			GUILayout.EndHorizontal();

			// Campaign rewards settings
			GUILayout.Label("Available in Campaign Reward Crates from Corporation");
			m_SelectedRewardCorp = -1;
			for (int i = 0; i < EditorWindowModDesigner.AvailableCorps.Length; i++)
			{
				if (EditorWindowModDesigner.AvailableCorps[i] == m_SelectedCorp.m_RewardCorp)
					m_SelectedRewardCorp = i;
			}

			m_SelectedRewardCorp = GUILayout.SelectionGrid(m_SelectedRewardCorp, EditorWindowModDesigner.VanillaCorpIcons, EditorWindowModDesigner.kVanillaCorps.Length, EditorGUITT.corpButton);
			if (m_SelectedRewardCorp != -1 && EditorWindowModDesigner.kVanillaCorps[m_SelectedRewardCorp] != m_SelectedCorp.m_RewardCorp)
			{
				dirty = true;
				m_SelectedCorp.m_RewardCorp = EditorWindowModDesigner.AvailableCorps[m_SelectedRewardCorp];
			}

			// Mesh for previews - if the mesh or the corp ID changes, update the model
			Mesh replacementMesh = (Mesh)EditorGUILayout.ObjectField(m_SelectedCorp.m_PreviewMesh, typeof(Mesh), false);
			if(replacementMesh != m_SelectedCorp.m_PreviewMesh || shortName != oldShortName)
			{
				UpdateCorpPreviewModel(shortName, oldShortName, replacementMesh);
				m_SelectedCorp.m_PreviewMesh = replacementMesh;
			}

			GUILayout.BeginHorizontal();
			{
				// Icon
				Texture2D icon = EditorGUITT.TextureField("Logo", m_SelectedCorp.m_Icon);
				if (icon != m_SelectedCorp.m_Icon)
				{
					dirty = true;
					m_SelectedCorp.m_Icon = icon;
				}


				// Verification
				GUILayout.BeginVertical();
				string verification = m_SelectedCorp.VerifyAsset();
				if (verification == null)
				{
					GUILayout.Label("Verification successful. Ready to export");
					GUILayout.Label(EditorGUITT.Tick);
				}
				else
				{
					GUILayout.Label(verification);
					GUILayout.Label(EditorGUITT.Cross);
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			if (m_SelectedCorp.m_DefaultSkinSlots == null || m_SelectedCorp.m_DefaultSkinSlots.Length != (int)TextureSlot.NUM_TEXTURE_SLOTS)
			{
				m_SelectedCorp.m_DefaultSkinSlots = new ModdedSkinDefinition[(int)TextureSlot.NUM_TEXTURE_SLOTS];
			}

			EditorGUITT.HorizontalLine(Color.black);
			//m_SelectedSlot = (TextureSlot)GUILayout.Toolbar((int)m_SelectedSlot, s_TextureSlotNames);
			//int selectedIndex = (int)m_SelectedSlot;
			int selectedIndex = 0;

			GUILayout.Label($"Currently editing {ModdedCorpDefinition.s_TextureSlotNames[selectedIndex]} Skin", EditorGUITT.boldText);
			//if(selectedIndex == 0)
			//{
			//	GUILayout.Label("This layer can have custom skins applied, and is required.");
			//}
			EditorGUITT.HorizontalLine(Color.black);

			string skinPath = $"{corpPath}/{m_SelectedCorp.name}_{ModdedCorpDefinition.s_TextureSlotNames[selectedIndex]}.asset";
			if (m_SelectedCorp.m_DefaultSkinSlots[selectedIndex] == null)
			{
				ModdedSkinDefinition skin = AssetDatabase.LoadAssetAtPath<ModdedSkinDefinition>(skinPath);
				if (skin != null)
				{
					m_SelectedCorp.m_DefaultSkinSlots[selectedIndex] = skin;
				}
				else if (GUILayout.Button("Create skin"))
				{
					skin = CreateInstance<ModdedSkinDefinition>();
					skin.m_IsCorpDefault = true;
					AssetDatabase.CreateAsset(skin, skinPath);
					AssetImportCatcher.ManualSaveAssets();
					m_SelectedCorp.m_DefaultSkinSlots[selectedIndex] = AssetDatabase.LoadAssetAtPath<ModdedSkinDefinition>(skinPath);
				}
			}
				
			if(m_SelectedCorp.m_DefaultSkinSlots[selectedIndex] != null)
			{
				if (selectedIndex != 0 && GUILayout.Button("Delete Skin"))
				{
					AssetDatabase.DeleteAsset(skinPath);
					m_SelectedCorp.m_DefaultSkinSlots[selectedIndex] = null;
				}
				SetEditorTarget(m_SelectedCorp.m_DefaultSkinSlots[selectedIndex]);
			}
			else
			{
				SetEditorTarget(null);
			}

			if (m_BaseSkinEditor != null)
			{
				m_BaseSkinEditor.OnInspectorGUI();
			}

			if(dirty)
			{
				// Now save
				EditorUtility.SetDirty(target);
			}
		}
	}

	private void UpdateCorpPreviewModel(string corpName, string oldCorpName, Mesh mesh)
	{
		Transform parent = GameObject.Find("SkinPreviews")?.transform;
		if (parent != null)
		{
			// Try deleting the old mesh object
			if (oldCorpName != null)
			{
				GameObject corpPreview = GameObject.Find($"SkinPreview_{oldCorpName}");
				if (corpPreview != null)
					DestroyImmediate(corpPreview);
			}

			// Then create a new one
			GameObject previewGO = new GameObject($"SkinPreview_{corpName}");
			MeshRenderer mr = previewGO.AddComponent<MeshRenderer>();
			mr.enabled = false;
			mr.sharedMaterial = new Material(EditorWindowModDesigner.GetCorpMaterial("GSO", TextureSlot.Main));
			previewGO.AddComponent<MeshFilter>().sharedMesh = mesh;
			previewGO.transform.SetParent(parent);
			previewGO.transform.localPosition = Vector3.zero;
			previewGO.transform.localRotation = Quaternion.identity;
			previewGO.transform.localScale = Vector3.one;
		}
		else Debug.LogError("[Mods] Could not find SkinPreviews object. Is our scene not loaded / corrupted?");
	}
	
	private void SetEditorTarget(Object target)
	{
		if (target != null)
		{
			if (m_BaseSkinEditor == null)
			{
				m_BaseSkinEditor = CreateEditor(target);
			}
			else if (m_BaseSkinEditor.target != target)
			{
				DestroyImmediate(m_BaseSkinEditor);
				m_BaseSkinEditor = CreateEditor(target);
			}
		}
		else if (m_BaseSkinEditor != null)
		{
			DestroyImmediate(m_BaseSkinEditor);
			m_BaseSkinEditor = null;
		}
	}
}