using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
// -----------------------------------------------------------------
// <SHARED CLASS> This is shared between TerraTech and TTModDesigner
// -----------------------------------------------------------------
[CreateAssetMenu(menuName = "TerraTech/Block Definition")]
public class ModdedBlockDefinition : ModdedAsset
{
	[Header("Block Palette Information")]
	public string m_Corporation = "GSO";
	public string m_BlockIdentifier = "";
	public string m_BlockDisplayName = "";
	public string m_BlockDescription = "";

	[Header("Campaign Progression")]
	public int m_Grade = 1;
	public BlockRarity m_Rarity = BlockRarity.Common;
	public int m_Price = 1;
	public BlockCategories m_Category = BlockCategories.Standard;
	public bool m_UnlockWithLicense = false;

	[Header("Render Data")]
	// I call this the physical prefab because it only contains render, collision, AP and filled cell data
	// More complicated module definitions are not setup in prefab form
	public TankBlockTemplate m_PhysicalPrefab;

	public Texture2D m_Icon;

	[Header("Other Properties")]
	public int m_MaxHealth = 250; 
	public float m_Mass = 1;
	public TextAsset m_Json;
	public DamageableType m_DamageableType;

	[Header("Module Data")]
	public List<ModdedModuleDefinition> m_ModuleData = new List<ModdedModuleDefinition>();

#if UNITY_EDITOR
	public void StripPreviews()
	{
		if (m_PhysicalPrefab != null)
		{
			Transform filledCellPreviews = m_PhysicalPrefab.transform.Find("Previews");
			if (filledCellPreviews != null)
			{
				DestroyImmediate(filledCellPreviews.gameObject, true);
			}
			//PrefabUtility.SavePrefabAsset(m_PhysicalPrefab.gameObject);
		}
	}

	public override string VerifyAsset()
	{
		if (m_Corporation.Length == 0)
			return "Invalid corp";
		//if (m_BlockIdentifier.Length == 0)
		//	return "Empty block identifier";
		if (m_BlockDisplayName.Length == 0)
			return "Invalid block name";

		if (m_Icon == null)
			return "Missing icon. Press \"Generate Previews\" to autogenerate one";

		if (m_PhysicalPrefab == null)
		{
			string path = AssetDatabase.GetAssetPath(this);
			string pathNoExt = path.Substring(0, path.LastIndexOf('.'));
			string prefabPath = $"{pathNoExt}_Prefab.prefab";

			m_PhysicalPrefab = AssetDatabase.LoadAssetAtPath<TankBlockTemplate>(prefabPath);

			if(m_PhysicalPrefab == null)
				return "Missing physical prefab";

			
		}

		string error;
		error = m_PhysicalPrefab.VerifyAsset();
		if (error != null)
			return $"Physical Prefab failed verification with ({error})";

		foreach(ModdedModuleDefinition module in m_ModuleData)
		{
			error = module.VerifyAsset();
			if (error != null)
				return $"Module ({module.GetType()}) failed verification with ({error})";
		}

		// Return null for success
		return null;
	}
#endif

}
