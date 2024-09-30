using UnityEngine;
using UnityEditor;
using System;

// -=-=-=- //

public class RoundifyCoordinates : ScriptableObject {
	// Floats
	[MenuItem("Vectorier/⚙ Utils/⌈⌋ Roundify coordinates attempt/⌊⌉ 2 decimal places (trim)", false, 1)]
	static void round_coordinates_2f() { roundCoordinates_Decimals(2); }

	[MenuItem("Vectorier/⚙ Utils/⌈⌋ Roundify coordinates attempt/⌊⌉ 3 decimal places (trim)", false, 2)]
	static void round_coordinates_3f() { roundCoordinates_Decimals(3); }

	// Integers
	// doesn't work fully, wontfix

	[MenuItem("Vectorier/⚙ Utils/⌈⌋ Roundify coordinates attempt/⌊⌉ Nearest integer (ceil)", false, 3)]
	static void round_coordinates_int_ceil() { roundCoordinates_NearestInteger(Mathf.Ceil); }

	[MenuItem("Vectorier/⚙ Utils/⌈⌋ Roundify coordinates attempt/⌊⌉ Nearest integer (floor)", false, 4)]
	static void round_coordinates_int_floor() { roundCoordinates_NearestInteger(Mathf.Floor); }

	// -=-=-=- //

	static void roundCoordinates_Decimals(int ndigits) {
		// Get all selected game objects
		GameObject[] selectedObjects = Selection.gameObjects;

		foreach (GameObject obj in selectedObjects) {
			Transform transform = obj.transform;

			Vector3 roundedPosition = new Vector3(
				RoundToDecimals(transform.position.x, ndigits),
				RoundToDecimals(transform.position.y, ndigits),
				RoundToDecimals(transform.position.z, ndigits)
			);

			transform.position = roundedPosition;

			// Mark the object as dirty to ensure changes are saved
			EditorUtility.SetDirty(transform);
		}
	}

	static float RoundToDecimals(float value, int ndigits) {
		// Convert to string with specified number of decimal places and then parse back to float
		return float.Parse(value.ToString("F" + ndigits));
	}

	static void roundCoordinates_NearestInteger(System.Func<float, float> roundingFunction) {
		// Get all selected game objects
		GameObject[] selectedObjects = Selection.gameObjects;

		foreach (GameObject obj in selectedObjects) {
			Transform transform = obj.transform;

			Vector3 roundedPosition = new Vector3(
				TruncateAfterRounding(roundingFunction(transform.position.x)),
				TruncateAfterRounding(roundingFunction(transform.position.y)),
				TruncateAfterRounding(roundingFunction(transform.position.z))
			);

			transform.position = roundedPosition;

			// Mark the object as dirty to ensure changes are saved
			EditorUtility.SetDirty(transform);
		}
	}

	static float TruncateAfterRounding(float value) {
		// Convert float to string
		string valueString = value.ToString();

		// Split the string by the decimal point
		string[] parts = valueString.Split('.');

		// The integer part is the first element of the split array
		string integerPartString = parts[0];

		// Parse the integer part back to a float
		float retval = (int)float.Parse(integerPartString);

		Debug.Log(retval);
		return retval;
	}
}
