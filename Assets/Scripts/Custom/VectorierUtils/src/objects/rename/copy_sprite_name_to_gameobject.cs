using UnityEngine;
using UnityEditor;

// -=-=-=- //

public class SpriteNameToGameObjectName : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/⎁ Rename GameObjects in hierarchy/SpriteRenderer sprite name %#M")]
	private static void CopySpriteNameToGameObjectName() {
		// Get the processed GameObjects based on the selection
		GameObject[] processedGameObjects;

		// Check if there are any selected GameObjects
		if (Selection.gameObjects.Length > 0) {
			// Process each selected GameObject
			var children = new System.Collections.Generic.List<GameObject>();

			foreach (GameObject selectedObj in Selection.gameObjects) {
				// Check if the selected GameObject has children
				if (selectedObj.transform.childCount > 0) {
					// Add all children to the list
					foreach (Transform child in selectedObj.transform) {
						children.Add(child.gameObject);
					}
				} else {
					// If no children, add the selected GameObject itself
					children.Add(selectedObj);
				}
			}

			processedGameObjects = children.ToArray();
		} else {
			// If no GameObjects are selected, do nothing (or you can handle this case as needed)
			Debug.LogWarning("No GameObjects selected to process.");
			return;
		}

		// Begin recording the undo operation
		Undo.RecordObjects(processedGameObjects, "Copy Sprite Name to GameObject Name");

		foreach (GameObject obj in processedGameObjects) {
			// Check if the GameObject has a SpriteRenderer component
			SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

			// Check if the GameObject has the TriggerSettings component
			TriggerSettings triggerSettings = obj.GetComponent<TriggerSettings>();

			// If it has SpriteRenderer and not TriggerSettings
			if (spriteRenderer != null && triggerSettings == null) {
				// Set the GameObject name to the sprite name
				string spriteName = spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NoSprite";
				obj.name = spriteName;
			}
		}
	}
}