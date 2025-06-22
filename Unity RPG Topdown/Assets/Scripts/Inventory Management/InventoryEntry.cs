using System;
using Sirenix.OdinInspector;
using Items;
using System.Diagnostics;

namespace InventoryManagement
{
    [Serializable]
    public class InventoryEntry
    {
        [HorizontalGroup("Item Info", Width = 50)]
        [PreviewField(Height = 50), HideLabel] // Display the item's icon
        public Item item;

        [HorizontalGroup("Item Info")]
        [VerticalGroup("Item Info/Details")]
        [ShowInInspector, LabelText("Name"), ReadOnly] // Show the item's name
        public string itemName => item != null ? item.itemName : "None";
        private bool IsStackable => item != null && item.stackableItem;

        [HorizontalGroup("Item Info")]
        [VerticalGroup("Item Info/Details")]
        [ShowInInspector, ShowIf("IsStackable"), LabelText("Quantity")]
        private int _quantity;

        public int quantity
        {
            get
            {
                if (!IsStackable)
                {
                    return 0;
                }
                else
                {
                    return _quantity;
                }
            }
            set
            {
                if (IsStackable)
                {
                    _quantity = value;
                }
                else
                {
                    Console.WriteLine("Trying to modify the quantity of an item that isn't marked as stackable!");
                }
            }
        }
    }
}