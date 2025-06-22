using Core.Enums;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using InventoryManagement;
using UIWindows;
using Core;
using Characters;

namespace Interactables
{
    public class ItemContainer : Interactable
    {
        [InlineEditor(Expanded = true)]
        [BoxGroup("Item Container"), SerializeField, Tooltip("The container holding all inventory items and quantities.")]
        private InventoryContainer inventoryContainer;
        private bool isInventoryContainerNull => inventoryContainer == null;


        // Create a button that instantiates a new inventory container
        [ShowIf("isInventoryContainerNull"), BoxGroup("Item Container"), Button("Create Inventory Container")]
        private void CreateInventoryContainer() {inventoryContainer = ScriptableObject.CreateInstance<InventoryContainer>();}


        /// <summary>
        /// Opens the inventory container linked to this item container
        /// </summary>
        /// <param name="playerObject"></param>
        /// <param name="modifyInteractableLabel"></param>
        public override void Interact(object playerObject, Action<ActionType> modifyInteractableLabel)
        {
            // Open the item container panel and pass the inventory container
            ItemContainerPanel.Instance.OpenContainer(inventoryContainer);

            // Publish an event to open the UI menu
            EventBus.Instance.Publish("OpenUIMenu");
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                // Do not do anything if the inventory container is null
                if (isInventoryContainerNull) return;

                player.AddToInteractables(this.gameObject, interactionAction);

                usagePromptGroup.SetActive(true);
            }
        }
    }
}