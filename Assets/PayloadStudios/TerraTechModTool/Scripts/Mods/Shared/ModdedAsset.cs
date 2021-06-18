using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------
// <SHARED CLASS> This is shared between TerraTech and TTModDesigner
// -----------------------------------------------------------------
public abstract class ModdedAsset : ScriptableObject
{
#if UNITY_EDITOR
	public abstract string VerifyAsset();
#endif
}
