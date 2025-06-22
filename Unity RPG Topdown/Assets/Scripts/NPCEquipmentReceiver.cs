
using UnityEngine;
using Items;
using System.Collections.Generic;
using InventoryManagement;

public class NPCEquipmentReceiver : MonoBehaviour
{
    public EquipmentHandler EquipmentHandler;

    [Header("The name of the profile to use")]
    public string ProfileName;

    NPCEquipmentRandomizer randomizer;

    private void Start()
    {
        if (EquipmentHandler == null && !TryGetComponent(out EquipmentHandler))
        {
            Debug.LogError($"Error on NPC {gameObject.name}: Equipment Handler instance not found on the gameobject!");
        }

        randomizer = NPCEquipmentRandomizer.Instance;

        if (randomizer == null)
        {
            randomizer = FindObjectOfType<NPCEquipmentRandomizer>();
        }

        EquipRandomItems();
    }

    public void EquipRandomItems()
    {
        if (EquipmentHandler == null)
            return;

        //Clear the current items
        EquipmentHandler.TargetInventory.ClearItems();  
        EquipmentHandler.TargetInventory.ClearGold();

        // Get a list of random items from the randomizer
        List<Item> randItemList = randomizer.GiveRandomItems(EquipmentHandler, ProfileName);

        // Add all of the randomly assigned items to the inventory
        foreach (Item item in randItemList)
        {
            EquipmentHandler.TargetInventory.AddItem(item);
        }

        int goldAmount = randomizer.GetRandomGoldAmount(ProfileName);

        EquipmentHandler.TargetInventory.AddGold(goldAmount);

        // Equips the best items in the inventory
        EquipmentHandler.EquipBestItems();
    }
}
