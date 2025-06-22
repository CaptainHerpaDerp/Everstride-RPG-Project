using Items;
using UnityEngine;
using System;

namespace InventoryManagement
{
    public class WeaponEquipmentSlot : EquipmentSlot
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
            if (newBind.GetType() != typeof(WeaponItem))
            {
                Debug.LogWarning("Trying to bind an item that does not match the equipment slot type!");
                return false;
            }

            if (boundItem == newBind)
            {
                // If the item is already bound to the slot, we return true as the item is already bound
                return true;
            }

            // Set the bound item to the new item
            boundItem = newBind;

            itemImage.sprite = boundItem.icon;
            itemImage.SetNativeSize();

            if (equipToPlayer)
                playerEquipmentHandler.EquipItem(newBind);

           // Debug.Log("Bound item to equipment slot");
            return true;
        }

        public override Type GetSlotType()
        {
            return typeof(WeaponItem);
        }
    }
}