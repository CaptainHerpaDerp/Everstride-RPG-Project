using Items;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomEquipmentProfile))]
public class RandomEquipmentProfileEditor : Editor
{
    RandomEquipmentProfile profile;

    private void OnEnable()
    {
        profile = (RandomEquipmentProfile)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RandomEquipmentProfile equipmentProfile = (RandomEquipmentProfile)target;

        if (GUILayout.Button("Reset All"))
        {
            profile.PossibleHelmets.Clear();
            profile.HelmetLikelihoods.Clear();
            profile.PossibleArmour.Clear();
            profile.ArmourLikelihoods.Clear();
            profile.PossibleBoots.Clear();
            profile.BootLikelihoods.Clear();
            profile.PossibleWeapons.Clear();
            profile.WeaponLikelihoods.Clear();
        }

        EditorGUILayout.Space(6);

        profile.profileName = EditorGUILayout.TextField("Profile Name", profile.profileName);

        #region Helmet Section

        EditorGUILayout.Space(6);

        // Add a bold label
        EditorGUILayout.LabelField("Helmet", EditorStyles.boldLabel);

        EditorGUILayout.Space(3);

        for (int i = 0; i < profile.PossibleHelmets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            profile.PossibleHelmets[i] = EditorGUILayout.ObjectField(profile.PossibleHelmets[i], typeof(Item), true) as Item;
            profile.HelmetLikelihoods[i] = EditorGUILayout.IntField("LikelyHood", profile.HelmetLikelihoods[i]);

            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
            if (GUILayout.Button("X", buttonOptions))
            {
                profile.PossibleHelmets.RemoveAt(i);
                profile.HelmetLikelihoods.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item"))
        {
            profile.PossibleHelmets.Add(null);
            profile.HelmetLikelihoods.Add(0);
        }

        EditorGUILayout.Space(6);

        #endregion

        #region ChestPlates Section

        EditorGUILayout.Space(6);

        // Add a bold label
        EditorGUILayout.LabelField("ChestPlates", EditorStyles.boldLabel);

        EditorGUILayout.Space(3);

        for (int i = 0; i < profile.PossibleArmour.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            profile.PossibleArmour[i] = EditorGUILayout.ObjectField(profile.PossibleArmour[i], typeof(Item), true) as Item;
            profile.ArmourLikelihoods[i] = EditorGUILayout.IntField("LikelyHood", profile.ArmourLikelihoods[i]);

            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
            if (GUILayout.Button("X", buttonOptions))
            {
                profile.PossibleArmour.RemoveAt(i);
                profile.ArmourLikelihoods.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item"))
        {
            profile.PossibleArmour.Add(null);
            profile.ArmourLikelihoods.Add(0);
        }

        EditorGUILayout.Space(6);

        #endregion

        #region Weapon Section

        EditorGUILayout.Space(6);

        // Add a bold label
        EditorGUILayout.LabelField("Weapons", EditorStyles.boldLabel);

        EditorGUILayout.Space(3);

        for (int i = 0; i < profile.PossibleWeapons.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            profile.PossibleWeapons[i] = EditorGUILayout.ObjectField(profile.PossibleWeapons[i], typeof(Item), true) as Item;
            profile.WeaponLikelihoods[i] = EditorGUILayout.IntField("LikelyHood", profile.WeaponLikelihoods[i]);

            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
            if (GUILayout.Button("X", buttonOptions))
            {
                profile.PossibleWeapons.RemoveAt(i);
                profile.WeaponLikelihoods.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item"))
        {
            profile.PossibleWeapons.Add(null);
            profile.WeaponLikelihoods.Add(0);
        }

        EditorGUILayout.Space(6);
         
        #endregion

        EditorGUILayout.LabelField("Misc Items", EditorStyles.boldLabel);
        DisplayDictionary(profile.PossibleMiscItems, profile.MiscItemLikelihoods);

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("Misc Item Count", EditorStyles.boldLabel);
        profile.MiscItemCount = EditorGUILayout.IntField("Misc Item Count", profile.MiscItemCount);

        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Gold Range", EditorStyles.boldLabel);
        profile.goldRangeMin = EditorGUILayout.IntField(profile.goldRangeMin);
        profile.goldRangeMax = EditorGUILayout.IntField(profile.goldRangeMax);

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(profile);
        }
    }

    private void DisplayDictionary(List<Item> possibleItems, List<int> itemLikelyhoods)
    {
        for (int i = 0; i < possibleItems.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            possibleItems[i] = EditorGUILayout.ObjectField(possibleItems[i], typeof(Item), true) as Item;
            itemLikelyhoods[i] = EditorGUILayout.IntField("LikelyHood", itemLikelyhoods[i]);

            GUILayoutOption[] buttonOptions = { GUILayout.Width(20) };
            if (GUILayout.Button("X", buttonOptions))
            {
                possibleItems.RemoveAt(i);
                itemLikelyhoods.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item"))
        {
            possibleItems.Add(null);
            itemLikelyhoods.Add(0);
        }
    }
}
