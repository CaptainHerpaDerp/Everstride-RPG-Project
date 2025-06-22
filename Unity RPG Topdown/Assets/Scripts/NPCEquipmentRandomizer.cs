using Core;
using InventoryManagement;
using Items;
using System.Collections.Generic;
using UnityEngine;

public class NPCEquipmentRandomizer : Singleton<NPCEquipmentRandomizer>
{
    public List<RandomEquipmentProfile> equipmentProfileList;
    public int GetRandomGoldAmount(string profileName)
    {
        RandomEquipmentProfile equipmentProfile = null;

        foreach (RandomEquipmentProfile profile in equipmentProfileList)
        {
            if (profile.profileName.ToLower() == profileName.ToLower())
            {
                equipmentProfile = profile;
            }
        }

        if (equipmentProfile == null)
        {
            Debug.LogError("No equipment profile found with the name: " + profileName);
            return 0;
        }

        return Random.Range(equipmentProfile.goldRangeMin, equipmentProfile.goldRangeMax);
    }

    public List<Item> GiveRandomItems(EquipmentHandler equipmentHandler, string profileName)
    {
        RandomEquipmentProfile equipmentProfile = null;

        foreach (RandomEquipmentProfile profile in equipmentProfileList)
        {
            if (profile.profileName.ToLower() == profileName.ToLower())
            {
                equipmentProfile = profile;
            }
        }

        if (equipmentProfile == null)
        {
            Debug.LogError("No equipment profile found with the name: " + profileName);
            return null;
        }


        // Create the final list that the NPC will be equipped with
        List<Item> items = new();

        // Clear the target inventory
        equipmentHandler.TargetInventory.ClearItems();

        AddItem(items, equipmentProfile, equipmentProfile.PossibleHelmets, equipmentProfile.HelmetLikelihoods);
        AddItem(items, equipmentProfile, equipmentProfile.PossibleArmour, equipmentProfile.ArmourLikelihoods);
        AddItem(items, equipmentProfile, equipmentProfile.PossibleWeapons, equipmentProfile.WeaponLikelihoods);

        if (equipmentProfile.MiscItemCount > 0)
        for (int i = 0; i < equipmentProfile.MiscItemCount; i++)
        {
            AddItem(items, equipmentProfile, equipmentProfile.PossibleMiscItems, equipmentProfile.MiscItemLikelihoods);
        }

        return items;
    }

    private void AddItem(List<Item> items, RandomEquipmentProfile equipmentProfile, List<Item> possibleItems, List<int> itemProbabilities)
    {
        System.Random rand = new();

        // Equip a random chestplate
        Dictionary<Item, int> weapons = new();

        for (int i = 0; i < possibleItems.Count; i++)
        {
            weapons.Add(possibleItems[i], itemProbabilities[i]);
        }

        // Create a list of chestplates weighted by their likelihoods
        List<Item> weightedList = new();
        foreach (var kvp in weapons)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                weightedList.Add(kvp.Key);
            }
        }

        // Randomly select a chestplate from the weighted list
        Item selectedWeapon = weightedList[rand.Next(weightedList.Count)];

        items.Add(selectedWeapon);
    }
}
