using UnityEngine;
using UnityEngine.EventSystems;
using UIElements;
using AudioManagement;
using Items;

namespace InventoryManagement
{
    public class BaseInventory : FadeablePanel, IPointerEnterHandler, IPointerExitHandler
    {
        // The inventory manager that stores the player's inventory items
        [SerializeField] protected Inventory inventory;

        // The item that will be used as a template for the player's inventory slots
        [SerializeField] protected InventorySlot presetItemSlot;

        // The grid that stores the player's equippable inventory items
        [SerializeField] protected Transform playerInventorySlotParent;

        // A reference to the item selector that will be used to show the player which item they are hovering over
        [SerializeField] protected GameObject itemSelector;

        [SerializeField] protected CanvasGroup canvasGroup;

        const float pointerYOffset = 25;

        protected GameObject heldItem;
        protected InventorySlot hoveredInventorySlot;

        // Singleton References
        protected AudioManager audioManager;
        protected FMODEvents fmodEvents;
        protected ItemInformationPanel itemInformationPanel;

        protected virtual void Start()
        {
            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;
            itemInformationPanel = ItemInformationPanel.Instance;
        }

        public bool IsOpen()
        {
            return gameObject.activeInHierarchy;
        }

        protected virtual void PopulateInventory()
        {
            // Clears the current inventory
            foreach (Transform child in playerInventorySlotParent)
            {
                Destroy(child.gameObject);
            }

            // Populates the inventory with items marked as equippable
            if (presetItemSlot != null)
                foreach (Item item in inventory.GetItems())
                {
                    InventorySlot newSlot = Instantiate(presetItemSlot, playerInventorySlotParent);

                    // Enable the slot
                    newSlot.gameObject.SetActive(true);

                    if (inventory.GetQuantityOfItem(item) > 0)
                    {
                        Debug.Log("Item quantity is " + inventory.GetQuantityOfItem(item));
                    }

                    // Set the slot's item and its quantity
                    newSlot.SetItem(item, inventory.GetQuantityOfItem(item));
                }

        }

        protected void ReloadItemQuantities()
        {
            foreach (Transform child in playerInventorySlotParent)
            {
                // Set the quantities of all items in the inventory
                if (!child.TryGetComponent(out InventorySlot inventorySlot))
                {
                    Debug.LogWarning("Inventory slot not found on child object");
                    continue;
                }

                int itemQuantity = inventory.GetQuantityOfItem(inventorySlot.LinkedItem);
                inventorySlot.SetItemQuantity(itemQuantity);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log(eventData.pointerEnter.name);

            if (eventData.pointerEnter.CompareTag("SelectableSlot"))
            {
                if (hoveredInventorySlot == null)
                {
                    PlayHoverSound();

                    // Sets the item selector to the position of the item being hovered over
                    SetPointerPosition(eventData);

                    InventorySlot inventoryItem = eventData.pointerEnter.GetComponent<InventorySlot>();

                    itemInformationPanel.ShowItemInformation(inventoryItem.LinkedItem);

                    hoveredInventorySlot = inventoryItem;
                }
            }
        }

        protected void SetPointerPosition(PointerEventData eventData)
        {
            itemSelector.transform.SetPositionAndRotation(eventData.pointerEnter.transform.position + new Vector3(0, pointerYOffset, 0), Quaternion.identity);
        }

        protected void HidePointer()
        {
            itemSelector.transform.position = new Vector3(0, -1000, 0);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (hoveredInventorySlot != null)
            {
                DeselectHoveredItem();
                HidePointer();
            }
        }

        protected void DeselectHoveredItem()
        {
            itemInformationPanel.Hide();
            hoveredInventorySlot = null;
        }

        public virtual void OpenInventory()
        {
            PopulateInventory();

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void CloseInventory()
        {
            itemInformationPanel.Hide();
            HidePointer();
            heldItem = null;
            hoveredInventorySlot = null;

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        #region Audio Methods

        protected virtual void PlayHoverSound()
        {
            audioManager.PlayOneShot(fmodEvents.hoverSound, transform.position);
        }

        protected virtual void PlayEquipSound()
        {
            audioManager.PlayOneShot(fmodEvents.equipSound, transform.position);
        }

        protected virtual void PlayUnequipSound()
        {
            Debug.Log("Playing unequip sound");
            audioManager.PlayOneShot(fmodEvents.unequipSound, transform.position);
        }

        #endregion
    }
}