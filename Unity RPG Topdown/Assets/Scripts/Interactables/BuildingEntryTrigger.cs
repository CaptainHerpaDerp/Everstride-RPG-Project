using Characters;
using System;
using UnityEngine;

namespace Interactables
{
    public class BuildingTrigger : MonoBehaviour
    {
        public Action onEnter;
        public Action onExit;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Player>())
            {
                onEnter?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {

            if (other.GetComponent<Player>())
            {
                onExit?.Invoke();
            }
        }

    }
}