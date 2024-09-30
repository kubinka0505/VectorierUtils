using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

// -=-=-=- //

public class MissingSpritesFinder : EditorWindow {
	private string searchPath = "Assets/Resources/Textures";
	private readonly string[] validExtensions = { ".png", ".jpg", ".jpeg", ".gif" };
	private Dictionary<string, Sprite> spriteDictionary;

	[MenuItem("Vectorier/⚙ Utils/🖼️ Sprites/⇄ Missing sprites replacer (automatic) %M")]
	public static void ShowWindow() {
		MissingSpritesFinder window = GetWindow<MissingSpritesFinder>("Find Missing Sprites");
		window.minSize = new Vector2(300, 200);
		window.maxSize = new Vector2(1000, 200);
	}

	private void OnGUI() {
		GUILayout.Label("Search Path", EditorStyles.boldLabel);
		searchPath = EditorGUILayout.TextField(searchPath);

		if (GUILayout.Button("Research")) {
			FindAndAssignMissingSprites();
		}
	}

	private void FindAndAssignMissingSprites() {
		spriteDictionary = new Dictionary<string, Sprite>();

		// Collect all sprite files into the dictionary
		Debug.Log("Collecting sprites...");
		CollectSprites(searchPath);

		// Iterate through all game objects in the scene
		Debug.Log("Checking game objects in the scene...");

		foreach (GameObject go in Object.FindObjectsOfType<GameObject>()) {
			CheckAndAssignSprite(go);
		}

		// Check prefabs in the scene
		Debug.Log("Checking prefabs...");
		string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

		foreach (string prefabPath in prefabPaths) {
			string path = AssetDatabase.GUIDToAssetPath(prefabPath);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

			if (prefab != null) {
				CheckAndAssignSprite(prefab);
			}
		}

		// Mark the scene as dirty
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

		Debug.Log("Finished finding and assigning missing sprites.");
	}

	private void CollectSprites(string path) {
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);

		foreach (FileInfo file in files) {
			if (IsValidSpriteFile(file)) {
				string relativePath = "Assets" + file.FullName.Substring(Application.dataPath.Length).Replace('\\', '/');
				Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);

				if (sprite != null) {
					string strippedName = StripName(sprite.name);
					if (!spriteDictionary.ContainsKey(strippedName)) {
						spriteDictionary.Add(strippedName, sprite);
					}
				} else {
					Debug.LogWarning($"Could not load sprite at path '{relativePath}'");
				}
			}
		}
	}

	private void CheckAndAssignSprite(GameObject go) {
		SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
		if (sr != null && sr.sprite == null) {
			string strippedName = StripName(go.name);

			if (spriteDictionary.TryGetValue(strippedName, out Sprite foundSprite)) {
				sr.sprite = foundSprite;
				Debug.Log($"Assigned sprite '{foundSprite.name}' to GameObject '{go.name}'");
				// Mark the GameObject as dirty to indicate a change
				EditorUtility.SetDirty(go);
			} else {
				Debug.LogWarning($"No matching sprite found for GameObject '{go.name}' with stripped name '{strippedName}'");
			}
		}

		// Recursively check children
		foreach (Transform child in go.transform) {
			CheckAndAssignSprite(child.gameObject);
		}
	}

	private string StripName(string name) {
		int bracketIndex = name.IndexOf('(');

		if (bracketIndex > 0) {
			name = name.Substring(0, bracketIndex).Trim();
		}

		return name.Replace(" ", "").Replace("(", "").Replace(")", "");
	}

	private bool IsValidSpriteFile(FileInfo file) {
		foreach (string extension in validExtensions) {
			if (file.Extension.Equals(extension, System.StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
		}

		return false;
	}
}