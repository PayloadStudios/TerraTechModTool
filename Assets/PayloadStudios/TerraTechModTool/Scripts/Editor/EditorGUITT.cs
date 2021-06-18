using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorGUITT
{
	public static GUIStyle horizontalLine = new GUIStyle()
	{
		normal = new GUIStyleState()
		{
			background = EditorGUIUtility.whiteTexture,
		},
		margin = new RectOffset(0, 0, 4, 4),
		fixedHeight = 1,
	};

	public static void HorizontalLine(Color color)
	{
		var c = GUI.color;
		GUI.color = color;
		GUILayout.Box(GUIContent.none, horizontalLine);
		GUI.color = c;
	}

	public static GUIStyle boldText = new GUIStyle(GUI.skin.label)
	{
		fontStyle = FontStyle.Bold,
	};

	public static GUIStyle wordWrapText = new GUIStyle(GUI.skin.label)
	{
		wordWrap = true,
	};

	public static GUIStyle tinyButton = new GUIStyle(GUI.skin.label)
	{
		fixedWidth = 16,
		fixedHeight = 16,
	};

	public static GUIStyle corpButton = new GUIStyle(GUI.skin.button)
	{
		fixedWidth = 32,
		fixedHeight = 32,
	};

	public static GUIStyle fixedSizeButton100 = new GUIStyle(GUI.skin.button)
	{
		fixedWidth = 100,
	};

	public static GUIStyle fixedSizeButton50 = new GUIStyle(GUI.skin.button)
	{
		fixedWidth = 50,
	};

	public static GUIStyle wordWrapToggle = new GUIStyle(GUI.skin.toggle)
	{
		wordWrap = true
	};

	private static Texture s_Tick;
	public static Texture Tick
	{
		get
		{
			if (s_Tick == null)
				s_Tick = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ModUtils.AssetsDir}/FixedAssets/Tick.png");
			return s_Tick;
		}
	}

	private static Texture s_Cross;
	public static Texture Cross
	{
		get
		{
			if (s_Cross == null)
				s_Cross = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ModUtils.AssetsDir}/FixedAssets/Cross.png");
			return s_Cross;
		}
	}

	public static Texture2D TextureField(string name, Texture2D texture, bool interactable = true)
	{
		GUILayout.BeginVertical();
		var style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.UpperLeft;
		style.fixedWidth = 90;
		GUILayout.Label(name, style);
		if (interactable)
		{
			texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(90), GUILayout.Height(90));
		}
		else
		{
			GUILayout.Label(texture, GUILayout.Width(90), GUILayout.Height(90));
		}
		GUILayout.EndVertical();
		return texture;
	}

	[System.Serializable]
	public class PositionWithFacing
	{
		public Vector3 position = Vector3.zero;
		public Vector3 forward = Vector3.forward;

		public PositionWithFacing(PositionWithFacing from) { position = from.position; forward = from.forward; }
		public PositionWithFacing(Vector3 pos, Vector3 forw) { position = pos; forward = forw; }
		public Quaternion orientation { get { return forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward); } }
		public static PositionWithFacing identity { get { return new PositionWithFacing(Vector3.zero, Vector3.forward); } }
		public Matrix4x4 Matrix { get { return Matrix4x4.TRS(position, orientation, Vector3.one); } }
	}

	// internal state for DragHandle()
	static int s_DragHandleHash = "DragHandleHash".GetHashCode();
	static Vector2 s_DragHandleMouseStart;
	static Vector2 s_DragHandleMouseCurrent;
	static Vector3 s_DragHandleWorldStart;
	static float s_DragHandleClickTime = 0;
	static int s_DragHandleClickID;
	static float s_DragHandleDoubleClickInterval = 0.5f;
	static bool s_DragHandleHasMoved;

	// externally accessible to get the ID of the most resently processed DragHandle
	public static int lastDragHandleID;

	public enum DragHandleResult
	{
		none = 0,

		LMBPress,
		LMBClick,
		LMBDoubleClick,
		LMBDrag,
		LMBRelease,

		RMBPress,
		RMBClick,
		RMBDoubleClick,
		RMBDrag,
		RMBRelease,
	};

	public static Vector3 DragHandle(Vector3 position, float handleSize, Handles.CapFunction capFunc, Color colorSelected, out DragHandleResult result)
	{
		return DragHandle(position, handleSize, capFunc, colorSelected, s_DragHandleHash, out result);
	}

	public static Vector3 DragHandle(Vector3 position, float handleSize, Handles.CapFunction capFunc, Color colorSelected, int controlHash, out DragHandleResult result)
	{
		int id = GUIUtility.GetControlID(controlHash, FocusType.Passive);
		lastDragHandleID = id;

		Vector3 screenPosition = Handles.matrix.MultiplyPoint(position);
		Matrix4x4 cachedMatrix = Handles.matrix;

		result = DragHandleResult.none;

		switch (Event.current.GetTypeForControl(id))
		{
			case EventType.MouseDown:
				if (HandleUtility.nearestControl == id && (Event.current.button == 0 || Event.current.button == 1))
				{
					GUIUtility.hotControl = id;
					s_DragHandleMouseCurrent = s_DragHandleMouseStart = Event.current.mousePosition;
					s_DragHandleWorldStart = position;
					s_DragHandleHasMoved = false;

					Event.current.Use();
					EditorGUIUtility.SetWantsMouseJumping(1);

					if (Event.current.button == 0)
						result = DragHandleResult.LMBPress;
					else if (Event.current.button == 1)
						result = DragHandleResult.RMBPress;
				}
				break;

			case EventType.MouseUp:
				if (GUIUtility.hotControl == id && (Event.current.button == 0 || Event.current.button == 1))
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
					EditorGUIUtility.SetWantsMouseJumping(0);

					if (Event.current.button == 0)
						result = DragHandleResult.LMBRelease;
					else if (Event.current.button == 1)
						result = DragHandleResult.RMBRelease;

					if (Event.current.mousePosition == s_DragHandleMouseStart)
					{
						bool doubleClick = (s_DragHandleClickID == id) &&
							(Time.realtimeSinceStartup - s_DragHandleClickTime < s_DragHandleDoubleClickInterval);

						s_DragHandleClickID = id;
						s_DragHandleClickTime = Time.realtimeSinceStartup;

						if (Event.current.button == 0)
							result = doubleClick ? DragHandleResult.LMBDoubleClick : DragHandleResult.LMBClick;
						else if (Event.current.button == 1)
							result = doubleClick ? DragHandleResult.RMBDoubleClick : DragHandleResult.RMBClick;
					}
				}
				break;

			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					s_DragHandleMouseCurrent += new Vector2(Event.current.delta.x, -Event.current.delta.y);
					Vector3 position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(s_DragHandleWorldStart))
						+ (Vector3)(s_DragHandleMouseCurrent - s_DragHandleMouseStart);
					position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2));

					if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
						position.z = s_DragHandleWorldStart.z;
					if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
						position.y = s_DragHandleWorldStart.y;
					if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
						position.x = s_DragHandleWorldStart.x;

					if (Event.current.button == 0)
						result = DragHandleResult.LMBDrag;
					else if (Event.current.button == 1)
						result = DragHandleResult.RMBDrag;

					s_DragHandleHasMoved = true;

					GUI.changed = true;
					Event.current.Use();
				}
				break;

			case EventType.Repaint:
				Color currentColour = Handles.color;
				if (id == GUIUtility.hotControl && s_DragHandleHasMoved)
					Handles.color = colorSelected;

				Handles.matrix = Matrix4x4.identity;
				capFunc(id, screenPosition, Quaternion.identity, handleSize, EventType.Repaint);
				Handles.matrix = cachedMatrix;

				Handles.color = currentColour;
				break;

			case EventType.Layout:
				Handles.matrix = Matrix4x4.identity;
				HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, handleSize));
				Handles.matrix = cachedMatrix;
				break;
		}

		return position;
	}

	static int s_DragHandleHashPwfFacing = "DragHandleHashPwfFacing".GetHashCode();

	public static GUIStyle s_HandleLabelGuiStyle;

	public static bool SceneEditPositionWithFacing(ref PositionWithFacing pwf, Color color, string label, Color labelColor, bool groundProject, UnityEngine.Object undoObject = null, Transform transformParent = null)
	{
		bool changed = false;
		Handles.color = color;
		DragHandleResult dhResult;

		Vector3 worldPos = transformParent ? transformParent.localToWorldMatrix.MultiplyPoint(pwf.position) : pwf.position;

		worldPos = DragHandle(worldPos, 3, Handles.SphereHandleCap, Color.white, out dhResult);

		if (dhResult == DragHandleResult.LMBDrag)
		{
			if (undoObject && !changed)
				Undo.RegisterCompleteObjectUndo(undoObject, "Move " + undoObject.name);

			if (transformParent)
				pwf.position = transformParent.worldToLocalMatrix.MultiplyPoint(worldPos);
			else
				pwf.position = worldPos;

			changed = true;
			GUI.changed = false;
		}

		Vector3 facingMarkerPos = pwf.position + pwf.forward * 1.8f;
		facingMarkerPos = transformParent ? transformParent.localToWorldMatrix.MultiplyPoint(facingMarkerPos) : facingMarkerPos;

		facingMarkerPos = DragHandle(facingMarkerPos, 1, Handles.SphereHandleCap, Color.white, s_DragHandleHashPwfFacing, out dhResult);

		facingMarkerPos = transformParent ? transformParent.worldToLocalMatrix.MultiplyPoint(facingMarkerPos) : facingMarkerPos;

		if (dhResult == DragHandleResult.LMBDrag)
		{
			if (undoObject && !changed)
				Undo.RegisterCompleteObjectUndo(undoObject, "Rotate " + undoObject.name);
			pwf.forward = (facingMarkerPos - pwf.position);
			pwf.forward.y = 0f;
			pwf.forward.Normalize();

			changed = true;
			GUI.changed = false;
		}

		if (label != null)
		{
			if (s_HandleLabelGuiStyle == null)
				s_HandleLabelGuiStyle = new GUIStyle(GUI.skin.label);

			s_HandleLabelGuiStyle.normal.textColor = labelColor;
			Handles.Label(worldPos, label, s_HandleLabelGuiStyle);
		}

		if (changed && undoObject)
		{
			EditorUtility.SetDirty(undoObject);
		}

		return changed;
	}


	public static void DrawThickenedLine(Vector3 p1, Vector3 p2, float width)
	{
		// offset vector perpendicular to line and camera direction, scaled to 1/2 desired width
		Vector3 offs = Vector3.Cross(p2 - p1, Camera.current.transform.position - p1).normalized * width * 0.5f;
		Handles.DrawSolidRectangleWithOutline(new Vector3[] { p1 + offs, p1 - offs, p2 - offs, p2 + offs },
			Handles.color, new Color(0, 0, 0, 0));
	}
}
