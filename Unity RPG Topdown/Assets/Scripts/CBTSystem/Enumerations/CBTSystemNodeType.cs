namespace CBTSystem.Enumerations
{
    public enum CBTActionType
    {
        MoveToTarget,
        LightAttack,
        HeavyAttack,
        CombatStance,
        HoldBlock,
    }

    public enum CBTConditionType
    {
        CheckDistance,
        CheckHealth,
        CheckStamina,
        CheckCooldown,
        TargetInAttackRange, 
        SelfInAttackRange,
        CombatTargetAttacking,
    }

    public enum LogicalOperator { And, Or };

}
