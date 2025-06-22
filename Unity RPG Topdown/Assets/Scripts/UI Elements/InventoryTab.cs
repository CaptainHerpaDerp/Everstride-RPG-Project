using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Core.Enums;


namespace UIElements
{
    public class InventoryTab : MonoBehaviour
    {
        [SerializeField] private Sprite tabClosed, tabOpen;
        [SerializeField] private Sprite iconClosed, iconOpen;

        [Header("Component References")]
        [SerializeField] private Image image;

        private bool isOpen;

        public TabType tabType;

        public Action<TabType> OnTabOpen;

        private const float activatedOffset = 18.75f;

        private void Start()
        {
            if (image == null && !TryGetComponent(out image))
            {
                Debug.LogError("Image component not found on InventoryTab");
            }

            image.sprite = tabClosed;
        }

        public void OpenTab()
        {
            if (isOpen)
                return;

            OnTabOpen.Invoke(tabType);

            isOpen = true;

            image.sprite = tabOpen;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + activatedOffset, transform.localPosition.z);
        }

        public void CloseTab()
        {
            if (!isOpen)
                return;

            isOpen = false;

            image.sprite = tabClosed;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - activatedOffset, transform.localPosition.z);
        }

    }
}