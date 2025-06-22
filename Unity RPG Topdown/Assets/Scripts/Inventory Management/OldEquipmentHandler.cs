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


    //public class OldEquipmentHandler : MonoBehaviour
    //{
    //    [SerializeField] private Color defaultHeadColor = new(60, 48, 36);
    //    [SerializeField] private Color equipColor = new(1, 1, 1);

    //    [SerializeField] private SpriteLibraryAsset defaultHead;

    //    [SerializeField] private SpriteLibrary hair, helmet, chest, feet, weapon, weaponTrail, shield, shieldTrail, neck, ring;

    //    public Item HeadItem { get; private set; }
    //    public Item ChestItem { get; private set; }
    //    public Item FeetItem { get; private set; }
    //    public Item WeaponItem { get; private set; }
    //    public Item ShieldItem { get; private set; }
    //    public Item NeckItem { get; private set; }
    //    public Item RingItem { get; private set; }

    //    // Dictionary to store equipped items, should always be identical to the character dictionary, communicated through the IEquipmentHandler interface
    //    protected Dictionary<ItemType, Item> equippedItems;

    //    GameObject weaponTrailObject, shieldTrailObject;

    //    // The targetCharacter that this equipment handler is attached to
    //    private Character targetCharacter;
    //    private Inventory targetInventory;

    //    public Inventory TargetInventory => GetComponent<Inventory>();

    //    private InventoryData equipmentData;

    //    [SerializeField] private ItemDatabase itemDatabase;

    //    public static Action<InventoryData> OnSaveInventoryData;

    //    public float TotalPhyiscalDamage { get; private set; }
    //    public float TotalPhyiscalResistance { get; private set; }

    //    private EventBus eventBus;

    //    [SerializeField] private SpriteRenderer hairSpriteRenderer, helmetSpriteRenderer;

    //    private void Start()
    //    {
    //        targetCharacter = GetComponent<Character>();
    //        targetInventory = GetComponent<Inventory>();

    //        targetInventory.OnItemRemoved += OnRemovedItem;

    //        eventBus = EventBus.Instance;

    //        if (eventBus == null)
    //        {
    //            Debug.LogError("EventBus could not be found!");
    //        }

    //        equippedItems = new();

    //        weaponTrailObject = weaponTrail.gameObject;
    //        if (shieldTrail != null)
    //            shieldTrailObject = shieldTrail.gameObject;

    //        // Set the default hair colour
    //        if (!HeadItem && hair != null)
    //        {
    //            hair.GetComponent<SpriteRenderer>().color = defaultHeadColor;
    //            hair.spriteLibraryAsset = defaultHead;
    //        }

    //        // Todo: do this in a better way
    //        if (targetCharacter is Player)
    //        {
    //            /* Manually find and add items to the player's inventory, for demo purposes where a save file has not been made */

    //            //Item greenShirtItem = itemDatabase.items.Find(x => x.itemName == "Green Shirt");
    //            //Item steelBasquinetItem = itemDatabase.items.Find(x => x.itemName == "Steel Bascinet");
    //            //Item leatherBootsItem = itemDatabase.items.Find(x => x.itemName == "Leather Boots");
    //            //Item ironLongSword = itemDatabase.items.Find(x => x.itemName == "Iron LongSword");
    //            //Item bow = itemDatabase.items.Find(x => x.itemName == "Bow");
    //            //Item ironShield = itemDatabase.items.Find(x => x.itemName == "IronShield");
    //            //Item arrow = itemDatabase.items.Find(x => x.itemName == "Iron Arrow");
    //            //arrow.quantity = 25;

    //            //targetCharacter.AddItem(greenShirtItem);
    //            //targetCharacter.AddItem(steelBasquinetItem);
    //            //targetCharacter.AddItem(leatherBootsItem);
    //            //targetCharacter.AddItem(ironLongSword);
    //            //targetCharacter.AddItem(bow);
    //            //targetCharacter.AddItem(ironShield);
    //            //targetCharacter.AddItem(arrow);


    //            //EquipItem(greenShirtItem);
    //            //EquipItem(leatherBootsItem);
    //            //EquipItem(ironLongSword);
    //            //EquipItem(ironShield);

    //            // Local load save 
    //            LoadEquipment();
    //        }

    //        // Equip the best items for NPCs
    //        if (targetCharacter is NPC)
    //        {
    //            EquipBestItems();
    //        }
    //    }

    //    /// <summary>
    //    /// Called when an item is removed from the inventory. Checks the currently equipped items to see if it needs to be unequipped.
    //    /// </summary>
    //    /// <param name="item"></param>
    //    private void OnRemovedItem(Item item)
    //    {
    //        if (item == null)
    //            return;

    //        if (item == HeadItem)
    //        {
    //            UnequipItem(HeadItem);
    //        }

    //        if (item == ChestItem)
    //        {
    //            UnequipItem(ChestItem);
    //        }

    //        if (item == FeetItem)
    //        {
    //            UnequipItem(FeetItem);
    //        }

    //        if (item == WeaponItem)
    //        {
    //            UnequipItem(WeaponItem);
    //        }

    //        if (item == ShieldItem)
    //        {
    //            UnequipItem(ShieldItem);
    //        }

    //        if (item == NeckItem)
    //        {
    //            UnequipItem(NeckItem);
    //        }

    //        if (item == RingItem)
    //        {
    //            UnequipItem(RingItem);
    //        }
    //    }

    //    private void Update()
    //    {
    //        if (targetCharacter)

    //            if (targetCharacter is Player)
    //            {
    //                // Detect if the player presses the control key and the s key at the same time
    //                if (Input.GetKey(KeyCode.BackQuote) && Input.GetKeyUp(KeyCode.S))
    //                {
    //                    Debug.Log("Saving equipment data...");
    //                    SaveEquipment();
    //                }

    //                // Detect if the player presses the control key and the l key at the same time
    //                if (Input.GetKey(KeyCode.BackQuote) && Input.GetKeyUp(KeyCode.L))
    //                {
    //                    Debug.Log("Loading equipment data...");
    //                    LoadEquipment();
    //                }
    //            }
    //    }

    //    public void EquipBestItems()
    //    {
    //        if (targetInventory == null)
    //        {
    //            targetInventory = GetComponent<Inventory>();
    //        }

    //        int attackLevel = 0;

    //        int headLevel = 0;
    //        int chestLevel = 0;
    //        int feetLevel = 0;

    //        foreach (Item item in targetInventory.GetItems())
    //        {
    //            if (item.itemType == ItemType.Weapon && item.WeaponDamage > attackLevel)
    //            {
    //                EquipItem(item);
    //                attackLevel = item.WeaponDamage;
    //            }

    //            if (item.itemType == ItemType.Armour)
    //                switch (item.ArmourType)
    //                {
    //                    case ArmourType.Head:
    //                        if (item.ArmourDefence > headLevel)
    //                        {
    //                            EquipItem(item);
    //                            headLevel = item.ArmourDefence;
    //                        }
    //                        break;

    //                    case ArmourType.Chest:
    //                        if (item.ArmourDefence > chestLevel)
    //                        {
    //                            EquipItem(item);
    //                            chestLevel = item.ArmourDefence;
    //                        }
    //                        break;

    //                    case ArmourType.Feet:
    //                        if (item.ArmourDefence > feetLevel)
    //                        {
    //                            EquipItem(item);
    //                            feetLevel = item.ArmourDefence;
    //                        }
    //                        break;
    //                }
    //        }
    //    }

    //    public List<Item> GetEquippedItems()
    //    {
    //        List<Item> itemStatus = new()
    //{
    //    HeadItem,
    //    ChestItem,
    //    FeetItem,
    //    WeaponItem,
    //    ShieldItem,
    //    NeckItem,
    //    RingItem
    //};

    //        return itemStatus;
    //    }

    //    public WeaponType GetWeaponType()
    //    {
    //        if (WeaponItem == null)
    //            return WeaponType.Unarmed;

    //        return WeaponItem.WeaponType;
    //    }

    //    public WeaponMode GetWeaponMode()
    //    {
    //        if (WeaponItem == null)
    //        {
    //            Debug.LogError("Error when trying to retrieve weapon mode - currently equipped weapon is null!");
    //            return WeaponMode.Slash;
    //        }

    //        return WeaponItem.WeaponMode;
    //    }

    //    public bool IsRangedAttacker()
    //    {
    //        if (WeaponItem.WeaponType == WeaponType.Bow || WeaponItem.WeaponType == WeaponType.Crossbow)
    //            return true;

    //        return false;
    //    }

    //    public void EquipItem(Item item)
    //    {
    //        if (!item.IsEquippable)
    //        {
    //            Debug.LogError("Trying to equip an item that is not marked as equipable!");
    //            return;
    //        }

    //        if (targetCharacter == null)
    //            targetCharacter = GetComponent<Character>();

    //        switch (item.itemType)
    //        {
    //            case ItemType.Armour:

    //                TotalPhyiscalResistance += item.ArmourDefence;

    //                switch (item.ArmourType)
    //                {
    //                    case ArmourType.Head:
    //                        //UnequipItem(HeadItem);

    //                        if (hairSpriteRenderer == null)
    //                        {
    //                            hairSpriteRenderer = hair.GetComponent<SpriteRenderer>();
    //                        }
    //                        if (helmetSpriteRenderer == null)
    //                        {
    //                            helmetSpriteRenderer = helmet.GetComponent<SpriteRenderer>();
    //                        }

    //                        // Disable the hair sprite renderer
    //                        hairSpriteRenderer.enabled = false;
    //                        helmetSpriteRenderer.enabled = true;

    //                        HeadItem = item;
    //                        helmet.GetComponent<SpriteRenderer>().color = equipColor;
    //                        helmet.spriteLibraryAsset = item.ItemLibraryAsset;

    //                        if (item.coloredItem)
    //                        {
    //                            SetEquipmentColor(helmet.GetComponent<SpriteRenderer>(), item.ItemColor);
    //                        }

    //                        break;

    //                    case ArmourType.Chest:
    //                        UnequipItem(ChestItem);
    //                        ChestItem = item;
    //                        chest.spriteLibraryAsset = item.ItemLibraryAsset;

    //                        if (item.coloredItem)
    //                        {
    //                            SetEquipmentColor(chest.GetComponent<SpriteRenderer>(), item.ItemColor);
    //                        }

    //                        break;

    //                    case ArmourType.Feet:
    //                        UnequipItem(FeetItem);
    //                        FeetItem = item;
    //                        feet.spriteLibraryAsset = item.ItemLibraryAsset;

    //                        if (item.coloredItem)
    //                        {
    //                            SetEquipmentColor(feet.GetComponent<SpriteRenderer>(), item.ItemColor);
    //                        }

    //                        break;

    //                    case ArmourType.Neck:
    //                        UnequipItem(NeckItem);
    //                        NeckItem = item;
    //                        neck.spriteLibraryAsset = item.ItemLibraryAsset;
    //                        break;

    //                    case ArmourType.Ring:
    //                        UnequipItem(RingItem);
    //                        RingItem = item;
    //                        ring.spriteLibraryAsset = item.ItemLibraryAsset;
    //                        break;
    //                }
    //                break;

    //            case ItemType.Weapon:

    //                // Unequip the currently equipped weapon
    //                UnequipItem(WeaponItem);

    //                WeaponItem = item;

    //              //  Debug.Log("Equipping weapon: " + item.itemName);
    //                targetCharacter.equippedWeapon = item;

    //                TotalPhyiscalDamage += item.WeaponDamage;

    //                weapon.spriteLibraryAsset = item.ItemLibraryAsset;

    //                targetCharacter.SetWeaponMode(item.WeaponMode);
    //                targetCharacter.SetWeaponType(item.WeaponType);

    //                if (item.WeaponMode == WeaponMode.Ranged)
    //                {

    //                }
    //                else
    //                {
    //                    targetCharacter.SetMeleeWeaponValues(item.WeaponRange, item.WeaponAngle);

    //                    if (weaponTrailObject != null)
    //                        weaponTrailObject.SetActive(true);
    //                }

    //                // If the weapon can block without a shield, unequip the shield
    //                if (item.CanBlock && item.WeaponType != WeaponType.Shield && ShieldItem != null)
    //                {
    //                    UnequipItem(ShieldItem);
    //                }

    //                break;

    //            case ItemType.Shield:

    //                UnequipItem(ShieldItem);

    //                targetCharacter.equippedShield = item;
    //             //   eventBus.Publish("OnPlayerShieldChanged", item);

    //                ShieldItem = item;
    //                shield.spriteLibraryAsset = item.ItemLibraryAsset;
    //                shieldTrail.spriteLibraryAsset = item.ShieldTrailAsset;
    //                shieldTrailObject.SetActive(true);
    //                break;
    //        }
    //    }

    //    public void UnequipItem(Item item)
    //    {
    //        if (item == null)
    //            return;

    //        switch (item.itemType)
    //        {
    //            case ItemType.Armour:

    //                TotalPhyiscalResistance -= item.ArmourDefence;

    //                switch (item.ArmourType)
    //                {
    //                    case ArmourType.Head:
    //                        HeadItem = null;
    //                        hairSpriteRenderer.enabled = true;
    //                        helmetSpriteRenderer.enabled = false;
    //                        break;

    //                    case ArmourType.Chest:
    //                        ChestItem = null;
    //                        chest.GetComponent<SpriteRenderer>().sprite = null;
    //                        chest.spriteLibraryAsset = null;
    //                        chest.GetComponent<SpriteRenderer>().color = Color.white;
    //                        break;

    //                    case ArmourType.Feet:
    //                        FeetItem = null;
    //                        feet.GetComponent<SpriteRenderer>().sprite = null;
    //                        feet.spriteLibraryAsset = null;
    //                        feet.GetComponent<SpriteRenderer>().color = Color.white;
    //                        break;

    //                    case ArmourType.Neck:
    //                        NeckItem = null;
    //                        neck.spriteLibraryAsset = null;
    //                        break;

    //                    case ArmourType.Ring:
    //                        RingItem = null;
    //                        ring.spriteLibraryAsset = null;
    //                        break;
    //                }

    //                break;

    //            case ItemType.Weapon:

    //                TotalPhyiscalDamage -= item.WeaponDamage;
    //                WeaponItem = null;

    //                targetCharacter.equippedWeapon = null;
    //                //eventBus.Publish("OnPlayerWeaponChanged", null);

    //                // Sets the currently displayed sprite to null, for if the weapon is visible when unequipped
    //                weapon.GetComponent<SpriteRenderer>().sprite = null;
    //                weapon.spriteLibraryAsset = null;
    //                targetCharacter.SetWeaponMode(WeaponMode.Slash);
    //                targetCharacter.SetWeaponType(WeaponType.Unarmed);
    //                weaponTrailObject.SetActive(false);
    //                break;

    //            case ItemType.Shield:
    //                ShieldItem = null;

    //                targetCharacter.equippedShield = null;

    //                shield.spriteLibraryAsset = null;
    //                shield.GetComponent<SpriteRenderer>().sprite = null;
    //                shieldTrailObject.SetActive(false);
    //                break;
    //        }
    //    }

    //    public bool CanBlock()
    //    {
    //        if (HasShield() || HasBlockableWeapon())
    //            return true;

    //        return false;
    //    }

    //    public bool HasShield()
    //    {
    //        return ShieldItem != null;
    //    }

    //    // Returns true if the character has a weapon that can block (eg. longsword)
    //    public bool HasBlockableWeapon()
    //    {
    //        if (WeaponItem != null && WeaponItem.CanBlock)
    //        {
    //            return true;
    //        }

    //        return false;
    //    }

    //    public void SaveEquipment()
    //    {
    //        // If the equipment data is null, create a new instance
    //        equipmentData ??= new InventoryData();

    //        if (equipmentData == null)
    //        {
    //            Debug.LogError("Equipment data is null!");
    //            return;
    //        }

    //        // do not save the head item if it is the default head
    //        if (helmet.spriteLibraryAsset != defaultHead)
    //        {
    //            equipmentData.EquippedHeadItemID = HeadItem != null ? HeadItem.ItemID : null;
    //        }

    //        // equipmentData.EquippedHeadItemID = HeadItem != null ? HeadItem.ItemID : null;
    //        equipmentData.EquippedChestItemID = ChestItem != null ? ChestItem.ItemID : null;
    //        equipmentData.EquippedFeetItemID = FeetItem != null ? FeetItem.ItemID : null;
    //        equipmentData.EquippedWeaponItemID = WeaponItem != null ? WeaponItem.ItemID : null;
    //        equipmentData.EquippedShieldItemID = ShieldItem != null ? ShieldItem.ItemID : null;


    //        equipmentData.ItemNames = new List<string>();
    //        equipmentData.ItemIds = new List<string>();
    //        equipmentData.ItemQuantities = new List<int>();

    //        foreach (Item item in targetInventory.GetItems())
    //        {
    //            equipmentData.ItemNames.Add(item.itemName);
    //            equipmentData.ItemIds.Add(item.ItemID);
    //            equipmentData.ItemQuantities.Add(item.quantity);
    //        }

    //       SaveLoadManager.SaveInventoryData(equipmentData);
    //    }


    //    private void LoadEquipment()
    //    {
    //        //if (itemDatabase == null)
    //        //{
    //        //    Debug.LogError("ItemDatabase is not assigned!");
    //        //}

    //        //// Load the equipment data from the file TODO: Possible Problem with Cast from Object
    //        //equipmentData = SaveLoadManager.LoadInventoryData<InventoryData>();

    //        //if (targetCharacter == null)
    //        //    Debug.LogError("Character is not assigned!");

    //        //if (equipmentData == null)
    //        //{
    //        //    Debug.LogWarning("No equipment data found!");
    //        //    return;
    //        //}

    //        //targetInventory.ClearItems();

    //        //for (int i = 0; i < equipmentData.ItemNames.Count; i++)
    //        //{
    //        //    Item item = itemDatabase.items.Find(x => x.itemName == equipmentData.ItemNames[i]);
    //        //    if (item != null)
    //        //    {
    //        //        targetInventory.AddItem(item, equipmentData.ItemIds[i], equipmentData.ItemQuantities[i]);
    //        //    }
    //        //}

    //        //// Equip items based on the loaded IDs
    //        //EquipItemById(equipmentData.EquippedHeadItemID, ArmourType.Head, helmet, HeadItem);
    //        //EquipItemById(equipmentData.EquippedChestItemID, ArmourType.Chest, chest, ChestItem);
    //        //EquipItemById(equipmentData.EquippedFeetItemID, ArmourType.Feet, feet, FeetItem);
    //        //EquipItemById(equipmentData.EquippedWeaponItemID, ItemType.Weapon, weapon, WeaponItem);
    //        //EquipItemById(equipmentData.EquippedShieldItemID, ItemType.Shield, shield, ShieldItem);
    //    }

    //    private void EquipItemById(string itemID, ItemType weaponType, SpriteLibrary spriteLibrary, Item currentItem)
    //    {
    //        // Load the item based on the ID
    //        // Item itemToEquip = SaveLoadManager.LoadItemData(itemID);

    //        Item itemToEquip = null;

    //        foreach (Item item in targetInventory.GetItems())
    //        {
    //            if (item.ItemID == itemID)
    //            {
    //                itemToEquip = item;
    //            }
    //        }

    //        if (itemToEquip == null)
    //        {
    //            Debug.LogWarning($"No item found with ID: {itemID}");
    //            return;
    //        }

    //        // Equip the item
    //        EquipItem(itemToEquip);
    //    }

    //    private void EquipItemById(string itemID, ArmourType armourType, SpriteLibrary spriteLibrary, Item currentItem)
    //    {
    //        // Load the item based on the ID
    //        Item itemToEquip = null;

    //        foreach (Item item in targetInventory.GetItems())
    //        {
    //            if (item.ItemID == itemID)
    //            {
    //                itemToEquip = item;
    //            }
    //        }

    //        if (itemToEquip == null)
    //        {
    //            Debug.LogWarning($"No item found with ID: {itemID}");
    //            return;
    //        }

    //        // Equip the item
    //        EquipItem(itemToEquip);
    //    }

    //    private void SetEquipmentColor(SpriteRenderer renderer, Color color)
    //    {
    //        if (renderer != null)
    //        {
    //            renderer.color = color;
    //        }
    //    }

   // }
