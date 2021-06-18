using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// -----------------------------------------------------------------
// <SHARED CLASS> This is shared between TerraTech and TTModDesigner
// -----------------------------------------------------------------
[CreateAssetMenu(menuName = "TerraTech/Corp Definition")]
public class ModdedCorpDefinition : ModdedAsset
{
	public string m_ShortName = "CORP";
	public string m_DisplayName = "Custom Corp";

	public string m_RewardCorp = "GSO";

	public Texture2D m_Icon;
	public ModdedSkinDefinition[] m_DefaultSkinSlots;


#if UNITY_EDITOR
	// Only used in the editor to generate preview images
	public Mesh m_PreviewMesh;

	public static readonly string[] s_TextureSlotNames = new string[]
	{
		"Main", "Tracks", "Extra",
	};

	public override string VerifyAsset()
	{
		if (m_ShortName == "CORP" || m_ShortName.Length == 0)
			return "Corp has unset short name";
		if (m_DisplayName == "Custom Corp" || m_DisplayName.Length == 0)
			return "Corp has unset display name";
		if (m_Icon == null)
			return "Corp icon unset";

		if (m_DefaultSkinSlots == null)
			return "No skins set";
		if(m_DefaultSkinSlots.Length == 0)
			m_DefaultSkinSlots = new ModdedSkinDefinition[(int)TextureSlot.NUM_TEXTURE_SLOTS];

		for (int i = 0; i < m_DefaultSkinSlots.Length; i++)
		{
			if (m_DefaultSkinSlots[i] == null)
			{
				string corpPath = AssetDatabase.GetAssetPath(this);
				corpPath = corpPath.Substring(0, corpPath.LastIndexOf('/'));
				string skinPath = $"{corpPath}/{name}_{s_TextureSlotNames[i]}.asset";
				ModdedSkinDefinition skin = AssetDatabase.LoadAssetAtPath<ModdedSkinDefinition>(skinPath);
				if (skin != null)
				{
					m_DefaultSkinSlots[i] = skin;
				}
			}

			if (m_DefaultSkinSlots[i] == null && i == 0)
			{
				return "Could not find base skin";
			}

			if (m_DefaultSkinSlots[i] != null)
			{
				string skinVerification = m_DefaultSkinSlots[i].VerifyAsset();
				if (skinVerification != null)
				{
					return $"Base Skin: {skinVerification}";
				}
			}
		}

		// Return null for success
		return null;
	}
#endif
}
