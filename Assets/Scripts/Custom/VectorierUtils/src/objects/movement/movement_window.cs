using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class MoveGameObjectsUtility : EditorWindow {
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/Movement window", false, 201)]
	public static void ShowWindow() {
		MoveGameObjectsUtility window = GetWindow<MoveGameObjectsUtility>("VectorierUtils - Movement tool");

		// Set initial window size and prevent resizing
		window.minSize = new Vector2(320, 128);
		window.maxSize = window.minSize;
	}

	// Default value is 25%
	private float sizePercentage = 25f;

	private void OnGUI() {
		GUILayout.Label("Move selected GameObjects", EditorStyles.boldLabel);
		GUILayout.Space(10);

		var multiplier = 5;

		GUILayout.BeginHorizontal();
		GUILayout.Label("Size (%)", GUILayout.Width(60));
		sizePercentage = EditorGUILayout.Slider(sizePercentage, 25f, 100f);

		// Round to nearest multiple
		sizePercentage = Mathf.Round(sizePercentage / (float)multiplier) * (float)multiplier;

		GUILayout.EndHorizontal();

		// Horizontal line
		Rect rect = EditorGUILayout.GetControlRect(false, 2);
		EditorGUI.DrawRect(rect, Color.gray);
		GUILayout.Space(10);

		// Create buttons for positioning
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("↖ Top Left", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(-GetSizeMultiplier(), GetSizeMultiplier()); }
		if (GUILayout.Button("↑ Top Middle", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(0.0f, GetSizeMultiplier()); }
		if (GUILayout.Button("↗ Top Right", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(GetSizeMultiplier(), GetSizeMultiplier()); }
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("← Middle Left", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(-GetSizeMultiplier(), 0.0f); }
		if (GUILayout.Button("→ Middle Right", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(GetSizeMultiplier(), 0.0f); }
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("↙ Bottom Left", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(-GetSizeMultiplier(), -GetSizeMultiplier()); }
		if (GUILayout.Button("↓ Bottom Middle", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(0.0f, -GetSizeMultiplier()); }
		if (GUILayout.Button("↘ Bottom Right", GUILayout.ExpandWidth(true))) { MoveSelectedObjects(GetSizeMultiplier(), -GetSizeMultiplier()); }
		GUILayout.EndHorizontal();
	}

	private float GetSizeMultiplier() {
		return sizePercentage / 100f;
	}

	private void MoveSelectedObjects(float xpos, float ypos) {
		if (Selection.gameObjects.Length == 0) {
			Utils.AdvancedLog("Error", "No GameObjects have been selected.");
			return;
		}

		foreach (GameObject obj in Selection.gameObjects) {
			if (obj != null) {
				if (PrefabUtility.IsAnyPrefabInstanceRoot(obj)) {
					MoveSelectedPrefab(obj, xpos, ypos);
				} else {
					MoveSelectedGameObject(obj, xpos, ypos);
				}
			}
		}
	}

	private void MoveSelectedGameObject(GameObject obj, float xpos, float ypos) {
		if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
			Undo.RecordObject(obj.transform, "Move Object");

			Vector3 size = renderer.bounds.size;
			obj.transform.position += new Vector3(size.x * xpos, size.y * ypos, 0);

			EditorUtility.SetDirty(obj.transform);
		} else {
			Utils.AdvancedLog("Warning", $"{obj.name} does not have any Renderer components, cannot determine size.");
		}
	}

	private void MoveSelectedPrefab(GameObject obj, float xpos, float ypos) {
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