using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Items;
using Core.Enums;

namespace InventoryManagement
{
    [Serializable]
    public class Inventory : MonoBehaviour
    {
        [BoxGroup("Inventory Settings")]
        [InlineEditor(Expanded = true)]
        [SerializeField, Tooltip("The container holding all inventory items and quantities.")]
        private InventoryContainer inventoryContainer;

        [BoxGroup("Inventory Settings")]
        [SerializeField, Tooltip("Maximum weight the inventory can hold.")]
        private float maxWeight = 100.0f;

        [BoxGroup("Inventory Settings")]
        [ReadOnly, Tooltip("Current total weight of items in the inventory.")]
        private float currentWeight;

        [SerializeField, Tooltip("Amount of gold the inventory holds.")]
        [MinValue(0)]
        private int currency;

        public Action<Item> OnItemRemoved;

        public void AddItem(Item item, int quantity = 1) => inventoryContainer.AddItem(item, quantity);

        public void AddItemByName(string addedItem, int quantity = 1) => inventoryContainer.AddItemByName(addedItem, quantity);

        public void RemoveItem(Item item, int quantity = 1) => inventoryContainer.RemoveItem(item, quantity);

        public void RemoveItemByID(string itemID, int quantity = 1) => inventoryContainer.RemoveItemByID(itemID, quantity);

        public InventoryContainer GetInventoryContainer() => inventoryContainer;

        /// <summary>
        /// Removes all of the item from the inventory
        /// </summary>
        public void RemoveAllOfItem(Item item)
        {
            inventoryContainer.RemoveAllInstancesOfItem(item);
            OnItemRemoved?.Invoke(item);
        }

        public Item GetAmmunitionForWeapon(WeaponType weaponType, bool removeAmmo = true)
        {
            Debug.LogError("METHOD NOT IMPLEMENTED");
            return null;

            //foreach (var item in inventoryContainer.GetInventoryEntries())
            //{
            //    if (item.GetType() == typeof(AmmunitionItem))
            //    {
            //        AmmunitionItem ammoItem = (AmmunitionItem)item;

            //        if (item.GetType() == typeof(AmmunitionItem) && weaponType == ammoItem.ammunitionType)
            //        {
            //            if (removeAmmo)
            //            {
            //                RemoveItem(item);
            //            }

            //            return item;
            //        }
            //    }
            //}

            return null;
        }

        public int GoldCount()
        {
            return currency;
        }

        public void AddGold(int amount)
        {
            currency += amount;
        }
        
        public void RemoveGold(int amount)
        {
            currency -= amount;
        }

        public void ClearGold()
        {
            currency = 0;
        }

        public bool ContainsItem(Item item) => inventoryContainer.ContainsInstanceOfItem(item);
        public bool ContainsItemByName(string itemName) => inventoryContainer.ContainsInstanceOfItem(itemName);


        public void ClearItems() => inventoryContainer.ClearInventory();

        public List<Item> GetItems() => inventoryContainer.GetAllItems();

        public List<EquippableItem> GetEquippableItems()
        {
            List<EquippableItem> equippableItems = new();

            foreach (var item in GetItems())
            {
                if (item.IsEquippable)
                {
                    equippableItems.Add((EquippableItem)item);
                }
            }

            return equippableItems;
        }

        public int GetQuantityOfItem(Item item) => inventoryContainer.GetQuantityOfItem(item);
    }
}