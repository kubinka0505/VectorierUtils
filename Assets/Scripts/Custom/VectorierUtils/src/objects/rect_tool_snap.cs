using UnityEngine;
using UnityEditor;

// -=-=-=- //

[InitializeOnLoad]
public class RectToolSnapper : Editor {
	// The snapping threshold distance
	private const float SnapThreshold = 0.1f;
	private const string MenuPath = "Vectorier/⚙ Utils/Snap scale";
	private static bool snappingEnabled;

	// Static constructor, called when Unity loads
	static RectToolSnapper() {
		// Load initial state of snapping from EditorPrefs
		snappingEnabled = EditorPrefs.GetBool(MenuPath, false);
		
		// Register the callback for the SceneView GUI
		SceneView.duringSceneGui += OnSceneGUI;
	}

	// Creates a toggleable menu item in Unity's toolbar
	[MenuItem(MenuPath)]
	public static void ToggleSnapScale() {
		// Toggle snapping
		snappingEnabled = !snappingEnabled;

		// Save the state in EditorPrefs
		EditorPrefs.SetBool(MenuPath, snappingEnabled);
	}

	// Ensure that the checkbox is correctly shown in the menu
	[MenuItem(MenuPath, true)]
	public static bool ToggleSnapScaleValidate() {
		// Set checkbox state
		Menu.SetChecked(MenuPath, snappingEnabled);

		// Always show this menu item
		return true;
	}

	private static void OnSceneGUI(SceneView sceneView) {
		// Only snap if snapping is enabled and the Rect Tool is selected
		if (!snappingEnabled || Tools.current != Tool.Rect) {
			return;
		};

		GameObject activeObject = Selection.activeGameObject;
		if (activeObject == null || PrefabUtility.IsPartOfPrefabAsset(activeObject)) {
			return;
		};

		// Get all 2D objects in the scene
		GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
		foreach (var obj in allObjects) {
			if (obj == activeObject || obj.GetComponent<Renderer>() == null) {
				// Skip if same or no renderer
				continue;
			}

			SnapObjectToEdges(activeObject, obj);
		}
	}

	private static void SnapObjectToEdges(GameObject activeObject, GameObject targetObject)
	{
		Bounds activeBounds = GetObjectBounds(activeObject);
		Bounds targetBounds = GetObjectBounds(targetObject);

		Vector3[] activeVertices = GetBoundsVertices(activeBounds);
		Vector3[] targetVertices = GetBoundsVertices(targetBounds);

		Vector3 snapOffset = Vector3.zero;

		foreach (var activeVertex in activeVertices) {
			foreach (var targetVertex in targetVertices) {

				// Check the distance between vertices
				if (Vector3.Distance(activeVertex, targetVertex) <= SnapThreshold) {
					snapOffset = targetVertex - activeVertex;
					MoveAndSnap(activeObject, snapOffset);

					break;
				}
			}
		}
	}

	private static void MoveAndSnap(GameObject activeObject, Vector3 offset) {
		Undo.RecordObject(activeObject.transform, "Snap to Edge");
		// Move object by the snap offset
		activeObject.transform.position += offset;
	}

	private static Vector3[] GetBoundsVertices(Bounds bounds) {
		return new Vector3[] {
			// Bottom-left
			new Vector3(bounds.min.x, bounds.min.y, 0),

			// Top-left
			new Vector3(bounds.min.x, bounds.max.y, 0),

			// Bottom-right
			new Vector3(bounds.max.x, bounds.min.y, 0),

			// Top-right
			new Vector3(bounds.max.x, bounds.max.y, 0) 
		};
	}

	private static Bounds GetObjectBounds(GameObject obj) {
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer != null) {
			return renderer.bounds;
		}

		// In case of 2D objects without renderers, use collider bounds (if available)
		Collider2D collider2D = obj.GetComponent<Collider2D>();
		if (collider2D != null) {
			return collider2D.bounds;
		}

		// Return empty bounds if none available
		return new Bounds();
	}
}
