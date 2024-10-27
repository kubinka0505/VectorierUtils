using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

// -=-=-=- //

public class UnpackAllPrefabsInScene {
	[MenuItem("Vectorier/⚙ Utils/❐ Prefabs/⟰ Unpack all")]
	private static void UnpackAllPrefabs() {
		Scene activeScene = SceneManager.GetActiveScene();

		// Find all root game objects in the scene
		GameObject[] rootGameObjects = activeScene.GetRootGameObjects();

		// Iterate through all root game objects and their children
		foreach (GameObject rootGameObject in rootGameObjects) {
			UnpackPrefabInChildren(rootGameObject);
		}

		// Mark the scene as dirty to enable saving
		EditorSceneManager.MarkSceneDirty(activeScene);
	}

	private static void UnpackPrefabInChildren(GameObject gameObject) {
		// Unpack the prefab if it is a prefab instance
		PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
		if (prefabInstanceStatus == PrefabInstanceStatus.Connected || prefabInstanceStatus == PrefabInstanceStatus.Disconnected) {
			// Record the undo operation for this game object
			Undo.RegisterCompleteObjectUndo(gameObject, "Unpack Prefab");
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
		}

		// Recursively unpack prefabs in children
		foreach (Transform child in gameObject.transform) {
			UnpackPrefabInChildren(child.gameObject);
		}
	}
}