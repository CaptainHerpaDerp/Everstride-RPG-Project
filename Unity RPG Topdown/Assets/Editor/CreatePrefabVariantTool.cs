using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

# if UNITY_EDITOR
public class CreatePrefabVariantTool : EditorWindow
{
    private const string prefabPath = "Assets/Static Objects/Tile Prefab Variant.prefab";
    private const string newPrefabPath = "Assets/Prefabs/TilePrefabs/Tile Prefab Variant.prefab";

    [MenuItem("Tools/Create Tile Prefab Variant")]
    private static void CreateTilePrefabVariant()
    {
        // Load the prefab from the specified path
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // Create a copy of the selected prefab variant
        GameObject duplicatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (duplicatedPrefab != null)
        { 
            // Set the new name if needed
            duplicatedPrefab.name = "New Tile Prefab";

            if (!AssetDatabase.IsValidFolder(newPrefabPath))
            {
                Debug.Log("Folder does not exist");
            }

            // Save the duplicated prefab to the specified folder
            string prefabPath = newPrefabPath;
            PrefabUtility.SaveAsPrefabAsset(duplicatedPrefab, prefabPath);

            Debug.Log("Prefab Variant duplicated and saved at path: " + prefabPath);
        }
        else
        {
            Debug.LogError("Failed to duplicate prefab variant.");
        }
    }
}
#endif