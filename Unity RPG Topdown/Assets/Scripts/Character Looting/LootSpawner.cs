using System.Collections.Generic;
using UnityEngine;
using Items;
using Characters;
using InventoryManagement;
using Characters.Interactables;

namespace LootManagement
{
    public class LootSpawner : MonoBehaviour
    {
        public static LootSpawner Instance { get; private set; }

        [SerializeField] private GameObject lootPointPrefab;

        private void OnEnable()
        {
            Character.OnActivateLootPoint += SpawnLootPoint;
        }


        private void Start()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            else
                Instance = this;
        }

        private void SpawnLootPoint(NPC npc)
        {
            if (npc.TryGetComponent(out Inventory inventory) == false)
            {
                Debug.Log("Could not create a loot spawn point at the NPC. The NPC does not have an inventory.");
                return;
            }
            
            // Do not spawn a loot point if the NPC does not have any items
            if (inventory.GetItems().Count == 0)
            {
                return;
            }

            SpawnLootPoint(inventory, npc.transform.position, 2);
        }

        public void SpawnLootPoint(Inventory inventory, Vector3 position, float accessRadius)
        {
            GameObject lootPoint = Instantiate(lootPointPrefab);
            lootPoint.GetComponent<LootPoint>().CreateLootPoint(inventory, position, accessRadius);
        }
    }
}