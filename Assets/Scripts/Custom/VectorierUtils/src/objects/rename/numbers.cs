using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using VectorierUtils;

// -=-=-=- //

public class RenameGameObjects : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/⎁ Rename GameObjects in hierarchy/𝑥 (ℕ) %#/")]
	static void RenameObjects() {
		// Get the root object
		GameObject root = Selection.activeGameObject;

		if (root == null) {
			Utils.AdvancedLog("Error", "No GameObject selected");
			return;
		}

		// Get only the direct child objects
		Transform[] children = root.transform.Cast<Transform>().ToArray();

		// Create a dictionary to store objects grouped by their base name
		Dictionary<string, List<GameObject>> groupedObjects = new Dictionary<string, List<GameObject>>();

		// Populate the dictionary
		foreach (Transform child in children) {
			// Skip objects that have their own children
			if (child.childCount > 0) {
				continue;
			}

			// Check if the object is part of a prefab instance
			if (PrefabUtility.IsPartOfAnyPrefab(child.gameObject)) {

				// Get the root of the prefab instance
				GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(child.gameObject);
				if (prefabRoot != null && prefabRoot.transform != root.transform) {
					continue; // Skip if the prefab root is not the selected root
				}
			}

			string baseName = child.name.Split('(')[0].Trim();

			if (!groupedObjects.ContainsKey(baseName)) {
				groupedObjects[baseName] = new List<GameObject>();
			}

			groupedObjects[baseName].Add(child.gameObject);
		}

		// Start an undo operation
		Undo.SetCurrentGroupName("Rename GameObjects");
		int undoGroupIndex = Undo.GetCurrentGroup();

		// Rename the objects within each group
		foreach (var kvp in groupedObjects) {
			List<GameObject> group = kvp.Value;

			// Sort the group based on hierarchy position
			group.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));

			// Rename the objects in the group
			for (int i = 0; i < group.Count; i++) {
				// Record the current name of the GameObject for undo
				Undo.RecordObject(group[i], "Rename GameObject");

				string baseName = kvp.Key;
				if (i == 0) {
					group[i].name = baseName;
				} else {
					group[i].name = baseName + " (" + i + ")";
				}
			}
		}

		// Close the undo group
		Undo.CollapseUndoOperations(undoGroupIndex);
	}
}