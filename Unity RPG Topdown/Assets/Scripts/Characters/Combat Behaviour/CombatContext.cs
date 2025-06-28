using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Behaviour
{
    public class CombatContext
    {
        // Self
        public float CurrentStaminaValue;
        public float CurrentStaminaPercentage;
        public float CurrentHealthPercentage;
        public float IsBlocking;
        public float StaminaRegenBlockedUntil;
        public float ExhaustionUntil;
        public float TimeSinceLastHit;

        // Target
        public Transform TargetTransform;
        public float DistanceToTarget;
        public float SeenIncomingAttack;
        public float TargetStaminaPercentage;
        public float TargetHealthPercentage;
        public float TargetBlocking;
        public float TargetAttackRange;
        public float TargetHeavyAttackChargePercentage; // Percentage of charge for heavy attack, if applicable

        // Weapons
        public float LightAttackRange;
        public float HeavyAttackRange;
        public float IdealLightAttackRange;
        public float LightAttackStaminaCost;
        public float HeavyAttackChargePercentage;
        public float RemainingStaminaAtChargePercentage;

        // …add more as needed…

        /// <summary>
        /// Constructor that pulls everything from the NPC
        /// </summary>
        /// <param name="mgr"></param>
        public CombatContext(NPC mgr)
        {
            // Self?status
            CurrentStaminaValue = mgr.CurrentStamina;
            CurrentStaminaPercentage = mgr.CurrentStaminaPercentage;
            CurrentHealthPercentage = mgr.CurrentHealthPercentage;

            IsBlocking = mgr.IsBlocking() ? 1 : 0;

            // Tunables
            LightAttackRange = mgr.LightAttackRange();
            HeavyAttackRange = mgr.HeavyAttackRange();
            LightAttackStaminaCost = mgr.equippedWeapon.lightAttackStaminaCost;

            HeavyAttackChargePercentage = mgr.GetHeavyAttackHoldPercentage();


            IdealLightAttackRange = LightAttackRange * 0.8f;

            StaminaRegenBlockedUntil = mgr.StaminaRegenBlockedUntil(); // When sucessfully blocking an attack, stamina regeneration is blocked for a while. This value shows how long until it can regenerate stamina again.
            ExhaustionUntil = mgr.EhaustionUntil(); // If stamina reaches 0, it will be exhausted for a while. This value shows how much longer until it can regenerate stamina again.
            TimeSinceLastHit = Time.time - mgr.LastHitTime();

            // Target info
            TargetTransform = mgr.CombatTarget.transform;
            DistanceToTarget = Vector3.Distance(mgr.transform.position,
                                                  TargetTransform.position);
            TargetStaminaPercentage = mgr.CombatTarget.CurrentStaminaPercentage;
            TargetHealthPercentage = mgr.CombatTarget.CurrentHealthPercentage;

            TargetHeavyAttackChargePercentage = mgr.CombatTarget.GetHeavyAttackHoldPercentage();
            TargetBlocking = mgr.CombatTarget.IsBlocking() ? 1 : 0;

            TargetAttackRange = mgr.CombatTarget.WeaponRange;

            SeenIncomingAttack = mgr.SeenIncomingAttackFlag() ? 1 : 0;

        }

        public float ValueLow(float pct, float min, float max)
        {
            return Mathf.Clamp01(1 - Mathf.InverseLerp(min, max, pct));
        }

        public float ValueHigh(float pct, float min, float max)
        {
            return Mathf.InverseLerp(min, max, pct);
        }

    }

}
