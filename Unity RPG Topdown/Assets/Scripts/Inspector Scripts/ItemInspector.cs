using Core.Enums;
using Items;
using UnityEditor;

#if UNITY_EDITOR

//[CustomEditor(typeof(Item))]
//public class ItemInspector : Editor
//{
//    private SerializedProperty itemName;
//    private SerializedProperty weight;
//    private SerializedProperty icon;
//    private SerializedProperty value;
//    private SerializedProperty equipableItem;
//    private SerializedProperty itemLibraryAsset;
//    private SerializedProperty itemType;
//    private SerializedProperty stackableItem;
//    private SerializedProperty quantity;
//    private SerializedProperty weaponMode;
//    private SerializedProperty weaponType;
//    private SerializedProperty weaponRange;
//    private SerializedProperty weaponAngle;
//    private SerializedProperty weaponDamage;
//    private SerializedProperty shieldTrailAsset;
//    private SerializedProperty armourType;
//    private SerializedProperty armourDefence;
//    private SerializedProperty consumableType;
//    private SerializedProperty consumableQuantity;
//    private SerializedProperty coloredItem;
//    private SerializedProperty itemColor;
//    private SerializedProperty shieldPowerMin;
//    private SerializedProperty shieldPowerMax;
//    private SerializedProperty ammunitionType;
//    private SerializedProperty ammunitionDamage;
//    private SerializedProperty physAmmoSprite;
//    private SerializedProperty projectilePrefab;
//    private SerializedProperty canBlock;

//    private void OnEnable()
//    {
//        itemName = serializedObject.FindProperty("itemName");
//        weight = serializedObject.FindProperty("weight");
//        icon = serializedObject.FindProperty("icon");
//        value = serializedObject.FindProperty("value");
//        equipableItem = serializedObject.FindProperty("equipableItem");
//        itemLibraryAsset = serializedObject.FindProperty("itemLibraryAsset");
//        itemType = serializedObject.FindProperty("itemType");
//        stackableItem = serializedObject.FindProperty("stackableItem");
//        quantity = serializedObject.FindProperty("quantity");
//        weaponMode = serializedObject.FindProperty("weaponMode");
//        weaponType = serializedObject.FindProperty("weaponType");
//        weaponRange = serializedObject.FindProperty("weaponRange");
//        weaponAngle = serializedObject.FindProperty("weaponAngle");
//        weaponDamage = serializedObject.FindProperty("weaponDamage");
//        shieldTrailAsset = serializedObject.FindProperty("shieldTrailAsset");
//        armourType = serializedObject.FindProperty("armourType");
//        armourDefence = serializedObject.FindProperty("armourDefence");
//        consumableType = serializedObject.FindProperty("consumableType");
//        consumableQuantity = serializedObject.FindProperty("consumableQuantity");
//        coloredItem = serializedObject.FindProperty("coloredItem");
//        itemColor = serializedObject.FindProperty("itemColor");
//        shieldPowerMin = serializedObject.FindProperty("shieldPowerMin");
//        shieldPowerMax = serializedObject.FindProperty("shieldPowerMax");
//        ammunitionType = serializedObject.FindProperty("ammunitionType");
//        ammunitionDamage = serializedObject.FindProperty("ammunitionDamage");
//        physAmmoSprite = serializedObject.FindProperty("physAmmoSprite");
//        projectilePrefab = serializedObject.FindProperty("projectilePrefab");
//        canBlock = serializedObject.FindProperty("canBlock");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        EditorGUILayout.PropertyField(itemName);
//        EditorGUILayout.PropertyField(weight);
//        EditorGUILayout.PropertyField(icon);
//        EditorGUILayout.PropertyField(value);
//        EditorGUILayout.PropertyField(stackableItem);
//        EditorGUILayout.PropertyField(equipableItem);

//        // Allows the user to provide a sprite library asset if the item is marked as equipable
//        if (equipableItem.boolValue)
//        {
//            EditorGUILayout.PropertyField(itemLibraryAsset);
//        }

//        // Show the stackableItem field if the item is marked as stackable

//        //EditorGUILayout.PropertyField(stackableItem);
//        //if (stackableItem.boolValue)
//        //{
//        //    EditorGUILayout.PropertyField(quantity);
//        //}

//        EditorGUILayout.PropertyField(itemType);

//        if ((ItemType)itemType.enumValueIndex == ItemType.Ammunition)
//        {
//            EditorGUILayout.PropertyField(ammunitionType);
//            EditorGUILayout.PropertyField(ammunitionDamage);
//            EditorGUILayout.PropertyField(physAmmoSprite);
//            EditorGUILayout.PropertyField(projectilePrefab);
//        }

//        // Show the weaponType field if the item is marked as a weapon
//        if (equipableItem.boolValue && (ItemType)itemType.enumValueIndex == ItemType.Weapon)
//        {
//            EditorGUILayout.PropertyField(weaponMode);

//            // Check if weaponType has changed
//            EditorGUI.BeginChangeCheck();
//            EditorGUILayout.PropertyField(weaponType);
//            if (EditorGUI.EndChangeCheck())
//            {
//                // WeaponType has changed, set default values based on the new weaponType
//                SetDefaultWeaponValues();
//            }

//            if ((WeaponMode)weaponMode.enumValueIndex != WeaponMode.Ranged)
//            {
//                EditorGUILayout.PropertyField(weaponRange);
//                EditorGUILayout.PropertyField(weaponAngle);
//                EditorGUILayout.PropertyField(weaponDamage);
//                EditorGUILayout.PropertyField(canBlock);
//            }
//        }

//        if (equipableItem.boolValue && (ItemType)itemType.enumValueIndex == ItemType.Armour)
//        {
//            EditorGUILayout.PropertyField(armourType);
//            EditorGUILayout.PropertyField(armourDefence);
//        }

//        if (equipableItem.boolValue && (ItemType)itemType.enumValueIndex == ItemType.Shield)
//        {
//            EditorGUILayout.PropertyField(shieldTrailAsset);
//            EditorGUILayout.PropertyField(shieldPowerMin);
//            EditorGUILayout.PropertyField(shieldPowerMax);
//        }

//        if ((ItemType)itemType.enumValueIndex == ItemType.Consumable)
//        {
//            EditorGUILayout.PropertyField(consumableType);
//            EditorGUILayout.PropertyField(consumableQuantity);
//        }

//        if (equipableItem.boolValue && (ItemType)itemType.enumValueIndex == ItemType.Armour)
//        {
//            EditorGUILayout.PropertyField(coloredItem);
//        }

//        if (equipableItem.boolValue && coloredItem.boolValue)
//        {
//            EditorGUILayout.PropertyField(itemColor);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }

//    private void SetDefaultWeaponValues()
//    {
//        WeaponType selectedWeaponType = (WeaponType)weaponType.enumValueIndex;

//        // Set default values based on the selected weaponType
//        switch (selectedWeaponType)
//        {
//            case WeaponType.Sword:
//                weaponRange.floatValue = 5.0f;
//                weaponAngle.floatValue = 3.0f;
//                break;
//            case WeaponType.Spear:
//                // Sets default values for spears
//                weaponRange.floatValue = 0.0f;
//                weaponAngle.floatValue = 0.0f;
//                break;
//            // Add cases for other weapon types with their default values
//            default:
//                weaponRange.floatValue = 0.0f;
//                weaponAngle.floatValue = 0.0f;
//                break;
//        }
//    }
//}
#endif