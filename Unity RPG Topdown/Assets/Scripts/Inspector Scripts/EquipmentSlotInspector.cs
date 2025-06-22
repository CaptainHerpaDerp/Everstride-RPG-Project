using Core.Enums;
using InventoryManagement;
using UnityEditor;

#if UNITY_EDITOR
//[CustomEditor(typeof(EquipmentSlot))]
//public class EquipmentSlotInspector : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        EquipmentSlot equipmentSlot = (EquipmentSlot)target;

//        EditorGUI.BeginChangeCheck();

//        // Draw default inspector without the armourType field
//        serializedObject.Update();

//        SerializedProperty defaultImage = serializedObject.FindProperty("defaultSprite");
//        EditorGUILayout.PropertyField(defaultImage);


//        SerializedProperty equipmentSlotTypeProperty = serializedObject.FindProperty("equipmentSlotType");
//        EditorGUILayout.PropertyField(equipmentSlotTypeProperty);
//        serializedObject.ApplyModifiedProperties();

//        if (equipmentSlot.equipmentSlotType == ItemType.Armour)
//        {
//            // If equipmentSlotType is Armour, show the armourType field
//            SerializedProperty armourTypeProperty = serializedObject.FindProperty("armourType");
//            EditorGUILayout.PropertyField(armourTypeProperty);
//        }

//        if (EditorGUI.EndChangeCheck())
//        {
//            // Apply changes to the serialized object
//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}
#endif