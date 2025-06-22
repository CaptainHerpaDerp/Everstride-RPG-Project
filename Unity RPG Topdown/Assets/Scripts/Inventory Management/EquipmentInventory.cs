using UnityEngine;
using UnityEngine.EventSystems;
using Items;
using Characters;
using UIElements;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Core.Enums;
using UnityEngine.UI;
using System;

// TODO: This should maybe be under the UI Windows namespace

namespace InventoryManagement
{
    public class EquipmentInventory : BaseInventory, IPointerEnterHandler, IPointerExitHandler
    {
        public static EquipmentInventory Instance { get; private set; }

        // The grid that stores the player's equipment slots
        [SerializeField] Transform playerEquipmentSlotParent;

        // Temp?
        [SerializeField] private EquipmentHandler playerEquipmentHandler;

        // A reference to the item selector that will be used to show the corresponding equipment slot of the item currently being held
        [SerializeField] private GameObject availableEquipmentSlotPointer;

        private EquipmentTotals equipmentTotals;

        private InventorySlot selectedInventorySlot;
        private EquipmentSlot selectedEquipmentSlot;

        [FoldoutGroup("Equipment Slot")]
        [SerializeField] private ArmourEquipmentSlot helmetSlot, chestSlot, feetSlot;
        [FoldoutGroup("Equipment Slot")]
        [SerializeField] private WeaponEquipmentSlot weaponSlot;
        [FoldoutGroup("Equipment Slot")]
        [SerializeField] private ShieldEquipmentSlot shieldSlot;

        [SerializeField, FoldoutGroup("Inventory Sorters")] private Button allSorter, weaponSorter, armourSorter, consumableSorter;

        [SerializeField, FoldoutGroup("Inventory Sorters/Sprites")] private Sprite allSorterSelected, allSorterUnselected;
        [SerializeField, FoldoutGroup("Inventory Sorters/Sprites")] private Sprite weaponSorterSelected, weaponSorterUnselected;
        [SerializeField, FoldoutGroup("Inventory Sorters/Sprites")] private Sprite armourSorterSelected, armourSorterUnselected;
        [SerializeField, FoldoutGroup("Inventory Sorters/Sprites")] private Sprite consumableSorterSelected, consumableSorterUnselected;

        private Type sortedType = null; // Use Type to store the current sorting class.

        private Dictionary<ArmourType, ArmourEquipmentSlot> armourEquipmentSlots;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogError("There are multiple instances of the EquipmentInventory script!");
                Destroy(gameObject);
            }
        }

        protected override void Start()
        {
            base.Start();
            equipmentTotals = FindObjectOfType<EquipmentTotals>();

            armourEquipmentSlots = new Dictionary<ArmourType, ArmourEquipmentSlot>
            {
                { ArmourType.Head, helmetSlot },
                { ArmourType.Chest, chestSlot },
                { ArmourType.Feet, feetSlot }
            };

            allSorter.onClick.AddListener(ResetSorting);
            weaponSorter.onClick.AddListener(() => SortByType(typeof(WeaponItem)));
            armourSorter.onClick.AddListener(() => SortByType(typeof(ArmourItem)));
            consumableSorter.onClick.AddListener(() => SortByType(typeof(ConsumableItem)));
        }

        #region Inventory Sorting

        private void ResetSorting()
        {
            ResetButtonGraphics();
            allSorter.GetComponent<Image>().sprite = allSorterSelected;
            sortedType = null;
            PopulateInventory();
        }

        private void SortByType(Type type)
        {
            Debug.Log("Sorting by type: " + type);

            ResetButtonGraphics();

            if (type == typeof(WeaponItem))
            {
                weaponSorter.GetComponent<Image>().sprite = weaponSorterSelected;
            }
            else if (type == typeof(ArmourItem))
            {
                armourSorter.GetComponent<Image>().sprite = armourSorterSelected;
            }
            else if (type == typeof(ConsumableItem))
            {
                consumableSorter.GetComponent<Image>().sprite = consumableSorterSelected;
            }
            else
            {
                allSorter.GetComponent<Image>().sprite = allSorterSelected;
            }

            sortedType = type;

            // Clear current inventory UI
            foreach (Transform child in playerInventorySlotParent)
            {
                if (child != presetItemSlot.transform)
                    Destroy(child.gameObject);
            }

            // Populate the inventory with items of the specified type
            foreach (Item item in inventory.GetItems())
            {
                if (type == null || type.IsInstanceOfType(item)) // Matches if null or matches type
                {
                    CreateInventorySlot(item);
                }
            }
        }

        private void ResetButtonGraphics()
        {
            allSorter.GetComponent<Image>().sprite = allSorterUnselected;
            weaponSorter.GetComponent<Image>().sprite = weaponSorterUnselected;
            armourSorter.GetComponent<Image>().sprite = armourSorterUnselected;
            consumableSorter.GetComponent<Image>().sprite = consumableSorterUnselected;
        }

        private void CreateInventorySlot(Item item)
        {
            InventorySlot newSlot = Instantiate(presetItemSlot, playerInventorySlotParent);

            presetItemSlot.SetItem(item, inventory.GetQuantityOfItem(item));

            newSlot.gameObject.SetActive(true);
        }

        #endregion

        public override void OpenInventory()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (inventory == null)
                Debug.LogError("Player inventory is null");

            PopulateInventory();

            // Why the hell do i have to wait for duration to bind the equipped items?
            StartCoroutine(Core.Utils.WaitDurationAndExecute(0.01f, () =>
            {
                BindEquippedItems();
            }));
        }

        public override void CloseInventory()
        {
            base.CloseInventory();

            selectedEquipmentSlot = null;
            availableEquipmentSlotPointer.SetActive(false);
        }

        /// <summary>
        /// Go through the equipped items and bind them to the equipment slots
        /// </summary>
        /// <returns></returns>
        private void BindEquippedItems()
        {
            //Debug.Log("Binding equipped items");

            UnbindAll();

            //TODO: optimize
            if (playerEquipmentHandler == null)
                playerEquipmentHandler = FindObjectOfType<Player>().GetComponent<EquipmentHandler>();

            // Go through the player's equipped items 
            foreach (var equippedItem in playerEquipmentHandler.GetEquippedItems())
            {
                // If the item is not currently equipped, continue
                if (equippedItem == null)
                {
                    continue;
                }

                EquipItemToEquipmentSlot(GetSlotCorrespondingToItem(equippedItem));
            }
        }

        private void UnbindAll()
        {
            foreach (Transform child in playerInventorySlotParent)
            {
                if (!child.TryGetComponent(out InventorySlot inventorySlot))
                {
                    Debug.LogError("InventorySlot component not found on object");
                    continue;
                }

                inventorySlot.ToggleDarkPanel(false);
            }
        }

        private InventorySlot GetSlotCorrespondingToItem(EquippableItem item)
        {
            foreach (Transform child in playerInventorySlotParent)
            {
                if (!child.TryGetComponent(out InventorySlot inventorySlot))
                {
                    Debug.LogError("InventorySlot component not found on object");
                    continue;
                }

                if (inventorySlot.LinkedItem == item)
                {
                    return inventorySlot;
                }
            }

            return null;
        }

        private void EquipItemToEquipmentSlot(InventorySlot inventorySlot)
        {
            if (inventorySlot == null)
            {
                Debug.LogError("Inventory slot is null");
                return;
            }

            EquippableItem item = (EquippableItem)inventorySlot.LinkedItem;

            if (item.GetType() == typeof(WeaponItem))
            {
                if (weaponSlot.BindEquipment(item))
                {
                    inventorySlot.ToggleDarkPanel(true);
                    equipmentTotals.ReloadTotals(playerEquipmentHandler.TotalPhysicalDamage, playerEquipmentHandler.TotalPhysicalResistance);
                }
            }
            else if (item.GetType() == typeof(ShieldItem))
            {
                if (shieldSlot.BindEquipment(item))
                {
                    inventorySlot.ToggleDarkPanel(true);
                    equipmentTotals.ReloadTotals(playerEquipmentHandler.TotalPhysicalDamage, playerEquipmentHandler.TotalPhysicalResistance);
                }
            }
            else if (item.GetType() == typeof(ArmourItem))
            {
                if (armourEquipmentSlots[((ArmourItem)item).armourType].BindEquipment(item))
                {
                    inventorySlot.ToggleDarkPanel(true);
                    equipmentTotals.ReloadTotals(playerEquipmentHandler.TotalPhysicalDamage, playerEquipmentHandler.TotalPhysicalResistance);
                }
            }
            else
            {
                Debug.LogWarning("Item type not found");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter.CompareTag("SelectableSlot"))
            {
                if (selectedInventorySlot == null && heldItem == null)
                {
                    PlayHoverSound();

                    // Displays the item information panel containing information about the currently hovered item
                    itemInformationPanel.gameObject.SetActive(true);

                    if (!eventData.pointerEnter.TryGetComponent(out InventorySlot inventorySlot))
                    {
                        Debug.LogError("InventoryItem component not found on object");
                        return;
                    }

                    Item item = inventorySlot.LinkedItem;

                    itemInformationPanel.ShowItemInformation(item);

                    // Sets the position of the item selector to the position of the hovered item
                    SetPointerPosition(eventData);

                    // Set the currently hovered item as the hovered game object
                    selectedInventorySlot = inventorySlot;

                    if (item.IsEquippable)
                    {
                        // Set the position of the available equipment slot pointer to the equipment slot that corresponds to the hovered item
                        item = inventorySlot.LinkedItem;
                        EquipmentSlot equipmentSlot = null;

                        if (item.GetType() == typeof(ArmourItem))
                        {
                            equipmentSlot = armourEquipmentSlots[((ArmourItem)item).armourType];
                        }
                        else if (item.GetType() == typeof(ShieldItem))
                        {
                            equipmentSlot = shieldSlot;
                        }
                        else if (item.GetType() == typeof(WeaponItem))
                        {
                            equipmentSlot = weaponSlot;
                        }
                        else
                        {
                            Debug.LogWarning("Item type not found");
                        }

                        if (equipmentSlot != null)
                        {
                            availableEquipmentSlotPointer.transform.SetPositionAndRotation(equipmentSlot.transform.position, Quaternion.identity);
                        }
                    }
                }
            }

            if (eventData.pointerEnter.TryGetComponent(out selectedEquipmentSlot))
            {
                // Sets the position of the item selector to the position of the hovered item
                SetPointerPosition(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // If we are holding an item, we need to preserve the selected inventory slot until we release the mouse button
            if (heldItem != null)
            {
                return;
            }

            // If the currently selected inventory slot isn't null, hide the pointer and set the selected slot to null
            if (selectedInventorySlot != null)
            {
                HidePointer();
                selectedInventorySlot = null;

                itemInformationPanel.gameObject.SetActive(false);
            }

            if (selectedEquipmentSlot != null)
            {
                HidePointer();
                selectedEquipmentSlot = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ReloadItemQuantities();
            }

            if (selectedInventorySlot != null)
            {
                // Sets the currently hovered item as the held item if the left mouse button is pressed
                if (Input.GetMouseButtonDown(0))
                {
                    // If the item is not equippable, return (we shouldn't have any need to drag around non-equippable items)
                    if (!selectedInventorySlot.LinkedItem.IsEquippable)
                    {
                        return;
                    }

                    // Sets the held item to the image component of the selected slot
                    heldItem = selectedInventorySlot.ImageObject;

                    itemInformationPanel.Hide();

                    // Hides the item selector
                    HidePointer();
                }
            }

            // Equips an item directly by right clicking on the item from the inventory
            if (selectedInventorySlot != null && Input.GetMouseButtonDown(1))
            {
                // If the item is not equippable, return (we can't equip non-equippable items)
                if (!selectedInventorySlot.LinkedItem.IsEquippable)
                {
                    return;
                }

                EquipItemToEquipmentSlot(selectedInventorySlot);

                PlayEquipSound();

                BindEquippedItems();
            }

            // Equips an item by dropping it into an equipment slot
            if (heldItem != null && Input.GetMouseButtonUp(0))
            {
                if (selectedEquipmentSlot != null && selectedEquipmentSlot.BindEquipment((EquippableItem)selectedInventorySlot.LinkedItem))
                {
                    PlayEquipSound();

                    BindEquippedItems();
                    equipmentTotals.ReloadTotals(playerEquipmentHandler.TotalPhysicalDamage, playerEquipmentHandler.TotalPhysicalResistance);
                }

                selectedInventorySlot = null;
                selectedEquipmentSlot = null;
                heldItem.transform.position = heldItem.transform.parent.position;
                heldItem = null;
            }

            // Unequips an equipped item diretly by right clicking on the equipment slot
            if (selectedEquipmentSlot != null && Input.GetMouseButtonDown(1))
            {
                if (selectedEquipmentSlot != null && selectedEquipmentSlot.isBound)
                {
                    selectedEquipmentSlot.UnbindEquipment();

                    BindEquippedItems();

                    equipmentTotals.ReloadTotals(playerEquipmentHandler.TotalPhysicalDamage, playerEquipmentHandler.TotalPhysicalResistance);

                    PlayUnequipSound();
                }
            }

            if (heldItem != null)
            {
                heldItem.transform.position = Input.mousePosition;
                availableEquipmentSlotPointer.SetActive(true);
            }
            else
            {
                availableEquipmentSlotPointer.SetActive(false);
            }
        }

        public bool IsEquipped(Item item)
        {
            for (int i = 0; i < playerEquipmentSlotParent.childCount; i++)
            {
                if (playerEquipmentSlotParent.GetChild(i).GetComponent<EquipmentSlot>() == null)
                    continue;

                if (playerEquipmentSlotParent.GetChild(i).GetComponent<EquipmentSlot>().isBound &&
                                   playerEquipmentSlotParent.GetChild(i).GetComponent<EquipmentSlot>().boundItem == item)
                {
                    return true;
                }
            }

            return false;
        }

        public bool UnequipItem(Item item)
        {
            if (IsEquipped(item))
            {
                for (int i = 0; i < playerEquipmentSlotParent.childCount; i++)
                {
                    if (playerEquipmentSlotParent.GetChild(i).GetComponent<EquipmentSlot>() == null)
                        continue;

                    EquipmentSlot equipmentSlot = playerEquipmentSlotParent.GetChild(i).GetComponent<EquipmentSlot>();

                    if (equipmentSlot.isBound &&
                    equipmentSlot.boundItem == item)
                    {
                        equipmentSlot.UnbindEquipment();
                        BindEquippedItems();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to get an equippable item from the hovered game object with an inventoryItem component
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        private EquippableItem GetEquippableItemFromObject(GameObject sourceObject)
        {
            if (!sourceObject.TryGetComponent(out InventorySlot inventoryItem))
            {
                Debug.LogError("InventoryItem component not found on object");
                return null;
            }

            if (inventoryItem.LinkedItem == null)
            {
                Debug.LogError("Item is null");
                return null;
            }

            if (!inventoryItem.LinkedItem.IsEquippable)
            {
                Debug.LogWarning($"Item is not equippable, the item type is: {inventoryItem.LinkedItem.GetType()}");
                return null;
            }

            return (EquippableItem)inventoryItem.LinkedItem;
        }
    }
}