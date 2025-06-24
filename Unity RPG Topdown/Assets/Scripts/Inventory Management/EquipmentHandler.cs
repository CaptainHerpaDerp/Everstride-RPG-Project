using System.Collections.Generic;
using UnityEngine;
using Items;
using UnityEngine.U2D.Animation;
using Core.Enums;
using Core;
using Characters;
using InventoryManagement;
using System;
using DataSave;
    
namespace InventoryManagement
{
    /// <summary>
    /// Manages the equipment of a character
    /// </summary>
    public class EquipmentHandler : MonoBehaviour
    {
        // The default colour of the hair
        [SerializeField] private Color defaultHairColor = new(60, 48, 36);
        [SerializeField] private Color equipColor = new(1, 1, 1);

        // Visual Variables
        [SerializeField] private SpriteLibraryAsset defaultHead;
        [SerializeField] private SpriteLibrary hairSpriteLibrary, helmetSpriteLibrary, chestSpriteLibrary, feetSpriteLibrary, weaponSpriteLibrary, weaponTrailSpriteLibrary, shieldSpriteLibrary, shieldTrailSpriteLibrary, neckSpriteLibrary, ringSpriteLibrary;
        private SpriteRenderer hairRenderer, helmetRenderer, chestRenderer, feetRenderer, weaponRenderer, shieldRenderer, neckRenderer, ringRenderer;
        private Dictionary<ArmourType, SpriteRenderer> armourRenderers;
        
        // Equipped Items
        private Dictionary<ArmourType, ArmourItem> equippedArmour = new();
        private WeaponItem equippedWeapon;
        private ShieldItem equippedShield;

        // Equipped stats
        public float TotalPhysicalDamage { get; private set; }
        public float TotalPhysicalResistance { get; private set; }

        private Inventory targetInventory;
        public Inventory TargetInventory => GetComponent<Inventory>();
        private InventoryData inventoryData;

        [SerializeField] private Character character;

        [SerializeField] private bool equipBestItemsOnStart = false;

        private void Start()
        {
            CollectSpriteRenderers();
            SetupDictionaries();

            if(equipBestItemsOnStart)
                EquipBestItems();

            if (character == null && !TryGetComponent(out character))
            {
                Debug.LogError("Character component not found on Equipment Handler.");
            }
        }

        #region Setups

        private void CollectSpriteRenderers()
        {
            hairRenderer = hairSpriteLibrary.GetComponent<SpriteRenderer>();
            helmetRenderer = helmetSpriteLibrary.GetComponent<SpriteRenderer>();
            chestRenderer = chestSpriteLibrary.GetComponent<SpriteRenderer>();
            feetRenderer = feetSpriteLibrary.GetComponent<SpriteRenderer>();
            weaponRenderer = weaponSpriteLibrary.GetComponent<SpriteRenderer>();
            shieldRenderer = shieldSpriteLibrary.GetComponent<SpriteRenderer>();
            //neckRenderer = neckSpriteLibrary.GetComponent<SpriteRenderer>();
            //ringRenderer = ringSpriteLibrary.GetComponent<SpriteRenderer>();
        }

        private void SetupDictionaries()
        {
            equippedArmour = new Dictionary<ArmourType, ArmourItem>
            {
                { ArmourType.Head, null },
                { ArmourType.Chest, null },
                { ArmourType.Feet, null },
                { ArmourType.Neck, null },
                { ArmourType.Ring, null }
            };

            armourRenderers = new Dictionary<ArmourType, SpriteRenderer>
            {
                { ArmourType.Head, helmetRenderer },
                { ArmourType.Chest, chestRenderer },
                { ArmourType.Feet, feetRenderer },
                { ArmourType.Neck, neckRenderer },
                { ArmourType.Ring, ringRenderer }
            };
        }

        #endregion

        #region Equipment Handling

        public void EquipBestItems()
        {
            if (targetInventory == null)
            {
                targetInventory = GetComponent<Inventory>();
            }

            float attackLevel = 0;

            float shieldPowerLevel = 0;

            float headLevel = 0;
            float chestLevel = 0;
            float feetLevel = 0;

            foreach (EquippableItem item in targetInventory.GetEquippableItems())
            {
                //Debug.Log($"Doing auto equip for {item}");

                if (item.GetType() == typeof(WeaponItem) && ((WeaponItem)item).weaponDamage > attackLevel)
                {
                    EquipItem(item);
                    attackLevel = ((WeaponItem)item).weaponDamage;
                }

                if (item.GetType() == typeof(ShieldItem) && ((ShieldItem)item).shieldPowerMax > shieldPowerLevel)
                {
                    EquipItem(item);
                    shieldPowerLevel = ((ShieldItem)item).shieldPowerMax;
                }

                else if (item.GetType() == typeof(ArmourItem))
                {
                    ArmourItem armourItem = (ArmourItem)item;

                    switch (armourItem.armourType)
                    {
                        case ArmourType.Head:
                            if (armourItem.armourDefense > headLevel)
                            {
                                //Debug.Log($"Equipping {item.itemName}");
                                EquipItem(item);
                                headLevel = armourItem.armourDefense;
                            }
                            break;

                        case ArmourType.Chest:
                            if (armourItem.armourDefense > chestLevel)
                            {
                                //Debug.Log($"Equipping {item.itemName}");
                                EquipItem(item);
                                chestLevel = armourItem.armourDefense;
                            }
                            break;

                        case ArmourType.Feet:
                            if (armourItem.armourDefense > feetLevel)
                            {
                                //Debug.Log($"Equipping {item.itemName}");
                                EquipItem(item);
                                feetLevel = armourItem.armourDefense;
                            }
                            break;
                    }
                }             
            }
        }

        public void EquipItem(EquippableItem item)
        {
            if (item is WeaponItem weapon)
            {
                EquipWeapon(weapon);
            }
            else if (item is ArmourItem armour)
            {
                EquipArmour(armour);
            }
            else if (item is ShieldItem shield)
            {
                EquipShield(shield);
            }
            else
            {
                Debug.LogError($"Item - {item.name} with type {item.GetType()} not recognised.");
            }
        }

        private void EquipArmour(ArmourItem armourItem)
        {
            // If the currently equipped armour of the same type is not null, unequip it
            ArmourType armourType = armourItem.armourType;
            if (equippedArmour[armourType] != null)
            {
                UnequipArmour(equippedArmour[armourType]);
            }   

            // Set the item in the equipment slot
            equippedArmour[armourItem.armourType] = armourItem;

            // Add the stats
            TotalPhysicalResistance += armourItem.armourDefense;

            // Update Colour
            HandleArmourColour(armourItem);

            // Update Sprite Libraries
            switch (armourItem.armourType)
            {
                case ArmourType.Head:

                    // Disable the hair renderer
                    if (hairRenderer != null)
                        hairRenderer.enabled = false;

                    helmetSpriteLibrary.spriteLibraryAsset = armourItem.itemLibraryAsset;
                    break;

                case ArmourType.Chest:
                    chestSpriteLibrary.spriteLibraryAsset = armourItem.itemLibraryAsset;
                    break;

                case ArmourType.Feet:
                    feetSpriteLibrary.spriteLibraryAsset = armourItem.itemLibraryAsset;
                    break;

                case ArmourType.Neck:
                    neckSpriteLibrary.spriteLibraryAsset = armourItem.itemLibraryAsset;
                    break;

                case ArmourType.Ring:
                    ringSpriteLibrary.spriteLibraryAsset = armourItem.itemLibraryAsset;                 
                    break;

                default:
                    Debug.LogError("Armour type not recognised.");
                    break;
            }

        }

        private void EquipWeapon(WeaponItem weapon)
        {
            // Unequip current weapon
            UnequipWeapon();

            //Debug.Log($"Equipped {weapon.itemName}.");

            equippedWeapon = weapon;
            TotalPhysicalDamage += weapon.weaponDamage;

            weaponSpriteLibrary.spriteLibraryAsset = weapon.itemLibraryAsset;

            character.SetEquippedWeapon(weapon);
        }
         
        private void EquipShield(ShieldItem shield)
        {
            // Unequip the current shield
            UnequipShield();

            //Debug.Log($"Equipped {shield.itemName}.");

            equippedShield = shield;

            shieldSpriteLibrary.spriteLibraryAsset = shield.itemLibraryAsset;

            character.SetEquippedShield(shield);
        }

        #endregion

        #region Unequipment Handling

        /// <summary>
        /// Called when an item is removed from the inventory. Checks the currently equipped items to see if it needs to be unequipped.
        /// </summary>
        /// <param name="item"></param>
        private void OnRemovedItem(EquippableItem removedEquippableItem)
        {
            if (removedEquippableItem == null)
            {
                Debug.LogError("Error in equipment handler, define object marked as removed is null");
            }

            if (removedEquippableItem is WeaponItem)
            {
                UnequipWeapon();
            }
            else if (removedEquippableItem is ArmourItem armourItem)
            {
                UnequipArmour(armourItem);
            }
        }

        public void UnequipItem(EquippableItem item)
        {
            if (item == null)
            {
                Debug.LogError("Error in equipment handler, define object marked as removed is null");
            }

            if (item is WeaponItem)
            {
                UnequipWeapon();
            }
            else if (item is ArmourItem armourItem)
            {
                UnequipArmour(armourItem);
            }
            else if (item is ShieldItem)
            {
                UnequipShield();
            }
            else
            {
                Debug.LogError("Item type not recognised.");
            }
        }

        public void UnequipArmour(ArmourItem armour)
        {
            if (armour == null)
            {
                Debug.LogError("Error in equipment handler, define object marked as removed is null");
            }

            if (equippedArmour.ContainsKey(armour.armourType))
            {
                // Subtract the stats
                TotalPhysicalResistance -= armour.armourDefense;

                // Set the equipped armour to null
                equippedArmour[armour.armourType] = null;

                // Update visuals
                switch (armour.armourType)
                {
                    case ArmourType.Head:
                        helmetSpriteLibrary.spriteLibraryAsset = null;
                        break;
                    case ArmourType.Chest:
                        chestSpriteLibrary.spriteLibraryAsset = null;
                        break;
                    case ArmourType.Feet:
                        feetSpriteLibrary.spriteLibraryAsset = null;
                        break;
                }

                // Set the current sprite of the renderer to nothing
                armourRenderers[armour.armourType].sprite = null;

                Debug.Log($"Unequipped {armour.armourType} armour.");
            }
        }

        public void UnequipWeapon()
        {
            if (equippedWeapon == null) 
            { 
               // Debug.LogWarning("No weapon equipped.");
             
                
                return;
            }

            // Subtract the stats
            TotalPhysicalDamage -= equippedWeapon.weaponDamage;

            // Reset the weapon sprite library
            weaponSpriteLibrary.spriteLibraryAsset = null;

            // Reset the sprite of the weapon renderer
            weaponRenderer.sprite = null;

            Debug.Log($"Unequipping {equippedWeapon.itemName}");

            character.SetEquippedWeapon(null);
            
            equippedWeapon = null;
        }

        private void UnequipShield()
        {
            if (equippedShield == null)
            {
                Debug.LogWarning("No shield equipped.");
                return;
            }

            equippedShield = null;

            character.SetEquippedShield(null);
        }

        #endregion

        #region Equipment Type Getters

        public bool HasShield => equippedShield != null;

        /// <summary>
        /// Returns true if the currently equipped weapon has the ability to block
        /// </summary>
        public bool HasBlockableWeapon => equippedWeapon != null && equippedWeapon.canBlock;

        public List<EquippableItem> GetEquippedItems()
        {
            List<EquippableItem> items = new();

            foreach (var armour in equippedArmour.Values)
            {
                items.Add(armour);
            }

            if (equippedWeapon != null)
            {
                items.Add(equippedWeapon);
            }

            if (equippedShield != null)
            {
                items.Add(equippedShield);
            }

            return items;
        }

        #endregion

        #region Saving and Loading

        public void SaveEquipmentData()
        {
            // If the equipment data is null, create a new instance
            inventoryData ??= new InventoryData();

            // Loop through each of the armour types
            foreach (var armourType in equippedArmour.Keys)
            {
                // Get the armour item
                ArmourItem armourItem = equippedArmour[armourType];

                // If the armour item is not null, save the item data
                if (armourItem != null)
                {
                    inventoryData.EquippedArmour[armourType] = armourItem.ItemID;
                }
            }

            inventoryData.EquippedWeaponItemID = equippedWeapon?.ItemID;
            inventoryData.EquippedShieldItemID = equippedShield?.ItemID;

        }

        #endregion

        #region Utilities

        private void HandleArmourColour(ArmourItem armourItem)
        {
            SpriteRenderer armourSpriteRenderer = armourRenderers[armourItem.armourType];

            // Set the colour of the armour to the default colour
            armourSpriteRenderer.color = equipColor;

            // If the armour item is coloured, set the colour to the item's colour
            if (armourItem.isColoured)
            {
                SetEquipmentColor(armourSpriteRenderer, armourItem.itemColor);
            }
        }

        private void SetEquipmentColor(SpriteRenderer renderer, Color color)
        {
            if (renderer != null)
            {
                renderer.color = color;
            }
        }

        #endregion

    }
}
