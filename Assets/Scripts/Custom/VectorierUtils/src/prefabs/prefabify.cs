using System.IO;
using UnityEngine;
using UnityEditor;
using VectorierUtils;

// -=-=-=- //

public class PrefabExporter : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/❐ Prefabs/✨ Consolidate selected GameObjects to .prefab", false, 9)]
	public static void PrefabifyManual() {
		ConvertToPrefab(true);
	}

	[MenuItem("Vectorier/⚙ Utils/❐ Prefabs/✨ Consolidate selected GameObjects to .prefab (fast)", false, 9)]
	public static void PrefabifyAutomatic() {
		ConvertToPrefab(false);
	}

	// -=-=-=- //

	public static void ConvertToPrefab(bool prompt) {
		if (Selection.gameObjects.Length == 0) {
			Utils.AdvancedLog("Warning", "No GameObjects to prefabify have been selected.");
			return;
		}

		// Setup
		string separator = Path.DirectorySeparatorChar.ToString();
		string randomNumber = UnityEngine.Random.Range(10000, 99999).ToString();
		string targetFilePath = $"_NewPrefab_{randomNumber}.prefab";
		string prefabPathRelative = targetFilePath;
		string resourcesPath = Path.Combine("Assets", "Resources");

		// Ask user where to save the prefab
		if (prompt) {
			prefabPathRelative = EditorUtility.SaveFilePanelInProject(
				"Save file",
				Path.GetFileNameWithoutExtension(targetFilePath),
				Path.GetExtension(targetFilePath),
				"Please enter a file name to save",
				resourcesPath
			);
		} else {
			prefabPathRelative = Path.Combine(resourcesPath, prefabPathRelative);
		}

		string prefabPathFull = Path.Combine(
			Path.GetDirectoryName(Application.dataPath),
			prefabPathRelative
		);
		prefabPathFull = prefabPathFull.Replace("/", separator);

		if (!string.IsNullOrEmpty(prefabPathRelative)) {
			float minX = float.MaxValue;
			float minY = float.MaxValue;

			foreach (var obj in Selection.gameObjects) {
				Vector3 pos = obj.transform.position;

				if (pos.x < minX) {
					minX = pos.x;
				};

				if (pos.y < minY) {
					minY = pos.y;
				}
			}

			GameObject root = new GameObject("TempRoot");

			foreach (var obj in Selection.gameObjects) {
				GameObject objCopy = Instantiate(obj);

				objCopy.transform.SetParent(root.transform);

				// Adjust the position relative to the minimum X and Y
				objCopy.transform.position = new Vector3(
					// Shift X so that minX becomes 0
					obj.transform.position.x - minX,

					// Shift Y so that minY becomes 0
					obj.transform.position.y - minY,

					// Z remains unchanged
					obj.transform.position.z
				);

				objCopy.transform.localRotation = obj.transform.rotation;
				objCopy.transform.localScale = obj.transform.lossyScale;
			}

			// Move the entire root to (0, 0, 0)
			root.transform.position = Vector3.zero;
			root.transform.rotation = Quaternion.identity;

			PrefabUtility.SaveAsPrefabAsset(root, prefabPathRelative);

			// Clean up the temporary root object
			DestroyImmediate(root);

			Debug.Log($"Prefab saved to \"{prefabPathFull}\"");
		}
	}
}