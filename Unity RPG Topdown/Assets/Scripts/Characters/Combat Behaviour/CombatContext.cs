using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Behaviour
{
    public class CombatContext
    {
        // Self
        public float CurrentStamina;
        public float MaxStamina;
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentStaminaPercentage;
        public float CurrentHealthPercentage;
        public bool IsBlocking;
        public float StaminaRegenBlockedUntil;
        public float ExhaustionUntil;
        public float TimeSinceLastHit;

        // Target
        public Transform TargetTransform;
        public float DistanceToTarget;
        public bool SeenIncomingAttack;

        // Tunables
        public float LightAttackRange;
        public float HeavyAttackRange;
        public float StaminaRetreatThreshold;
        public float HealthRetreatThreshold;
        public float SafeRadius;
        public float CombatStanceRadius;

        // …add more as needed…

        /// <summary>
        /// Constructor that pulls everything from the NPC
        /// </summary>
        /// <param name="mgr"></param>
        public CombatContext(NPC mgr)
        {
            // Self?status
            CurrentStamina = mgr.CurrentStamina;
            MaxStamina = mgr.MaxStamina;
            CurrentHealth = mgr.CurrentHealth;
            MaxHealth = mgr.MaxHealth;
            CurrentStaminaPercentage = mgr.CurrentStaminaPercentage;
            CurrentHealthPercentage = mgr.CurrentHealthPercentage;

            IsBlocking = mgr.IsBlocking();

            // Tunables
            LightAttackRange = mgr.LightAttackRange();
            HeavyAttackRange = mgr.HeavyAttackRange();
            StaminaRetreatThreshold = mgr.staminaRetreatThreshhold;
            HealthRetreatThreshold = mgr.healthRetreatThreshhold;
            SafeRadius = mgr.safeRadius;
            CombatStanceRadius = mgr.combatStanceRadius;

            StaminaRegenBlockedUntil = mgr.StaminaRegenBlockedUntil(); // When sucessfully blocking an attack, stamina regeneration is blocked for a while. This value shows how long until it can regenerate stamina again.
            ExhaustionUntil = mgr.EhaustionUntil(); // If stamina reaches 0, it will be exhausted for a while. This value shows how much longer until it can regenerate stamina again.
            TimeSinceLastHit = Time.time - mgr.LastHitTime();

            // Target info
            TargetTransform = mgr.CombatTarget.transform;
            DistanceToTarget = Vector3.Distance(mgr.transform.position,
                                                  TargetTransform.position);

            SeenIncomingAttack = mgr.SeenIncomingAttackFlag();
        }
    }

}
