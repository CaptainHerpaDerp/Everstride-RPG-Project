using Items;
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
        public float MaxStamina;
        public float MaxHealth;
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
        public float IncomingDamage;

        public float TargetLightAttackDamage;
        public float TargetMinHeavyAttackDamage;
        public float TargetMaxHeavyAttackDamage;
        public float TargetCurHeavyAttackMultiplierDrain;

        // Weapons
        public float LightAttackRange;
        public float HeavyAttackRange;
        public float IdealLightAttackRange;
        public float LightAttackStaminaCost;
        public float HeavyAttackChargePercentage;
        public float RemainingStaminaAtChargePercentage;
        public float StaminaPercDrainPerBlock;

        // …add more as needed…

        /// <summary>
        /// Constructor that pulls everything from the NPC
        /// </summary>
        /// <param name="mgr"></param>
        public CombatContext(NPC mgr)
        {
            // Self
            CurrentStaminaValue = mgr.CurrentStamina;
            CurrentStaminaPercentage = mgr.CurrentStaminaPercentage;
            CurrentHealthPercentage = mgr.CurrentHealthPercentage;
            MaxStamina = mgr.MaxStamina;
            MaxHealth = mgr.MaxHealth;

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
            DistanceToTarget = Vector3.Distance(mgr.transform.position, TargetTransform.position);
            TargetStaminaPercentage = mgr.CombatTarget.CurrentStaminaPercentage;
            TargetHealthPercentage = mgr.CombatTarget.CurrentHealthPercentage;
            TargetHeavyAttackChargePercentage = mgr.CombatTarget.GetHeavyAttackHoldPercentage();
            TargetBlocking = mgr.CombatTarget.IsBlocking() ? 1 : 0;
            TargetLightAttackDamage = mgr.CombatTarget.equippedWeapon != null ? mgr.CombatTarget.equippedWeapon.weaponDamage : 0;
            TargetMinHeavyAttackDamage = mgr.CombatTarget.equippedWeapon != null ? mgr.CombatTarget.equippedWeapon.weaponDamage * mgr.CombatTarget.GetHeavyAttackDamageMultiplier(mgr.CombatTarget.chargeAttackMinTime) : 0;
            TargetMaxHeavyAttackDamage = mgr.CombatTarget.equippedWeapon != null ? mgr.CombatTarget.equippedWeapon.weaponDamage * mgr.CombatTarget.GetHeavyAttackDamageMultiplier(mgr.CombatTarget.chargeAttackMaxTime) : 0;
            TargetCurHeavyAttackMultiplierDrain = mgr.GetHeavyBlockStaminaDrawMultiplier(mgr.CombatTarget.chargeHoldTime);

            TargetAttackRange = mgr.CombatTarget.WeaponRange;

            SeenIncomingAttack = mgr.SeenIncomingAttackFlag() ? 1 : 0;

            IncomingDamage = GetEnemyIncomingDamage(mgr);

            StaminaPercDrainPerBlock = mgr.staminaPercDrainedPerBlock;
        }

        private float GetEnemyIncomingDamage(NPC mgr)
        {
            if (mgr.equippedWeapon == null)
            {
                return 0;
            }

            // If the target is not attacking, return 0
            if (!mgr.SeenIncomingAttackFlag())
            {
                return 0;
            }

            // If the enemy is charging for a heavy attack
            if (mgr.CombatTarget.GetHeavyAttackHoldPercentage() > 0)
            {
                // Multiply the base weapon damage by the heavy damage modifier
                return mgr.CombatTarget.equippedWeapon.weaponDamage * mgr.CombatTarget.GetHeavyAttackDamageMultiplier(mgr.CombatTarget.chargeHoldTime);
            }

            // Otherwise, they are performing a light attack
            else
            {
                return mgr.equippedWeapon.weaponDamage;
            }
        }

    }

}
