using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// -=-=-=- //

public class SpritePathCollector : MonoBehaviour {
    [MenuItem("Vectorier/⚙ Utils/🖼️ Sprites/🗀 Show paths (deprecated)", false, 9998)]
    static void CollectSpritePaths() {
        List<string> spritePaths = new List<string>();
        string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")); // Get the absolute project path

        // Determine the correct path separator for the current operating system
        string pathSeparator = Path.DirectorySeparatorChar.ToString();

        // Iterate over all GameObjects in the scene
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()) {
            // Check if the GameObject has a SpriteRenderer component
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null) {
                // Get the path of the sprite texture
                string assetPath = AssetDatabase.GetAssetPath(spriteRenderer.sprite.texture);

                // Adjust path separators if running on Windows
                if (Path.DirectorySeparatorChar == '\\') {
                    assetPath = assetPath.Replace('/', '\\');
                }

                string fullPath = projectPath + pathSeparator + assetPath;
                spritePaths.Add(fullPath);
            }
        }

        // Display the unique paths in a dialog box without removing duplicates
        string pathsString = string.Join("\n", spritePaths);
        ShowDialog(pathsString);
    }

    static void ShowDialog(string pathsString) {
        EditorWindow window = EditorWindow.GetWindow<SpritePathDialog>(true, "VectorierUtils - Show sprite paths", true);
        SpritePathDialog dialog = window as SpritePathDialog;
        dialog.Initialize(pathsString);

        window.minSize = new Vector2(800, 600);
        window.maxSize = new Vector2(800, 600);

        window.Show();
    }
}

// -=-=-=- //

public class SpritePathDialog : EditorWindow {
    private string pathsString;
    private string[] displayedPaths;
    private bool showRelativePaths = false;
    private bool showFilenames = false;
    private SortOption sortOption = SortOption.None;
    private bool reverseSort = false;
    private Vector2 scrollPosition;

    private enum SortOption {
        None,
        Name,
        NameLength,
        FileSize
    }

    public void Initialize(string pathsString) {
        this.pathsString = pathsString;
        ProcessDisplayedPaths();
    }

    void OnGUI() {
        // Options GUI
        EditorGUILayout.BeginHorizontal();
        showRelativePaths = EditorGUILayout.Toggle("Relative paths", showRelativePaths);
        showFilenames = EditorGUILayout.Toggle("Filenames + Extensions", showFilenames);
        EditorGUILayout.EndHorizontal();

        // Sort options GUI
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Sort By:");
        sortOption = (SortOption)EditorGUILayout.EnumPopup(sortOption);
        reverseSort = EditorGUILayout.Toggle("Reverse", reverseSort);
        EditorGUILayout.EndHorizontal();

        // Process paths according to options
        ProcessDisplayedPaths();

        // Begin scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // Display the processed paths in a text area
        EditorGUILayout.TextArea(string.Join("\n", displayedPaths), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // End scroll view
        EditorGUILayout.EndScrollView();

        // Copy button
        if (GUILayout.Button("Copy")) {
            EditorGUIUtility.systemCopyBuffer = string.Join("\n", displayedPaths);
        }
    }

	// -=-=-=- //

    private void ProcessDisplayedPaths() {
        List<string> paths = new List<string>(pathsString.Split('\n'));

        // Apply options
        if (showRelativePaths) {
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            for (int i = 0; i < paths.Count; i++) {
                paths[i] = paths[i].Replace(projectPath + Path.DirectorySeparatorChar, "");
            }
        }

        if (!showFilenames) {
            for (int i = 0; i < paths.Count; i++) {
                paths[i] = Path.GetDirectoryName(paths[i]);
            }
        }

        // Sorting
        switch (sortOption) {
            case SortOption.Name:
                paths.Sort((a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b)));
                break;
            case SortOption.NameLength:
                paths.Sort((a, b) => a.Length.CompareTo(b.Length));
                break;
            case SortOption.FileSize:
                paths.Sort((a, b) => new FileInfo(a).Length.CompareTo(new FileInfo(b).Length));
                break;
            default:
                break;
        }

        // Reverse if necessary
        if (reverseSort) {
            paths.Reverse();
        }

        // Remove duplicates after processing
        paths = paths.Distinct().ToList();

        // Update displayed paths
        displayedPaths = paths.ToArray();
    }
}
