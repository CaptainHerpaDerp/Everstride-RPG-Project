using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using Core.Enums;

namespace UIElements
{
    public class ItemBar : MonoBehaviour
    {
        [SerializeField] private Image ItemIcon, DamageIcon, ArmourIcon, HealthIcon;
        [SerializeField] private TextMeshProUGUI ItemName, ItemMainValue, ItemValue;

        [SerializeField] private GameObject itemQuantityGroup;
        [SerializeField] private TextMeshProUGUI itemQuantityText;

        public void InitializeBar(Item item, Sprite itemIcon, string ItemName, int ItemMainValue, int ItemCostValue, int ItemQuantity)
        {
            ItemIcon.sprite = itemIcon;
            this.ItemName.text = ItemName;

            this.ItemMainValue.text = ItemMainValue.ToString();
            ItemValue.text = ItemCostValue.ToString();

            if (ItemQuantity > 1)
            {
                itemQuantityGroup.SetActive(true);
                itemQuantityText.text = ItemQuantity.ToString();
            }
            else
            {
                itemQuantityGroup.SetActive(false);
            }

            if (item is WeaponItem)
            {
                DamageIcon.gameObject.SetActive(true);

            }
            else if (item is ArmourItem)
            {
                ArmourIcon.gameObject.SetActive(true);
            }
            else if (item is ConsumableItem)
            {
                // Cast the item to a consumable item
                ConsumableItem consumableItem = item as ConsumableItem;
                if (consumableItem != null)
                {
                    switch (consumableItem.consumableType)
                    {
                        case ConsumableType.HealthPotion:
                            HealthIcon.gameObject.SetActive(true);
                            break;
                    }
                }
            }
        }

        public void SetValue(int value)
        {
            ItemValue.SetText(value.ToString());
        }

        public void SetName(string name)
        {
            ItemName.SetText(name);
        }
    }
}
