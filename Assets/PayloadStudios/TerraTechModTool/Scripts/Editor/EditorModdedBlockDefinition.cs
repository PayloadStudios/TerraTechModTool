using System.IO;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(ModdedBlockDefinition))]
public class EditorModdedBlockDefinition : Editor
{
	private ModdedBlockDefinition m_SelectedBlock = null;
	private int m_SelectedCorp = 0;
	private JSONTemplater templater = null;

	public override void OnInspectorGUI()
	{
		m_SelectedBlock = (ModdedBlockDefinition)target;
		string path = AssetDatabase.GetAssetPath(m_SelectedBlock);
		string pathNoExt = path.Substring(0, path.LastIndexOf('.'));

		if(m_SelectedBlock != null)
		{
			bool dirty = false;

			GUILayout.Label($"Currently Editing {m_SelectedBlock.name}", EditorGUITT.boldText);
			
			// Name
			GUILayout.BeginHorizontal();
			GUILayout.Label("Display Name:");
			string udpatedName = GUILayout.TextField(m_SelectedBlock.m_BlockDisplayName);
			if (udpatedName != m_SelectedBlock.m_BlockDisplayName)
			{
				m_SelectedBlock.m_BlockDisplayName = udpatedName;
				dirty = true;
			}
			GUILayout.EndHorizontal();

			// Desc
			GUILayout.Label("Description:");
			string updatedDesc = GUILayout.TextArea(m_SelectedBlock.m_BlockDescription);
			if (updatedDesc != m_SelectedBlock.m_BlockDescription)
			{
				m_SelectedBlock.m_BlockDescription = updatedDesc;
				dirty = true;
			}

			// Corp selector --------------------
			GUILayout.Label("Corporation");
			m_SelectedCorp = -1;
			for (int i = 0; i < EditorWindowModDesigner.AvailableCorps.Length; i++)
			{
				if (EditorWindowModDesigner.AvailableCorps[i] == m_SelectedBlock.m_Corporation)
					m_SelectedCorp = i;
			}

			m_SelectedCorp = GUILayout.SelectionGrid(m_SelectedCorp, EditorWindowModDesigner.AvailableCorpIcons, 6, EditorGUITT.corpButton);
			if (m_SelectedCorp != -1 && EditorWindowModDesigner.AvailableCorps[m_SelectedCorp] != m_SelectedBlock.m_Corporation)
			{
				dirty = true;
				m_SelectedBlock.m_Corporation = EditorWindowModDesigner.AvailableCorps[m_SelectedCorp];
				if(m_SelectedBlock.m_PhysicalPrefab != null)
				{
					m_SelectedBlock.m_PhysicalPrefab.m_Corp = m_SelectedBlock.m_Corporation;
					EditorUtility.SetDirty(m_SelectedBlock.m_PhysicalPrefab);
				}
				UpdateBlockPreview(corpChanged: true);
			}

			// Grade, Rarity, Price
			int grade = EditorGUILayout.IntSlider("Grade", m_SelectedBlock.m_Grade, 1, 5);
			BlockCategories category = (BlockCategories)EditorGUILayout.EnumPopup("Category", m_SelectedBlock.m_Category);
			int price = Mathf.Abs(EditorGUILayout.IntField("Price", m_SelectedBlock.m_Price));
			BlockRarity rarity = (BlockRarity)EditorGUILayout.EnumPopup("Rarity", m_SelectedBlock.m_Rarity);
			DamageableType dType = (DamageableType)EditorGUILayout.EnumPopup("DamageType", m_SelectedBlock.m_DamageableType);
			bool unlockWithLicense = EditorGUILayout.Toggle("Unlock with License", m_SelectedBlock.m_UnlockWithLicense);
			if (grade != m_SelectedBlock.m_Grade || price != m_SelectedBlock.m_Price
			|| rarity != m_SelectedBlock.m_Rarity || category != m_SelectedBlock.m_Category
			|| dType != m_SelectedBlock.m_DamageableType || unlockWithLicense != m_SelectedBlock.m_UnlockWithLicense)
			{
				m_SelectedBlock.m_UnlockWithLicense = unlockWithLicense;
				m_SelectedBlock.m_Grade = grade;
				m_SelectedBlock.m_Price = price;
				m_SelectedBlock.m_Rarity = rarity;
				m_SelectedBlock.m_Category = category;
				m_SelectedBlock.m_DamageableType = dType;
				dirty = true;
			}

			// Physical properties
			int maxHP = EditorGUILayout.IntField("Max Health", m_SelectedBlock.m_MaxHealth);
			float mass = EditorGUILayout.FloatField("Mass", m_SelectedBlock.m_Mass);
			if(mass != m_SelectedBlock.m_Mass || maxHP != m_SelectedBlock.m_MaxHealth)
			{
				m_SelectedBlock.m_Mass = Mathf.Clamp(mass, 0.001f, float.MaxValue);
				m_SelectedBlock.m_MaxHealth = Mathf.Clamp(maxHP, 0, int.MaxValue);
				dirty = true;
			}

			// Model, JSON and Templates
			string jsonPath = $"{pathNoExt}.json";
			string prefabPath = $"{pathNoExt}_Prefab.prefab";

			if (m_SelectedBlock.m_Json == null)
			{
				m_SelectedBlock.m_Json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
			}

			if (templater == null)
				templater = new JSONTemplater();

			EditorGUITT.HorizontalLine(Color.black);
			if (m_SelectedBlock.m_PhysicalPrefab == null || m_SelectedBlock.m_Json == null)
			{
				GUILayout.Label("Select Optional Templates");
				templater.DrawButtons();

				if(GUILayout.Button("Create Physical Prefab and Block JSON"))
				{
					// First try getting an existing prefab
					m_SelectedBlock.m_PhysicalPrefab = AssetDatabase.LoadAssetAtPath<TankBlockTemplate>(prefabPath);
					// If that fails, make a new one
					if (m_SelectedBlock.m_PhysicalPrefab == null)
					{
						GameObject prefab = new GameObject($"{m_SelectedBlock.name}_Prefab");
						TankBlockTemplate tbt = prefab.AddComponent<TankBlockTemplate>();
						tbt.m_Corp = m_SelectedBlock.m_Corporation;
						prefab.AddComponent<MeshFilter>();
						prefab.AddComponent<MeshRenderer>();
						templater.CreateObjects(prefab);
						m_SelectedBlock.m_PhysicalPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath).GetComponent<TankBlockTemplate>();
						DestroyImmediate(prefab);
						EditorUtility.SetDirty(target);
						AssetImportCatcher.ManualSaveAssets();
					}

					AssetDatabase.CreateAsset(new TextAsset(), jsonPath);
					File.WriteAllText(jsonPath, templater.GetContents());
				}
			}
			else
			{
				if (GUILayout.Button("Edit Physical Prefab"))
				{
					AssetDatabase.OpenAsset(m_SelectedBlock.m_PhysicalPrefab);
				}
				if (GUILayout.Button("Edit Advanced Properties (Block Json)"))
				{
					AssetDatabase.OpenAsset(m_SelectedBlock.m_Json);
				}

				EditorGUITT.HorizontalLine(Color.black);

				if (GUILayout.Button("Generate previews"))
				{
					var stage = PrefabStageUtility.GetCurrentPrefabStage();
					if (stage != null && stage.prefabAssetPath == path)
					{
						EditorSceneManager.SaveOpenScenes();
					}

					EditorWindow.GetWindow<EditorWindowModDesigner>().RefreshCorpList();

					// Turn off any skin previews so we have a clear scene
					GameObject skinPreviews = GameObject.Find($"SkinPreviews");
					if (skinPreviews != null)
						skinPreviews.SetActive(false);

					// The block preview is dirty, so we need to re-render a preview icon
					string assetPath = $"{pathNoExt}_preview.png";

					TankBlockTemplate blockPreview = Instantiate(m_SelectedBlock.m_PhysicalPrefab, Vector3.zero, Quaternion.identity);
					Transform filledCellPreviews = blockPreview.transform.Find("Previews");
					if (filledCellPreviews != null)
					{
						DestroyImmediate(filledCellPreviews.gameObject);
					}
					foreach(MeshRendererTemplate mrt in blockPreview.GetComponentsInChildren<MeshRendererTemplate>())
					{
						mrt.GetComponent<MeshRenderer>().sharedMaterial = EditorWindowModDesigner.GetCorpMaterial(blockPreview.m_Corp, mrt.slot);
					}

					Bounds bounds = blockPreview.GetBounds();
					float maxDimension = 2f * Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);


					Camera.main.transform.position = bounds.center +
						new Vector3(1f, 1f, 1f) * maxDimension * 0.6f;
					Camera.main.transform.LookAt(bounds.center, Vector3.up);


					// Give the camera a render texture of fixed size
					RenderTexture rendTex = RenderTexture.GetTemporary(512, 512, 24, RenderTextureFormat.ARGB32);
					RenderTexture.active = rendTex;
					RenderTexture old = Camera.main.targetTexture;

					// Render the tech
					Camera.main.targetTexture = rendTex;
					Camera.main.Render();

					// Copy it into our target texture
					Texture2D preview = new Texture2D(512, 512);
					preview.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);

					// Write the target texture to disk
					File.WriteAllBytes(assetPath, ImageConversion.EncodeToPNG(preview));
					m_SelectedBlock.m_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

					// Return the camera to its previous settings
					Camera.main.targetTexture = old;
					RenderTexture.active = null;
					RenderTexture.ReleaseTemporary(rendTex);

					DestroyImmediate(blockPreview.gameObject);
					dirty = true;
				}
			}

			if(dirty)
			{
				// Now save
				EditorUtility.SetDirty(target);
				AssetImportCatcher.ManualSaveAssets();
			}

			EditorGUITT.TextureField("Preview", m_SelectedBlock.m_Icon, false);

			// Verification box
			GUILayout.BeginVertical();
			string verification = m_SelectedBlock.VerifyAsset();
			if (verification == null)
			{
				GUILayout.Label("Verification successful. Ready to export");
				GUILayout.Label(EditorGUITT.Tick);
			}
			else
			{
				GUILayout.Label(verification, EditorGUITT.wordWrapText);
				GUILayout.Label(EditorGUITT.Cross);
			}
			GUILayout.EndVertical();
		}
	}

	private void UpdateBlockPreview(bool meshChanged = false, bool collChanged = false, bool corpChanged = false)
	{
		/*
		GameObject skinPreviews = GameObject.Find($"SkinPreviews");
		GameObject blockPreviews = GameObject.Find("BlockPreviews");
		if (skinPreviews != null)
		{
			skinPreviews.SetActive(false);
		}
		foreach(Transform child in blockPreviews.transform)
		{
			if(!child.name.StartsWith(m_SelectedBlock.name))
				DestroyImmediate(child.gameObject);
		}

		if (m_SelectedBlock != null && m_SelectedBlock.m_RenderMesh != null)
		{
			if (sceneViewPreview == null)
			{
				GameObject previewGO = new GameObject($"{m_SelectedBlock.name} Preview");
				previewGO.transform.SetParent(blockPreviews.transform);
				previewGO.transform.localPosition = Vector3.zero;
				previewGO.transform.localRotation = Quaternion.identity;
				previewGO.transform.localScale = Vector3.one;

				sceneViewPreview = previewGO.AddComponent<MeshFilter>();
				previewGO.AddComponent<MeshRenderer>().sharedMaterial 
					= EditorWindowModDesigner.GetCorpMaterial(m_SelectedBlock.m_Corporation);

				previewGO.AddComponent<MeshCollider>().sharedMesh = m_SelectedBlock.m_CollisionMesh;
			}

			if (meshChanged)
			{
				sceneViewPreview.sharedMesh = m_SelectedBlock.m_RenderMesh;
			}

			if(collChanged)
			{
				sceneViewPreview.GetComponent<MeshCollider>().sharedMesh = m_SelectedBlock.m_CollisionMesh;
			}

			if(corpChanged)
			{
				sceneViewPreview.GetComponent<MeshRenderer>().sharedMaterial 
					= EditorWindowModDesigner.GetCorpMaterial(m_SelectedBlock.m_Corporation);
			}
		}
		else
		{
			if(sceneViewPreview != null)
			{
				DestroyImmediate(sceneViewPreview.gameObject);
				sceneViewPreview = null;
			}
		}
		*/
	}
}
