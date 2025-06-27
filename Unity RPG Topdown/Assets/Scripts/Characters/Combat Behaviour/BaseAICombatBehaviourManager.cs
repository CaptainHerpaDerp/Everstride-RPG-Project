using CBTSystem.Elements;
using CBTSystem.Enumerations;
using CBTSystem.ScriptableObjects.Nodes;
using Items;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Enums;

namespace Characters.Behaviour
{
    /// <summary>
    /// Manages the AI combat behaviour for a character using a Behaviour Tree system (CBT). Does NOT include any utility behaviour nodes, only action and condition nodes.
    /// </summary>
    public class BaseAICombatBehaviourManager : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private Mover mover;
        [BoxGroup("Component References"), SerializeField] NPC npc;
        [BoxGroup("Component References"), SerializeField] private CBTSystemContainerSO CBTConfig;

        [BoxGroup("Combat Stance"), SerializeField] private float combatStanceMoveSpeed;
        [BoxGroup("Combat Stance"), SerializeField] private float radiusStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveRadius = 1.5f;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveSafeRadius = 2.5f;
        [BoxGroup("Combat Stance"), SerializeField] private float maxAngleStep = 1, minAngleStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float waitDuration = 1;


        // A list of the current condition nodes that are connected to the current node
        private List<CBTSystemConditionNodeSO> curConnectedConditionNodes = new();
        private List<CBTSystemActionNodeSO> curConnectedActionNodes = new();

        private Transform combatTarget => npc.CombatTarget.transform;

        // The current action node that is being executed
        private CBTSystemActionNodeSO _currentActionNode;

        private Coroutine combatStanceCoroutine = null;

        public static Action<string> OnNodeChanged;

        // When we set the new action node, automatically get the new list of connected condition nodes
        private CBTSystemActionNodeSO currentActionNode
        {
            get { return _currentActionNode; }

            set
            {
                if (value == null)
                {
                    Debug.LogError("Error: Action node is null! Cannot set current action node to null.");
                    return;
                }

                _currentActionNode = value;

                OnNodeChanged?.Invoke(_currentActionNode.NodeID);

                // Assigns the condition node list, and sorts the nodes by their priority
                curConnectedConditionNodes = GetConnectedConditionNodes(_currentActionNode);
                curConnectedActionNodes = GetConnectedActionNodes(_currentActionNode);

                // Debug.Log($"Current Action Node: {_currentActionNode.NodeID} - {curConnectedConditionNodes.Count} connected condition nodes found.");
            }
        }

        private void Start()
        {
            if (CBTConfig != null)
            {
                CBTSystemNodeSO rootNode = GetRootNode();

                if (rootNode != null)
                {
                    // Debug.Log($"Root Node Type: {rootNode.GetType()}");

                    StartCoroutine(CheckForCondition(rootNode as CBTSystemActionNodeSO));
                }
                else
                {
                    Debug.LogError("No root node found in the CBT configuration.");
                }
            }
        }

        private IEnumerator CheckForCondition(CBTSystemActionNodeSO startingActionNode)
        {
            currentActionNode = startingActionNode;

            while (true)
            {
                if (npc.CombatTarget == null)
                {
                    yield return new WaitForEndOfFrame();

                    continue;
                }

                if (currentActionNode is CBTSystemActionNodeSO actionNode)
                {
                    ExecuteActionNode(actionNode);
                }

                yield return new WaitForEndOfFrame();

                // Try to check if any condition nodes are met, and change the current action node if so
                if (TryCheckConditionNodes()) ;
                if (TryEnterActionNodes()) ;
            }
        }

        private void ExecuteActionNode(CBTSystemActionNodeSO actionNode)
        {
            if (combatTarget == null)
                return;

            if (npc.State == CharacterState.Death)
                return;

            switch (actionNode.ActionType)
            {
                case CBTActionType.MoveToStanceRadius:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    mover.Resume();
                    mover.SetTarget(combatTarget.transform, defensiveRadius);
                    break;

                case CBTActionType.MoveToAttackRange:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    mover.Resume();
                    mover.SetTarget(combatTarget.transform);

                    break;

                case CBTActionType.CombatStance:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Defensive);

                    npc.DoTargetViewLock = true;

                    if (combatStanceCoroutine == null)
                    {
                        combatStanceCoroutine = StartCoroutine(DoCombatStance());
                    }

                    break;

                // The NPC adopts a block stance and holds it until the action is exited
                case CBTActionType.HoldBlock:

                    npc.OnUpdateCombatState?.Invoke(NPCState.TargetBlocking);

                    // Set the NPC to block stance
                    npc.EnterBlockState(combatTarget.transform);
                    // Stop the mover
                    mover.Stop();
                    break;

                case CBTActionType.LightAttack:

                    // Debug.Log("Performing Light Attack");

                    npc.OnUpdateCombatState?.Invoke(NPCState.Attacking);
                    npc.LightAttack(combatTarget);

                    break;

                case CBTActionType.StartHeavyAttack:
                    // Start the heavy attack
                    npc.OnUpdateCombatState?.Invoke(NPCState.Attacking);
                    npc.StartHeavyAttack(combatTarget);
                    break;

                case CBTActionType.ReleaseHeavyAttack:
                    // End the heavy attack
                    npc.EndHeavyAttack();
                    break;

            }
        }

        /// <summary>
        /// If there are existing condition nodes connected to the current action node, this method will check if any of the conditions are met
        /// </summary>
        /// <returns>Returns true if a condition node was successfully found and connected, and false if there were no successfull connections with condition nodes</returns>
        private bool TryCheckConditionNodes()
        {
            // Simply retrieve the current condition nodes from the current action node
            if (curConnectedConditionNodes == null || curConnectedConditionNodes.Count == 0)
            {
                return false;
            }

            // Since all of our condition nodes are sorted by priority, we can simply iterate through them and check if any of the conditions are met

            foreach (var conditionNode in curConnectedConditionNodes)
            {
                if (CheckConditionNode(conditionNode))
                {
                    // Get the next action node from the condition node
                    CBTSystemNodeSO cbtSystemNode = GetNodeByID(conditionNode.GetConnectedNode()) as CBTSystemActionNodeSO;

                    if (cbtSystemNode == null)
                    {
                        Debug.LogWarning($"No connected node found for condition node {conditionNode.NodeID}");
                        return false;
                    }

                    if (cbtSystemNode.GetType() == typeof(CBTSystemActionNodeSO))
                    {
                        // Exit out of the current action node if it is currently executing
                        if (!TryExitCurrentActionNode())
                        {
                            return false;
                        }

                        // Set the new action node
                        currentActionNode = cbtSystemNode as CBTSystemActionNodeSO;

                        return true;

                        // Break out of the loop, as we only want to execute one action node at a time
                        break;
                    }
                    else
                    {
                        Debug.LogError("Error: Next node is not an action node!");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// If there is an existing action node connected to the current action node, this method will set it as the current action node and return true
        /// </summary>
        /// <returns></returns>
        private bool TryEnterActionNodes()
        {
            if (curConnectedActionNodes == null || curConnectedActionNodes.Count == 0)
            {
                return false;
            }

            if (curConnectedActionNodes.Count > 1)
            {
                // If there are multiple action nodes, we can only execute the first one (this should never happen)
                Debug.LogWarning("Multiple action nodes found, executing the first one.");
            }

            CBTSystemActionNodeSO newActionNode = curConnectedActionNodes[0];

            if (newActionNode.GetType() == typeof(CBTSystemActionNodeSO))
            {
                // Exit out of the current action node if it is currently executing
                if (!TryExitCurrentActionNode())
                {
                    return false;
                }

                // Set the new action node
                currentActionNode = newActionNode;

                return true;
            }
            else
            {
                Debug.LogError("Error: Next node is not an action node!");
            }

            return false;
        }

        /// <summary>
        /// When exiting/switching the currently preformed action, this method will be called to perform any necessary cleanup or state changes
        /// </summary>
        private bool TryExitCurrentActionNode()
        {
            switch (currentActionNode.ActionType)
            {
                case CBTActionType.CombatStance:
                    if (combatStanceCoroutine != null)
                    {
                        StopCoroutine(combatStanceCoroutine);
                        combatStanceCoroutine = null;
                    }

                    npc.DoTargetViewLock = false;
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);
                    mover.Stop();

                    return true;


                case CBTActionType.HoldBlock:

                    // If the NPC is still in the required block state time, we can't exit the block state yet
                    if (!npc.CanExitBlockState())
                        return false;

                    // Exit the block state
                    npc.ExitBlockState();
                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    mover.Resume();

                    return true;

                case CBTActionType.LightAttack:

                    // Do not exit the state if the character is still attacking
                    if (npc.State == CharacterState.Attacking)
                    {
                        return false;
                    }

                    return true;

                case CBTActionType.ReleaseHeavyAttack:

                    // Do not exit the state if the character is still attacking
                    if (npc.State == CharacterState.Attacking)
                    {
                        return false;
                    }

                    return true;

                // The npc can only release the heavy attack if the charge threshold has been reached
                case CBTActionType.StartHeavyAttack:

                    return npc.MinChargeTimeMet();

                default:
                    return true; // For all other action types, we can exit without any special handling
            }
        }

        /// <summary>
        /// Check if the condition node is met by comparing the node's condition value to its threshold
        /// </summary>
        /// <param name="conditionNode"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        bool CheckConditionNode(CBTSystemConditionNodeSO conditionNode)
        {
            if (conditionNode.ConditionEntries == null)
            {
                Debug.LogError($"Condition node {conditionNode.NodeID} is null!");
                return false;
            }

            if (conditionNode.ConditionEntries.Count == 0)
            {
                Debug.LogError($"Condition node {conditionNode.NodeID} has no condition entries!");
                return false;
            }

            if (EvaluateAllConditions(conditionNode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool EvaluateAllConditions(CBTSystemConditionNodeSO conditionNode)
        {
            int cacheIndex = 0;
            List<List<bool>> conditionEntries = new();

            if (conditionNode.ConditionEntries == null)
            {
                Debug.LogError($"Condition node {conditionNode.NodeID} is null!");
                return false;
            }

            if (conditionNode.ConditionEntries.Count == 0)
            {
                return true;
            }

            // Firstly, check the condition of the first entry in the condition node
            bool firstResult = CheckConditionEntry(conditionNode.ConditionEntries[0]);

            conditionEntries.Add(new List<bool> { });

            conditionEntries[cacheIndex].Add(firstResult);

            for (int i = 1; i < conditionNode.ConditionEntries.Count; i++)
            {
                bool next = CheckConditionEntry(conditionNode.ConditionEntries[i]);

                // First we need to find out if the next logical operator is an And, if so, we connect the previous result to the next condition check.

                // If the next condition is an "Or", we close the group of condition(s). connected by "And" operators
                var op = conditionNode.Connectors[i - 1];

                // If the operator is Or, we need to close the current group of conditions and start a new one
                if (op == LogicalOperator.Or)
                {
                    // increase the index
                    cacheIndex++;

                    // add the newly checked condition entry to the new group
                    conditionEntries.Add(new List<bool> { next });
                }

                // If the operator is And, simply add the condition entry to the current group
                else if (op == LogicalOperator.And)
                {
                    conditionEntries[cacheIndex].Add(next);
                }
            }

            // Now we need to check if all the conditions in each group are true

            // To do this, we need to iterate through each group
            foreach (var conditionGroup in conditionEntries)
            {
                // If any of the conditions in the group are false, we return false
                if (!conditionGroup.Contains(false))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckConditionEntry(ConditionEntry conditionEntry)
        {
            string conditionOperator = conditionEntry.Operator;

            // The threshold value for the condition
            float conditionValue = (int)conditionEntry.Value;

            // The actual value of the condition based on the condition type
            float conditionTrueValue = GetConditionValue(conditionEntry.ConditionType);

            // If the condition type is a bool, we simply need to check if the true value (0 or 1) matches the condition value (which should also be either 0 or 1
            if (conditionEntry.IsBooleanCondition())
            {
                if (conditionTrueValue != 0 && conditionTrueValue != 1)
                {
                    Debug.LogError("Error! The condition's true value is not 0 or 1!");
                }

                if (conditionValue != 0 && conditionValue != 1)
                {
                    Debug.LogError("Error! The condition's value is not 0 or 1!");
                }

                return conditionTrueValue == conditionValue;
            }

            switch (conditionOperator)
            {
                case "<": return conditionTrueValue < conditionValue;
                case "<=": return conditionTrueValue <= conditionValue;
                case ">": return conditionTrueValue > conditionValue;
                case ">=": return conditionTrueValue >= conditionValue;
                case "=": return Mathf.Approximately(conditionTrueValue, conditionValue);
                default: throw new InvalidOperationException($"Invalid op “{conditionOperator}”");
            }
        }

        #region Condition Checking

        //private bool EvaluateAll(CombatContext ctx)
        //{
        //    if (ConditionEntries.Count == 0)
        //        return true;

        //    bool result = CheckSingle(ConditionEntries[0], ctx);
        //    for (int i = 1; i < ConditionEntries.Count; i++)
        //    {
        //        bool next = CheckSingle(ConditionEntries[i], ctx);
        //        var op = Connectors[i - 1];
        //        result = (op == LogicalOp.And) ? (result && next) : (result || next);
        //    }
        //    return result;
        //}

        #endregion

        /// <summary>
        /// Returns the actual value of a given condition
        /// </summary>
        /// <param name="conditionType"></param>
        /// <returns></returns>
        private float GetConditionValue(CBTConditionType conditionType)
        {
            Character combatTargetCharacter;
            float targetDist;

            switch (conditionType)
            {
                case CBTConditionType.CheckDistance:
                    return Vector3.Distance(transform.position, combatTarget.position);

                case CBTConditionType.CheckHealth:
                    return npc.CurrentHealthPercentage * 100;

                case CBTConditionType.CheckStamina:
                    return npc.CurrentStaminaPercentage * 100;

                case CBTConditionType.TargetInAttackRange:
                    targetDist = Vector3.Distance(transform.position, combatTarget.position);

                    // if the NPC does not have a weapon equipped, we cannot check the attack range
                    if (npc.equippedWeapon == null)
                    {
                        return 0;
                    }

                    // The return type for this is technically a bool, so we return either a 1 or a 0
                    return targetDist <= npc.equippedWeapon.weaponRange ? 1 : 0;

                case CBTConditionType.TargetRangeCoverage:
                    targetDist = Vector3.Distance(transform.position, combatTarget.position);

                    // if the NPC does not have a weapon equipped, we cannot check the attack range
                    if (npc.equippedWeapon == null)
                    {
                        return 0;
                    }

                    float maxDistance = npc.equippedWeapon.weaponRange;

                    float remainingAttackDist = Mathf.Clamp01((maxDistance - targetDist) / maxDistance);

                    //  Debug.Log($"Remainding: {remainingAttackDist}");

                    return remainingAttackDist * 100; // Return as a percentage (0-100)

                case CBTConditionType.CombatTargetAttacking:

                    if (!combatTarget.TryGetComponent(out combatTargetCharacter))
                    {
                        Debug.LogError("Combat target does not have a Character component!");
                        return 0;
                    }

                    return combatTargetCharacter.IsAttacking() ? 1 : 0;

                case CBTConditionType.SelfInAttackRange:

                    if (!combatTarget.TryGetComponent(out combatTargetCharacter))
                    {
                        Debug.LogError("Combat target does not have a Character component!");
                        return 0;
                    }

                    targetDist = Vector3.Distance(transform.position, combatTarget.position);

                    WeaponItem enemyWeapon = combatTargetCharacter.equippedWeapon as WeaponItem;

                    // If the enemy does not have a weapon equipped, we cannot check the attack range
                    if (enemyWeapon == null)
                    {
                        return 0;
                    }

                    return targetDist <= enemyWeapon.weaponRange ? 1 : 0;

                case CBTConditionType.HeavySwingChargeProgress:

                    if (npc.State != Core.Enums.CharacterState.Attacking)
                    {
                        Debug.LogWarning("NPC is not in attacking state, cannot check heavy swing charge progress.");
                        return 0;
                    }

                    //  Debug.Log($"Heavy attack hold percentage: {npc.GetHeavyAttackHoldPercentage()}");

                    return npc.GetHeavyAttackHoldPercentage() * 100;


                default:
                    Debug.LogWarning($"Condition type {conditionType} not implemented.");
                    return 0;
            }
        }

        #region Action Callback Methods

        private void MoveToTarget(Action callBack)
        {
            mover.SetTarget(combatTarget.position);


        }

        #endregion

        #region Action Methods

        private IEnumerator DoCombatStance()
        {
            npc.DoTargetViewLock = true;

            npc.SetMovementSpeed(combatStanceMoveSpeed);

            while (true)
            {
                // This will give a normalized vector
                Vector2 difference = (transform.position - combatTarget.position);

                float angleDifference = Mathf.Atan2(difference.y, difference.x);

                float angleStep = UnityEngine.Random.Range(minAngleStep, maxAngleStep);

                float angle = angleDifference + (angleStep * UnityEngine.Random.Range(-1, 1));

                float distance = defensiveRadius + UnityEngine.Random.Range(-radiusStep, radiusStep);

                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;

                Vector3 targetPosition = combatTarget.position + new Vector3(offset.x, offset.y, 0);

                mover.SetTarget(targetPosition);

                // Wait for the mover to reach target
                yield return new WaitUntil(() => mover.AgentArrived());

                yield return new WaitForSeconds(waitDuration);

            }
        }

        #endregion

        #region Node Retrieval Methods

        /// <summary>
        /// Get the first node in the CBT graph that is marked as a root node (empty input port)
        /// </summary>
        /// <returns></returns>
        private CBTSystemNodeSO GetRootNode()
        {
            foreach (var node in CBTConfig.UngroupedNodes)
            {
                if (node.IsRootNode)
                {
                    if (node.GetType() == typeof(CBTSystemActionNodeSO))
                    {
                        return node;
                    }
                    else
                    {
                        Debug.LogError("Error in root node: Root node is not an action node!");

                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Grab all connected nodes from the given node
        /// </summary>
        /// <param name="node"></param>
        private List<CBTSystemConditionNodeSO> GetConnectedConditionNodes(CBTSystemNodeSO node)
        {
            // 1) Gather all connected condition nodes as before
            List<CBTSystemNodeSO> connectedNodes = GetConnectedNodes(node.NextNodeIDs);
            var foundConditionNodes = connectedNodes
                .OfType<CBTSystemConditionNodeSO>()
                .ToList();

            // 2) Simply sort by Priority (ascending: lower number = higher priority)
            foundConditionNodes.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            // 3) Return the sorted list
            return foundConditionNodes;
        }

        private List<CBTSystemActionNodeSO> GetConnectedActionNodes(CBTSystemNodeSO node)
        {
            //Gather all connected action nodes
            List<CBTSystemNodeSO> connectedNodes = GetConnectedNodes(node.NextNodeIDs);
            var foundActionNodes = connectedNodes
                .OfType<CBTSystemActionNodeSO>()
                .ToList();

            return foundActionNodes;
        }

        private List<CBTSystemUtilitySelectorNodeSO> GetConnectedUtilityNodes(CBTSystemNodeSO node)
        {
            // Gather all connected utility selector nodes
            List<CBTSystemNodeSO> connectedNodes = GetConnectedNodes(node.NextNodeIDs);
            var foundUtilityNodes = connectedNodes
                .OfType<CBTSystemUtilitySelectorNodeSO>()
                .ToList();
            return foundUtilityNodes;
        }

        private List<CBTSystemNodeSO> GetConnectedNodes(List<string> nextNodeIDs)
        {
            // Create the return list
            List<CBTSystemNodeSO> returnList = new();

            if (nextNodeIDs == null)
            {
                Debug.LogError("Error: Next node ID's are null!");
            }

            // Go through each of the connected node ID's
            foreach (var nextNodeID in nextNodeIDs)
            {
                // Go through each of the ungrouped nodes in the entire graph
                foreach (var node in CBTConfig.UngroupedNodes)
                {
                    // If the id of the next node is the same as the id of the node in the graph, add it to the return list
                    if (nextNodeID == node.NodeID)
                    {
                        returnList.Add(node);
                    }
                }
            }

            // Go through each of the connected node ID's
            foreach (var nextNodeID in nextNodeIDs)
            {
                // Go through each of the groups 
                foreach (var group in CBTConfig.Groups)
                {
                    // Go through each node in the group
                    foreach (var node in group.Value)
                    {
                        // If the id of the next node is the same as the id of the node in the graph, add it to the return list
                        if (nextNodeID == node.NodeID)
                        {
                            returnList.Add(node);
                        }
                    }
                }
            }

            return returnList;
        }

        private CBTSystemNodeSO GetNodeByID(string nodeID)
        {
            // Go through each of the ungrouped nodes in the entire graph
            foreach (var node in CBTConfig.UngroupedNodes)
            {
                // If the id of the next node is the same as the id of the node in the graph, return it
                if (nodeID == node.NodeID)
                {
                    return node;
                }
            }

            // Go through each of the groups 
            foreach (var group in CBTConfig.Groups)
            {
                // Go through each node in the group
                foreach (var node in group.Value)
                {
                    // If the id of the next node is the same as the id of the node in the graph, return it
                    if (nodeID == node.NodeID)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

