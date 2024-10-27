using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using Debug = UnityEngine.Debug;

// -=-=-=- //

public class DZMake : MonoBehaviour {
	// (level + track content + unity scene + music + textures + prefabs + meta)
	[MenuItem("Vectorier/⚙ Utils/🗃️ Advanced level build %#G")]
	public static void CreateTrackContent_Invoke_Basic() {
		CreateTrackContent(false);
	}

	[MenuItem("Vectorier/⚙ Utils/🗃️ Advanced level build + Run game")]
	public static void CreateTrackContent_Invoke_RunGame() {
		CreateTrackContent(true);
	}

	// -=-=-=- //
	// Functions

	private static string GetScriptPath() {
        StackTrace stackTrace = new StackTrace(true);

        // Find the frame containing this method
        StackFrame frame = null;
        for (int i = 0; i < stackTrace.FrameCount; i++) {
            if (stackTrace.GetFrame(i).GetMethod() == MethodBase.GetCurrentMethod()) {
				// Get the frame where this method was called
                frame = stackTrace.GetFrame(i + 1);
                break;
            }
        }

		return frame.GetFileName();
	}

	// -=-=-=- //

	BuildMap buildMap;

	// Setup

	private static readonly string directoryOutputGameName = "Vector"; //"Put to game directory";
	private static readonly string directoryOutputProjectName = "Vectorier"; // "Put to editor directory";

	// Steam game app ID
	private static readonly string gameAppID = "248970";

	// Archive extension supported by 7z CLI. "7z" or "zip" reccomended. Case sensitive!
	private static readonly string fileOutputArchive_Full_Type = "7z";

	// ----- //

	// Variables

	private static readonly bool dummyArchiveMode = false; // after this script is done, restore the level archive that was before

	private static readonly string separator = Path.DirectorySeparatorChar.ToString();
	private static readonly string randomNumber = UnityEngine.Random.Range(10000, 99999).ToString();
	private static readonly string[] extensionsImagesAllowed = { ".png", ".jpg", ".gif", ".plist" };

	// Paths
	private static readonly string directoryAppdata = Application.dataPath.Replace("/", separator);
	private static readonly string cwd = Path.GetDirectoryName(GetScriptPath());

	// Game
	private string gamePath;
	private string musicFilePath;
	private string musicFilePathRelative;
	private string levelArchivePath;
	private string levelArchivePathBackup;
	private string backgroundImagePath;

	private static readonly string scenePath = SceneManager.GetActiveScene().path;
	private static readonly string sceneName = Path.GetFileNameWithoutExtension(scenePath);

	// Other paths
	private static readonly string directoryAssetsTextures = Path.Combine(directoryAppdata, "Resources", "Textures");
	private static readonly string directoryTEMP = Path.GetTempPath().Replace("/", separator);
	private static readonly string directoryProject = Path.Combine(directoryAppdata, scenePath).Replace("/", separator);

	private static readonly string directoryOutputTop = Path.Combine(
		directoryAppdata,
		"Scenes",
		"z_Exported" // Top directory basename
	).Replace("/", separator);

	private static readonly string directoryOutput = Path.Combine(
		directoryOutputTop,
		sceneName,
		DateTime.Now.ToString("dd_MM_yyyy-HH_mm_ss")
	).Replace("/", separator);

	// --- //

	// Final directories
	
	private static readonly string directoryOutputGame = Path.Combine(directoryOutput, directoryOutputGameName).Replace("/", separator);
	private static readonly string directoryOutputProject = Path.Combine(directoryOutput, directoryOutputProjectName).Replace("/", separator);

	// Archive files
	private static readonly string fileOutputArchive_Full = Path.Combine(directoryOutput, sceneName + "." + fileOutputArchive_Full_Type);
	private static readonly string fileOutputArchive_TrackContent = Path.Combine(directoryOutputGame, "track_content_2048.dz").Replace("/", separator);
	
	private static readonly string directoryOutputAssets = Path.Combine(directoryOutputProject, "Assets").Replace("/", separator);
	private static readonly string directoryOutputScenes = Path.Combine(directoryOutputAssets, "Scenes").Replace("/", separator);
	private static readonly string directoryOutputAssetsTextures = Path.Combine(directoryOutputAssets, "Resources", "Textures").Replace("/", separator);


	// Temporary directories and files
	private static readonly string directoryWorkingBase = Path.Combine(directoryTEMP, $"!_VectorierUtils_{randomNumber}");
	private static readonly string directoryWorkingImages = Path.Combine(directoryWorkingBase, "Images");
	private static readonly string fileWorkingConfig = Path.Combine(directoryWorkingBase, "config.dcl");

	// -=-=-=- //

	// Executables
	private static readonly string fileExecutable7zip = Path.Combine(Path.GetDirectoryName(cwd), "..", "bin", "7z.exe").Replace("/", separator);
	private static readonly string fileExecutable7zip_Arguments = $"a -t{fileOutputArchive_Full_Type} -mx=9 \"{fileOutputArchive_Full}\" \"{directoryOutput}/*\"".Replace("/", separator);

	private static readonly string fileExecutableDzip = Path.Combine(directoryAppdata, "XML", "dzip", "dzip.exe");
	private static readonly string fileExecutableDzip_Arguments = $"\"{fileWorkingConfig}\" -v";

	private static readonly string fileExecutableExplorer = @"C:/Windows/explorer.exe".Replace("/", separator);
	private static readonly string fileExecutableCMD = @"C:/Windows/System32/cmd.exe".Replace("/", separator);

	// -=-=-=- //

	void Awake() {
		// - test here -
		// Debug.Log(directoryOutput);

		buildMap = FindObjectOfType<BuildMap>();

		// Paths
		gamePath = buildMap.vectorFilePath;
		levelArchivePath = Path.Combine(gamePath, "level_xml.dz");

		// Correct the variable name here
		levelArchivePathBackup = Path.GetFileNameWithoutExtension(levelArchivePath) 
			+ $"_{randomNumber}" 
			+ Path.GetExtension(levelArchivePath);

		if (dummyArchiveMode) {
			File.Copy(levelArchivePath, levelArchivePathBackup);
		}

		// Build map
		BuildMap.Build(true, true);

		// --- //

		if (buildMap != null) {
			musicFilePathRelative = buildMap.levelMusic + ".mp3";
			musicFilePathRelative = musicFilePathRelative.Replace("/", separator).Trim(separator.ToCharArray());

			musicFilePath = Path.Combine(gamePath, musicFilePathRelative);

			if (!File.Exists(musicFilePath)) {
				Debug.LogError($"Music file path was not found! (\"{musicFilePath}\")");
			}

			// asserts backgrounds have PNG / JPG / JPEG extensions
			backgroundImagePath = Path.Combine(directoryAssetsTextures, buildMap.customBackground);
			string[] bgFormats = { ".png", ".jpg", ".jpeg" };

			foreach (string extension in bgFormats) {
				if (File.Exists(backgroundImagePath + extension)) {
					backgroundImagePath += extension;

					// find first occurence
					break;
				}
			}

			// Verify
			if (!Path.HasExtension(backgroundImagePath)) {
				Debug.LogWarning("Background image file was not classified as an image.");
			}

			if (!File.Exists(backgroundImagePath)) {
				Debug.LogWarning("Background image file was not found.");
			}
		}
	}

	// -=-=-=- //
	// Function to copy a file while preserving directory structure within the destination directory

	void CopyWithSubfiles(string sourceFile, string destDir, bool copyMeta, int index) {
		// Ensure destination directory exists
		if (!Directory.Exists(destDir)) {
			Directory.CreateDirectory(destDir);
		}

		// Get relative path of sourceFile within its directory
		if (!File.Exists(sourceFile)) {
			Debug.Log("File cannot be accessed");
		} else {
			string sourceFileName = Path.GetFileName(sourceFile);
			string sourceFileDir = Path.GetDirectoryName(sourceFile);
			string relativePath = sourceFileDir.Substring(sourceFileDir.IndexOf("Assets") + index);

			// Construct destination path including directory structure
			string destFilePath = Path.Combine(destDir, relativePath, sourceFileName);

			// Create directories for the sourceFile if they don't exist
			Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

			// Copy the sourceFile to the destination directory
			File.Copy(sourceFile, destFilePath, true);

			if (copyMeta) {
				string metaExt = ".meta";
				if (File.Exists(sourceFile + metaExt)) {
					File.Copy(
						sourceFile + metaExt,
						destFilePath + metaExt,
						true
					);
				}
			}
		}
	}

	// -=-=-=- //
	// Find prefab paths

	private HashSet<string> FindPrefabPaths() {
		// Get active scene
		var activeScene = SceneManager.GetActiveScene();
		var rootGameObjects = activeScene.GetRootGameObjects();

		HashSet<string> paths = new HashSet<string>();

		Action<GameObject, HashSet<string>> findPrefabs = null;
		findPrefabs = (gameObject, paths) => {
			// Check if the GameObject is a prefab instance
			var prefabType = PrefabUtility.GetPrefabInstanceStatus(gameObject);

			if (prefabType == PrefabInstanceStatus.Connected || prefabType == PrefabInstanceStatus.Disconnected) {
				var prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);

				if (prefabAsset != null) {
					var path = AssetDatabase.GetAssetPath(prefabAsset);

					if (!string.IsNullOrEmpty(path)) {
						paths.Add(path);

						// Check for .meta file existence
						var metaPath = path + ".meta";
						if (File.Exists(metaPath)) {
							paths.Add(metaPath);
						}
					}
				}
			}

			// Recursively check for prefabs in children
			foreach (Transform child in gameObject.transform) {
				findPrefabs(child.gameObject, paths);
			}
		};

		// Iterate through all root GameObjects in the scene
		foreach (var rootGameObject in rootGameObjects) {
			// Recursively find all prefabs in the scene
			findPrefabs(rootGameObject, paths);
		}

		return paths;
	}

	// -=-=-=- //
	// Main

	public static void CreateTrackContent(bool runGameAfterBuild) {
		DZMake instance = new DZMake();
		instance.Awake();

		// Create a HashSet to store file paths
		HashSet<string> filePathsImages = new HashSet<string>();

		// Find all GameObjects with SpriteRenderer components
		foreach (var spriteRenderer in FindObjectsOfType<SpriteRenderer>()) {
			if (spriteRenderer != null && spriteRenderer.sprite != null) {
				string spritePath = AssetDatabase.GetAssetPath(spriteRenderer.sprite.texture);
				filePathsImages.Add(spritePath);
			}
		}

		// Add background texture (from "ScriptManager") to the HashSet
		filePathsImages.Add(instance.backgroundImagePath);

		// -=-=-=- //

		// Create directories
		Directory.CreateDirectory(directoryOutputGame);
		Directory.CreateDirectory(directoryOutputScenes);
		Directory.CreateDirectory(directoryWorkingBase);
		Directory.CreateDirectory(directoryWorkingImages);

		// Hide directory to avoid .meta file creation
		DirectoryInfo directoryInfo = new DirectoryInfo(directoryOutputTop);
		directoryInfo.Attributes |= FileAttributes.Hidden;

		// -=-=-=- //
		// Copy files

		// Scene
		File.Copy(
			scenePath,
			Path.Combine(
				directoryOutputScenes,
				Path.GetFileName(scenePath)
			),
			true
		);

		try {
			File.Copy(
				scenePath,
				Path.Combine(
					directoryOutputScenes,
					Path.GetFileName(scenePath) + ".meta"
				),
				true
			);
		} catch {
			// pass
		}

		// Level XML archive
		File.Copy(
			instance.levelArchivePath,
			Path.Combine(
				directoryOutputGame,
				Path.GetFileName(instance.levelArchivePath)
			),
			true
		);

		// Restore backup
		if (dummyArchiveMode) {
			File.Delete(instance.levelArchivePath);
			File.Move(instance.levelArchivePathBackup, instance.levelArchivePath);

			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		// Music file path
		if (instance.gamePath != null) {
			if (instance.musicFilePathRelative.Contains(separator)) {
				string musicCustomDirectory = Path.Combine(
					directoryOutputGame,
					Path.GetDirectoryName(instance.musicFilePathRelative)
				);

				Directory.CreateDirectory(musicCustomDirectory);

				File.Copy(
					instance.musicFilePath,
					Path.Combine(
						musicCustomDirectory,
						Path.GetFileName(instance.musicFilePathRelative)
					),
					true
				);
			} else {
				File.Copy(
					instance.musicFilePath,
					Path.Combine(
						directoryOutputGame,
						Path.GetFileName(instance.musicFilePath)
					),
					true
				);
			}
		}

		// -=-=-=- //

		// Copy prefabs paths
		foreach (var prefabPath in instance.FindPrefabPaths()) {
			instance.CopyWithSubfiles(prefabPath, directoryOutputAssets, true, 7);
		}

		// Copy files in subdirectories
		foreach (var filePath in filePathsImages) {
			instance.CopyWithSubfiles(filePath, directoryOutputAssets, true, 7);
		}

		// -=-=-=- //

		// Copy files to Images directory
		foreach (var filePath in filePathsImages) {
			if (extensionsImagesAllowed.Contains(Path.GetExtension(filePath).ToLower())) {
				string fileName = Path.GetFileName(filePath);
				string destPath = Path.Combine(directoryWorkingImages, fileName);

				// Copy the main file
				File.Copy(filePath, destPath, true);
			}
		}

		// -=-=-=- //

		// Create config file
		using (StreamWriter writer = new StreamWriter(fileWorkingConfig)) {
			writer.WriteLine($"archive \"{fileOutputArchive_TrackContent}\"");
			writer.WriteLine($"basedir \"{directoryWorkingImages}\"");

			foreach (var filePath in Directory.GetFiles(directoryWorkingImages)) {
				string formattedFilePath = Path.GetFileName(filePath).Replace("/", separator);
				// DZ, for smaller file size
				writer.WriteLine($"file \"{formattedFilePath}\" 0 dz");

				// ZLIB, for huge amount of files
				// writer.WriteLine($"file \"{formattedFilePath}\" 0 zlib");
			}
		}

		// -=-=-=- //

		// Execute dzip command
		var processInfo_Dzip = new ProcessStartInfo {
			FileName = fileExecutableDzip,
			Arguments = fileExecutableDzip_Arguments,
			UseShellExecute = true
		};

		using (var process = Process.Start(processInfo_Dzip)) {
			process.WaitForExit();
		}

		// -=-=-=- //

		// Wait 1000ms before checking file existence (dzip.exe has broken stdin)
		// System.Threading.Thread.Sleep(1000 + 3); // lookahead

		// Check if track content archive exists and show it in explorer
		if (File.Exists(fileOutputArchive_TrackContent)) {
			// Create archive
			var processInfo_7zip = new ProcessStartInfo {
				FileName = fileExecutable7zip,
				Arguments = fileExecutable7zip_Arguments,
				UseShellExecute = true
			};

			using (var process = Process.Start(processInfo_7zip)) {
				process.WaitForExit();
			}

			// Show in explorer
			Process.Start(new ProcessStartInfo {
				FileName = fileExecutableExplorer,
				Arguments = $"/select,\"{fileOutputArchive_Full}\"",
				UseShellExecute = true
			});
		}

		// Remove temporary files directory
		Directory.Delete(directoryWorkingBase, true);

		// Run the game
		if (runGameAfterBuild) {
			Process.Start(new ProcessStartInfo {
				FileName = fileExecutableCMD,
				Arguments = $"/c start steam://run/" + gameAppID,
				UseShellExecute = true
			});
		}
	}
}