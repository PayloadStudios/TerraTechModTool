using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static EditorGUITT;

[CustomEditor(typeof(TankBlockTemplate))]
public class TankBlockInspector : Editor
{
	public enum Brush
	{
		None,
		AddFilledCells,
		APToggle,
	};

	static string[] s_BrushModes = new string[]
	{
		"None",
		"Filled Cells",
		"Attach Points",
	};

	static bool s_InternalDataFoldout = false;
	static Brush s_CurrentBrush = Brush.None;

	// pass an enumerable which renders each GUI item (using a lambda)
	void HangingTitleList(string label, float maxWidthRelative, IEnumerable<string> renderList)
	{
		EditorGUILayout.BeginHorizontal();

		// left hand column: title only
		EditorGUILayout.LabelField(label, GUILayout.MaxWidth(Screen.width * maxWidthRelative));

		EditorGUILayout.BeginVertical();

		// right hand column: just enumerate (don't have to do anything with the results)
		System.Action<string> doNothing = s => { };
		foreach (var item in renderList) { doNothing(item); }

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();
	}

	private void AutoGenerateFilledCells(TankBlockTemplate block)
	{
		ClearRenderPreviews(block);

		List<IntVector3> filledCells = new List<IntVector3>();
		Mesh mesh = block.GetComponent<MeshFilter>().sharedMesh;
		for (int i = Mathf.FloorToInt(mesh.bounds.min.x); i <= Mathf.CeilToInt(mesh.bounds.max.x); i++)
		{
			for (int j = Mathf.FloorToInt(mesh.bounds.min.y); j <= Mathf.CeilToInt(mesh.bounds.max.y); j++)
			{
				for (int k = Mathf.FloorToInt(mesh.bounds.min.z); k <= Mathf.CeilToInt(mesh.bounds.max.z); k++)
				{
					Collider[] overlaps = Physics.OverlapBox(block.transform.position + new Vector3(i, j, k), new Vector3(0.4f, 0.4f, 0.4f));
					foreach(Collider overlap in overlaps)
					{
						if(overlap.GetComponentInParent<TankBlockTemplate>() == block)
						{
							filledCells.Add(new IntVector3(i, j, k));
							break;
						}
					}
				}
			}
		}

		block.filledCells = filledCells;
	}

	public void AutoGenerateAPs(TankBlockTemplate block)
	{
		Mesh mesh = block.GetComponent<MeshFilter>().sharedMesh;
		List<Vector3> aps = new List<Vector3>();
		Vector3[] v = new Vector3[3]; // v0, v1, v2;
		float m01, m12, m20;
		Dictionary<Vector3, int> apNumTris = new Dictionary<Vector3, int>();

		for (int i = 0; i < mesh.triangles.Length / 3; i++)
		{
			v[0] = mesh.vertices[mesh.triangles[i * 3]];
			v[1] = mesh.vertices[mesh.triangles[i * 3 + 1]];
			v[2] = mesh.vertices[mesh.triangles[i * 3 + 2]];

			// We're looking for a isoceles triangle of base length 0.26 and 0.1 high, but tilted back. Long edge length is 0.2092
			m01 = Vector3.Distance(v[0], v[1]);
			m12 = Vector3.Distance(v[1], v[2]);
			m20 = Vector3.Distance(v[2], v[0]);

			// Quick perimeter check
			if (Mathf.Abs(m01 + m12 + m20 - (0.2092f * 2f + 0.26f)) < 0.03f)
			{
				int topVertex = -1;
				// Find the long edge
				if (Mathf.Abs(m01 - 0.262f) < 0.03f)
					topVertex = 2;
				else if (Mathf.Abs(m12 - 0.262f) < 0.03f)
					topVertex = 0;
				else if (Mathf.Abs(m20 - 0.262f) < 0.03f)
					topVertex = 1;

				if (topVertex != -1)
				{
					if (!apNumTris.ContainsKey(v[topVertex]))
						apNumTris.Add(v[topVertex], 1);
					else apNumTris[v[topVertex]]++;
				}
			}
		}

		foreach (var kvp in apNumTris)
		{
			if (kvp.Value == 4)
			{
				//Debug.Log($"Added AP at {kvp.Key}, constrained to {ConstrainAPPosition(kvp.Key)}");
				aps.Add(ConstrainAPPosition(kvp.Key));
			}
			else
			{
				//Debug.Log($"Found candidate AP at {kvp.Key} with {kvp.Value} edges");
			}
		}
		block.attachPoints = aps;

		block.CleanupDeadAPs();
	}

	public override void OnInspectorGUI()
	{
		TankBlockTemplate block = (TankBlockTemplate)target;

		EditorGUI.BeginChangeCheck();

		if (block.filledCells.Count == 0)
			block.filledCells.Add(IntVector3.zero);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Brush:", EditorGUITT.boldText);
		Brush newEditMode = (Brush)GUILayout.SelectionGrid((int)s_CurrentBrush, s_BrushModes, 3);
		GUILayout.EndHorizontal();

		if (newEditMode != s_CurrentBrush)
		{
			s_CurrentBrush = newEditMode;
			SceneView.RepaintAll();
		}

		switch (s_CurrentBrush)
		{
			case Brush.AddFilledCells:
			{
				GUILayout.Label("Click on cell surfaces to add new filled cells");
				GUILayout.Label("Hold SHIFT to add an entire plane of filled cells");
				GUILayout.Label("Hold CTRL to remove filled cells");
				break;
			}
			case Brush.APToggle:
			{
				GUILayout.Label("Click on Attach Point markers to turn them on and off");
				break;
			}
		}

		EditorGUITT.HorizontalLine(Color.black);

		MeshFilter filter = block.GetComponent<MeshFilter>();
		bool valid = filter != null && filter.sharedMesh != null;
		EditorGUI.BeginDisabledGroup(!valid);
		if (!valid)
		{
			GUILayout.Label("No meshes present on block");
		}
		if(block.gameObject.scene.GetPhysicsScene() == Physics.defaultPhysicsScene)
		{
			if (GUILayout.Button("Auto Generate Filled Cells"))
			{
				AutoGenerateFilledCells(block);
			}
		}
		EditorGUI.EndDisabledGroup();
		if (GUILayout.Button("Auto Generate Attach Points"))
		{
			AutoGenerateAPs(block);
		}
		EditorGUI.EndDisabledGroup();

		s_InternalDataFoldout = EditorGUILayout.Foldout(s_InternalDataFoldout, "Internal Data (read only)");

		if (s_InternalDataFoldout)
		{
			EditorGUI.indentLevel++;

			// Select() builds a list of items that each render a GUI field when evaluated
			HangingTitleList(string.Format("Attach Points ({0})", block.attachPoints.Count), 0.3f,
				block.attachPoints.Select(i => EditorGUILayout.Vector3Field("", i).ToString()));

			HangingTitleList(string.Format("Filled Cells ({0})", block.filledCells.Count), 0.3f,
				block.filledCells.Select(i => EditorGUILayout.Vector3Field("", i).ToString()));

			EditorGUI.indentLevel--;
		}

		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}

	void OnDisable()
	{
		if(target != null)
			ClearRenderPreviews((TankBlockTemplate)target);
	}

	static Color s_FilledCellAddColour = new Color(0.0f, 0.5f, 1.0f, 1.0f);
	static Color s_FilledCellRemoveColour = new Color(1.0f, 0.5f, 0.0f, 1.0f);
	static Color s_FilledCellCantRemoveColour = new Color(1.0f, 0.0f, 0.0f, 1.0f);
	static Color s_APAddColour = new Color(0.9f, 0.2f, 0.8f, 1.0f);
	static Color s_APRemoveColour = new Color(1.0f, 0.0f, 0.2f, 1.0f);

	static Transform s_CellPrefab = null, s_APPrefab = null;
	static Material s_SelectedMat;
	static List<Ray> s_GrabbedSurfaces = new List<Ray>();

	void ClearRenderPreviews(TankBlockTemplate block)
	{
		Transform previewParent = block.transform.Find("Previews");
		if(previewParent != null)
		{
			DestroyImmediate(previewParent.gameObject);
		}
	}

	Transform GetRenderPreviewRoot(TankBlockTemplate block)
	{
		if (s_CellPrefab == null || s_APPrefab == null || s_SelectedMat == null)
		{
			s_CellPrefab = AssetDatabase.LoadAssetAtPath<Transform>($"{ModUtils.AssetsDir}/FixedAssets/CellHighlight.prefab");
			s_APPrefab = AssetDatabase.LoadAssetAtPath<Transform>($"{ModUtils.AssetsDir}/FixedAssets/APHighlight.prefab");
			s_SelectedMat = AssetDatabase.LoadAssetAtPath<Material>($"{ModUtils.AssetsDir}/FixedAssets/SelectedCell.mat");
		}

		// Group all preview content under a single gameobject for easy cleanup
		Transform previewParent = block.transform.Find("Previews");
		if (previewParent == null)
		{
			previewParent = new GameObject("Previews").transform;
		}
		previewParent.SetParent(block.transform);
		previewParent.localPosition = Vector3.zero;
		previewParent.localRotation = Quaternion.identity;

		return previewParent;
	}

	void OnSceneGUI()
	{
		TankBlockTemplate block = (TankBlockTemplate)target;
		Transform previewParent = GetRenderPreviewRoot(block);

		// List our existing previews, to determine which to add and which to remove
		List<string> existingPreviews = new List<string>(previewParent.childCount);
		foreach(Transform child in previewParent)
		{
			existingPreviews.Add(child.name);
		}

		if (s_CurrentBrush != Brush.None)
		{
			// For each filled cell, make sure there exists a preview
			foreach (Vector3 filledCell in block.filledCells)
			{
				string previewName = $"FC:{filledCell.x},{filledCell.y},{filledCell.z}";

				if (existingPreviews.Contains(previewName))
				{
					existingPreviews.Remove(previewName);
				}
				else
				{
					Transform cell = Instantiate(s_CellPrefab, filledCell, Quaternion.identity, previewParent);
					cell.name = previewName;
					cell.localPosition = filledCell;
				}
			}

			// For each AP, make sure there exists a preview
			foreach (Vector3 ap in block.attachPoints)
			{
				string previewName = $"AP:{ap.x},{ap.y},{ap.z}";

				if (existingPreviews.Contains(previewName))
				{
					existingPreviews.Remove(previewName);
				}
				else
				{
					Transform cell = Instantiate(s_APPrefab, ap, Quaternion.identity, previewParent);
					cell.name = previewName;
					cell.localPosition = ap;
				}
			}
		}

		// The remaining list is filled with all the filled cells and APs that we no longer have, so delete them
		foreach (string excess in existingPreviews)
		{
			Transform child = previewParent.Find(excess);
			DestroyImmediate(child.gameObject);
		}

		if (s_CurrentBrush != Brush.None)
		{
			// Now check for input and handle based on our current paint mode
			Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

			s_SelectedMat.SetPass(0);
			RaycastHit[] hits = new RaycastHit[32];
			int numHits = block.gameObject.scene.GetPhysicsScene().Raycast(mouseRay.origin, mouseRay.direction, hits);

			float minDist = float.MaxValue;
			RaycastHit hit = new RaycastHit();
			bool isAP = false;
			for (int i = 0; i < numHits; i++)
			{
				RaycastHit candidate = hits[i];
				TempFilledCell fc = candidate.collider.GetComponent<TempFilledCell>();
				TempAP ap = candidate.collider.GetComponent<TempAP>();
				if (candidate.distance < minDist && (fc != null || ap != null))
				{
					isAP = ap != null;
					minDist = candidate.distance;
					hit = candidate;
				}
			}

			if (minDist < float.MaxValue)
			{
				TankBlockTemplate t = hit.collider.GetComponentInParent<TankBlockTemplate>();
				if (t == block)
				{
					switch (s_CurrentBrush)
					{
						case Brush.APToggle:
						{
							if (isAP)
							{
								if (Event.current.type == EventType.MouseDown && Event.current.button == 0) // LMB
								{
									// Remove the AP in question
									block.RemoveAP(hit.collider.transform.localPosition);
								}
							}
							else // We must be clicking on a surface, so add an AP there
							{
								Vector3 apPos = ConstrainAPPosition(hit.collider.transform.localPosition + hit.normal * 0.5f);
								if (!block.HasAP(apPos))
								{
									s_SelectedMat.color = s_APAddColour;
									DrawFace(previewParent.TransformPoint(hit.collider.transform.localPosition), hit.normal, 0.75f);
								}

								if (Event.current.type == EventType.MouseDown && Event.current.button == 0) // LMB
								{
									// Add an AP at the centre of that surface
									block.AddAP(apPos);
								}
							}
							break;
						}
						case Brush.AddFilledCells:
						{
							if (!isAP)
							{
								s_GrabbedSurfaces.Clear();
								Ray targetRay = new Ray(hit.collider.transform.localPosition, hit.normal);
								bool multiSelect = Event.current.shift;
								bool add = !Event.current.control;

								// Gather the list of surfaces we care about. If we are holding shift, grab everything coplanar
								if (multiSelect)
								{
									// Find every coplanar filled cell
									float plane = Vector3.Dot(targetRay.origin, targetRay.direction);
									foreach (Transform child in previewParent)
									{
										if (Vector3.Dot(child.localPosition, hit.normal) == plane && child.GetComponent<TempFilledCell>() != null)
										{
											s_GrabbedSurfaces.Add(new Ray(child.localPosition, targetRay.direction));
										}
									}
									// Remove any that have a filled cell in front of them in the direction we intend to extend in
									foreach (Transform child in previewParent)
									{
										if (Vector3.Dot(child.localPosition, hit.normal) == plane + 1 && child.GetComponent<TempFilledCell>() != null)
										{
											s_GrabbedSurfaces.Remove(new Ray(child.localPosition - targetRay.direction, targetRay.direction));
										}
									}
								}
								else
								{
									s_GrabbedSurfaces.Add(targetRay);
								}

								// Now draw them all
								s_SelectedMat.color = add ? s_FilledCellAddColour : s_FilledCellRemoveColour;

								// Special case for when we would remove the final block
								if (!add && s_GrabbedSurfaces.Count == block.filledCells.Count)
								{
									s_SelectedMat.color = s_FilledCellCantRemoveColour;
								}

								foreach (Ray face in s_GrabbedSurfaces)
								{
									DrawFace(previewParent.TransformPoint(face.origin), face.direction);
								}

								// And when we click, add or remove them appropriately
								if (Event.current.type == EventType.MouseDown && Event.current.button == 0) // LMB
								{
									if (add)
									{
										foreach (Ray ray in s_GrabbedSurfaces)
										{
											block.filledCells.Add(ray.origin + ray.direction);
											Vector3 deadAP = block.FindAPInDirection(ray.origin, ray.direction);
											if (deadAP != Vector3.zero)
											{
												block.AddAP(deadAP + ray.direction);
											}
										}
									}
									else // Remove									
									{
										if (s_GrabbedSurfaces.Count != block.filledCells.Count)
										{
											foreach (Ray ray in s_GrabbedSurfaces)
											{
												block.filledCells.Remove(ray.origin);
											}
										}
									}

									block.CleanupDeadAPs();
								}
							}

							break;
						}
					}

					// This consumes any other handle interactions we may have been considering having
					HandleUtility.AddDefaultControl(0);
				}
			}
		}
		SceneView.RepaintAll();
	}

	// used to 'clamp' any position to the nearest valid AP position (any face centre on unit cube grid)
	public static Vector3 ConstrainAPPosition(Vector3 localPos)
	{
		// find nearest 'corner point', get relative position to that
		Vector3 halfShift = localPos - new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 corner = new Vector3(Mathf.Round(halfShift.x), Mathf.Round(halfShift.y), Mathf.Round(halfShift.z));
		Vector3 offset = halfShift - corner;

		// constrain to centre of one face, whichever is closest
		float smallest = Mathf.Min(Mathf.Abs(offset.x), Mathf.Abs(offset.y), Mathf.Abs(offset.z));
		if (Mathf.Abs(offset.x) == smallest)
		{
			offset.x = 0;
			offset.y *= 0.5f / Mathf.Abs(offset.y);
			offset.z *= 0.5f / Mathf.Abs(offset.z);
		}
		else if (Mathf.Abs(offset.y) == smallest)
		{
			offset.x *= 0.5f / Mathf.Abs(offset.x);
			offset.y = 0;
			offset.z *= 0.5f / Mathf.Abs(offset.z);
		}
		else
		{
			offset.x *= 0.5f / Mathf.Abs(offset.x);
			offset.y *= 0.5f / Mathf.Abs(offset.y);
			offset.z = 0;
		}

		// apply constrained offset to corner position
		return corner + offset + new Vector3(0.5f, 0.5f, 0.5f);
	}

	void DrawFace(Vector3 position, Vector3 dir, float size = 1.0f)
	{
		GL.PushMatrix();

		GL.Begin(GL.TRIANGLES);

		dir *= 0.5f;
		Vector3 xAxis = new Vector3(Mathf.Abs(dir.y), Mathf.Abs(dir.z), Mathf.Abs(dir.x));
		Vector3 zAxis = new Vector3(Mathf.Abs(dir.z), Mathf.Abs(dir.x), Mathf.Abs(dir.y));

		if (dir.x < 0 || dir.y < 0 || dir.z < 0)
			xAxis *= -1.0f;

		xAxis *= size;
		zAxis *= size;

		dir *= 1.1f;

		GL.Vertex(position + dir - xAxis - zAxis);
		GL.Vertex(position + dir - xAxis + zAxis);
		GL.Vertex(position + dir + xAxis + zAxis);

		GL.Vertex(position + dir - xAxis - zAxis);
		GL.Vertex(position + dir + xAxis + zAxis);
		GL.Vertex(position + dir + xAxis - zAxis);

		GL.End();
		GL.PopMatrix();
	}
}
