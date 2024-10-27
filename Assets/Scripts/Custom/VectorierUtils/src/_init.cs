using System;
using UnityEngine;
using UnityEditor;

// -=-=-=- //

public static class VectorierMenu {
	[MenuItem("Vectorier/⚙ Utils/", true, 1000)]
	private static void Utils() {
		// placeholder for name
	}
}

// -=-=-=- //
// Functions

namespace VectorierUtils {
	public static class Utils {

		public static void AdvancedLog(string title = "Information", string msg = "", string buttonText = "OK", string logType = "info", bool logOnly = false) {
			// Define func as a delegate
			Action<string> func = Debug.Log;

			// Convert to lowercase for comparisons
			string titleLower = title.ToLower();
			logType = logType.ToLower();

			// Determine based on title
			if (titleLower == "") {
				func = Debug.Log;
			} else if (titleLower.StartsWith("warn")) {
				func = Debug.LogWarning;
			} else if (titleLower.StartsWith("err")) {
				func = Debug.LogError;
			}

			if (logType.StartsWith("info")) {
				func = Debug.Log;
			} else if (logType.StartsWith("warn")) {
				func = Debug.LogWarning;
			} else if (logType.StartsWith("err")) {
				func = Debug.LogError;
			}

			// Show a dialog if not logOnly
			if (!logOnly) {
				EditorUtility.DisplayDialog(title, msg, buttonText);
			}

			// Log the message
			func(msg);
		}
	}

	public class String {
		public static string GetRelativePath(string fromPath, string toPath) {
			Uri fromUri = new Uri(fromPath);
			Uri toUri = new Uri(toPath);

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			return relativePath;
		}
	}
}