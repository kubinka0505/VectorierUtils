using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// -=-=-=- //

public class SortGameObjectsByPosition : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🗺 Position/↔ X (Horizontal)/Left to Right %#,")]
	public static void sort_child_objects_pos_x() { SortChildGameObjects(true, false); }

	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🗺 Position/↔ X (Horizontal)/Right to Left %#&,")]
	public static void sort_child_objects_pos_x_inv() { SortChildGameObjects(true, true); }

	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🗺 Position/↕ Y (Vertical)/Top to Bottom %#.")]
	public static void sort_child_objects_by_pos_y_inv() { SortChildGameObjects(false, true); }

	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🗺 Position/↕ Y (Vertical)/Bottom to Top %#&.")]
	public static void sort_child_objects_by_pos_y() { SortChildGameObjects(false, false); }

	// --- //

	private static void SortChildGameObjects(bool sortByX, bool inverted) {
		GameObject parent = Selection.activeGameObject;
		if (parent == null) {
			string message = "No GameObject selected. Please select a GameObject in the hierarchy.";

			Debug.LogWarning(message);
			EditorUtility.DisplayDialog("Warning", message, "OK");

			return;
		}

		// --- //

		List<Transform> children = new List<Transform>();
		foreach (Transform child in parent.transform) {
			children.Add(child);
		}

		if (children.Count == 0) {
			string message = "Selected GameObject has no children.";

			Debug.LogWarning(message);
			EditorUtility.DisplayDialog("Warning", message, "OK");

			return;
		}

		// Find the reference GameObject (leftmost or topmost) //
		Transform referenceObject = children[0];
		float referenceValue = sortByX ? GetLowerLeftX(referenceObject) : GetUpperLeftY(referenceObject);

		foreach (Transform child in children) {
			float childValue = sortByX ? GetLowerLeftX(child) : GetUpperLeftY(child);

			if (childValue < referenceValue) {
				referenceObject = child;
				referenceValue = childValue;
			}
		}

		// Set the scanning start position //
		float startValue = referenceValue - 1;

		// Sort GameObjects based on position //
		children.Sort((a, b) => {
			float aValue = sortByX ? GetLowerLeftX(a) : GetUpperLeftY(a);
			float bValue = sortByX ? GetLowerLeftX(b) : GetUpperLeftY(b);

			return aValue.CompareTo(bValue);
		});

		if (inverted) {
			children.Reverse();
		}

		for (int i = 0; i < children.Count; i++) {
			if ((sortByX && GetLowerLeftX(children[i]) >= startValue) ||
				(!sortByX && GetUpperLeftY(children[i]) >= startValue)) {
				children[i].SetSiblingIndex(i);
			}
		}
	}

	// --- //

	private static float GetLowerLeftX(Transform transform) {
		Renderer renderer = transform.GetComponent<Renderer>();

		if (renderer != null) {
			return renderer.bounds.min.x;
		}

		RectTransform rectTransform = transform.GetComponent<RectTransform>();
		if (rectTransform != null) {
			return rectTransform.position.x - rectTransform.rect.width / 2;
		}

		// Default to transform position if no specific component is found //
		return transform.position.x;
	}

	// --- //

	private static float GetUpperLeftY(Transform transform)
	{
		Renderer renderer = transform.GetComponent<Renderer>();
		if (renderer != null) {
			return renderer.bounds.max.y;
		}

		RectTransform rectTransform = transform.GetComponent<RectTransform>();
		if (rectTransform != null) {
			return rectTransform.position.y + rectTransform.rect.height / 2;
		}

		// Default to transform position if no specific component is found //
		return transform.position.y;
	}
}
