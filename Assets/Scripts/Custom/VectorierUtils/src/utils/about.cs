using UnityEngine;
using UnityEditor;
using System.Text;

// -=-=-=- //

public class AboutVectorierUtils : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/ⓘ About", false, 10000)]
	static void LogScriptsInformation() {

		// Create a StringBuilder instance
		StringBuilder horizontalLine = new StringBuilder();

		for (int i = 0; i < 8; i++) {
			horizontalLine.Append("—");
		}

		string br = horizontalLine.ToString();

		// -=-=-=- //

		// Create the message
		string message = $@"
			[ Description ]
			Standalone utils scripts for Vectorier level editor. 

			{br}

			[ Special thanks (alphabetically) ]
			• FlipThoseTitle
				Implementing ideas 💡
			• floofly.
				Testing & suggestions ⚙️

			{br}

			Made by kubinka0505
			Latest update on 01.10.2024
		";

		// Normalize
		message = message.Replace("&", "&&");
		message = message.Replace("%", "%%");
		message = message.Replace("\t\t\t", "");
		message = message.Replace("\t", "  ");
		message = message.Replace("\n", "");
		message = message.Trim('"');

		// Display the dialog
		EditorUtility.DisplayDialog("About", message, "OK");
	}
}