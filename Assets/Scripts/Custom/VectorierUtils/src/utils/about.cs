using UnityEngine;
using UnityEditor;
using System.Text;

// -=-=-=- //

public class AboutVectorierUtils : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/ⓘ About", false, 10000)]
	static void LogScriptsInformation() {
		// Create a StringBuilder instance
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < 8; i++) {
			sb.Append("—");
		}

		// Create the message
		string message = $"Made on 20th June 2024\n" +
				$"\u200B\n" +
				$"{sb.ToString()}" +
				$"\u200B\n" +
				$"\nby kubinka0505";

		// Display the dialog
		EditorUtility.DisplayDialog("About", message, "OK");
	}
}