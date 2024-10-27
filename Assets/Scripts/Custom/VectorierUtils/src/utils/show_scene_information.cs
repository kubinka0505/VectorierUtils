using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using VectorierUtils;

using Debug = UnityEngine.Debug;

// -=-=-=- //

public class SceneInfoLogger : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/ⓘ Show scene Information #`", false, 9999)]
	static void LogSceneInformation() {
		// Scene name
		string sceneName = SceneManager.GetActiveScene().name;
		string scenePath = SceneManager.GetActiveScene().path;
		string sceneMeta = scenePath + ".meta";
		string sceneFirstFile = sceneMeta; // scenePath

		// Objects
		GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects()
			.SelectMany(go => go.GetComponentsInChildren<Transform>(true))
			.Select(t => t.gameObject)
			.ToArray();

		int objectCount2D = allObjects.Count(go => go.GetComponent<SpriteRenderer>() != null);
		int objectCount3D = allObjects.Count(go => go.GetComponent<MeshRenderer>() != null);

		int objectsWithParents = allObjects.Count(go => go.transform.parent != null);

		// Scene size
		Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach (GameObject go in allObjects) {
			Renderer renderer = go.GetComponent<Renderer>();
			if (renderer != null) {
				sceneBounds.Encapsulate(renderer.bounds);
			}
		}

		Vector3 sceneSize = sceneBounds.size;
		Vector3 lowerLeft = sceneBounds.min;
		Vector3 upperRight = sceneBounds.max;

		// File Size
		long sceneFileSize = GetFileSize(scenePath);

		// File creation (unreliable)
		TimeSpan timeSinceCreation = GetTimeSinceCreation(sceneFirstFile);

		// Unique Objects
		var uniqueObjects = allObjects.GroupBy(go => go.name).Select(g => g.First()).ToArray();
		int uniqueObjectCount = uniqueObjects.Length;

		// Unique Textures (SpriteRenderer)
		HashSet<string> uniqueSpriteTextures = new HashSet<string>();

		foreach (GameObject go in allObjects) {
			SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();

			if (spriteRenderer != null && spriteRenderer.sprite != null) {
				string spritePath = AssetDatabase.GetAssetPath(spriteRenderer.sprite.texture);
				uniqueSpriteTextures.Add(spritePath);
			}
		}
		int uniqueSpriteTextureCount = uniqueSpriteTextures.Count;

		// Scene information string
		string sceneInfo = $"Information\n" +
						$"  Name\t\t\t{sceneName}\n" +
						$"  Scene file size\t\t{FormatFileSize(sceneFileSize)}\n" +
						$"  Time since creation\t{FormatTimeSpan(timeSinceCreation)}\n" +
						$"\n" +
						$"  Unique sprites\t\t{uniqueSpriteTextureCount}\n" +
						$"\n" +
						$"—————————————————————————\n" +
						$"\n" +
						$"  Objects\n" +
						$"    All\t\t\t{allObjects.Length}\n" +
						$"    Unique\t\t{uniqueObjectCount}\n" +
						$"    With parents\t\t{objectsWithParents}\n" +
						$"\n" +
						$"    2D\t\t\t{objectCount2D}\n" +
						$"    3D\t\t\t{objectCount3D}\n" +
						$"\n" +
						$"—————————————————————————\n" +
						$"\n" +
						$"  Scene dimensions\n" +
						$"    Size\t\t\t{sceneSize}\n" +
						$"    Lower Left\t\t{lowerLeft}\n" +
						$"    Upper Right\t\t{upperRight}\n";


		// Show custom editor window
		SceneInfoWindow.ShowWindow(sceneName, sceneInfo);
	}

	static long GetFileSize(string filePath) {
		if (File.Exists(filePath)) {
			FileInfo fileInfo = new FileInfo(filePath);
			return fileInfo.Length;
		}

		return 0;
	}

	static string FormatFileSize(long fileSize) {
		if (fileSize < 1024) {
			return fileSize + " B";
		}

		if (fileSize < 1024 * 1024) {
			return (fileSize / 1024.0).ToString("F2") + " KB";
		}

		return (fileSize / (1024.0 * 1024.0)).ToString("F2") + " MB";
	}

	static TimeSpan GetTimeSinceCreation(string filePath) {
		if (File.Exists(filePath.ToString())) {
			FileInfo fileInfo = new FileInfo(filePath);
			var time = fileInfo.CreationTime < fileInfo.LastWriteTime ? fileInfo.CreationTime : fileInfo.LastWriteTime;
			return DateTime.Now - time;
		}

		return TimeSpan.Zero;
	}

	static string FormatTimeSpan(TimeSpan timeSpan) {
		int totalDays = Math.Abs(timeSpan.Days);
		int totalHours = Math.Abs(timeSpan.Hours);
		int totalMinutes = Math.Abs(timeSpan.Minutes);
		int totalSeconds = Math.Abs(timeSpan.Seconds);

		return $"{totalDays}d {totalHours}h {totalMinutes}m {totalSeconds}s";
	}
}

// -=-=-=- //

public class SceneInfoWindow : EditorWindow {
	private static string sceneName;
	private static string sceneInfo;
	private static Texture2D logoTexture;
	private const float PaddingPercentage = 0.0325f;

	public static void ShowWindow(string name, string info) {
		sceneName = name;
		sceneInfo = info;
		SceneInfoWindow window = GetWindow<SceneInfoWindow>("VectorierUtils - Scene Info");
		window.minSize = new Vector2(325, 440);
		window.maxSize = window.minSize;
		window.Show();
	}

	private static string GetScriptPath() {
		StackTrace stackTrace = new StackTrace(true);

		// Find the frame containing this method
		StackFrame frame = null;
		for (int i = 0; i < stackTrace.FrameCount; i++) {
			if (stackTrace.GetFrame(i).GetMethod() == MethodBase.GetCurrentMethod()) {
				// Get the frame where this method was called
				frame = stackTrace.GetFrame(i + 1);
				break;
			}
		}

		return frame.GetFileName();
	}

	private void OnEnable() {
		// Get the script path
		string scriptPath = GetScriptPath();
		string scriptDirectory = Path.GetDirectoryName(scriptPath);
		
		// Navigate up two directory levels
		string parentDirectory = Directory.GetParent(scriptDirectory).FullName;
		
		// Combine the path with the relative path to the image file
		string logoPath = VectorierUtils.String.GetRelativePath(
			Application.dataPath,
			Path.Combine(parentDirectory, "..", "img", "logo.png")
		);

		// Load the logo texture
		logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath);

		// Log if the logo texture was not found
		if (logoTexture == null) {
			Debug.LogWarning($"Logo texture not found at path: {logoPath}");
		}
	}

	private void OnGUI() {
		float padding = position.width * PaddingPercentage;
		float availableWidth = position.width - 2 * padding;
		float verticalPadding = position.height * PaddingPercentage;

		// Display the logo image centered if it is loaded
		if (logoTexture != null) {
			float aspectRatio = (float)logoTexture.height / logoTexture.width;
			float imageHeight = availableWidth * aspectRatio;

			GUILayout.BeginHorizontal();
			GUILayout.Space(padding * 2);

			GUILayout.BeginVertical();
			GUILayout.Space(verticalPadding + 12);
			GUILayout.Label(logoTexture, GUILayout.Width(availableWidth), GUILayout.Height(imageHeight));
			GUILayout.Space(verticalPadding);

			GUILayout.EndVertical();
			GUILayout.Space(padding);
			GUILayout.EndHorizontal();
		}

		// Scene information
		GUILayout.Label(sceneInfo, EditorStyles.wordWrappedLabel);

		// Copy button
		if (GUILayout.Button("Copy")) {
			EditorGUIUtility.systemCopyBuffer = sceneInfo;
		}
	}
}
