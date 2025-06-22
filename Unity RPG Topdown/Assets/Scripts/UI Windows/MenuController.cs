using UnityEngine;
using System;
using Characters;
using Core;

namespace UIWindows
{
    public class MenuController : MonoBehaviour
    {
        public MenuController Instance { get; private set; }

        [SerializeField] private KeyCode openKey = KeyCode.Tab;

        //public Action OnMenuOpen, OnMenuClosed;

        private EventBus eventBus;

        public bool IsOpen { get; private set; }

        private void Start()
        {
            eventBus = EventBus.Instance;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            eventBus.Subscribe("OpenUIMenu", OpenMenu);
            eventBus.Subscribe("CloseUIMenu", CloseMenu);
        }

        private void OnDisable()
        {
            //OnMenuOpen -= InventoryMenu.Instance.OpenMenu;
            //OnMenuClosed -= InventoryMenu.Instance.CloseMenu;

            //Shop.OnShopToggle -= ToggleMenu;

            //DialogueController.Instance.DialogueOpenedEvent -= OpenMenu;
            //DialogueController.Instance.DialogueClosedEvent -= CloseMenu;

            //AIDialogueController.Instance.DialogueOpenedEvent -= OpenMenu;
            //AIDialogueController.Instance.DialogueClosedEvent -= CloseMenu;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KC.OpenInventoryKey))
            {
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            IsOpen = !IsOpen;

            if (IsOpen)
            {
                // invoke the inventory menu open event
                eventBus.Publish("OpenInventoryMenu");
                Player.Instance.LockPlayerControl(true);
            }
            else
            {
                eventBus.Publish("CloseMenu");
                Player.Instance.UnlockPlayerControl();
            }
        }

        private void OpenMenu()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;

            Player.Instance.LockPlayerControl(true);
        }

        private void CloseMenu()
        {
            IsOpen = false;

            Player.Instance.UnlockPlayerControl();
        }
    }
}