using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class MoveGameObjectsUtility : MonoBehaviour {
	// Full size

	// Top
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↖ Top Left", false, 1)]
	static void Move_Full_Top_Left() { Move_Selected_Objects(-1.0f, 1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↑ Top", false, 2)]
	static void Move_Full_Top() { Move_Selected_Objects(0.0f, 1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↗ Top Right", false, 3)]
	static void Move_Full_Top_Right() { Move_Selected_Objects(1.0f, 1.0f); }

	// Center
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/← Left", false, 4)]
	static void Move_Full_Left() { Move_Selected_Objects(-1.0f, 0.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/→ Right", false, 5)]
	static void Move_Full_Right() { Move_Selected_Objects(1.0f, 0.0f); }

	// Bottom
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↙ Bottom Left", false, 6)]
	static void Move_Full_Bottom_Left() { Move_Selected_Objects(-1.0f, -1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↓ Bottom", false, 7)]
	static void Move_Full_Bottom() { Move_Selected_Objects(0.0f, -1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧱ Move each by full size/↘ Bottom Right", false, 8)]
	static void Move_Full_Bottom_Right() { Move_Selected_Objects(1.0f, -1.0f); }

	// -=-=-=- //

	// Half size

	// Top
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↖ Top Left", false, 1)]
	static void Move_Half_Top_Left() { Move_Selected_Objects(-0.5f, 0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↑ Top", false, 2)]
	static void Move_Half_Top() { Move_Selected_Objects(0.0f, 0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↗ Top Right", false, 3)]
	static void Move_Half_Top_Right() { Move_Selected_Objects(0.5f, 0.5f); }

	// Center
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/← Left", false, 4)]
	static void Move_Half_Left() { Move_Selected_Objects(-0.5f, 0.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/→ Right", false, 5)]
	static void Move_Half_Right() { Move_Selected_Objects(0.5f, 0.0f); }

	// Bottom
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↙ Bottom Left", false, 6)]
	static void Move_Half_Bottom_Left() { Move_Selected_Objects(-0.5f, -0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↓ Bottom", false, 7)]
	static void Move_Half_Bottom() { Move_Selected_Objects(0.0f, -0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧰ Move each by half size/↘ Bottom Right", false, 8)]
	static void Move_Half_Bottom_Right() { Move_Selected_Objects(0.5f, -0.5f); }

	// -=-=-=- //
	// Quarter size

	// Top
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↖ Top Left #%&T", false, 1)]
	static void Move_Quarter_Top_Left() { Move_Selected_Objects(-0.25f, 0.25f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↑ Top #%&Y", false, 2)]
	static void Move_Quarter_Top() { Move_Selected_Objects(0.0f, 0.25f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↗ Top Right #%&U", false, 3)]
	static void Move_Quarter_Top_Right() { Move_Selected_Objects(0.25f, 0.25f); }

	// Center
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/← Left #%&G", false, 4)]
	static void Move_Quarter_Left() { Move_Selected_Objects(-0.25f, 0.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/→ Right #%&J", false, 5)]
	static void Move_Quarter_Right() { Move_Selected_Objects(0.25f, 0.0f); }

	// Bottom
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↙ Bottom Left #%&B", false, 6)]
	static void Move_Quarter_Bottom_Left() { Move_Selected_Objects(-0.25f, -0.25f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↓ Bottom #%&N", false, 7)]
	static void Move_Quarter_Bottom() { Move_Selected_Objects(0.0f, -0.25f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⧲ Move each by quarter size/↘ Bottom Right #%&M", false, 8)]
	static void Move_Quarter_Bottom_Right() { Move_Selected_Objects(0.25f, -0.25f); }

	// -=-=-=- //
	// Main

	static void Move_Selected_Objects(float xpos, float ypos) {
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

	static void Move_Regular_Object(GameObject obj, float xpos, float ypos) {
		if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
			Undo.RecordObject(obj.transform, "Move Object");

			Vector3 size = renderer.bounds.size;
			obj.transform.position += new Vector3(size.x * xpos, size.y * ypos, 0);

			EditorUtility.SetDirty(obj.transform);
		} else {
			Utils.AdvancedLog("Warning", $"{obj.name} does not have any Renderer components, cannot determine size.");
		}
	}

	static void Move_Prefab_Instance(GameObject obj, float xpos, float ypos) {
		// Iterate over all the Renderers in the prefab instance
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
		if (renderers.Length > 0) {
			Undo.RecordObject(obj.transform, "Move Object");

			// Calculate the bounding box of all renderers
			Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
			foreach (Renderer renderer in renderers) {
				bounds.Encapsulate(renderer.bounds);
			}

			Vector3 size = bounds.size;
			Vector3 newPosition = obj.transform.position + new Vector3(size.x * xpos, size.y * ypos, 0);
			obj.transform.position = newPosition;

			// Mark the prefab instance as dirty to apply changes
			EditorUtility.SetDirty(obj);

			// Apply changes to the prefab asset
			PrefabUtility.RecordPrefabInstancePropertyModifications(obj);

			// Set dirty to update prefab asset changes
			EditorUtility.SetDirty(obj.transform);
		} else {
			Utils.AdvancedLog("Warning", $"{obj.name} does not have any Renderer components, cannot determine size.");
		}
	}
}