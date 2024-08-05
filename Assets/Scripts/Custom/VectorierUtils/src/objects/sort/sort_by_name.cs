using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// -=-=-=- //

public class SortGameObjectsByName : MonoBehaviour {
	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🔤 Name/Normal %#;")]
	public static void SortByNameNormal() {
		SortByName(false);
	}

	[MenuItem("Vectorier/⚙ Utils/☲ Sort in hierarchy/🔤 Name/Reversed %#&;")]
	public static void SortByNameReversed() {
		SortByName(true);
	}

	private static void SortByName(bool reversed) {
		GameObject parentObject = Selection.activeGameObject;

		if (parentObject == null) {
			string message = "No parent GameObject in a hierarchy selected.";

			Debug.LogWarning(message);
			EditorUtility.DisplayDialog("Warning", message, "OK");

			return;
		}

		List<Transform> children = new List<Transform>();

		foreach (Transform child in parentObject.transform) {
			children.Add(child);
		}

		if (reversed) {
			children = children.OrderByDescending(child => child.name, new NaturalStringComparer()).ToList();
		} else {
			children = children.OrderBy(child => child.name, new NaturalStringComparer()).ToList();
		}

		for (int i = 0; i < children.Count; i++) {
			children[i].SetSiblingIndex(i);
		}
	}

	private class NaturalStringComparer : IComparer<string> {
		public int Compare(string x, string y) {
			if (x == null) {
				return y == null ? 0 : -1;
			}

			if (y == null) {
				return 1;
			}

			int lx = x.Length, ly = y.Length;
			int ix = 0, iy = 0;

			while (ix < lx && iy < ly) {
				char cx = x[ix], cy = y[iy];

				if (char.IsDigit(cx) && char.IsDigit(cy)) {
					int nx = GetNumber(x, ref ix);
					int ny = GetNumber(y, ref iy);

					int compare = nx.CompareTo(ny);
					if (compare != 0) {
						return compare;
					}
				} else {
					int compare = cx.CompareTo(cy);
					if (compare != 0) {
						return compare;
					}
					ix++;
					iy++;
				}
			}

			return lx - ly;
		}

		private int GetNumber(string s, ref int index) {
			int number = 0;
			while (index < s.Length && char.IsDigit(s[index])) {
				number = number * 10 + (s[index] - '0');
				index++;
			}
			return number;
		}
	}
}