using Core.Enums;
using Items;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
namespace InventoryManagement
{
    public class ArmourEquipmentSlot : EquipmentSlot
    {
        [EnumToggleButtons]
        public ArmourType armourType;

        /// <summary>
        /// Bind an inventory item to an equipment slot
        /// </summary>
        /// <param name="newBind"></param>
        /// <param name="equipToPlayer"></param>
        /// <returns></returns>
        public override bool BindEquipment(EquippableItem newBind, bool equipToPlayer = true)
        {
            // If the type of the equippable item does not match the equipment slot type, return false
            if (newBind.GetType() != typeof(ArmourItem))
            {
                Debug.LogWarning("Trying to bind an item that does not match the equipment slot type!");
                return false;
            }

            // If the item being bound is armour and the armour type does not match the equipment slot's armour type, return false    
            if (armourType != ((ArmourItem)newBind).armourType)
            {
                Debug.LogWarning("Trying to bind an item that does not match the equipment slot armour type!");
                return false;
            }        

            if (boundItem == newBind)
            {
                // If the item is already bound to the slot, we return true as the item is already bound
                Debug.LogWarning("Item is already bound to the equipment slot");
                return true;
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
            return typeof(ArmourItem);
        }
    }
}