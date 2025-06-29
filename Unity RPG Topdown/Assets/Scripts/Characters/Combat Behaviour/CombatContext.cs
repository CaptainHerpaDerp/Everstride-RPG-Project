using Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Behaviour
{
    public class CombatContext
    {
        #region Self State
        public float CurrentStaminaValue { get; }
        public float CurrentStaminaPercentage { get; }
        public float CurrentHealthPercentage { get; }
        public float MaxStamina { get; }
        public float MaxHealth { get; }
        public float IsBlocking { get; }
        public float StaminaRegenBlockedUntil { get; }
        public float ExhaustionUntil { get; }
        public float TimeSinceLastHit { get; }

        #endregion

        #region Target State

        public Transform TargetTransform { get; }
        public float DistanceToTarget { get; }
        public float HasSeenIncomingAttack { get; }
        public float TargetStaminaPercentage { get; }
        public float TargetHealthPercentage { get; }
        public float TargetBlocking { get; }
        public float TargetAttackRange { get; }
        public float TargetHeavyAttackChargePercentage { get; }
        public float TargetLightAttackDamage { get; }
        public float TargetMinHeavyAttackDamage { get; }
        public float TargetMaxHeavyAttackDamage { get; }
        public float TargetCurrentHeavyAttackStaminaDrain { get; }
        public float IncomingDamage { get; }

        #endregion

        #region Weapon Stats

        public float LightAttackRange { get; }
        public float HeavyAttackRange { get; }
        public float IdealLightAttackRange { get; }
        public float LightAttackStaminaCost { get; }
        public float HeavyAttackChargePercentage { get; }
        public float RemainingStaminaAtChargePercentage { get; }
        public float StaminaPercentageDrainPerBlock { get; }

        #endregion

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
            TargetCurrentHeavyAttackStaminaDrain = mgr.GetHeavyBlockStaminaDrawMultiplier(mgr.CombatTarget.chargeHoldTime);

            TargetAttackRange = mgr.CombatTarget.WeaponRange;

            HasSeenIncomingAttack = mgr.SeenIncomingAttackFlag() ? 1 : 0;

            IncomingDamage = GetEnemyIncomingDamage(mgr);

            StaminaPercentageDrainPerBlock = mgr.staminaPercDrainedPerBlock;
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
