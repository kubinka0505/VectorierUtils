using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using VectorierUtils;

public class AlignmentWindow : EditorWindow {
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/Alignment window", false, 200)]
	public static void ShowWindow() {
		AlignmentWindow window = GetWindow<AlignmentWindow>("VectorierUtils - Alignment tool");
		
		// Set initial window size and prevent resizing
		window.minSize = new Vector2(360, 265);
		window.maxSize = window.minSize;
	}

	private enum RelativeTo {
		SelectionSize,
		FirstSelected,
		LastSelected,
		SmallestObject,
		LargestObject
	}

	private enum Alignment {
		Left,
		Right,
		Top,
		Bottom,
		HorizontalCenter,
		VerticalCenter
	}

	private enum Distribute {
		Horizontal,
		Vertical
	}

	private RelativeTo relativeTo = RelativeTo.SelectionSize;

	// -=-=-=- //

	private void OnGUI() {
		GUILayout.Label("Align >= 2 selected GameObjects", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = 100;

		// Buttons styling

		// * Middle center
		GUIStyle buttonStyleMC = new GUIStyle(GUI.skin.button);
		buttonStyleMC.alignment = TextAnchor.MiddleCenter;

		// --- //

		relativeTo = (RelativeTo)EditorGUILayout.EnumPopup("Relative to:", relativeTo);

		GUILayout.Space(10);

		if (GUILayout.Button("⤒ Top", buttonStyleMC)) { AlignSelectedObjects(Alignment.Top); }
		if (GUILayout.Button("⇤ Left", buttonStyleMC)) { AlignSelectedObjects(Alignment.Left); }
		if (GUILayout.Button("⇥ Right", buttonStyleMC)) { AlignSelectedObjects(Alignment.Right); }
		if (GUILayout.Button("⤓ Bottom", buttonStyleMC)) { AlignSelectedObjects(Alignment.Bottom); }
		if (GUILayout.Button("Horizontal Center", buttonStyleMC)) { AlignSelectedObjects(Alignment.HorizontalCenter); }
		if (GUILayout.Button("Vertical Center", buttonStyleMC)) { AlignSelectedObjects(Alignment.VerticalCenter); }

		GUILayout.Space(10);

		// Horizontal line
		Rect rect = EditorGUILayout.GetControlRect(false, 2);
		EditorGUI.DrawRect(rect, Color.gray);
		GUILayout.Space(10);

		GUILayout.Label("Distribute >= 2 selected GameObjects", EditorStyles.boldLabel);

		if (GUILayout.Button("↔ Horizontally", buttonStyleMC)) { DistributeSelectedObjects(Distribute.Horizontal); }
		if (GUILayout.Button("↕ Vertically", buttonStyleMC)) { DistributeSelectedObjects(Distribute.Vertical); }
	}

	private void AlignSelectedObjects(Alignment alignment) {
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0) {
			Utils.AdvancedLog("Error", "No objects have been selected for alignment.");
			return;
		}

		// Determine the target bounds based on the "Relative to" setting
		Bounds targetBounds = GetTargetBounds(selectedObjects);

		foreach (var obj in selectedObjects) {
			// If the object is a prefab, we need to handle it separately
			if (PrefabUtility.IsPartOfPrefabInstance(obj)) {
				// Prefab instance handling (register undo to keep prefab logic intact)
				Undo.RecordObject(obj.transform, "Align Prefab Instance");
				PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
			} else {
				// Register Undo for non-prefab objects
				Undo.RecordObject(obj.transform, "Align Object");
			}

			var objPos = obj.transform.position;

			switch (alignment) {
				case Alignment.Left:
					objPos.x = targetBounds.min.x;
					break;
				case Alignment.Right:
					objPos.x = targetBounds.max.x;
					break;
				case Alignment.Top:
					objPos.y = targetBounds.max.y;
					break;
				case Alignment.Bottom:
					objPos.y = targetBounds.min.y;
					break;
				case Alignment.HorizontalCenter:
					objPos.x = targetBounds.center.x;
					break;
				case Alignment.VerticalCenter:
					objPos.y = targetBounds.center.y;
					break;
				default:
					break;
			}

			obj.transform.position = objPos;

			// Mark the object as dirty to update the scene and prefab instance
			EditorUtility.SetDirty(obj);
		}

		// Save the scene if changes were made
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
	}

	private void DistributeSelectedObjects(Distribute distribute) {
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length < 2) {
			Utils.AdvancedLog("Error", "At least two objects must be selected for distribution.");
			return;
		}

		// Sort objects by position
		if (distribute == Distribute.Horizontal) {
			System.Array.Sort(selectedObjects, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
		} else {
			System.Array.Sort(selectedObjects, (a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
		}

		// Get the bounds of all selected objects
		Bounds bounds = GetSelectionBounds(selectedObjects);

		float totalDistance = distribute == Distribute.Horizontal
			? bounds.max.x - bounds.min.x
			: bounds.max.y - bounds.min.y;

		float gap = totalDistance / (selectedObjects.Length - 1);

		for (int i = 0; i < selectedObjects.Length; i++) {
			// If the object is a prefab, we need to handle it separately
			if (PrefabUtility.IsPartOfPrefabInstance(selectedObjects[i])) {
				Undo.RecordObject(selectedObjects[i].transform, "Distribute Prefab Instance");
				PrefabUtility.RecordPrefabInstancePropertyModifications(selectedObjects[i].transform);
			} else {
				// Register Undo for non-prefab objects
				Undo.RecordObject(selectedObjects[i].transform, "Distribute Object");
			}

			var objPos = selectedObjects[i].transform.position;

			if (distribute == Distribute.Horizontal) {
				objPos.x = bounds.min.x + i * gap;
			} else {
				objPos.y = bounds.min.y + i * gap;
			}

			selectedObjects[i].transform.position = objPos;

			// Mark the object as dirty to update the scene and prefab instance
			EditorUtility.SetDirty(selectedObjects[i]);
		}

		// Save the scene if changes were made
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
	}

	private Bounds GetTargetBounds(GameObject[] selectedObjects) {
		switch (relativeTo) {
			case RelativeTo.FirstSelected:
				return new Bounds(selectedObjects[0].transform.position, Vector3.zero);

			case RelativeTo.LastSelected:
				return new Bounds(selectedObjects[selectedObjects.Length - 1].transform.position, Vector3.zero);

			case RelativeTo.SmallestObject:
				GameObject smallest = selectedObjects.OrderBy(o => GetObjectBounds(o).size.magnitude).First();
				return GetObjectBounds(smallest);

			case RelativeTo.LargestObject:
				GameObject largest = selectedObjects.OrderByDescending(o => GetObjectBounds(o).size.magnitude).First();
				return GetObjectBounds(largest);

			case RelativeTo.SelectionSize:
			default:
				return GetSelectionBounds(selectedObjects);
		}
	}

	private Bounds GetObjectBounds(GameObject obj) {
		Renderer renderer = obj.GetComponent<Renderer>();

		if (renderer != null) {
			return renderer.bounds;
		}

		// Default to position if no renderer exists
		return new Bounds(obj.transform.position, Vector3.zero);
	}

	private Bounds GetSelectionBounds(GameObject[] selectedObjects) {
		if (selectedObjects.Length == 0) {
			return new Bounds(Vector3.zero, Vector3.zero);
		}

		Bounds bounds = GetObjectBounds(selectedObjects[0]);

		foreach (var obj in selectedObjects) {
			bounds.Encapsulate(GetObjectBounds(obj));
		}

		return bounds;
	}
}