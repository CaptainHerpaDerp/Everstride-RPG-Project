using Items;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements {

    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private GameObject darkPanel;
        [SerializeField] private CanvasGroup quantityCanvasGroup;
        [SerializeField] private TMPro.TextMeshProUGUI quantityTextComponent;
        [SerializeField] private Image itemImage; 

        public Item LinkedItem;

        private string quantityText { get => quantityTextComponent.text; set => quantityTextComponent.text = value; }
        public GameObject ImageObject => itemImage.gameObject;

        public void ToggleDarkPanel(bool active)
        {
            darkPanel.SetActive(active);
        }

        /// <summary>
        /// Set the linked item, as well as modify any UI elements to reflect the item
        /// </summary>
        /// <param name="item"></param>
        public void SetItem(Item item, int quantity = 1)
        {
            LinkedItem = item;
            itemImage.sprite = item.icon;
            itemImage.SetNativeSize();

            SetItemQuantity(quantity);
        }

        public void SetItemQuantity(int quantity)
        {
            if (quantityTextComponent == null || quantityCanvasGroup == null)
            {
                Debug.LogError("Quantity text component or canvas group is null");
                return;
            }

            if (quantity > 1)
            {
                // Set the quantity text and make the quantity canvas group visible
                quantityText = quantity.ToString();
                quantityCanvasGroup.alpha = 1;
            }
            else
            {
                // Hide the quantity canvas group
                quantityCanvasGroup.alpha = 0;
            }
        }
    }
}