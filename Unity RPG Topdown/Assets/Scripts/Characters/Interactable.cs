using Characters;
using Core.Enums;
using Core.Interfaces;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// An object that can be interacted with by the player.
    /// </summary>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        GameObject player;       

        [BoxGroup("Components"), SerializeField] private CircleCollider2D circleCollider;
        [BoxGroup("Components"), SerializeField] private GameObject UsagePromptGroupPrefab;
        protected GameObject usagePromptGroup;

        [BoxGroup("Interactable Settings"), SerializeField] private float interactionRange;
        [BoxGroup("Interactable Settings"), SerializeField] protected ActionType interactionAction;


        protected virtual void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

            circleCollider.radius = interactionRange;

            usagePromptGroup = Instantiate(UsagePromptGroupPrefab, transform.position, rotation: UsagePromptGroupPrefab.transform.rotation, parent: this.transform);
            usagePromptGroup.SetActive(false);
        }

        public abstract void Interact(object playerObject, Action<ActionType> modifyInteractableLabel);

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.AddToInteractables(this.gameObject, interactionAction);

                usagePromptGroup.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.RemoveFromInteractables(this.gameObject);

                usagePromptGroup.SetActive(false);
            }
        }
    }
}