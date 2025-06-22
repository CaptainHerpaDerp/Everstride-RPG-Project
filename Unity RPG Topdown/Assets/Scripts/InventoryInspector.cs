using InventoryManagement;
using Items;
using UnityEditor;
using UnityEngine;

//#if UNITY_EDITOR
//[CustomEditor(typeof(Inventory))]
//public class InventoryInspector : Editor
//{
//    SerializedProperty items, itemQuantities, goldQuantity;

//    private void OnEnable()
//    {
//        items = serializedObject.FindProperty("items");
//        itemQuantities = serializedObject.FindProperty("itemQuantities");
//        goldQuantity = serializedObject.FindProperty("currency");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        Inventory inventory = (Inventory)target;

//        EditorGUILayout.PropertyField(goldQuantity);

//        // Button to add a new item
//        if (GUILayout.Button("Reset Items"))
//        {
//            inventory.ClearItems();
//            inventory.itemQuantities.Clear();
//        }

//        for (int i = 0; i < inventory.GetItems().Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            inventory.GetItems()[i] = EditorGUILayout.ObjectField(inventory.GetItems()[i], typeof(Item), true) as Item;

//            if (inventory.GetItems()[i] != null && i < inventory.itemQuantities.Count && inventory.GetItems()[i].stackableItem)
//                inventory.itemQuantities[i] = EditorGUILayout.IntField("Quantity", inventory.itemQuantities[i]);

//            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
//            if (GUILayout.Button("X", buttonOptions))
//            {
//                inventory.GetItems().RemoveAt(i);
//                inventory.itemQuantities.RemoveAt(i);
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        // Button to add a new item
//        if (GUILayout.Button("Add Item"))
//        {
//            // Add a new empty item to the shop data
//            inventory.GetItems().Add(null);
//            inventory.itemQuantities.Add(1);
//        }

//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(inventory);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
//#endif