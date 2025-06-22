using Core;
using InventoryManagement;
using Sirenix.OdinInspector;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using AudioManagement;
using TMPro;
using UnityEngine.UI;
using Items;

namespace UIWindows
{
    /// <summary>
    /// A UI panel that shows all items in an item container (eg. a chest, crate, barrel)
    /// </summary>
    public class ItemContainerPanel : Singleton<ItemContainerPanel>, IPointerEnterHandler, IPointerExitHandler
    {
        [FoldoutGroup("Components"), SerializeField] private CanvasGroup canvasGroup;
        [FoldoutGroup("Components"), SerializeField] protected GameObject itemSelector;

        [FoldoutGroup("ItemGrid"), SerializeField] private Transform itemParentTransform;
        [FoldoutGroup("ItemGrid"), SerializeField] private ItemContainerBlock itemContainerBlockPrefab;

        [FoldoutGroup("Name Selection Panels"), SerializeField] private NameSelectionPanel playerNameSelectionPanel, containerNameSelectionPanel;
        [FoldoutGroup("Name Selection Panels"), SerializeField] private TextMeshProUGUI arrowTextComponent;
        [FoldoutGroup("Name Selection Panels"), SerializeField] private Button nameSelectionPanelButton;

        [FoldoutGroup("Player Components"), SerializeField] private Inventory playerInventory;

        [BoxGroup("Debug"), SerializeField] private InventoryContainer debugContainer;

        // The Item we are currently hovering over
        private ItemContainerBlock hoveredItemContainerBlock;

        const float pointerYOffset = 25;

        // If true, we are viewing the items from the container, otherwise we are viewing the items from the player
        private bool isContainerInventoryOpen = true;

        // The container that was opened originally
        [ShowInInspector] private InventoryContainer openedContainer;

        // Singleton References
        private AudioManager audioManager;
        private FMODEvents fmodEvents;
        private ItemInformationPanel itemInformationPanel;
        private EventBus eventBus;

        // Bit sloppy but whaddayagonnado
        private bool isOpen => canvasGroup.alpha == 1;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;
            itemInformationPanel = ItemInformationPanel.Instance;
            eventBus = EventBus.Instance;

            // Initially, set the selected container to the container
            containerNameSelectionPanel.SetSelected(true);

            // Ensure the player name selection panel is not 'selected'
            playerNameSelectionPanel.SetSelected(false);

            // If the name selection panel button is pressed, we need to switch the container that is being viewed on the panel
            nameSelectionPanelButton.onClick.AddListener(() =>
            {
                SwitchContainerView();
            });

            // If we receive an event to close the menu, close the container
            eventBus.Subscribe("CloseMenu", CloseContainer);
        }


#if UNITY_EDITOR
        private void Update()
        {
            if (isOpen && Input.GetKeyUp(KeyCode.LeftAlt))
            {
                SwitchContainerView();
            }
        }
#endif

        private void SwitchContainerView()
        {
            isContainerInventoryOpen = !isContainerInventoryOpen;

            // Hide the item info panel and selector as we are no longer supposed to be hovering over an item
            HideObjectVisualFeedback();

            // Set the viewed container to the container that was opened
            if (isContainerInventoryOpen)
            {
                PopulateContainer(openedContainer);
            }
            // Set the viewed container to the player's inventory
            else
            {
                PopulateContainer(playerInventory.GetInventoryContainer());
            }

            ReloadNameSelectionText();
        }

        #region Panel Toggling

        public void OpenContainer(InventoryContainer container, string containerName = "Container")
        {
            PlayMenuOpenSound();

            // Set it so that we are viewing the containers contents (and not the player's)
            isContainerInventoryOpen = true;

            // Set the name of the container and reload the names
            containerNameSelectionPanel.Name = containerName;
            ReloadNameSelectionText();

            // Set the base container to the container that was opened
            openedContainer = container;

            PopulateContainer(container);

            // Fade in the panel and populate it afterwards
            StartCoroutine(Utils.FadeCanvasGroup(canvasGroup, fadingIn: true));
        }

        public void CloseContainer()
        {
            PlayMenuCloseSound();

            // Fade out the panel
            StartCoroutine(Utils.FadeCanvasGroup(canvasGroup, fadingIn: false));
        }

        #endregion

        /// <summary>
        /// Instantiates item container blocks for each item in the container, sets the item and quantity and sets them as children of the item parent transform
        /// </summary>
        /// <param name="container"></param>
        private void PopulateContainer(InventoryContainer container)
        {
            // First, remove all items currently within the panel
            foreach (Transform itemChild in itemParentTransform)
            {
                Destroy(itemChild.gameObject);
            }

            foreach (var item in container.GetAllItems())
            {
                ItemContainerBlock itemBlock = Instantiate(itemContainerBlockPrefab, itemParentTransform);
                itemBlock.SetItem(item, container.GetQuantityOfItem(item));

                // Subscribe to the item blocks button click event
                itemBlock.Button.onClick.AddListener(() =>
                {
                    // If the button item block we are pressing is the item block we are hovering over, then we are preforming an action with the item
                    if (hoveredItemContainerBlock == itemBlock)
                    {
                        // Transfer the item either from the container to the player or vise versa
                        TransferItemOwnership(container, item, itemBlock);
                    }
                });
            }
        }

        protected void TransferItemOwnership(InventoryContainer container, Item transferredItem, ItemContainerBlock itemBlock)
        {
            Debug.Log($"Transferring item {transferredItem.itemName} from {container.name} to the player");

            // int itemQuantity = container.GetQuantityOfItem(transferredItem);
            int itemQuantity = 1;

            // Check whether we are transferring the item from the container to the player or vice versa
            if (isContainerInventoryOpen)
            {
                playerInventory.AddItem(transferredItem, itemQuantity);
            }
            else
            {
                openedContainer.AddItem(transferredItem, itemQuantity);
            }

            // Remove the item from the container
            container.RemoveItem(transferredItem, itemQuantity);

            // If the remaining quantity of the item is 0, remove the item block from the item container panel
            if (container.GetQuantityOfItem(transferredItem) <= 0)
            {
                Destroy(itemBlock.gameObject);

                // If the item is destroyed, hide the item info panel and selector
                HideObjectVisualFeedback();

                // Nullify the tracked hovered item and hide all ui shown when hovering over an item
                hoveredItemContainerBlock = null;
            }

            // Set the item quantity to the new quantity    
            else
            {
                itemBlock.ReloadQuantity(container.GetQuantity(transferredItem));
            }

        }

        #region IPointerHandler Implementation

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log(eventData.pointerEnter.name);

            if (eventData.pointerEnter.transform.parent.gameObject.TryGetComponent(out ItemContainerBlock itemContainer))
            {
                PlayHoverSound();

                // Sets the item selector to the position of the item being hovered over
                SetPointerPosition(eventData);

                itemInformationPanel.ShowItemInformation(itemContainer.LinkedItem);

                // Set the hovered item container block
                hoveredItemContainerBlock = itemContainer;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoveredItemContainerBlock != null)
            {
                hoveredItemContainerBlock = null;
                itemInformationPanel.Hide();
                itemSelector.SetActive(false);
            }
        }

        #endregion

        #region Visual Object Management

        protected void HideObjectVisualFeedback()
        {
            itemInformationPanel.Hide();
            itemSelector.SetActive(false);
        }

        protected void SetPointerPosition(PointerEventData eventData)
        {
            itemSelector.SetActive(true);
            itemSelector.transform.SetPositionAndRotation(eventData.pointerEnter.transform.position + new Vector3(0, pointerYOffset, 0), Quaternion.identity);
        }

        private void ReloadNameSelectionText()
        {
            playerNameSelectionPanel.SetSelected(!isContainerInventoryOpen);
            containerNameSelectionPanel.SetSelected(isContainerInventoryOpen);

            // Set the arrow text to the opposite of the current view
            arrowTextComponent.text = isContainerInventoryOpen ? "->" : "<-";
        }

        #endregion

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

        private void PlayMenuOpenSound()
        {
            audioManager.PlayOneShot(fmodEvents.menuOpenSound, transform.position);
        }

        private void PlayMenuCloseSound()
        {
            audioManager.PlayOneShot(fmodEvents.menuCloseSound, transform.position);
        }


        #endregion
    }
}