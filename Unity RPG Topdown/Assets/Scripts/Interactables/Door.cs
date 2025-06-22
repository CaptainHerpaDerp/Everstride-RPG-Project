using UnityEngine;
using UnityEngine.AI;
using System;
using Core.Enums;
using Core.Interfaces;
using Characters;
using AudioManagement;

namespace Interactables
{
    /// <summary>
    /// An interactable that can be opened and closed by the player and NPCs.
    /// </summary>
    public class Door : Interactable, INPCPathInteractable
    {
        private bool isOpen;
        public bool isLocked;
        private SpriteRenderer doorSprite;
        private BoxCollider2D physicsCollider;
        private NavMeshObstacle navMeshObstacle;
        public Sprite doorOpen;
        public Sprite doorClosed;

        // Singleton References
        private AudioManager audioManager;
        private FMODEvents fmodEvents;

        public Vector2 Position { get => transform.position; set => transform.position = value; }

        protected override void Start()
        {
            base.Start();

            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;

            doorSprite = GetComponent<SpriteRenderer>();
            doorSprite.sprite = doorClosed;
            physicsCollider = GetComponentInChildren<BoxCollider2D>();
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        public override void Interact(object playerObject, Action<ActionType> modifyInteractableLabel)
        {
            if (!isOpen)
            {
                modifyInteractableLabel(ActionType.Use);
                doorSprite.sprite = doorOpen;
                physicsCollider.enabled = false;
                navMeshObstacle.enabled = false;

                audioManager.PlayOneShot(fmodEvents.doorOpenSound, transform.position); 

                // doorSignal.Raise();
            }
            else
            {
                modifyInteractableLabel(ActionType.Use);
                doorSprite.sprite = doorClosed;
                physicsCollider.enabled = true;
                navMeshObstacle.enabled = true;

                audioManager.PlayOneShot(fmodEvents.doorCloseSound, transform.position);

                // doorSignal.Raise();
            }

            isOpen = !isOpen;
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (isLocked)
                return;

            ActionType action;

            if (isOpen)
            {
                action = ActionType.Use;
            }
            else
            {
                action = ActionType.Use;
            }

            if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
            {
                player.AddToInteractables(this.gameObject, action);
            }
        }

        public void Open()
        {
            if (isLocked)
                return;

            isOpen = true;
            doorSprite.sprite = doorOpen;
            physicsCollider.enabled = false;
            navMeshObstacle.enabled = false;
        }

        public void Unlock()
        {
            isLocked = false;
        }

        public void Lock()
        {
            isLocked = true;
        }
    }
}
