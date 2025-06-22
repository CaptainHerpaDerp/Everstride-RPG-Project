#if UNITY_EDITOR
using Items;
using UnityEditor;
using UnityEngine;

public class ItemDatabaseMenu
{
    private const string databasePath = "Assets/Resources/ItemDatabase.asset"; // Change this path as needed.

    [MenuItem("Tools/Open Item Database")]
    private static void OpenItemDatabase()
    {
        // Load the ItemDatabase asset
        ItemDatabase itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>(databasePath);

        if (itemDatabase != null)
        {
            // Focus on the database in the inspector
            Selection.activeObject = itemDatabase;
            Debug.Log("Item Database opened in the Inspector.");
        }
        else
        {
            Debug.LogError($"Item Database not found at path: {databasePath}. Ensure the path is correct.");
        }
    }

    [MenuItem("Tools/Refresh Item Database")]
    private static void RefreshItemDatabase()
    {
        // Load the ItemDatabase asset
        ItemDatabase itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>(databasePath);

        if (itemDatabase != null)
        {
            // Call the refresh method
            itemDatabase.RefreshItemDatabase();
            Debug.Log("Item Database refreshed.");
        }
        else
        {
            Debug.LogError($"Item Database not found at path: {databasePath}. Ensure the path is correct.");
        }
    }
}
#endif
