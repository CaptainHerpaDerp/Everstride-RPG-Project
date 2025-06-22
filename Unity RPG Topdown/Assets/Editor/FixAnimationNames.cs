using UnityEngine;
using UnityEditor;
using System.IO;

public class FixAnimationNames : MonoBehaviour
{
    [MenuItem("Tools/Fix Animation Clip Names in Folder")]
    public static void FixAnimationClipNamesInFolder()
    {
        // Get all selected folders or assets in the editor
        string[] selectedFolders = Selection.assetGUIDs;

        foreach (string guid in selectedFolders)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (Directory.Exists(folderPath)) // Ensure it's a folder
            {
                ProcessFolder(folderPath);
            }
            else
            {
                Debug.LogWarning($"Skipping non-folder selection: {folderPath}");
            }
        }

        AssetDatabase.SaveAssets(); // Save all changes
        Debug.Log("Animation clip names fixed!");
    }

    private static void ProcessFolder(string folderPath)
    {
        // Get all files in the folder and subfolders
        string[] filePaths = Directory.GetFiles(folderPath, "*.anim", SearchOption.AllDirectories);

        foreach (string filePath in filePaths)
        {
            // Load the animation clip asset
            string relativePath = filePath.Replace(Application.dataPath, "Assets");
            AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(relativePath);

            if (animationClip != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath); // Extract the file name

                if (animationClip.name != fileName)
                {
                    Debug.Log($"Fixing animation clip name: '{animationClip.name}' -> '{fileName}'");
                    animationClip.name = fileName; // Update the animation clip's name to match the file name
                    EditorUtility.SetDirty(animationClip); // Mark the animation clip as dirty for saving
                }
                else
                {
                    Debug.Log($"Animation clip '{animationClip.name}' already matches the file name.");
                }
            }
        }
    }
}
