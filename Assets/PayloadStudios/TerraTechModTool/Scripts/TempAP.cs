using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TempAP : MonoBehaviour
{
	private Vector3 position;

    void Start()
    {
		position = transform.localPosition;
	}

    void Update()
    {
		transform.localPosition = position;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;
    }
}
