using UnityEngine;
using DialogueSystem;
using System;
using Core.Enums;
using Core.Interfaces;

namespace Characters.Interactables
{
    public class DialogueActivator : MonoBehaviour, IInteractable
    {
        [SerializeField] private NPC parentNPC;
        [SerializeField] private Dialogue dialogue;
        [SerializeField] private CircleCollider2D dialogueActivationCollider;
        [SerializeField] private float dialogueActivationRadius = 1f;
        private Player player;

        private void Awake()
        {
            dialogueActivationCollider.radius = dialogueActivationRadius;

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.AddToInteractables(this.gameObject, ActionType.Talk);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.RemoveFromInteractables(this.gameObject);
            }
        }

        public void Interact(object playerObject, Action<ActionType> modifyInteractableLabel)
        {
            DialogueController.Instance.StartDialogue(dialogue, parentNPC.transform);

            parentNPC.LookTowards(player.transform);

            player.LookTowards(parentNPC.transform);
        }
    }
}