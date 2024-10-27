using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using VectorierUtils;

// -=-=-=- //

public class PrefabReplacerEditor : EditorWindow {
	private Vector2 scrollPos;
	private List<GameObject> gameObjectsWithMissingPrefabs = new List<GameObject>();
	private Dictionary<GameObject, string> prefabPaths = new Dictionary<GameObject, string>();
	private Dictionary<GameObject, string> initialPrefabPaths = new Dictionary<GameObject, string>();
	private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
	private bool hasChanges = false;

	// -=-=-=- //

	[MenuItem("Vectorier/⚙ Utils/❐ Prefabs/⇄ Missing prefabs replacer")]
	public static void ShowWindow() {
		var window = CreateInstance<PrefabReplacerEditor>();
		window.titleContent = new GUIContent("VectorierUtils - Missing prefabs replacer");
		if (window.Initialize()) {
			window.Show();
		} else {
			Utils.AdvancedLog(msg: "No missing prefabs have been found.");
			DestroyImmediate(window);
		}
	}

	private bool Initialize() {
		FindObjectsWithMissingPrefabs();
		return gameObjectsWithMissingPrefabs.Count > 0;
	}

	private void OnEnable() {
		if (gameObjectsWithMissingPrefabs == null || gameObjectsWithMissingPrefabs.Count == 0) {
			try {
				Close();
			} catch (System.NullReferenceException) {
				// pass
			}
		}
	}

	private void OnGUI() {
		if (gameObjectsWithMissingPrefabs == null || gameObjectsWithMissingPrefabs.Count == 0) {
			try {
				Close();
			} catch (System.NullReferenceException) {
				// pass
			}

			return;
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));

		// -=-=-=- //

		foreach (var go in gameObjectsWithMissingPrefabs) {
			EditorGUILayout.BeginHorizontal();

			// Extract the actual name without the "(Missing Prefab with guid: ...)" part
			string goName = ExtractName(go.name);
			EditorGUILayout.LabelField(goName, GUILayout.Width(300));
			Rect textFieldRect = EditorGUILayout.GetControlRect(GUILayout.Width(400));

			string newPath = EditorGUI.TextField(
				textFieldRect,
				prefabPaths.ContainsKey(go) ? prefabPaths[go] : ""
			);

			// -=-=-=- //

			if (textFieldRect.Contains(Event.current.mousePosition)) {
				if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences[0] is GameObject) {
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

			if (prefabPaths.ContainsKey(go) && prefabPaths[go] != newPath) {
				prefabPaths[go] = newPath;
				hasChanges = true;
			} else if (!prefabPaths.ContainsKey(go) && !string.IsNullOrEmpty(newPath)) {
				prefabPaths[go] = newPath;
				hasChanges = true;
			}

			EditorGUILayout.EndHorizontal();
		}

		// -=-=-=- //

		EditorGUILayout.EndScrollView();

		GUI.enabled = hasChanges;
		if (GUILayout.Button("Append and Update")) {
			AppendAndUpdate();
		}
		GUI.enabled = true;
	}

	private void FindObjectsWithMissingPrefabs() {
		gameObjectsWithMissingPrefabs.Clear();
		prefabPaths.Clear();
		initialPrefabPaths.Clear();
		originalPositions.Clear();
		
		var allObjects = FindObjectsOfType<GameObject>();

		// -=-=-=- //

		foreach (var go in allObjects) {
			var prefabType = PrefabUtility.GetPrefabAssetType(go);
			if (prefabType == PrefabAssetType.MissingAsset) {
				gameObjectsWithMissingPrefabs.Add(go);

				// Initialize prefab paths
				prefabPaths[go] = string.Empty;

				// Track initial state
				initialPrefabPaths[go] = string.Empty;

				// Store original position
				originalPositions[go] = go.transform.position;
			}
		}

		gameObjectsWithMissingPrefabs.Sort((a, b) => a.name.CompareTo(b.name));
	}

	private void AppendAndUpdate() {
		List<string> invalidPaths = new List<string>();

		// Update prefabs for valid paths
		foreach (var kvp in prefabPaths) {
			var go = kvp.Key;
			var path = kvp.Value;

			if (string.IsNullOrEmpty(path)) {
				// Skip empty paths
				continue;
			}

			if (File.Exists(path)) {
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if (prefab != null) {
					var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					if (instance != null) {
						instance.transform.SetParent(go.transform.parent);
						instance.transform.SetSiblingIndex(go.transform.GetSiblingIndex());

						// Set the position to the stored position
						if (originalPositions.ContainsKey(go)) {
							instance.transform.position = originalPositions[go];
						}

						Undo.RegisterCreatedObjectUndo(instance, "Replace Missing Prefab");
						Undo.DestroyObjectImmediate(go);
					}

					// Mark the editor as dirty
					EditorUtility.SetDirty(instance);
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

		FindObjectsWithMissingPrefabs();
		Repaint();

		if (gameObjectsWithMissingPrefabs.Count == 0) {
			Close();
		}

		// Reset change tracking after update
		hasChanges = false;
	}

	private string ExtractName(string originalName) {
		int index = originalName.IndexOf(" (Missing Prefab with guid: ");
		if (index > 0) {
			return originalName.Substring(0, index);
		}
		return originalName;
	}
}
