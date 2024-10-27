using System.IO;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using VectorierUtils;

// -=-=-=- //

public class InputMapProcessor : EditorWindow {
	[Tooltip("Number for AI")]
	private static int aiNumber = 0;

	private static string varAI = "<AI>";
	private static string varKey = "<KEY>";

	[TextArea(3, 10)]
	private string xmlTemplate = 
$@"<Init>
	<SetVariable Name=""$Active"" Value=""1""/>
	<SetVariable Name=""$Node"" Value=""COM""/>
	<SetVariable Name=""$AI"" Value=""{varAI}""/>
	<SetVariable Name=""Key"" Value=""{varKey}""/>
	<SetVariable Name=""Flag1"" Value=""0""/>
</Init>
<Template Name=""AI_noFollow""/>";

	[MenuItem("Vectorier/⚙ Utils/⸬ Input Map Processor")]
	public static void ShowWindow() {
		InputMapProcessor window = GetWindow<InputMapProcessor>("VectorierUtils - Input Map Processor");
		Vector2 winSize = new Vector2(350, 252);
		window.minSize = winSize;
		window.maxSize = winSize;
	}

	void OnGUI() {
		// Input field for AI Number
		GUILayout.Label("AI Number", EditorStyles.boldLabel);
		aiNumber = EditorGUILayout.IntField(aiNumber);

		GUILayout.Space(5);

		// Editable XML template field
		
		GUILayout.Label("XML Template", EditorStyles.boldLabel);
		GUILayout.Label($@"⚠️ The ""{varAI}"" and ""{varKey}"" variables have to be included!", EditorStyles.wordWrappedLabel);
		xmlTemplate = EditorGUILayout.TextArea(xmlTemplate, GUILayout.Height(140));

		GUILayout.Space(8);

		// Load button
		if (GUILayout.Button("Load")) {
			if (!xmlTemplate.Contains(varAI)) {
				Utils.AdvancedLog("Warning", $@"XML template does not contain ""{varAI}"" string!");
				return;
			}

			if (!xmlTemplate.Contains(varKey)) {
				Utils.AdvancedLog("Warning", $@"XML template does not contain ""{varAI}"" string!");
				return;
			}

			ProcessInputMap(aiNumber.ToString());
		}
	}

	// Change ProcessInputMap to an instance method (non-static)
	public void ProcessInputMap(string ai) {
		float MultiplicationFactor = 1.0f;
		float MoveFactor = 6.022646f;

		string filePath = EditorUtility.OpenFilePanel(
			"Select Input Map File",
			"Assets/Resources",
			"txt"
		);

		if (string.IsNullOrEmpty(filePath)) {
			Debug.LogWarning("No file selected.");
			return;
		}

		Color randomColor = Color.HSVToRGB(Random.value, 1f, 1f);
		randomColor.a = 1f;

		GameObject inputsParent = new GameObject("Inputs (AI " + $"{aiNumber})");

		string[] allLines = File.ReadAllLines(filePath);

		float positionDifferenceX = 0.0f;
		int counter = 0;

		foreach (string line in allLines) {
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] parts = line.Split(':');
			if (parts.Length != 2) continue;

			string key = Capitalize(parts[0].Trim());
			float value = float.Parse(parts[1].ToString().Trim(), CultureInfo.InvariantCulture.NumberFormat);

			if (counter == 0) {
				positionDifferenceX = value;
				counter++;
			}

			float xPosition = (value - positionDifferenceX) * MoveFactor * MultiplicationFactor;

			GameObject inputObject = new GameObject();
			inputObject.name = key + ", " + value.ToString("F4").Replace(',', '.') + "s";
			inputObject.transform.position = new Vector3(xPosition, 0, -1);
			inputObject.transform.localScale = new Vector3(1.0f, 2.0f, -1);
			inputObject.transform.parent = inputsParent.transform;

			SpriteRenderer spriteRenderer = inputObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = Resources.Load<Sprite>("Textures/trigger_white");
			spriteRenderer.color = randomColor;

			TriggerSettings triggerSettings = inputObject.AddComponent<TriggerSettings>();
			triggerSettings.Content = xmlTemplate.Replace(
				varAI, ai
			).Replace(
				varKey, key
			);
		}

		void AlignWithView(GameObject inputsParent) {
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			Vector3 cameraPosition = sceneCamera.transform.position;
			Vector3 cameraForward = sceneCamera.transform.forward;

			inputsParent.transform.position = cameraPosition + cameraForward * 5f;
			inputsParent.transform.rotation = Quaternion.LookRotation(cameraForward);
			Vector3 newPosition = inputsParent.transform.position;
			newPosition.z = -1;
			inputsParent.transform.position = newPosition;
		}

		AlignWithView(inputsParent);
		Selection.activeGameObject = inputsParent;
	}

	public static string Capitalize(string str) {
		if (str == null) return null;
		if (str.Length > 1) return char.ToUpper(str[0]) + str.Substring(1);
		return str.ToUpper();
	}
}