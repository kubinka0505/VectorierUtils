using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using VectorierUtils;

// -=-=-=- //

public class SelectSpriteRendererFlips : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/⌖ Select flipped GameObjects/▙ Only X", false, 1)]
	static void SelectAllSpriteRendererFlipTypeX() { SelectAllSpriteRendererFlipType(0); }

	[MenuItem("Vectorier/⚙ Utils/⌖ Select flipped GameObjects/▜ Only Y", false, 2)]
	static void SelectAllSpriteRendererFlipTypeY() { SelectAllSpriteRendererFlipType(1); }

	[MenuItem("Vectorier/⚙ Utils/⌖ Select flipped GameObjects/▞ Both X ∕ Y", false, 3)]
	static void SelectAllSpriteRendererFlipTypeXY() { SelectAllSpriteRendererFlipType(2); }

	static void SelectAllSpriteRendererFlipType(int selectType) {
		SpriteRenderer[] allSpriteRenderers = GameObject.FindObjectsOfType<SpriteRenderer>();
		Dictionary<int, GameObject> selectedObjectsDict = new Dictionary<int, GameObject>();
		int id = 0;
		string flipType;

		if (selectType == 0) {
			selectedObjectsDict = allSpriteRenderers
				.Where(sr => sr.flipX && !sr.flipY)
				.ToDictionary(sr => id++, sr => sr.gameObject);
			flipType = "x";
		} else if (selectType == 1) {
			selectedObjectsDict = allSpriteRenderers
				.Where(sr => sr.flipY && !sr.flipX)
				.ToDictionary(sr => id++, sr => sr.gameObject);
			flipType = "y";
		} else {
			selectedObjectsDict = allSpriteRenderers
				.Where(sr => sr.flipX && sr.flipY)
				.ToDictionary(sr => id++, sr => sr.gameObject);
			flipType = "x/y";
		}

		if (selectedObjectsDict.Count > 0) {
			SelectSpriteRendererFlipsWindow.ShowWindow(selectedObjectsDict);
		} else {
			Utils.AdvancedLog(msg: $"No GameObjects with targeted SpriteRenderer flip {flipType.ToUpper()} checked found.");
		}
	}
}

public class SelectSpriteRendererFlipsWindow : EditorWindow {
	private Dictionary<int, GameObject> objectsDict;
	private Vector2 scrollPos;

	public static void ShowWindow(Dictionary<int, GameObject> objectsDict) {
		if (objectsDict != null && objectsDict.Count > 0) {
			SelectSpriteRendererFlipsWindow window = GetWindow<SelectSpriteRendererFlipsWindow>("Select SpriteRenderer Flips");
			window.objectsDict = objectsDict;
			window.Show();
		}
	}

	private void OnGUI() {
		if (objectsDict == null || objectsDict.Count == 0) {
			EditorGUILayout.LabelField("No objects to display.");
			return;
		}

		EditorGUILayout.LabelField("Select the GameObject from the list below:");
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		foreach (var item in objectsDict) {
			if (GUILayout.Button(item.Value.name)) {
				Selection.activeGameObject = item.Value;
				EditorGUIUtility.PingObject(item.Value);
				SceneView.FrameLastActiveSceneView();
			}
		}

		EditorGUILayout.EndScrollView();
	}
}