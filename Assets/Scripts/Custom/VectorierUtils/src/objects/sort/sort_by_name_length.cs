using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using VectorierUtils;

// -=-=-=- //

public class SortGameObjectsByLength : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/⎁ Name Length/Normal")]
	public static void sort_by_length_normal() { Sort_By_Length(false); }

	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/⎁ Name Length/Reversed")]
	public static void sort_by_length_reversed() { Sort_By_Length(true); }

	// -=-=-=-
	// Main //

	private static void Sort_By_Length(bool reversed) {
		GameObject parentObject = Selection.activeGameObject;

		if (parentObject == null) {
			Utils.AdvancedLog("Warning", "No parent GameObject in a hierarchy has been selected.");
			return;
		}

		List<Transform> children = new List<Transform>();

		// --- //

		foreach (Transform child in parentObject.transform) {
			children.Add(child);
		}

		if (reversed) {
			children = children.OrderByDescending(child => child.name.Length).ToList();
		} else {
			children = children.OrderBy(child => child.name.Length).ToList();
		}

		for (int i = 0; i < children.Count; i++) {
			children[i].SetSiblingIndex(i);
		}
	}
}
