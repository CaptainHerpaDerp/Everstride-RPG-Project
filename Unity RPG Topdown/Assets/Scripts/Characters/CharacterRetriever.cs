using Core.Enums;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Utilities
{
    /// <summary>
    /// Retreives enemies and allies in range
    /// </summary>
    public class CharacterRetriever : MonoBehaviour
    {
        [BoxGroup("Components"), SerializeField] private NPC parentNPC;

        [BoxGroup("Settings"), SerializeField] private LayerMask characterLayer;

        [Header("The detection range for charcters")]
        [BoxGroup("Settings"), SerializeField] private float targetOverlapRange;

        private void Start()
        {
            if (parentNPC == null && !TryGetComponent(out parentNPC))
            {
                Debug.LogError("Parent NPC is null, and couldn't manually retrieve it!");
            }
        }

        public Character GetClosestEnemy(List<Faction> alliedFactions)
        {
            List<Character> enemies = GetEnemiesInRange(alliedFactions);
            if (enemies.Count == 0)
            {
                Debug.Log("Could not find any enemies");
                return null;
            }
            Character closestEnemy = enemies[0];
            float closestDistance = Vector2.Distance(transform.position, closestEnemy.transform.position);
            foreach (Character enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }


        #region Retreival Methods

        public List<Character> GetAlliesInRange(List<Faction> alliedFactions)
        {
            List<Character> returnCharacters = new List<Character>();

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, targetOverlapRange, characterLayer);

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Character character))
                {
                    // Skip the parent NPC
                    if (character == parentNPC)
                    {
                        continue;
                    }

                    // If the character is in the allied factions, add them to the return characters
                    if (character.IsInFaction(alliedFactions))
                    {
                        returnCharacters.Add(character);
                    }

                    returnCharacters.Add(character);
                }
            }

            return returnCharacters;
        }

        public List<Character> GetEnemiesInRange(List<Faction> alliedFactions)
        {
            List<Character> returnCharacters = new List<Character>();

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, targetOverlapRange, characterLayer);

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Character character))
                {
                    // Skip the parent NPC
                    if (character == parentNPC)
                    {
                        continue;
                    }

                    // If the character is NOT in the allied factions, add them to the return characters
                    if (!character.IsInFaction(alliedFactions))
                    {
                        returnCharacters.Add(character);
                    }

                    returnCharacters.Add(character);
                }
            }

            if (returnCharacters.Count == 0)
            {
                Debug.LogWarning("Could not find any characters");
            }

            return returnCharacters;
        }

        public List<Character> GetCharactersInRange()
        {

            List<Character> returnCharacters = new List<Character>();

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, targetOverlapRange, characterLayer);

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Character character))
                {
                    // Skip the parent NPC
                    if (character == parentNPC)
                    {
                        continue;
                    }

                    returnCharacters.Add(character);
                }
            }

            return returnCharacters;
        }

        #endregion
    }
}

