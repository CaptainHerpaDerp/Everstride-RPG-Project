using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    /// <summary>
    /// This script is used to apply a knockback effect to an object. It is designed to work with a NavMeshAgent and a Rigidbody2D.
    /// </summary>
    public class KnockbackController : MonoBehaviour
    {
        public float initialKnockbackForce = 10f; // Adjust as needed.
        public float knockbackDuration = 0.5f; // How long the knockback effect lasts.

        private NavMeshAgent agent;
        private Rigidbody2D agentRigidbody;
        private bool isKnockbackActive = false;
        private float knockbackTimer = 0f;

        // Variables for fading force.
        private Vector3 knockbackDirection;
        private float currentKnockbackForce;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            agentRigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Check if the knockback is active and decrement the timer.
            if (isKnockbackActive)
            {
                knockbackTimer -= Time.deltaTime;

                // Gradually reduce the force over time.
                if (knockbackTimer > 0)
                {
                    currentKnockbackForce = initialKnockbackForce * (knockbackTimer / knockbackDuration);

                    // Apply the fading knockback force.
                    agentRigidbody.velocity = knockbackDirection * currentKnockbackForce;
                }
                else
                {
                    // If the knockback duration has elapsed, reset everything.
                    isKnockbackActive = false;
                    agent.isStopped = false;
                    agentRigidbody.isKinematic = true;
                    agentRigidbody.velocity = Vector2.zero;
                }
            }
        }

        public bool IsKnockbackActive()
        {
            return isKnockbackActive;
        }

        public void ApplyKnockback(Vector3 knockbackSource)
        {
            if (!isKnockbackActive)
            {
                // Calculate the initial knockback direction.
                knockbackDirection = (transform.position - knockbackSource).normalized;
                currentKnockbackForce = initialKnockbackForce;

                // Disable the NavMeshAgent and enable the Rigidbody.
                agent.isStopped = true;
                agentRigidbody.isKinematic = false;

                // Apply the initial knockback force.
                agentRigidbody.velocity = knockbackDirection * initialKnockbackForce;

                // Start the knockback timer.
                knockbackTimer = knockbackDuration;

                isKnockbackActive = true;
            }
        }
    }
}