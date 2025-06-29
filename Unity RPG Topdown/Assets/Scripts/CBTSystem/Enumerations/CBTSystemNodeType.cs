namespace CBTSystem.Enumerations
{
    public enum CBTActionType
    {
        MoveToStanceRadius,
        CombatStance,
        HoldBlock,
        LightAttack,
        StartHeavyAttack,
        ReleaseHeavyAttack,
        MoveToAttackRange,
        DodgeAttack,
        Retreat
    }

    public enum CBTConditionType
    {
        CheckDistance,
        CheckHealth,
        CheckStamina,
        TargetRangeCoverage, // The percentage of the remaining attack distance to the target (0% when out of the attack range, 100% when closest to npc)
        TargetInAttackRange, 
        SelfInAttackRange,
        CombatTargetAttacking,
        HeavySwingChargeProgress,// Percentage of charge progress for a heavy attack
        HeavySwingCurrentStaminaCost, // The cost of the current heavy attack swing relative to the max stamina of the caracter
    }

    public enum LogicalOperator { And, Or };

    public enum ActionPriority
    {
        Idle,
        Combat,
        Emergency, // Emergency actions like blocking or dodging
    }

}
