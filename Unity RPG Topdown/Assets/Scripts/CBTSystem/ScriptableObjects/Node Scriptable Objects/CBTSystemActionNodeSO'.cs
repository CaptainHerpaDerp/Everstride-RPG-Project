using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{
    using Enumerations;
    using System.Collections.Generic;

    public class CBTSystemActionNodeSO : CBTSystemNodeSO
    {
        [field: SerializeField] public CBTActionType ActionType { get; set; }

        public void Initialize(string nodeID, List<string> nextNodeIDs, bool isRootNode, CBTActionType actionType)
        {
            base.Initialize(nodeID, nextNodeIDs, isRootNode);    

            ActionType = actionType;
        }

        public ActionPriority GetPriority()
        {
            switch (ActionType)
            {
                // low-risk, 
                case CBTActionType.MoveToStanceRadius:
                case CBTActionType.CombatStance:
                    return ActionPriority.Idle;

                // core offensive moves
                case CBTActionType.MoveToAttackRange:
                case CBTActionType.LightAttack:
                case CBTActionType.StartHeavyAttack:
                case CBTActionType.ReleaseHeavyAttack:
                    return ActionPriority.Combat;

                // reflex / must-interrupt
                case CBTActionType.HoldBlock:
                case CBTActionType.DodgeAttack:
                    return ActionPriority.Emergency;

                default:
                    Debug.LogError($"Unknown action type: {ActionType}");
                    return ActionPriority.Idle;
            }
        }

    }
}