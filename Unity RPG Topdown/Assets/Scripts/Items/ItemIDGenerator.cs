#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Items
{
    public static class ItemIDGenerator
    {
        [MenuItem("Tools/Assign Missing ItemIDs")]
        public static void AssignItemIDs()
        {
            string[] guids = AssetDatabase.FindAssets("t:BaseItem");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

                if (item != null && string.IsNullOrEmpty(item.ItemID))
                {
                    SerializedObject serializedObject = new SerializedObject(item);
                    serializedObject.FindProperty("itemID").stringValue = Guid.NewGuid().ToString();
                    serializedObject.ApplyModifiedProperties();

                    Debug.Log($"Assigned new ItemID to {item.name}");
                    EditorUtility.SetDirty(item);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Finished assigning ItemIDs.");
        }
    }
}
#endif
