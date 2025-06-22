using Characters;
using Items;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryManagement
{
    public abstract class EquipmentSlot : MonoBehaviour
    {
        public EquippableItem boundItem { get; protected set; }

        public Type equipmentSlotItemType => GetSlotType();

        public bool isBound => boundItem != null;

        // UI Element References
        [SerializeField] protected Image itemImage;
        [SerializeField] protected Sprite defaultSprite;
        [SerializeField] protected EquipmentHandler playerEquipmentHandler;

        private void Start()
        {
            if (playerEquipmentHandler == null)
                playerEquipmentHandler = FindObjectOfType<Player>().GetComponent<EquipmentHandler>();

            itemImage.sprite = defaultSprite;
            itemImage.SetNativeSize();
        }

        /// <summary>
        /// Bind an inventory item to an equipment slot
        /// </summary>
        /// <param name="newBind"></param>
        /// <param name="equipToPlayer"></param>
        /// <returns></returns>
        public abstract bool BindEquipment(EquippableItem newBind, bool equipToPlayer = true);

        public abstract Type GetSlotType();

        public void UnbindEquipment()
        {
            if (!isBound)
            {
                Debug.LogWarning("Trying to unbind an empty equipment slot");
                return;
            }

            // Tell the equipment handler to unequip current item
            playerEquipmentHandler.UnequipItem(boundItem);

            itemImage.sprite = defaultSprite;
            itemImage.SetNativeSize();

            boundItem = null;
        }
    }
}