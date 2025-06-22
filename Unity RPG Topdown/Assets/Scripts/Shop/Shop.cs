using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;
using TMPro;
using PlayerShopping;
using Core;
using UIElements;

namespace InventoryManagement
{
    public class Shop : BaseInventory
    {
        // The main panel of the shop
        [SerializeField] GameObject mainPanel;

        // The panel that contains the non-fading elements of the shop
        [SerializeField] GameObject nonFadingElements;

        // The grid that stores the player's equippable inventory items
        [SerializeField] Transform sellerInventorySlotParent;

        // Text that displays the player's and seller's gold
        [SerializeField] TextMeshProUGUI playerGold, sellerGold, sellerLabel;

        [SerializeField] Inventory playerInventory;

        private ShopData linkedShopData;

        private EventBus eventBus;

        [SerializeField] private Button exitButton;

        protected override void Start()
        {
            base.Start();

            eventBus = EventBus.Instance;
            eventBus.Subscribe("CloseMenu", CloseShop);

            exitButton.onClick.AddListener(ExitShop);
        }

        /// <summary>
        /// Close the shop by other means than the exit key from the menu controller
        /// </summary>
        private void ExitShop()
        {
            eventBus.Publish("CloseUIMenu");
            CloseShop();
        }

        public void OpenShop(ShopData shopData)
        {
            if (shopData == null)
            {
                Debug.LogError("Cannot open shop, Shop data is null!");
                return;
            }

            // Sets the linked shop data to the shop data passed in
            linkedShopData = shopData;

            // Activates the main panel
            gameObject.SetActive(true);

            // Fades in the main panel
            FadeIn(mainPanel);

            // Activates the non-fading elements of the shop
            nonFadingElements.SetActive(true);

            if (eventBus == null)
            {
                Debug.LogWarning("Event bus is null, getting instance");

                eventBus = EventBus.Instance;
            }

            // Tells the menu controller that a menu is open so that the player can't move and the camera can't zoom
            eventBus.Publish("OpenUIMenu");

            // Sets the seller's label to the shop owner's name
            sellerLabel.SetText(linkedShopData.shopOwner + "' Items");

            UpdateGoldValues();

            //InitializeShopInventory();

            // Do not do this every time the shop opens! This is temporary! 
            linkedShopData.Initialize();

            PopulatePlayerInventory();
            PopulateSellerInventory();

            ReloadItemQuantities();
            //ReloadSellerItemQuantities();
        }

        public void CloseShop()
        {
            //linkedShopData.itemQuantities.Clear();

            //for (int i = 0; i < linkedShopData.items.Count; i++)
            //{
            //    linkedShopData.itemQuantities.Add(linkedShopData.items[i].quantity);
            //}

            //itemInformationPanel.Hide();

            //nonFadingElements.SetActive(false);

            //if (gameObject.activeInHierarchy)
            //FadeOut(mainPanel);
        }

        /// <summary>
        /// Adds all items from the shop data to the shop's inventory manager
        /// </summary>
        private void InitializeShopInventory()
        {
            //List<Item> newItemsList = new();

            //foreach (Item item in linkedShopData.items)
            //{
            //    Item newItem = Instantiate(item);
            //    newItemsList.Add(newItem);
            //}

            //playerInventory.ClearItems();
            //playerInventory.GetItems().AddRange(newItemsList);
        }

        /// <summary>
        /// Populates the seller's inventory from the shops's inventory manager
        /// </summary>
        private void PopulateSellerInventory()
        {
            // Destroys all children of the seller's inventory slot parent
            //foreach (Transform child in sellerInventorySlotParent)
            //{
            //    if (child != presetItem.transform)
            //        Destroy(child.gameObject);
            //}

            //foreach (Item item in linkedShopData.items)
            //{
            //    GameObject newSlot = Instantiate(presetItem, sellerInventorySlotParent);

            //    GameObject newItem = newSlot.transform.GetChild(0).gameObject;
            //    GameObject textObject = newSlot.transform.GetChild(1).gameObject;

            //    newItem.GetComponent<Image>().sprite = item.icon;
            //    newItem.GetComponent<InventoryItem>().SetItem(item);
            //    newItem.GetComponent<Image>().SetNativeSize();

            //    if (item.quantity > 1)
            //    {
            //        textObject.SetActive(true);
            //        textObject.GetComponentInChildren<TextMeshProUGUI>().SetText(item.quantity.ToString());
            //    }
            //    else
            //    {
            //        textObject.SetActive(false);
            //    }

            //    newSlot.SetActive(true);
            //}
        }

        /// <summary>
        /// Populates the player inventory from the player's inventory manager
        /// </summary>
        private void PopulatePlayerInventory()
        {
            // Destroys all children of the player's inventory slot parent
            //foreach (Transform child in playerInventorySlotParent)
            //{
            //    if (child != presetItem.transform)
            //        Destroy(child.gameObject);
            //}

            //foreach (Item item in playerInventory.GetItems())
            //{
            //    GameObject newSlot = Instantiate(presetItem, playerInventorySlotParent);

            //    GameObject newItem = newSlot.transform.GetChild(0).gameObject;
            //    GameObject textObject = newSlot.transform.GetChild(1).gameObject;   

            //    newItem.GetComponent<Image>().sprite = item.icon;
            //    newItem.GetComponent<InventoryItem>().SetItem(item);
            //    newItem.GetComponent<Image>().SetNativeSize();

            //    if (item.quantity > 1)
            //    {
            //        textObject.SetActive(true);
            //        textObject.GetComponentInChildren<TextMeshProUGUI>().SetText(item.quantity.ToString());
            //    }
            //    else
            //    {
            //        textObject.SetActive(false);
            //    }

            //    newSlot.SetActive(true);
            //}
        }

        /// <summary>
        /// Reloads the text representing the gold values of the player and seller
        /// </summary>
        private void UpdateGoldValues()
        {
            playerGold.SetText(playerInventory.GoldCount().ToString());
            sellerGold.SetText(linkedShopData.sellerGold.ToString());
        }

        private void DoItemTransfer()
        {
            //if (Input.GetMouseButtonDown(1) && hoveredItem != null)
            //{
            //    Item item = hoveredItem.Item;

            //    // Deselects the hovered item if the item's quantity is 1, as the item will be removed from the inventory
            //    if (item.quantity == 1)
            //        DeselectHoveredItem();

            //    // Sell Item
            //    if (playerInventory.ContainsItem(item))
            //    {
            //        Debug.Log("Selling item");

            //        if (linkedShopData.sellerGold < item.value)
            //        {
            //            Debug.LogWarning("Seller does not have enough gold!");
            //            return;
            //        }

            //        if (equipmentInventory.UnequipItem(item))
            //        {
            //            Debug.Log("Unequipped item");
            //        }

            //        soundManager.PlaySound(shopSoundLibrary.RandomClip(shopSoundLibrary.sellSounds));

            //        // Adds the item's value to the player's gold
            //        playerInventory.AddGold(item.value);
            //        linkedShopData.sellerGold -= item.value;

            //        TransferPlayerToSeller(item);

            //        // TODO: this is not ideal, but it guarantees items in players visual inventory are acutally part of the inventory
            //        PopulatePlayerInventory();
            //    }

            //    // Buy Item
            //    else if (ContainsInstance(linkedShopData.items, item))
            //    {
            //        Debug.Log("Buying item");

            //        if (playerInventory.GoldCount() < item.value)
            //        {
            //            Debug.LogWarning("Not enough gold!");
            //            return;
            //        }

            //        playerInventory.AddGold(-item.value);
            //        linkedShopData.sellerGold += item.value;

            //        TransferSellerToPlayer(item);

            //        PopulateSellerInventory();
            //    }

            //    else
            //    {
            //        Debug.LogWarning("Item not found within inventories!");
            //    }

            //    // Reload the player and seller's display item quantities
            //    ReloadItemQuantities();
            //    ReloadSellerItemQuantities();

            //    UpdateGoldValues();
            //}
        }

        protected void ReloadSellerItemQuantities()
        {
            //foreach (Transform child in sellerInventorySlotParent)
            //{
            //    if (child == null)
            //        continue;

            //    if (child != presetItem.transform)
            //    {
            //        InventoryItem inventoryItem = child.GetChild(0).GetComponent<InventoryItem>();

            //        if (inventoryItem.Item == null)
            //        {
            //            continue;
            //        }

            //        if (inventoryItem.Item.stackableItem && inventoryItem.Item.quantity > 1)
            //        {
            //            child.GetChild(1).gameObject.SetActive(true);
            //            child.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().SetText(inventoryItem.Item.quantity.ToString());
            //        }
            //        else
            //        {
            //            child.GetChild(1).gameObject.SetActive(false);
            //        }
            //    }
            //}
        }
        void TransferPlayerToSeller(Item item)
        {
            //for (int i = 0; i < playerInventorySlotParent.childCount; i++)
            //{
            //    // Verifies that the item is in the player's inventory
            //    if (playerInventorySlotParent.GetChild(i).GetComponentInChildren<InventoryItem>().Item == item)
            //    {
            //        DeselectHoveredItem();
            //        HidePointer();

            //        // If the item's quantity is 1, it is removed from the player's display inventory
            //        if (item.quantity == 1 || !item.stackableItem)
            //        {
            //            Destroy(playerInventorySlotParent.GetChild(i).gameObject);
            //        }

            //        // Check if the item is already in the seller's inventory, and if it is, increase the quantity by 1
            //        bool duplicate = false;

            //        foreach (var sellerInventoryItem in linkedShopData.items)
            //        {
            //            if (item.stackableItem && sellerInventoryItem.itemName == item.itemName)
            //            {
            //                linkedShopData.AddItem(sellerInventoryItem);
            //                duplicate = true;
            //            }
            //        }

            //        if (!duplicate)
            //        {
            //            // Instantiate a new Item object for the seller
            //            Item newItemForSeller = Instantiate(item);
            //            newItemForSeller.quantity = 1;

            //            InstantiateItemForSeller(newItemForSeller);

            //            linkedShopData.AddItem(newItemForSeller);
            //        }

            //        // Removes the item from the player's inventory, or decreases the quantity by 1 if the quantity is greater than 1
            //        playerInventory.RemoveItem(item);

            //        return;
            //    }
            //}

            //Debug.LogError("The purchased item was not located in the buyer's inventory!");

        }

        void TransferSellerToPlayer(Item item)
        {
            //for (int i = 0; i < sellerInventorySlotParent.childCount; i++)
            //{
            //    // Verifies that the item is in the seller's inventory
            //    if (sellerInventorySlotParent.GetChild(i).GetComponentInChildren<InventoryItem>().Item == item)
            //    {
            //        DeselectHoveredItem();
            //        HidePointer();

            //        // If the item's quantity is 1, it is removed from the player's display inventory
            //        if (item.quantity == 1 || !item.stackableItem)
            //        {
            //            Destroy(sellerInventorySlotParent.GetChild(i).gameObject);
            //        }

            //        // Check if the item is already in the seller's inventory, and if it is, increase the quantity by 1
            //        bool duplicate = false;

            //        foreach (var buyerInventoryItem in playerInventory.GetItems())
            //        {
            //            if (item.stackableItem && buyerInventoryItem.itemName == item.itemName)
            //            {
            //                playerInventory.AddItem(buyerInventoryItem);
            //                duplicate = true;
            //            }
            //        }

            //        if (!duplicate)
            //        {
            //            // Instantiate a new Item object for the seller                        
            //            Item newItemForPlayer = Instantiate(item);
            //            newItemForPlayer.quantity = 1;

            //            InstantiateItemForPlayer(newItemForPlayer);

            //            playerInventory.AddItem(newItemForPlayer);
            //        }

            //        // Removes the item from the player's inventory, or decreases the quantity by 1 if the quantity is greater than 1
            //        linkedShopData.RemoveItem(item);

            //        return;
            //    }
            //}

            Debug.LogError("The purchased item was not located in the seller's inventory!");
        }

        void InstantiateItemForSeller(Item item)
        {
            // Instantiate a new item for the seller's inventory
            InventorySlot newItemSlot = Instantiate(presetItemSlot, sellerInventorySlotParent);

            presetItemSlot.SetItem(item);

            newItemSlot.gameObject.SetActive(true);
        }

        void InstantiateItemForPlayer(Item item)
        {
            // Instantiate a new item for the player's inventory
            InventorySlot newItemSlot = Instantiate(presetItemSlot, playerInventorySlotParent);

            presetItemSlot.SetItem(item);
             
            newItemSlot.gameObject.SetActive(true);
        }

        private bool ContainsInstance(List<Item> items, Item item)
        {
            foreach (Item i in items)
            {
                if (i.GetInstanceID() == item.GetInstanceID())
                {
                    return true;
                }
            }

            return false;
        }

        void Update()
        {
            DoItemTransfer();
        }
    }
}