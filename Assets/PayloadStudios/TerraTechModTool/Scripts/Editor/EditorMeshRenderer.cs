using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshRenderer))]
public class EditorMeshRenderer : Editor
{
	public override void OnInspectorGUI()
	{
		MeshRenderer mr = (MeshRenderer)target; 
		MeshFilter mf = mr.GetComponent<MeshFilter>();

		if(mf == null)
		{
			mf = mr.gameObject.AddComponent<MeshFilter>();
		}

		Object mesh = EditorGUILayout.ObjectField("Mesh", mf.sharedMesh, typeof(Mesh), allowSceneObjects:false);
		if(mesh != mf.sharedMesh)
		{
			mf.sharedMesh = mesh as Mesh;
			EditorUtility.SetDirty(this);
		}

		TankBlockTemplate template = mr.GetComponentInParent<TankBlockTemplate>();
		EditorGUI.BeginDisabledGroup(template == null);
		if(template != null)
		{
			MeshRendererTemplate mrTemplate = mr.GetComponent<MeshRendererTemplate>();
			if (mrTemplate == null)
				mrTemplate = mr.gameObject.AddComponent<MeshRendererTemplate>();

			TextureSlot slot = TextureSlot.Main;
			if(mr.sharedMaterial != null)
				slot = EditorWindowModDesigner.GetSlotFromMaterialName(mr.sharedMaterial.name);
			TextureSlot[] available = EditorWindowModDesigner.GetAvailableSlotsForCorp(template.m_Corp);
			string[] names = new string[available.Length];
			for (int i = 0; i < available.Length; i++)
				names[i] = available[i].ToString();
			TextureSlot newSlot = (TextureSlot)EditorGUILayout.Popup((int)slot, names);

			if (slot != newSlot || mr.sharedMaterial == null)
			{
				mrTemplate.slot = newSlot;
				mr.sharedMaterial = EditorWindowModDesigner.GetCorpMaterial(template.m_Corp, newSlot);
				EditorUtility.SetDirty(this);
			}
		}
		else
		{
			GUILayout.Label("Block template not found. Add a TankBlockTemplate component to continue choosing textures");
		}
		EditorGUI.EndDisabledGroup();
	}
}
