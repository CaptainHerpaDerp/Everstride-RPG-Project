using CBTSystem.Enumerations;
using System;
using UnityEngine;

namespace CBTSystem.Elements
{
    [Serializable]                            
    public class ConditionEntry
    {
        [SerializeField] public CBTConditionType ConditionType;
        [SerializeField] public string Operator;
        [SerializeField] public float Value;

        public bool IsBooleanCondition()
        {
            return ConditionType == CBTConditionType.TargetInAttackRange || 
                   ConditionType == CBTConditionType.SelfInAttackRange || 
                   ConditionType == CBTConditionType.CombatTargetAttacking;
        }

        public bool IsPercentageCondition()
        {
            return ConditionType == CBTConditionType.HeavySwingChargeProgress ||
                   ConditionType == CBTConditionType.CheckHealth ||
                   ConditionType == CBTConditionType.CheckStamina ||
                   ConditionType == CBTConditionType.TargetRangeCoverage;
        }
    }
}
