using UnityEngine;
using System;
using Core.Enums;
using Characters;
using Core.Interfaces;

namespace DialogueSystem
{
    public class AIDialogueActivator : MonoBehaviour, IInteractable
    {
        [SerializeField] private NPC parentNPC;
        [SerializeField] private CircleCollider2D dialogueActivationTriggerCollider;
        [SerializeField] private float dialogueActivationRadius = 1f;
        [SerializeField] private CharacterContext characterContext;

        private AIDialogueController aiDialogueController;

        private Player player;


        private void Awake()
        {
            dialogueActivationTriggerCollider.radius = dialogueActivationRadius;
        }

        private void Start()
        {
            aiDialogueController = AIDialogueController.Instance;

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
            if (aiDialogueController == null)
            {
                Debug.LogError("AIDialogueController is null");
                return;
            }

            if (characterContext == null)
            {
                Debug.LogError("CharacterContext is null");
                return;
            }


            aiDialogueController.StartDialogue(characterContext);

            parentNPC.LookTowards(player.transform);

            player.LookTowards(parentNPC.transform);
        }
    }
}