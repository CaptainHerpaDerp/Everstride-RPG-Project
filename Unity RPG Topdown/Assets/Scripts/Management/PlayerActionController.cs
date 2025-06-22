using Characters;
using Core.Enums;
using InventoryManagement;
using Items;
using UnityEngine;

namespace Management
{
    public class PlayerActionController : MonoBehaviour
    {
        // Temp
        KeyCode healthPotionKey = KeyCode.H;

        [SerializeField] private Inventory playerInventory;
        [SerializeField] private Player playerClass;

        private void Start()
        {
            if (playerClass == null)
                playerClass = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            if (playerInventory == null)
                playerInventory = playerClass.GetComponent<Inventory>();

            //DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(healthPotionKey))
            {
                Item appropriatePotion = null;
                float smallestDifference = float.MaxValue;

                foreach (Item item in playerInventory.GetItems())
                {
                    if (item.GetType() == typeof(ConsumableItem))
                    {
                        ConsumableItem potion = item as ConsumableItem;

                        if (potion.consumableType == ConsumableType.HealthPotion)
                        {
                            float remainingHealth = playerClass.HitPointsMax - playerClass.HitPoints;
                            float currentDifference = remainingHealth - potion.effectQuantity;

                            if (currentDifference > 0 && currentDifference < smallestDifference)
                            {
                                smallestDifference = currentDifference;
                                appropriatePotion = item;
                            }                          
                        }
                    }
                }

                if (appropriatePotion != null)
                {
                    Debug.LogError("Potion drink action needs implementing!!");
                    //playerClass.DrinkPotion(appropriatePotion);
                    playerInventory.RemoveItem(appropriatePotion);
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (Time.timeScale < 100)
                    Time.timeScale += 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Time.timeScale > 0)
                    Time.timeScale -= 0.1f;
            }
        }



    }
}