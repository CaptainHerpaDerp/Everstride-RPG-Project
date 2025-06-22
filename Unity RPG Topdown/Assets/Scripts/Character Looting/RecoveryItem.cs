using Characters;
using InventoryManagement;
using Items;
using System.Collections;
using UnityEngine;

namespace LootManagement
{
    public class RecoveryItem : MonoBehaviour
    {
        public Item item;
        private bool canBePickedUp = false;

        private void Start()
        {
            StartCoroutine(EnablePickup());
        }

        IEnumerator EnablePickup()
        {
            yield return new WaitForFixedUpdate();
            canBePickedUp = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!canBePickedUp)
                return;

            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Player picked up " + item.itemName);
                Player player = collision.gameObject.GetComponent<Player>();

                if (player == null)
                    return;

                if (item != null)
                {
                    player.GetComponent<Inventory>().AddItem(item);
                }

                Destroy(transform.parent.gameObject);
            }
        }
    }

}