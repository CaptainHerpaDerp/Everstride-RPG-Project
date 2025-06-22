using InventoryManagement;
using Items;
using System;
using UnityEngine;

public class ShieldEquipmentSlot : EquipmentSlot
{
    /// <summary>
    /// Bind an inventory item to an equipment slot
    /// </summary>
    /// <param name="newBind"></param>
    /// <param name="equipToPlayer"></param>
    /// <returns></returns>
    public override bool BindEquipment(EquippableItem newBind, bool equipToPlayer = true)
    {
        if (newBind == null)
        {
            Debug.LogWarning("Trying to bind a null item to the equipment slot!");
            return false;
        }

        // If the type of the equippable item does not match the equipment slot type, return false
        if (newBind.GetType() != typeof(ShieldItem))
        {
            return false;
        }

        // Check to see if the item we're trying to bind is already bound to the equipment slot
        if (boundItem == newBind)
        {
            Debug.LogWarning("Trying to bind an item that is already bound to the equipment slot!");
            return false;
        }

        // Set the bound item to the new item
        boundItem = newBind;

        itemImage.sprite = boundItem.icon;
        itemImage.SetNativeSize();

        if (equipToPlayer)
            playerEquipmentHandler.EquipItem(newBind);

        Debug.Log("Bound item to equipment slot");
        return true;
    }

    public override Type GetSlotType()
    {
        return typeof(ShieldItem);
    }
}
