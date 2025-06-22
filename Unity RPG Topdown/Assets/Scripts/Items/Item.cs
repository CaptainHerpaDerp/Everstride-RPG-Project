using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Ligofff.CustomSOIcons;

namespace Items
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        #region Item Identification 

        [ShowInInspector, ReadOnly]
        private string itemID;
        public string ItemID => itemID;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = Guid.NewGuid().ToString();
            }
        }

        private void Reset()
        {
            // Assign a new GUID when the item is first created
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = Guid.NewGuid().ToString();
            }
        }

        #endregion

        [BoxGroup("General Info")]
        public string itemName;

        [BoxGroup("General Information")]
        [CustomAssetIcon, PreviewField]
        public Sprite icon;

        [BoxGroup("General Information")]
        public float weight;

        [BoxGroup("General Information")]
        public int value;

        [BoxGroup("General Information")]
        public bool stackableItem;
        
        /// <summary>
        /// If the item is either an equippable item or a subclass of an equippable item
        /// </summary>
        public bool IsEquippable => this.GetType() == typeof(EquippableItem) || this.GetType().IsSubclassOf(typeof(EquippableItem));
    }
}