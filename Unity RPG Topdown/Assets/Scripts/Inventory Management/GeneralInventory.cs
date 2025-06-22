using UnityEngine;
using UnityEngine.UI;
using Items;
using System;
using UIElements;

namespace InventoryManagement
{
    public class GeneralInventory : BaseInventory
    {
        private Type sortedType = null; // Use Type to store the current sorting class.

        [Header("Sorter Buttons")]
        [SerializeField] private GameObject allSorter;
        [SerializeField] private GameObject weaponSorter, armourSorter, consumableSorter;

        [Header("All Sorter Graphics")]
        [SerializeField] private Sprite allSorterSelected, allSorterUnselected;

        [Header("Weapon Sorter Graphics")]
        [SerializeField] private Sprite weaponSorterSelected, weaponSorterUnselected;

        [Header("Armour Sorter Graphics")]
        [SerializeField] private Sprite armourSorterSelected, armourSorterUnselected;

        [Header("Consumable Sorter Graphics")]
        [SerializeField] private Sprite consumableSorterSelected, consumableSorterUnselected;

        protected override void Start()
        {
            base.Start();

            allSorter.GetComponent<Button>().onClick.AddListener(ResetSorting);
            weaponSorter.GetComponent<Button>().onClick.AddListener(() => SortByType(typeof(WeaponItem)));
            armourSorter.GetComponent<Button>().onClick.AddListener(() => SortByType(typeof(ArmourItem)));
            consumableSorter.GetComponent<Button>().onClick.AddListener(() => SortByType(typeof(ConsumableItem)));
        }

        public override void OpenInventory()
        {
            base.OpenInventory();

            if (sortedType != null)
            {
                SortByType(sortedType);
            }
            else
            {
                ResetSorting();
            }
        }

        private void ResetButtonGraphics()
        {
            allSorter.GetComponent<Image>().sprite = allSorterUnselected;
            weaponSorter.GetComponent<Image>().sprite = weaponSorterUnselected;
            armourSorter.GetComponent<Image>().sprite = armourSorterUnselected;
            consumableSorter.GetComponent<Image>().sprite = consumableSorterUnselected;
        }

        private void SortByType(Type type)
        {
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

        private void CreateInventorySlot(Item item)
        {
            InventorySlot newSlot = Instantiate(presetItemSlot, playerInventorySlotParent);

            presetItemSlot.SetItem(item, inventory.GetQuantityOfItem(item));

            newSlot.gameObject.SetActive(true);
        }

        private void ResetSorting()
        {
            ResetButtonGraphics();
            allSorter.GetComponent<Image>().sprite = allSorterSelected;
            sortedType = null;
            PopulateInventory();
        }
    }

}