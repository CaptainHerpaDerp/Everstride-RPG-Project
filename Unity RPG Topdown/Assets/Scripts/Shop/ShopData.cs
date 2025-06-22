using System;
using System.Collections.Generic;
using UnityEngine;
using Items;
using InventoryManagement;

namespace PlayerShopping
{
    [CreateAssetMenu(fileName = "New Shop Data", menuName = "Shop Data")]
    [Serializable]
    public class ShopData : ScriptableObject
    {
        public string shopOwner = "Seller";

        public int sellerGold = 0;

        public List<Item> vendorItems = new();

        public void Initialize()
        {
            //List<Item> list = new(defaultItems);

            //items.Clear();

            //for (int i = 0; i < list.Count; i++)
            //{
            //    Item item = Instantiate(list[i]);
            //    items.Add(item);
            //}

            //for (int i = 0; i < items.Count; i++)
            //{
            //    if (!items[i].stackableItem)
            //        continue;

            //    if (i < itemQuantities.Count)
            //    items[i].quantity = itemQuantities[i];
            //}
        }

        public void AddItem(Item item, int quantity = 1)
        {
            //Item inventoryItem = items.Find(x => x == item);
            //if (inventoryItem != null)
            //{
            //    inventoryItem.quantity += quantity;
            //}
            //else
            //{
            //    items.Add(item);
            //}
        }

        public void RemoveItem(Item item, int quantity = 1)
        {
            //Item inventoryItem = items.Find(x => x == item);
            //if (inventoryItem != null)
            //{
            //    inventoryItem.quantity -= quantity;
            //    if (inventoryItem.quantity <= 0)
            //    {
            //        items.Remove(inventoryItem);
            //    }
            //}
        }
    }
}