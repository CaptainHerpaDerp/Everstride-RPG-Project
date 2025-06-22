using Items;
using PlayerShopping;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
//[CustomEditor(typeof(ShopData))]
//public class ShopDataInspector : Editor
//{
//    SerializedProperty defaultItems;
//    SerializedProperty itemQuantities;
//    SerializedProperty shopOwner;
//    SerializedProperty sellerGold;

//    private void OnEnable()
//    {
//        defaultItems = serializedObject.FindProperty("defaultItems");
//        itemQuantities = serializedObject.FindProperty("itemQuantities");
//        shopOwner = serializedObject.FindProperty("shopOwner");
//        sellerGold = serializedObject.FindProperty("sellerGold");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        ShopData shopData = (ShopData)target;

//        shopOwner.stringValue = EditorGUILayout.TextField("Shop Owner", shopOwner.stringValue);

//        sellerGold.intValue = EditorGUILayout.IntField("Seller Gold", sellerGold.intValue);

//        // Button to add a new item
//        if (GUILayout.Button("Reset"))
//        {
//            shopData.defaultItems.Clear();
//            shopData.itemQuantities.Clear();
//        }

//        for (int i = 0; i < shopData.defaultItems.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            shopData.defaultItems[i] = EditorGUILayout.ObjectField(shopData.defaultItems[i], typeof(Item), true) as Item;

//            if (shopData.defaultItems[i] != null && i < shopData.itemQuantities.Count && shopData.defaultItems[i].stackableItem)
//                shopData.itemQuantities[i] = EditorGUILayout.IntField("Quantity", shopData.itemQuantities[i]);

//            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
//            if (GUILayout.Button("X", buttonOptions))
//            {
//                shopData.defaultItems.RemoveAt(i);
//                shopData.itemQuantities.RemoveAt(i);
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        // Button to add a new item
//        if (GUILayout.Button("Add Item"))
//        {
//            // Add a new empty item to the shop data
//            shopData.defaultItems.Add(null);
//            shopData.itemQuantities.Add(1);
//        }

//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(shopData);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
#endif