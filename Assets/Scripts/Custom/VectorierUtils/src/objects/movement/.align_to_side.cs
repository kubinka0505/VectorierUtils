using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class MoveObjectsToPos : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⤒  Align ⩾ 2 to top most edge", false, 200)]
	static void MoveToTop() { MoveObjects(Vector2.up); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⇤ Align ⩾ 2 to left most edge", false, 201)]
	static void MoveToLeft() { MoveObjects(Vector2.left); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⇥ Align ⩾ 2 to right most edge", false, 202)]
	static void MoveToRight() { MoveObjects(Vector2.right); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⤓  Align ⩾ 2 to most bottom edge", false, 203)]
	static void MoveToBottom() { MoveObjects(Vector2.down); }

	// -=-=-=- //

	static void MoveObjects(Vector2 direction) {
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0) {
			Utils.AdvancedLog("Error", "No GameObjects have been selected.");
			return;
		}

		Vector3 referencePosition = selectedObjects[0].transform.position;

		foreach (GameObject obj in selectedObjects) {
			if (direction == Vector2.up && obj.transform.position.y > referencePosition.y) {
				referencePosition = obj.transform.position;
			} else if (direction == Vector2.down && obj.transform.position.y < referencePosition.y) {
				referencePosition = obj.transform.position;
			} else if (direction == Vector2.left && obj.transform.position.x < referencePosition.x) {
				referencePosition = obj.transform.position;
			} else if (direction == Vector2.right && obj.transform.position.x > referencePosition.x) {
				referencePosition = obj.transform.position;
			}
		}

		foreach (GameObject obj in selectedObjects) {
			Vector3 newPosition = obj.transform.position;

			if (direction == Vector2.up || direction == Vector2.down) {
				newPosition.y = referencePosition.y;
			} else if (direction == Vector2.left || direction == Vector2.right) {
				newPosition.x = referencePosition.x;
			}

			Undo.RecordObject(obj.transform, "Move Objects");
			obj.transform.position = newPosition;
		}
	}
}
