using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;
using InventoryManagement;
using UIElements;
using System;
using Core;
using System.Linq;

namespace Characters.Interactables
{
    /// <summary>
    /// The menu that would appear when the player interacts with a loot point
    /// </summary>
    public class LootMenu : FadeablePanel
    {
        public static LootMenu Instance { get; private set; }

        [SerializeField] private Transform itemBarParent;
        [SerializeField] private GameObject itemBarPrefab, moneyItemBarPrefab;
        [SerializeField] private Inventory playerInventory;

        [SerializeField] private Transform elementGroup;
        [SerializeField] private GameObject mainPanel;

        private LootPoint lootPoint;

        [SerializeField] private KeyCode lootAllKey;

        private List<GameObject> itemBars = new();

        [SerializeField] private Inventory targetInventory;

        private EventBus eventBus;

        private bool isOpen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple LootMenu Instances Present!");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            eventBus = EventBus.Instance;
            eventBus.Subscribe("CloseMenu", CloseLootMenu);
        }

        public bool IsOpen()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Initializes the loot menu with the items from the dead character
        /// </summary>
        public void OpenLootMenu(string characterName, Inventory characterInventory, LootPoint targetLootPoint)
        {
            eventBus.Publish("OpenUIMenu");

            isOpen = true;

            elementGroup.gameObject.SetActive(true);
            FadeIn(elementGroup);

            lootPoint = targetLootPoint;
            targetInventory = characterInventory;
            List<Item> items = characterInventory.GetItems();

            int goldAmount = characterInventory.GoldCount();

            GameObject newGoldItemBar = Instantiate(moneyItemBarPrefab, itemBarParent);
            newGoldItemBar.GetComponent<ItemBar>().SetValue(goldAmount);
            newGoldItemBar.GetComponent<ItemBar>().SetName("Gold");

            itemBars.Add(newGoldItemBar);

            newGoldItemBar.GetComponent<Button>().onClick.AddListener(() => AddGold(newGoldItemBar, goldAmount));

            // TODO: Sort by item value

            for (int i = 0; i < items.Count; i++)
            {

                GameObject newItemBar = Instantiate(itemBarPrefab, itemBarParent);
                newItemBar.GetComponent<ItemBar>().InitializeBar(items[i], items[i].icon, items[i].itemName, 9999, items[i].value, 9999);

                itemBars.Add(newItemBar);

                Item item = items[i];
                newItemBar.GetComponent<Button>().onClick.AddListener(() => AddItem(newItemBar, item));
            }
        }

        private void AddGold(GameObject itemBar, int currencyCount)
        {
            playerInventory.AddGold(currencyCount);
            targetInventory.ClearGold();
            Destroy(itemBar);

            if (targetInventory.GetItems().Count == 0)
            {
                lootPoint.DestroyLootPoint();
                CloseLootMenu();
            }
        }

        /// <summary>
        /// Adds item from the loot menu to the player's inventory
        /// </summary>
        private void AddItem(GameObject itemBar, Item item)
        {
            // Add item to inventory
            //if (item.itemName == "Gold")
            //{
            //    playerInventory.AddGold(item.quantity);
            //    targetInventory.ClearGold();
            //}
            //else
            //{
            //    Debug.Log($"Adding x{item.quantity} of {item.name} to player inventory");
            //    playerInventory.AddItem(item, item.quantity);
            //}

            targetInventory.RemoveAllOfItem(item);
            Destroy(itemBar);

            // Close menu if no more items
            if (targetInventory.GetItems().Count == 0)
            {
                lootPoint.DestroyLootPoint();
                CloseLootMenu();
            }
        }

        private void DestroyAllItemBars()
        {
            for (int i = 0; i < itemBars.Count; i++)
            {
                Destroy(itemBars[i]);
            }
        }

        /// <summary>
        /// Closes the loot menu
        /// </summary>
        public void CloseLootMenu()
        {
            eventBus.Publish("CloseUIMenu");

            isOpen = false;

            DestroyAllItemBars();
            FadeOut(elementGroup);
        }

        public void LootAll()
        {
            //foreach (var item in targetInventory.GetItems().ToList())
            //{
            //    if (item.itemName == "Gold")
            //    {
            //        playerInventory.AddGold(item.quantity);
            //        targetInventory.ClearGold();
            //    }
            //    else
            //    {
            //        Debug.Log($"Adding x{item.quantity} of {item.name} to player inventory");
            //        playerInventory.AddItem(item, item.quantity);

            //        targetInventory.RemoveAllOfItem(item);
            //    }
            //}

            lootPoint.DestroyLootPoint();
            CloseLootMenu();
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(lootAllKey))
                LootAll();
        }
    }
}