using UnityEditor;
using UnityEngine;
using VectorierUtils;

// -=-=-=- //

public class ChangePivot : MonoBehaviour {
	// Left
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◰ Left Top", false, 100)]
	static void change_pivot_left_top() { Change_Sprite_Pivot(0.0f, 1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◧ Left Center", false, 101)]
	static void change_pivot_left_center() { Change_Sprite_Pivot(0.0f, 0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◱ Left Bottom", false, 102)]
	static void change_pivot_left_bottom() { Change_Sprite_Pivot(0.0f, 0.0f); }

	// Center //
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◓ Center Top", false, 103)]
	static void change_pivot_center_top() { Change_Sprite_Pivot(0.5f, 1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/▣ Center", false, 104)]
	static void change_pivot_center_center() { Change_Sprite_Pivot(0.5f, 0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◒ Center Bottom", false, 105)]
	static void change_pivot_center_bottom() { Change_Sprite_Pivot(0.5f, 0.0f); }

	// Right //
	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◳ Right Top", false, 106)]
	static void change_pivot_right_top() { Change_Sprite_Pivot(1.0f, 1.0f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◨ Right Center", false, 107)]
	static void change_pivot_right_center() { Change_Sprite_Pivot(1.0f, 0.5f); }

	[MenuItem("Vectorier/⚙ Utils/✥ Objects positioning/⛋ Change sprites pivot points (deprecated)/◲ Right Bottom", false, 108)]
	static void change_pivot_right_bottom() { Change_Sprite_Pivot(1.0f, 0.0f); }

	// Main //
	static void Change_Sprite_Pivot(float xpos, float ypos) {
		GameObject[] selectedObjects = Selection.gameObjects;

		foreach (GameObject selectedObject in selectedObjects) {
			SpriteRenderer spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();

			if (spriteRenderer != null && spriteRenderer.sprite != null) {
				// Register the current state for undo
				Undo.RecordObject(selectedObject.transform, "Change Sprite Pivot");
				Undo.RecordObject(spriteRenderer, "Change Sprite Pivot");

				// Get the original sprite
				Sprite originalSprite = spriteRenderer.sprite;

				// Calculate the delta position based on the original pivot and new pivot
				Vector2 originalPivot = originalSprite.pivot / originalSprite.rect.size;
				Vector2 deltaPosition = (new Vector2(xpos, ypos) - originalPivot) * originalSprite.rect.size / originalSprite.pixelsPerUnit;

				// Adjust the position of the object
				selectedObject.transform.position += new Vector3(deltaPosition.x, deltaPosition.y, 0);

				// Create a new sprite with the new pivot
				Sprite newSprite = Sprite.Create(
					originalSprite.texture,
					originalSprite.rect,
					new Vector2(xpos, ypos),
					originalSprite.pixelsPerUnit,
					0,
					SpriteMeshType.Tight,
					originalSprite.border
				);

				spriteRenderer.sprite = newSprite;

				// Mark the new sprite as dirty so that the change is saved
				EditorUtility.SetDirty(spriteRenderer);
			} else {
				Utils.AdvancedLog("Error", $"No SpriteRenderer or Sprite found on {selectedObject.name}.");
				return;
			}
		}
	}
}