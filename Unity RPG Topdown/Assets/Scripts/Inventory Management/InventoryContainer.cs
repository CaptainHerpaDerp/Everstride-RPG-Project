using System.Collections.Generic;
using UnityEngine;
using Items;
using Sirenix.OdinInspector;

namespace InventoryManagement
{
    [CreateAssetMenu(fileName = "InventoryContainer", menuName = "Inventory/Inventory Container")]
    public class InventoryContainer : ScriptableObject
    {
        [SerializeField, Tooltip("Current inventory entries")]
        [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
        public List<InventoryEntry> inventoryEntries = new();

        public void AddItemByName(string itemName, int quantity = 0)
        {
            Item item = ItemDatabase.Instance.GetItemByName(itemName);

            if (item != null)
            {
                AddItem(item, quantity);
            }
        }

        public void AddItem(Item item, int quantity = 1)
        {
            Debug.Log($"Adding {quantity} of {item.itemName} to the inventory");

            var entry = inventoryEntries.Find(e => e.item == item);

            if (entry != null)
            {
                // If the item is marked as stackable, simply increase the quantity of that item
                if (entry.item.stackableItem)
                {
                    Debug.Log($"Added {quantity} to an item with a quantity of {entry.quantity}");
                    entry.quantity += quantity;
                }

                // Otherwise, we need to create a new instance of the item
                else
                {
                    Debug.Log("Created a new instance of an item");
                    inventoryEntries.Add(new InventoryEntry { item = item, quantity = 0 });
                }
            }            

            // If the item doesn't exist in the inventory
            else
            {
                // If the added item is stackable, create an instance of the item and set its quantity to the given quantity
                if (item.stackableItem)
                {
                    Debug.Log("Instantiated a new stackable item");
                    inventoryEntries.Add(new InventoryEntry { item = item, quantity = quantity });
                }
                else
                {
                    Debug.Log($"Instantiating {quantity} new instances of an item");

                    // Otherwise, we need to make a new instance of the item by the number of the quantity given and set its quantity to 1
                    for (int i = 0; i < quantity; i++)
                    {               
                        inventoryEntries.Add(new InventoryEntry { item = item, quantity = 0 });
                    }
                }
            }
        }

        /// <summary>
        /// Remove the specified amount of an item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        public void RemoveItem(Item item, int quantity)
        {
            var entry = inventoryEntries.Find(e => e.item == item);
            if (entry != null)
            {
                // If the item is stackable, remove the specified quantity
                if (entry.item.stackableItem)
                {
                    // We need to check if the item is currently equipped
                    // TODO
                    entry.quantity -= quantity;
                    if (entry.quantity <= 0)
                    {
                        inventoryEntries.Remove(entry);
                    }                   
                }
                // Otherwise, remove the item entirely
                else
                {
                    inventoryEntries.Remove(entry);
                }
            }
        }

        /// <summary>
        /// Remove the specified amount of an item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        public void RemoveItemByID(string itemID, int quantity)
        {
            var entry = inventoryEntries.Find(e => e.item.ItemID == itemID);
            if (entry != null)
            {
                // If the item is stackable, remove the specified quantity
                if (entry.item.stackableItem)
                {
                    // We need to check if the item is currently equipped
                    // TODO
                    entry.quantity -= quantity;
                    if (entry.quantity <= 0)
                    {
                        inventoryEntries.Remove(entry);
                    }
                }
                // Otherwise, remove the item entirely
                else
                {
                    inventoryEntries.Remove(entry);
                }
            }
        }

        /// <summary>
        /// Remove an item entirely from the items list
        /// </summary>
        /// <param name="item"></param>
        public void RemoveAllInstancesOfItem(Item item)
        {
            var entry = inventoryEntries.Find(e => e.item == item);
            if (entry != null)
            {
                inventoryEntries.Remove(entry);
            }
        }

        public List<InventoryEntry> GetInventoryEntries() => inventoryEntries;

        public int GetQuantity(Item item)
        {
            var entry = inventoryEntries.Find(e => e.item == item);
            return entry?.quantity ?? 0;
        }

        public bool ContainsInstanceOfItem(Item item)
        {
            var entry = inventoryEntries.Find(e => e.item == item);

            return entry != null;
        }

        /// <summary>
        /// Check if an item is in the inventory with the specified name or ID
        /// </summary>
        /// <param name="itemSearch"></param>
        /// <returns></returns>
        public bool ContainsInstanceOfItem(string itemSearch)
        {
            foreach (var entry in inventoryEntries)
            {
                if (entry == null)
                    continue;

                if (entry.item.itemName == itemSearch)
                {
                    return true;
                }

                if (entry.item.ItemID == itemSearch)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearInventory()
        {
            inventoryEntries.Clear();
        }

        public List<Item> GetAllItems()
        {
            List<Item> itemList = new();

            foreach (var entry in inventoryEntries)
            {
                itemList.Add(entry.item);
            }

            return itemList;
        }

        public int GetQuantityOfItem(Item item)
        {
            InventoryEntry entry = inventoryEntries.Find(e => e.item == item);

            if (entry != null)
            {
                return entry.quantity;
            }

            Debug.LogWarning("Error in Inventory Container! - Attempting to get the quantity of an item that isn't part of the container");

            return 0;
        }
    }
}