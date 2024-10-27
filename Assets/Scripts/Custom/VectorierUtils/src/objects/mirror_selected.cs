using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class MirrorSelectedObjects : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/⧎ Mirror selected objects", false, 3)]
	static void MirrorObjects() {
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0) {
			Utils.AdvancedLog("Error", "No GameObjects have been selected.");
			return;
		}

		// Start recording undo event
		Undo.RecordObjects(selectedObjects, "Mirror Objects");

		// Calculate the centroid of the selected objects
		Vector3 centroid = Vector3.zero;

		foreach (GameObject obj in selectedObjects) {
			centroid += obj.transform.position;
		}

		centroid /= selectedObjects.Length;

		// --- //

		// Mirror each object around the centroid
		foreach (GameObject obj in selectedObjects) {
			// Record object transform for undo
			Undo.RecordObject(obj.transform, "Mirror Object Position and Rotation");

			// Mirror position around centroid
			Vector3 mirroredPosition = obj.transform.position;
			mirroredPosition.x = 2 * centroid.x - mirroredPosition.x;
			obj.transform.position = mirroredPosition;

			// Handle rotation mirroring
			Quaternion mirroredRotation = obj.transform.rotation;
			mirroredRotation.y = -mirroredRotation.y;  // Mirror the Y-axis
			mirroredRotation.z = -mirroredRotation.z;  // Mirror the Z-axis
			obj.transform.rotation = mirroredRotation;

			// Check if object has a SpriteRenderer and handle flipping
			SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null) {
				// Record SpriteRenderer state for undo
				Undo.RecordObject(spriteRenderer, "Mirror Object Sprite Flip");

				// Toggle flipX for horizontal mirroring
				spriteRenderer.flipX = !spriteRenderer.flipX;
			}
		}

		// Mark the scene as dirty to indicate changes
		EditorUtility.SetDirty(Selection.activeGameObject);
	}
}