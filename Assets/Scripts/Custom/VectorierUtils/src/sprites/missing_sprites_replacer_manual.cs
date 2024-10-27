using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using VectorierUtils;

// -=-=-=- //

public class SpriteReplacerEditor : EditorWindow {
	private Vector2 scrollPos;
	private List<GameObject> gameObjectsWithMissingSprites = new List<GameObject>();
	private Dictionary<GameObject, string> texturePaths = new Dictionary<GameObject, string>();
	private Dictionary<GameObject, string> initialTexturePaths = new Dictionary<GameObject, string>();
	private bool hasChanges = false;

	// -=-=-=- //

	[MenuItem("Vectorier/⚙ Utils/🖼️ Sprites/⇄ Missing sprites replacer (manual) %&M")]
	public static void ShowWindow() {
		var window = CreateInstance<SpriteReplacerEditor>();
		window.titleContent = new GUIContent("VectorierUtils - Missing Sprites Replacer");

		if (window.Initialize()) {
			window.Show();
		} else {
			Utils.AdvancedLog(msg: "No missing sprites have been found.");
			DestroyImmediate(window);
		}
	}

	private bool Initialize() {
		FindObjectsWithMissingSprites();
		return gameObjectsWithMissingSprites.Count > 0;
	}

	private void OnEnable() {
		if (gameObjectsWithMissingSprites == null || gameObjectsWithMissingSprites.Count == 0) {
			try {
				Close();
			} catch (System.NullReferenceException) {
				// pass
			}
		}
	}

	private void OnGUI() {
		if (gameObjectsWithMissingSprites == null || gameObjectsWithMissingSprites.Count == 0) {
			try {
				Close();
			} catch (System.NullReferenceException) {
				// pass
			}

			return;
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));

		// -=-=-=- //

		foreach (var go in gameObjectsWithMissingSprites) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(go.name, GUILayout.Width(150));
			Rect textFieldRect = EditorGUILayout.GetControlRect(GUILayout.Width(400));

			string newPath = EditorGUI.TextField(
				textFieldRect,
				texturePaths.ContainsKey(go) ? texturePaths[go] : ""
			);

			// -=-=-=- //

			if (textFieldRect.Contains(Event.current.mousePosition)) {
				if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences[0] is Texture2D) {
					if (Event.current.type == EventType.DragUpdated) {
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
					} else if (Event.current.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag();
						newPath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
						Event.current.Use();
					}
				}
			}

			if (texturePaths.ContainsKey(go) && texturePaths[go] != newPath) {
				texturePaths[go] = newPath;
				hasChanges = true;
			} else if (!texturePaths.ContainsKey(go) && !string.IsNullOrEmpty(newPath)) {
				texturePaths[go] = newPath;
				hasChanges = true;
			}

			EditorGUILayout.EndHorizontal();
		}

		// -=-=-=- //

		EditorGUILayout.EndScrollView();

		GUI.enabled = hasChanges;
		if (GUILayout.Button("Append and update")) {
			AppendAndUpdate();
		}
		GUI.enabled = true;
	}

	private void FindObjectsWithMissingSprites() {
		gameObjectsWithMissingSprites.Clear();
		texturePaths.Clear();
		initialTexturePaths.Clear();
		
		var allObjects = FindObjectsOfType<GameObject>();

		// -=-=-=- //

		foreach (var go in allObjects) {
			var spriteRenderer = go.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null && spriteRenderer.sprite == null) {
				gameObjectsWithMissingSprites.Add(go);

				// Initialize texture paths
				texturePaths[go] = string.Empty;

				// Track initial state
				initialTexturePaths[go] = string.Empty;
			}
		}

		gameObjectsWithMissingSprites.Sort((a, b) => a.name.CompareTo(b.name));
	}

	private void AppendAndUpdate() {
		List<string> invalidPaths = new List<string>();

		// Update sprites for valid paths
		foreach (var kvp in texturePaths) {
			var go = kvp.Key;
			var path = kvp.Value;

			if (string.IsNullOrEmpty(path)) {
				// Skip empty paths
				continue;
			}

			if (File.Exists(path)) {
				var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
				if (sprite != null) {
					var spriteRenderer = go.GetComponent<SpriteRenderer>();
					spriteRenderer.sprite = sprite;

					// Mark the editor as dirty
					EditorUtility.SetDirty(spriteRenderer);
				}
			} else {
				invalidPaths.Add($"{go.name} \"{path}\" cannot be accessed.");
			}
		}

		// Second phase: report non-existing paths
		if (invalidPaths.Count > 0) {
			foreach (var errorMessage in invalidPaths) {
				// Log each error message individually
				Debug.LogError(errorMessage);
			}

			EditorUtility.DisplayDialog("Error", string.Join("\n", invalidPaths), "OK");
		}

		FindObjectsWithMissingSprites();
		Repaint();

		if (gameObjectsWithMissingSprites.Count == 0) {
			Close();
		}

		// Reset change tracking after update
		hasChanges = false;
	}
}
