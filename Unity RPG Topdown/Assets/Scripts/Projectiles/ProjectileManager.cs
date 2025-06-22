using Characters;
using Core;
using Core.Enums;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileManager : Singleton<ProjectileManager>
    {
        [BoxGroup("Spell Prefabs")]
        public GameObject fireballPrefab, iceSpellPrefab, earthSpellPrefab, airSpellPrefab;

        [BoxGroup("Arrow Prefabs")]
        public GameObject defaultArrowPrefab;

        private Dictionary<SpellType, GameObject> spellTypePrefabPairs;

        // Singleton References
        private EventBus eventBus;

        private void Start()
        {
            // Singleton References
            eventBus = EventBus.Instance;

            spellTypePrefabPairs = new Dictionary<SpellType, GameObject>
            {
                { SpellType.Fire, fireballPrefab },
                { SpellType.Ice, iceSpellPrefab },
                { SpellType.Earth, earthSpellPrefab },
                { SpellType.Wind, airSpellPrefab }
            };

            // Subscribe to events
            eventBus.Subscribe<ProjectileFireData>("FireProjectile", FireProjectileFrom);
            eventBus.Subscribe<SpellProjectileFireData>("FireSpellProjectile", FireSpellProjectileFrom);
        }

        public void FireProjectileFrom(ProjectileFireData projectileFireData)
        {
            GameObject projectilePrefab = projectileFireData.projectilePrefab;

            // If the projectile prefab is null, set it to the default arrow prefab
            if (projectilePrefab == null)
            {
                projectilePrefab = defaultArrowPrefab;
            }

            FireProjectileFrom(projectileFireData.sourcePos, projectileFireData.fireAngle, projectilePrefab, projectileFireData.exclusionObject, projectileFireData.projectileSpawnDistance);
        }

        public void FireSpellProjectileFrom(SpellProjectileFireData spellProjectileFireData)
        {
            FireSpellProjectileFrom(spellProjectileFireData.sourcePos, spellProjectileFireData.fireAngle, spellProjectileFireData.spellType, spellProjectileFireData.exclusionObject, spellProjectileFireData.projectileSpawnDistance);
        }

        public void FireProjectileFrom(Vector2 position, float angle, GameObject projectilePrefab, GameObject exclusionObject, float arrowSpawnDistance = 0.45f)
        {
            Quaternion arrowRot = Quaternion.Euler(0, 0, angle);

            float arrowDistance = arrowSpawnDistance;

            float angleRad = angle * Mathf.Deg2Rad;

            float arrowX = Mathf.Cos(angleRad) * arrowDistance;
            float arrowY = Mathf.Sin(angleRad) * arrowDistance;

            Projectile projectileObject = Instantiate(projectilePrefab, position + new Vector2(arrowX, arrowY), arrowRot, parent: this.transform).GetComponent<Projectile>();
            projectileObject.ExclusionObject = exclusionObject; 

            //if (recoverableItem != null)
            //{
            //    gameObject.name = recoverableItem.itemName;
            //   // Item newItemInstance = Instantiate(recoverableItem);
            //   // gameObject.GetComponentInChildren<RecoveryItem>().item = newItemInstance;

            //    if (recoverableItem.AmmoSprite != null)
            //    {
            //        Sprite customSprite = recoverableItem.AmmoSprite;
            //        gameObject.GetComponent<SpriteRenderer>().sprite = customSprite;
            //    }
            //}

            //gameObject.GetComponent<Projectile>().LaunchProjectile();
        }

        public void FireSpellProjectileFrom(Vector2 position, float angle, SpellType spellType, GameObject exclusionObject, float arrowSpawnDistance = 0.45f)
        {       
            Quaternion projectileRot = Quaternion.Euler(0, 0, angle);       

            float arrowDistance = arrowSpawnDistance;

            float angleRad = angle * Mathf.Deg2Rad;

            float arrowX = Mathf.Cos(angleRad) * arrowDistance;
            float arrowY = Mathf.Sin(angleRad) * arrowDistance;

            SpellProjectile spellProjectileObject = Instantiate(spellTypePrefabPairs[spellType], position + new Vector2(arrowX, arrowY), projectileRot, parent: this.transform).GetComponent<SpellProjectile>();
            spellProjectileObject.ExclusionObject = exclusionObject;
        }

        // Todo: make item recovery event based, invoke event when player picks up item

    }
}
