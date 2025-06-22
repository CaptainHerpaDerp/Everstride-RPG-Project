using Characters;
using Core.Enums;
using UnityEngine;

namespace Projectiles
{
    public class SpiderProjectile : Projectile
    {
        [SerializeField] private Sprite orthogonalProjSprite, diagonalProjSprite;

        public void LaunchProjectile(float angle)
        {
            rb.AddForce(transform.right * speed, ForceMode2D.Impulse);
            StartCoroutine(EndTravel());
        }


        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<Character>())
            {
                Character character = collision.gameObject.GetComponent<Character>();
                character.DealDamage(damage, transform);
                character.ApplyStatusEffect(StatusEffect.TempSlow, 5, transform);
            }

            Destroy(gameObject);
        }
    }
}