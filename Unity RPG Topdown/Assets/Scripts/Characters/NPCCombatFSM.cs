using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Needed for NavMeshAgent

namespace Characters
{
    public class NPCCombatFSM : MonoBehaviour
    {
        public enum CombatState
        {
            Idle, EngageSolo, EngageGroup, Retreat, Regroup, Flank
        }

        public NPC parentNPC;
        public Mover mover;
        public List<Transform> allies;

        [Header("The Layer that all characters share - used for target retreival")]
        [SerializeField] private LayerMask characterLayer;

        [Header("The rate (sec) at which the NPC will retreive targets")]
        [SerializeField] private float targetRetreivalRate = 0.5f;

        [Header("The percentage (/100) of health before the npc retreats from combat")]
        [SerializeField] private float retreatPercentageThreshold = 0.2f;

        [Header("Distance Between This NPC And Its Target Before It Apprehends the Target")]
        [SerializeField] private float viewRange;

        [Header("The range at which the NPC will detect its allies to change combat behavior")]
        [SerializeField] private float groupCombatRange = 10f;

        public CombatState state = CombatState.Idle;

        private Character combatTarget;

        private IEnumerator attackCR = null;

        [SerializeField] private int navMeshAreaMask;

        #region Decision Weightings
        [SerializeField] private float attackWeight = 0.5f;
        [SerializeField] private float strafeWeight = 0.2f;
        [SerializeField] private float flankWeight = 0.15f;
        [SerializeField] private float tauntWeight = 0.15f;
        #endregion

        [Header("The minimum distance required for the enemy to flank their target")]
        [SerializeField] private float minFlankDistance = 3f;

        [Header("The maximum distance required for the enemy to flank their target")]
        [SerializeField] private float maxFlankDistance = 3f;

        [Header("The radius at which the NPC will sample for a flank position")]
        [SerializeField] private float flankSampleRadius = 1f;

        [SerializeField] private bool flankClockwise;

        private int flankDirection;

        private float healthPercentage => parentNPC.CurrentHealthPercentage;

        private bool hasTarget => combatTarget != null;

        private bool targetIsWithinViewRange => Vector2.Distance(combatTarget.transform.position, transform.position) < viewRange;

        private bool targetIsWithinAttackRange => Vector2.Distance(combatTarget.transform.position, transform.position) < parentNPC.WeaponRange;

        private float attackAnimationDuration => parentNPC.AttackAnimationDuration;

        private float attackCooldown => parentNPC.AttackCooldown;

        // Retreat Variables
        [SerializeField] private float retreatDistance;
        [SerializeField] private float retreatSampleRadius;
        [SerializeField] private float retreatPointRefreshRate;

        private void Start()
        {
            if (maxFlankDistance >= viewRange)
            {
                Debug.LogWarning("Max flank distance is greater than view range");
            }

            StartCoroutine(RetrieveAvailableTargets());
            StartCoroutine(MoveToCombatTargetInRange());
            StartCoroutine(RetreatFromCombatTarget());
            StartCoroutine(FlankCombatTarget());
        }

        // Weighted decision logic
        private void DecideNextAction()
        {
            float totalWeight = attackWeight + flankWeight ;
            float randomValue = Random.Range(0f, totalWeight);

            if (randomValue <= attackWeight)
            {
                state = CombatState.EngageSolo;
            }
            else if (randomValue <= attackWeight + strafeWeight + flankWeight)
            {
                // Set flank direction to either 1 or -1
                flankDirection = Random.Range(0, 2) == 0 ? 1 : -1;
                state = CombatState.Flank;
            }
        }


        private IEnumerator RetrieveAvailableTargets()
        {
            while (true)
            {
                yield return new WaitForSeconds(targetRetreivalRate);

                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, viewRange, characterLayer);

                foreach (Collider2D collider in colliders)
                {
                    if (collider.TryGetComponent(out Character character))
                    {
                        if (character == parentNPC)
                        {
                            continue;
                        }

                        // Skip allied npcs
                        if (character.IsInFaction(parentNPC))
                        {
                            continue;
                        }

                        //Debug.Log("Found target: " + character.name);
                        combatTarget = character;
                        break;
                    }
                }
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (combatTarget != null)
            {
                Debug.DrawLine(transform.position, combatTarget.transform.position, Color.red);
            }
            else
            {
                state = CombatState.Idle;
            }

            // Check conditions to change states
            switch (state)
            {
                case CombatState.Idle:
                    HandleIdleState();
                    break;
                case CombatState.EngageSolo:
                    HandleEngageSoloState();
                    break;
                //case CombatState.EngageGroup:
                //    HandleEngageGroupState();
                //    break;
                case CombatState.Retreat:
                    break;
                    //case CombatState.Regroup:
                    //    HandleRegroupState();
                    //    break;
                case CombatState.Flank:
                    break;
            }
        }

        void HandleIdleState()
        {
            // Check if the player is within awareness range
            if (hasTarget && targetIsWithinViewRange)
            {
                // If health is low, consider retreating, otherwise engage
                if (healthPercentage <= retreatPercentageThreshold)
                {
                    state = CombatState.Retreat;
                }
                else
                {
                    state = CombatState.Flank;
                    flankClockwise = Random.Range(0, 2) == 0 ? true : false;
                }
            }
        }

        void HandleEngageSoloState()
        {
            // If the player is within attacrange, attack
            if (hasTarget && targetIsWithinAttackRange)
            {
            }
            else
            {
                // Move towards the player using NavMeshAgent
            }

            //// Check for allies nearby to switch to EngageGroup
            //if (allies.Count > 1 && Vector3.Distance(player.position, transform.position) < groupCombatRange)
            //{
            //    state = CombatState.EngageGroup;
            //}

            if (healthPercentage <= retreatPercentageThreshold)
            {
                state = CombatState.Retreat;
            }
        }


        public bool InAttackingState()
        {
            if (hasTarget && (state == CombatState.EngageSolo || state == CombatState.EngageGroup))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Character GetCombatTarget()
        {
            return combatTarget;
        }

        #region Constant Coroutines

        protected virtual IEnumerator MoveToCombatTargetInRange()
        {
            while (true)
            {
                if (InAttackingState())
                {
                    if (targetIsWithinViewRange)
                    {
                        mover.SetTarget(combatTarget.transform);
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        protected virtual IEnumerator RetreatFromCombatTarget()
        {
            while(true)
            {
                if (state == CombatState.Retreat)
                {
                    if (!NavMesh.SamplePosition(transform.position + (transform.position - combatTarget.transform.position).normalized * retreatDistance, out NavMeshHit hit, retreatSampleRadius, navMeshAreaMask))
                    {
                        Debug.Log("Failed to find a retreat point");
                        state = CombatState.EngageSolo;
                    }

                    mover.SetTarget(hit.position);
                }

                yield return new WaitForSeconds(retreatPointRefreshRate);
            }          
        }

        protected virtual IEnumerator FlankCombatTarget()
        {
            while (true)
            {
                // If the 
                if (state == CombatState.Flank)
                {
                    //  If already flanking, check if the required distance is met
                    float distanceToTarget = Vector2.Distance(transform.position, combatTarget.transform.position);

                    //if (distanceToTarget > maxFlankDistance || distanceToTarget < minFlankDistance)
                    //{
                    //    state = CombatState.EngageSolo;
                    //}

                    // The flank distance should lerp between 0 and the max flank distance based on the distance to the player
                    float flankDistance;

                    if (distanceToTarget <= minFlankDistance || distanceToTarget >= maxFlankDistance)
                    {
                        state = CombatState.EngageSolo;
                        continue;
                    }
                    else
                    {
                        if (distanceToTarget < maxFlankDistance / 2)
                        {
                            // Interpolate flank distance between min and max based on proximity
                            flankDistance = Mathf.Lerp(0f, maxFlankDistance, (distanceToTarget - minFlankDistance) / (maxFlankDistance - minFlankDistance));
                        }
                        else
                        {
                            // Interpolate flank distance between max and min based on proximity
                            flankDistance = Mathf.Lerp(maxFlankDistance, 0f, (distanceToTarget - minFlankDistance) / (maxFlankDistance - minFlankDistance));
                        }
                    }

                    // Get the direction from NPC to the player in 2D (normalized)
                    Vector2 directionToPlayer = (combatTarget.transform.position - transform.position).normalized;

                    Vector2 flankDirection = Vector2.zero;

                    if (flankClockwise)
                    {
                        flankDirection = new Vector2(directionToPlayer.y, -directionToPlayer.x).normalized;
                    }
                    else
                    {
                        // Calculate the perpendicular direction for flanking (left or right of the player)
                        flankDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x).normalized;
                    }


                    // Determine the target flank position
                    Vector2 flankPosition = (Vector2)combatTarget.transform.position + flankDirection * flankDistance;

                    if (!NavMesh.SamplePosition(flankPosition, out NavMeshHit hit, flankSampleRadius, navMeshAreaMask))
                    {
                        Debug.Log("Failed to find a flank position");
                        state = CombatState.EngageSolo;
                    }

                    // Move the NPC towards the flank position
                    mover.SetTarget(flankPosition);
                }

                if (state == CombatState.EngageSolo)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, combatTarget.transform.position);

                    if (distanceToTarget > minFlankDistance && distanceToTarget < maxFlankDistance)
                    {
                        state = CombatState.Flank;
                        flankClockwise = Random.Range(0, 2) == 0 ? true : false;
                    }
                }
                

                yield return new WaitForFixedUpdate();
            }
        }

        #endregion
    }
}