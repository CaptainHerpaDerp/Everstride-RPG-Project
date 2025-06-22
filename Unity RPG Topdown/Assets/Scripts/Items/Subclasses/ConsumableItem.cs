using Core.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
    public class ConsumableItem : Item
    {
        [BoxGroup("Consumable Attributes")]
        public ConsumableType consumableType;

        [BoxGroup("Consumable Attributes")]
        public int effectQuantity;
    }
}