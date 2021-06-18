using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
	public static IEnumerable<GameObject> EnumerateParents(this GameObject go, bool thisObjectFirst = true)
	{
		if (thisObjectFirst)
			yield return go;

		while (go.transform.parent != null)
		{
			go = go.transform.parent.gameObject;
			yield return go;
		}
	}
}
