using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Items
{

    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        #region Singleton Management

        private static ItemDatabase instance;

        public static ItemDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ItemDatabase>("ItemDatabase");
                    if (instance == null)
                    {
                        Debug.LogError("ItemDatabase asset not found in Resources folder!");
                    }
                }
                return instance;
            }
        }

        #endregion

        [Tooltip("Automatically populated list of all items in the specified folder.")]
        [ReadOnly, ListDrawerSettings(Expanded = true)]
        public List<Item> items = new();

        public Item GetItemByName(string itemName)
        {
            return items.Find(i => i.itemName == itemName);
        }


#if UNITY_EDITOR
        [Tooltip("Folder path to search for item assets.")]
        [SerializeField]
        private string itemSearchPath = "Assets/Items"; // Change this path as needed.

        [Button("Refresh Item Database", ButtonSizes.Medium)]
        public void RefreshItemDatabase()
        {
            items.Clear();

            if (string.IsNullOrEmpty(itemSearchPath))
            {
                Debug.LogError("Item search path is not set. Please specify a valid path.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Object", new[] { itemSearchPath });

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning("Encountered an invalid GUID or empty asset path.");
                    continue;
                }

                // Skip folders
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                Item Item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

                if (Item == null)
                {
                    Debug.LogWarning($"Asset at path '{assetPath}' is not a valid Item or derived type.");
                    continue;
                }

                // Check if the asset is Item or its subclass
                if (Item.GetType() == typeof(Item) || Item.GetType().IsSubclassOf(typeof(Item)))
                {
                    items.Add(Item);
                }
            }

            // Sort the items alphabetically by name (case-insensitive)
            items = items.OrderBy(i => i.itemName, StringComparer.OrdinalIgnoreCase).ToList();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log($"Item database refreshed and sorted alphabetically. Found {items.Count} items.");
        }

        public string GetItemNameFromID(string ID)
        {
            foreach (var item in items)
            {
                if (item.ItemID == ID)
                {
                    return item.itemName;
                }
            }

            Debug.LogError($"Item with ID {ID} not found in the database.");
            return "";
        }

#endif
    }
}