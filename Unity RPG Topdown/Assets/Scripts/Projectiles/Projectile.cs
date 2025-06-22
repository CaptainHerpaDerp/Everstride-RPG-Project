using AudioManagement;
using Characters;
using Core;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Projectiles
{
    public class Projectile : MonoBehaviour
    {
        [BoxGroup("Component References")]
        [SerializeField] protected Rigidbody2D rb;
        [BoxGroup("Component References")]
        [SerializeField] protected CircleCollider2D circleCollider;
        [BoxGroup("Component References")]
        [SerializeField] protected CircleCollider2D pickupCollider;

        [BoxGroup("Projectile Properties")]
        [SerializeField] protected float speed = 10f;
        [BoxGroup("Projectile Properties")]
        [SerializeField] protected float damage = 10f;
        [BoxGroup("Projectile Properties")]
        [SerializeField] protected float lifeTime = 3f;
        [BoxGroup("Projectile Properties")]
        [SerializeField] protected bool recoverable = false;

        [BoxGroup("Start Settings")]
        [SerializeField] public GameObject ExclusionObject;

        protected bool contact = false;

        // Singleton References
        protected AudioManager audioManager;
        protected FMODEvents fMODEvents;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            fMODEvents = FMODEvents.Instance;

            LaunchProjectile();
        }

        /// <summary>
        /// Send the projectile in the direction it is facing and start the end travel coroutine to destroy the projectile after a certain amount of time
        /// </summary>
        public virtual void LaunchProjectile()
        {
            rb.AddForce(transform.right * speed, ForceMode2D.Impulse);
            StartCoroutine(EndTravel());
        }

        protected virtual IEnumerator EndTravel()
        {
            yield return new WaitForSeconds(lifeTime);

            OnTravelEnd();
        }

        protected virtual void OnTravelEnd()
        {
            recoverable = true;
            contact = true;

            // Disable the Rigidbody to stop physics simulation
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            if (recoverable)
                // Activates the collider which allows the player to pick up the arrow
                pickupCollider.enabled = true;

            // Optionally, you can disable the Collider to prevent further collisions
            GetComponent<Collider2D>().enabled = false;
        }

        protected virtual void OnImpact(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<Projectile>())
            {
                return;
            }

            // Do not collide with the character that fired the projectile (exclusion object)
            if (collision.gameObject == ExclusionObject)
            {
                return;
            }

            if (collision.gameObject.GetComponent<Character>())
            {
                // Debug.Log("Hit " + collision.gameObject.name);
                collision.gameObject.GetComponent<Character>().DealDamage(damage, transform);
                Destroy(gameObject);
            }

            else
            {
                // Stick the arrow to the point of contact
                StickToTarget(collision.contacts[0].point);

                // Activates the collider which allows the player to pick up the arrow
                transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (contact)
            {
                return;
            }

            OnImpact(collision);    
        }

        protected virtual void StickToTarget(Vector2 contactPoint)
        {
            rb.freezeRotation = true;
            recoverable = true;
            contact = true;

            // Disable the Rigidbody to stop physics simulation
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // Set the arrow's position to the contact point
            transform.position = contactPoint;

            // Optionally, you can disable the Collider to prevent further collisions
            GetComponent<Collider2D>().enabled = false;
        }
    }
}