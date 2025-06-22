using UnityEngine;
using Core.Enums;
using UIElements;
using Core;
using Sirenix.OdinInspector;
using AudioManagement;
using System;
using InventoryManagement;

namespace UIWindows
{
    public class InventoryMenu : Singleton<InventoryMenu>
    {
        [BoxGroup("Canvas Fading")]
        [SerializeField] private CanvasGroup canvasGroup;
        [BoxGroup("Canvas Fading")]
        [SerializeField] private float fadeInTime = 1f, fadeOutTime = 1f;

        [SerializeField] private Transform tabParent;

        [Header("Menu Parents")]

        [SerializeField] EquipmentInventory playerEquipmentInventory;
        [SerializeField] QuestMenu playerQuestMenu;

        [ShowInInspector] private bool isOpen;
        private TabType currentTab = TabType.Equipment;

        // Singleton Reference
        private EventBus eventBus;
        private AudioManager audioManager;
        private FMODEvents fmodEvents;

        // Actions
        public Action OnQuestMenuOpen, OnQuestMenuClosed;

        void Start()
        {
            eventBus = EventBus.Instance;
            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;

            eventBus.Subscribe("OpenInventoryMenu", OpenMenu);
            eventBus.Subscribe("CloseMenu", CloseMenu);

            InitializeTabs();
        }

        private void InitializeTabs()
        {
            for (int i = 0; i < tabParent.childCount; i++)
            {
                InventoryTab tab = tabParent.GetChild(i).GetComponent<InventoryTab>();

                if (tab == null)
                    continue;

                // Subscribe to the tab's OnTabOpen event
                tab.OnTabOpen += OpenTabMenu;
            }
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void OpenMenu()
        {
            if (isOpen)
                return;

            PlayMenuOpenSound();

            isOpen = true;

            // Enables all the Tabs
            tabParent.gameObject.SetActive(true);

            // Opens the last opened tab
            OpenTab(currentTab);

            StartCoroutine(Utils.FadeCanvasGroup(canvasGroup, fadeInTime, fadingIn: true));
        }

        public void CloseMenu()
        {
            if (!isOpen && canvasGroup.alpha > 0)
                return;

            PlayMenuCloseSound();

            isOpen = false;

            StartCoroutine(Utils.FadeCanvasGroup(canvasGroup, fadeOutTime, fadingIn: false, afterFadeAction: () =>
            {

                // Returns all tabs to a disabled state 
                for (int i = 0; i < tabParent.childCount; i++)
                {
                    InventoryTab tab = tabParent.GetChild(i).GetComponent<InventoryTab>();

                    if (tab == null)
                        continue;

                    tab.CloseTab();
                }

                CloseAllWindows();

            }));
        }

        private void OpenTabMenu(TabType tabType)
        {
            CloseTab(currentTab);

            currentTab = tabType;

            switch (tabType)
            {
                case TabType.Inventory:
                    //playerGeneralInventory.OpenInventory();
                    break;
                case TabType.Equipment:
                    playerEquipmentInventory.OpenInventory();
                    break;
                case TabType.Magic:

                    break;
                case TabType.Skills:
                    break;
                case TabType.Quests:
                    OnQuestMenuOpen?.Invoke();
                    break;
                case TabType.Map:
                    break;
            }

            // TODO: Play Tab Sound
        }

        private void CloseTab(TabType type)
        {
            for (int i = 0; i < tabParent.childCount; i++)
            {
                InventoryTab tab = tabParent.GetChild(i).GetComponent<InventoryTab>();

                if (tab == null)
                    continue;

                if (tab.tabType == type)
                {
                    tab.CloseTab();
                    
                    switch (type)
                    {
                        case TabType.Inventory:
                        //    playerGeneralInventory.CloseInventory();
                            break;
                        case TabType.Equipment:
                            playerEquipmentInventory.CloseInventory();
                            break;
                        case TabType.Skills:
                            break;
                        case TabType.Quests:
                            OnQuestMenuClosed.Invoke();
                            break;
                        case TabType.Map:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the tab of the given type, would only be called when the menu is being opened and the last opened tab needs to be reactivated.
        /// </summary>
        /// <param name="type"></param>
        private void OpenTab(TabType type)
        {
            for (int i = 0; i < tabParent.childCount; i++)
            {
                InventoryTab tab = tabParent.GetChild(i).GetComponent<InventoryTab>();

                if (tab == null)
                    continue;

                if (tab.tabType == type)
                    tab.OpenTab();
            }
        }

        private void CloseAllWindows()
        {
           // playerGeneralInventory.CloseInventory();
            playerEquipmentInventory.CloseInventory();
            playerQuestMenu.CloseMenu();

            OnQuestMenuClosed?.Invoke();    
        }

        #region Audio Methods

        private void PlayMenuOpenSound()
        {
            audioManager.PlayOneShot(fmodEvents.menuOpenSound, transform.position);
        }

        private void PlayMenuCloseSound()
        {
            audioManager.PlayOneShot(fmodEvents.menuCloseSound, transform.position);
        }

        #endregion
    }
}
