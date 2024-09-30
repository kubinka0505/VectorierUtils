using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class MoveGameObjectsUtility : EditorWindow {
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/Movement window", false, 201)]
	public static void ShowWindow() {
		MoveGameObjectsUtility window = GetWindow<MoveGameObjectsUtility>("VectorierUtils - Movement tool");

		// Set initial window size and prevent resizing
		window.minSize = new Vector2(300, 140);
		window.maxSize = window.minSize;
	}

	private readonly string[] sizeOptions = { "100% | Full size", "50% | Half size", "25% | Quarter size" };
	private int selectedSizeIndex = 2;

	private void OnGUI() {
		GUILayout.Label("Move selected GameObjects", EditorStyles.boldLabel);
		GUILayout.Space(10);

		// Size dropdown
		selectedSizeIndex = EditorGUILayout.Popup(selectedSizeIndex, sizeOptions);
		GUILayout.Space(10);

		// Horizontal line
		Rect rect = EditorGUILayout.GetControlRect(false, 2);
		EditorGUI.DrawRect(rect, Color.gray);
		GUILayout.Space(10);

		// Create buttons for positioning
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("↖ Top Left", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(-GetSizeMultiplier(), GetSizeMultiplier()); }
		if (GUILayout.Button("↑ Top", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(0.0f, GetSizeMultiplier()); }
		if (GUILayout.Button("↗ Top Right", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(GetSizeMultiplier(), GetSizeMultiplier()); }
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Middle Left", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(-GetSizeMultiplier(), 0.0f); }
		if (GUILayout.Button("Middle Right", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(GetSizeMultiplier(), 0.0f); }
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("↙ Bottom Left", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(-GetSizeMultiplier(), -GetSizeMultiplier()); }
		if (GUILayout.Button("↓ Bottom", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(0.0f, -GetSizeMultiplier()); }
		if (GUILayout.Button("↘ Bottom Right", GUILayout.ExpandWidth(true))) { Move_Selected_Objects(GetSizeMultiplier(), -GetSizeMultiplier()); }
		GUILayout.EndHorizontal();
	}

	private float GetSizeMultiplier() {
		switch (selectedSizeIndex) {
			// Quarter Size
			case 0:
				return 0.25f;

			// Half Size
			case 1:
				return 0.5f;

			// Full Size
			case 2:
				return 1.0f;

			// Default to Quarter Size
			default:
				return 0.25f;
		}
	}

	private void Move_Selected_Objects(float xpos, float ypos) {
		if (Selection.gameObjects.Length == 0) {
			Utils.AdvancedLog("Error", "No GameObjects have been selected.");
			return;
		}

		foreach (GameObject obj in Selection.gameObjects) {
			if (obj != null) {
				if (PrefabUtility.IsAnyPrefabInstanceRoot(obj)) {
					Move_Prefab_Instance(obj, xpos, ypos);
				} else {
					Move_Regular_Object(obj, xpos, ypos);
				}
			}
		}
	}

	private void Move_Regular_Object(GameObject obj, float xpos, float ypos) {
		if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
			Undo.RecordObject(obj.transform, "Move Object");

			Vector3 size = renderer.bounds.size;
			obj.transform.position += new Vector3(size.x * xpos, size.y * ypos, 0);

			EditorUtility.SetDirty(obj.transform);
		} else {
			Utils.AdvancedLog("Warning", $"{obj.name} does not have any Renderer components, cannot determine size.");
		}
	}

	private void Move_Prefab_Instance(GameObject obj, float xpos, float ypos) {
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

		if (renderers.Length > 0) {
			Undo.RecordObject(obj.transform, "Move Object");

			Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
			foreach (Renderer renderer in renderers) {
				bounds.Encapsulate(renderer.bounds);
			}

			Vector3 size = bounds.size;
			Vector3 newPosition = obj.transform.position + new Vector3(size.x * xpos, size.y * ypos, 0);
			obj.transform.position = newPosition;

			EditorUtility.SetDirty(obj);
			PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
			EditorUtility.SetDirty(obj.transform);
		} else {
			Utils.AdvancedLog("Warning", $"{obj.name} does not have any Renderer components, cannot determine size.");
		}
	}
}
