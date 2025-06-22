using UnityEngine;
using System;
using Core.Enums;
using InventoryManagement;
using Core.Interfaces;

namespace Characters.Interactables
{
    public class LootPoint : MonoBehaviour, IInteractable
    {
        private Inventory characterInventory;
        private LootMenu lootMenu;

        public void Start()
        {
            lootMenu = LootMenu.Instance;
        }

        public void Interact(object playerObject, Action<ActionType> modifyInteractableLabel)
        {
            if (lootMenu != null && characterInventory != null && characterInventory.GetItems().Count > 0)
            lootMenu.OpenLootMenu("", characterInventory, this);
        }

        public void DestroyLootPoint()
        {
            FindObjectOfType<Player>().RemoveFromInteractables(this.gameObject);
            Destroy(this.gameObject);
        }

        public void CreateLootPoint(Inventory inventory, Vector3 position, float radius)
        {
            GetComponent<CircleCollider2D>().radius = radius * 1.5f;
            transform.position = position;
            characterInventory = inventory;
        }

        // Called when the player enters the loot point
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.AddToInteractables(this.gameObject, ActionType.Loot);
            }
        }

        // Called when the player leaves the loot point
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.RemoveFromInteractables(this.gameObject);

                // Closes the loot menu if the player leaves the loot point while the menu is open
                if (lootMenu != null && lootMenu.IsOpen())
                lootMenu.CloseLootMenu();
            }
        }
    }
}