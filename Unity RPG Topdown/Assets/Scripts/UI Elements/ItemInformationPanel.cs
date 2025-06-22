using TMPro;
using UnityEngine;
using Items;
using Core.Enums;
using Core;
using AudioManagement;

namespace UIElements
{
    public class ItemInformationPanel : Singleton<ItemInformationPanel>  
    {
        [SerializeField] TextMeshProUGUI hoveredItemNameText, hoveredItemTypeText, hoveredItemMainValue, hoveredItemGoldValue, hoveredItemWeightValue;

        [SerializeField] private Vector2 itemInfoPanelOffset = new Vector2(0, 0);

        [SerializeField] Transform ItemIconsParent;

        [SerializeField] private CanvasGroup canvasGroup;

        private void Start()
        {
            Hide();
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
        }

        private void SetToMousePosition()
        {
            // Get the mouse position in screen space
            Vector3 mousePosScreen = Input.mousePosition;

            transform.position = mousePosScreen + (Vector3)itemInfoPanelOffset;
        }

        /// <summary>
        /// EVERY FIELD IS RESET FOR WHATEVER REASON, SO I HAVE TO SET THEM ALL AGAIN
        /// </summary>
        /// <param name="item"></param>
        public void ShowItemInformation(Item item)
        {
            canvasGroup.alpha = 1;
            SetToMousePosition();

            if (item == null)
            {
                Debug.LogError("Item is null");
                return;
            }

            //hoveredItemNameText = transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            //hoveredItemTypeText = transform.Find("ItemType").GetComponent<TextMeshProUGUI>();
            //hoveredItemMainValue = transform.Find("ItemMainValue").GetComponent<TextMeshProUGUI>();
            //hoveredItemGoldValue = transform.Find("ItemGoldValue").GetComponent<TextMeshProUGUI>();
            //hoveredItemWeightValue = transform.Find("ItemWeightValue").GetComponent<TextMeshProUGUI>();
            //ItemIconsParent = transform.Find("ItemIcons");

            hoveredItemNameText.SetText(item.itemName);
            //hoveredItemNameText.SetText(item.ItemID);

            if (item is WeaponItem)
            {
                WeaponItem weaponItem = item as WeaponItem;

                SetItemIconActive("AttackIcon");
                SetMainValue(weaponItem.weaponDamage, "Attack");

                hoveredItemTypeText.SetText("Weapon - " + weaponItem.weaponType.ToString());
            }

            else if (item is ShieldItem)
            {
                ShieldItem shieldItem = item as ShieldItem;

                SetItemIconActive("DefenceIcon");
                SetMainValue(shieldItem.shieldPowerMin, shieldItem.shieldPowerMax, "Reduction");

                hoveredItemTypeText.SetText("Shield");
            }

            else if (item is ArmourItem)
            {
                ArmourItem ArmourItem = item as ArmourItem;

                SetItemIconActive("DefenceIcon");
                SetMainValue(ArmourItem.armourDefense, "Defence");

                hoveredItemTypeText.SetText("armour - " + ArmourItem.armourType.ToString());
            }

            else if (item is ConsumableItem)
            {
                ConsumableItem consumableItem = item as ConsumableItem;

                switch (consumableItem.consumableType)
                {
                    case ConsumableType.HealthPotion:
                        SetItemIconActive("HealthIcon");
                        SetMainValue(consumableItem.effectQuantity, "Health");
                        hoveredItemTypeText.SetText("Consumable - Potion");
                        break;

                    case ConsumableType.Food:
                        SetItemIconActive("HealthIcon");
                        SetMainValue(consumableItem.effectQuantity, "Health");
                        hoveredItemTypeText.SetText("Consumable - Food");
                        break;
                }
            }

            else if (item is AmmunitionItem)
            {
                AmmunitionItem ammunitionItem = item as AmmunitionItem;

                SetItemIconActive("AttackIcon");
                SetMainValue(ammunitionItem.damage, "Attack");
                hoveredItemTypeText.SetText("Ammunition");

            }

            // If the item is just a regular item, then just show the item name and type
            else
            {
                DisableAllItemIcons();
                hoveredItemTypeText.SetText("Item");
            }

            hoveredItemGoldValue.SetText("Value: " + item.value.ToString());
            hoveredItemWeightValue.SetText("Weight: " + item.weight.ToString());
        }

        private void DisableAllItemIcons()
        {
            for (int i = 0; i < ItemIconsParent.childCount; i++)
            {
                ItemIconsParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void SetItemIconActive(string iconName)
        {
            DisableAllItemIcons();

            for (int i = 0; i < ItemIconsParent.childCount; i++)
            {
                if (ItemIconsParent.GetChild(i).name == iconName)
                {
                    ItemIconsParent.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
        private void SetMainValue(float value, string valueOf)
        {
            hoveredItemMainValue.SetText(value.ToString() + " " + valueOf);
        }

        private void SetMainValue(float value1, float value2, string valueOf)
        {
            hoveredItemMainValue.SetText($"{value1} - {value2} {valueOf}");
        }

        public void Update()
        {
            SetToMousePosition();
        }
    }
}