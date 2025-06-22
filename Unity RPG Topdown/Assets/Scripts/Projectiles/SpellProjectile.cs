using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using Core;
using Core.Enums;
using Characters;

namespace Projectiles
{
    public class SpellProjectile : Projectile
    {
        [BoxGroup("Component References")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;

        [BoxGroup("Spell Properties")]
        [SerializeField] SpellType spellType;

        [BoxGroup("Spell Properties")]
        [SerializeField] protected float fadeOutTime = 0.5f;
        [SerializeField] protected bool preserveSpriteRotation = false;

        [BoxGroup("Animation Properties")]
        [SerializeField] protected float explodeAnimationTime = 0.5f;
        private const string EXPLODE = "Explode";

        #region Overrides

        public override void LaunchProjectile()
        {
            base.LaunchProjectile();

            // For some spells (like the twister spell), we want to have the sprite always face the original direction
            if (preserveSpriteRotation)
            {
                // Get the current rotation of the projectile (just need the z-axis)
                float rotationZ = transform.rotation.z;

                // Set the sprite rotation to the opposite of the current rotation
                spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -rotationZ);

                // If we are firing the projectile to the left, we need to flip the sprite
                if (transform.right.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
            }
        }

        // When a spell projectile ends its travel, it will simply fade out and destroy itself
        protected override void OnTravelEnd()
        {
            // Do not fade out if the projectile has made contact with something
            if (contact)
            {
                return;
            }

            StartCoroutine(FadeOutAndInvokeAction(() =>
            {
                Destroy(gameObject);
            }));
        }

        protected override void OnImpact(Collision2D collision)
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

            // Mark contact as true so that the projectile doesn't fade out or hit anything else
            contact = true;

            if (collision.gameObject.GetComponent<Character>())
            {
                // Debug.Log("Hit " + collision.gameObject.name);
                collision.gameObject.GetComponent<Character>().DealDamage(damage, transform);
            }

            // Disable the collider so that the projectile doesn't hit anything else
            rb.velocity = Vector2.zero;
            circleCollider.enabled = false;

            // Play the explode animation
            animator.Play(EXPLODE);

            // Play the explode sound
            audioManager.PlayOneShot(fMODEvents.GetCorrespondingSpellExplodeSound(spellType), transform.position);  

            // Destroy the projectile after the animation has finished
            StartCoroutine(Utils.WaitDurationAndExecute(explodeAnimationTime, () =>
            {
                Destroy(gameObject);
            }));    
        }

        #endregion

        private IEnumerator FadeOutAndInvokeAction(Action action)
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutTime)
            {
                float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeOutTime);
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            action?.Invoke();
        }
    }
}