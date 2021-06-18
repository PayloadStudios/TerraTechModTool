using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModdedModuleDefinition : ModdedAsset
{
	public Dictionary<string, string> m_Data;


#if UNITY_EDITOR
	public override string VerifyAsset()
	{
		// Return null for success
		return null;
	}
#endif
}
