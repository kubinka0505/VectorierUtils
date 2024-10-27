using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using VectorierUtils;

// -=-=-=- //

public class DistributeObjects : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/▦ Align/⁚ Evenly/Horizontally %#&6", false, 300)]
	public static void distribute_evenly_horizontally() {
		Distribute(Selection.gameObjects.ToList(), DistributionType.Evenly, true);
	}

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/▦ Align/⁚ Evenly/Vertically %#&7", false, 301)]
	public static void distribute_evenly_vertically() {
		Distribute(Selection.gameObjects.ToList(), DistributionType.Evenly, false);
	}

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/▦ Align/⋮ Oddly/Horizontally %#&8", false, 302)]
	public static void distribute_oddly_horizontally() {
		Distribute(Selection.gameObjects.ToList(), DistributionType.Oddly, true);
	}

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/▦ Align/⋮ Oddly/Vertically %#&9", false, 303)]
	public static void distribute_oddly_vertically() {
		Distribute(Selection.gameObjects.ToList(), DistributionType.Oddly, false);
	}

	public enum DistributionType { Evenly, Oddly };

	public static void Distribute(List<GameObject> objects, DistributionType distributionType, bool horizontal) {
		if (objects == null || objects.Count == 0) {
			Utils.AdvancedLog("Error", "No objects to distribute.");
			return;
		}

		// Filter out objects without any Renderer component in them or their children
		objects = objects.Where(obj => GetRendererBounds(obj) != null).ToList();

		if (objects.Count == 0) {
			Utils.AdvancedLog("Error", "No objects with SpriteRenderer component has been found!");
			return;
		}

		// Sort objects based on their current position
		objects.Sort((a, b) => horizontal ? a.transform.position.x.CompareTo(b.transform.position.x) : a.transform.position.y.CompareTo(b.transform.position.y));

		// Get the total width or height of all objects combined
		float totalSize = 0f;
		foreach (GameObject obj in objects) {
			var bounds = GetRendererBounds(obj);
			if (horizontal) {
				totalSize += bounds.size.x;
			} else {
				totalSize += bounds.size.y;
			}
		}

		// Calculate spacing based on distribution type
		float totalSpacing = 0f;
		if (distributionType == DistributionType.Evenly) {
			totalSpacing = (horizontal ? objects[objects.Count - 1].transform.position.x - objects[0].transform.position.x : objects[objects.Count - 1].transform.position.y - objects[0].transform.position.y) - totalSize;
		} else if (distributionType == DistributionType.Oddly) {
			totalSpacing = (horizontal ? objects[objects.Count - 1].transform.position.x - objects[0].transform.position.x : objects[objects.Count - 1].transform.position.y - objects[0].transform.position.y);
		}

		// Calculate starting position
		Vector3 startPos = objects[0].transform.position;

		// Align objects
		float currentPos = horizontal ? startPos.x : startPos.y;
		for (int i = 0; i < objects.Count; i++) {
			var bounds = GetRendererBounds(objects[i]);
			Vector3 newPos = objects[i].transform.position;
			if (horizontal) {
				newPos.x = currentPos + bounds.size.x / 2;
			} else {
				newPos.y = currentPos + bounds.size.y / 2;
			}

			objects[i].transform.position = newPos;
			currentPos += (horizontal ? bounds.size.x : bounds.size.y) + (distributionType == DistributionType.Evenly ? totalSpacing / (objects.Count - 1) : 0f);
		}
	}

	private static Bounds GetRendererBounds(GameObject obj) {
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer != null) {
			return renderer.bounds;
		}

		Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
		if (childRenderers.Length > 0) {
			Bounds combinedBounds = childRenderers[0].bounds;

			for (int i = 1; i < childRenderers.Length; i++) {
				combinedBounds.Encapsulate(childRenderers[i].bounds);
			}

			return combinedBounds;
		}
		return default(Bounds);
	}
}
