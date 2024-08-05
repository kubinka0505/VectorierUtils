using UnityEngine;
using UnityEditor;

// -=-=-=- //

public class MirrorSelectedObjects : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/⧎ Mirror selected objects", false, 3)]
	static void MirrorObjects() {
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0) return;

		// Calculate the centroid of the selected objects
		Vector3 centroid = Vector3.zero;
		foreach (GameObject obj in selectedObjects) {
			centroid += obj.transform.position;
		}
		centroid /= selectedObjects.Length;

		// --- //

		// Mirror each object around the centroid
		foreach (GameObject obj in selectedObjects) {
			Vector3 mirroredPosition = obj.transform.position;
			mirroredPosition.x = 2 * centroid.x - mirroredPosition.x;
			obj.transform.position = mirroredPosition;
			
			// flip the object itself if necessary
			Vector3 scale = obj.transform.localScale;
			scale.x *= -1;
			obj.transform.localScale = scale;
		}
	}
}
