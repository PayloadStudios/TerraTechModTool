using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class TankBlockTemplate : MonoBehaviour
{
	public List<Vector3> attachPoints = new List<Vector3>();
	public List<IntVector3> filledCells = new List<IntVector3>();
#if UNITY_EDITOR
	public string m_Corp = "";

	public void RemoveAP(Vector3 ap)
	{
		if(!attachPoints.Remove(ap))
		{
			// Could be a floating point error, so double check for close-enough aps
			for(int i = attachPoints.Count - 1; i >= 0; i--)
			{
				if(Vector3.Distance(ap, attachPoints[i]) < Mathf.Epsilon)
				{
					attachPoints.RemoveAt(i);
					return;
				}
			}
		}
	}

	public void AddAP(Vector3 ap)
	{
		if (!attachPoints.Contains(ap))
			attachPoints.Add(ap);
	}

	public bool HasAP(Vector3 pos)
	{
		if (attachPoints.Contains(pos))
			return true;

		foreach (Vector3 ap in attachPoints)
		{
			if (Vector3.Distance(ap, pos) < Mathf.Epsilon)
			{
				return true;
			}
		}

		return false;
	}

	public Vector3 FindAPInDirection(Vector3 filledCell, Vector3 dir)
	{
		Vector3 pos = filledCell + dir * 0.5f;
		if (attachPoints.Contains(pos))
			return pos;

		foreach(Vector3 ap in attachPoints)
		{
			if (Vector3.Distance(ap, pos) < Mathf.Epsilon)
			{
				return ap;
			}
		}

		return Vector3.zero; // Can't be an ap at 0,0,0
	}

	public void CleanupDeadAPs()
	{
		for(int i = attachPoints.Count - 1; i >= 0; i--)
		{
			Vector3 ap = attachPoints[i];
			// AP has to be a permutation of the form (a, b, c + 0.5) for integers a,b,c. Find the fractional coordinate
			Vector3 axis = new Vector3(ap.x - Mathf.Floor(ap.x), ap.y - Mathf.Floor(ap.y), ap.z - Mathf.Floor(ap.z));
			IntVector3 leftFC = new IntVector3(ap + axis);
			IntVector3 rightFC = new IntVector3(ap - axis);
			// If we are between two filled cells or no filled cells, we are invalid
			if(filledCells.Contains(leftFC) == filledCells.Contains(rightFC))
			{
				attachPoints.RemoveAt(i);
			}
		}
	}

	public Bounds GetBounds()
	{
		Bounds bounds = new Bounds();
		foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
		{
			bounds.Encapsulate(mr.bounds);
		}
		return bounds;
	}

	private string VerifyHeirarchy(Transform t)
	{
		if(Vector3.Distance(t.localScale, Vector3.one) > Mathf.Epsilon)
			return $"Transform {t.name} does not have unit scale. Please set the scale to (1,1,1)";
		foreach(Transform child in t)
		{
			string verification = VerifyHeirarchy(child);
			if (verification != null)
				return verification;
		}
		return null;
	}

	public string VerifyAsset()
	{
		if (transform.localPosition != Vector3.zero)
			return "Root transform not at (0,0,0)";

		VerifyHeirarchy(transform);

		Collider[] colliders = GetComponents<Collider>();
		if (colliders.Length == 0)
			return "No colliders present";

		foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
		{
			if(!mr.GetComponent<MeshRendererTemplate>())
				return $"MeshRenderer {mr.name} did not register a texture slot for its material";
		}

		return null;
	}
#endif
}
