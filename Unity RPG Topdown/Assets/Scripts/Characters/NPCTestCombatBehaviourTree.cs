using UnityEngine;
using Sirenix.OdinInspector;
using CBTSystem.ScriptableObjects.Nodes;
using CBTSystem.Enumerations;
using System.Collections;
using System.Collections.Generic;
using System;
using CBTSystem.Elements;
using Items;

namespace Characters
{
    public class NPCTestCombatBehaviourTree : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private Mover mover;
        [BoxGroup("Component References"), SerializeField] NPC npc;

        [SerializeField] private Transform combatTarget;

        [SerializeField] private CBTSystemContainerSO CBTConfig;

        // A list of the current condition nodes that are connected to the current node
        private HashSet<CBTSystemConditionNodeSO> currentConditionNodes = new();

        // The current action node that is being executed
        private CBTSystemActionNodeSO _currentActionNode;

        [BoxGroup("Combat Stance"), SerializeField] private float combatStanceMoveSpeed;

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

                currentConditionNodes = GetConnectedConditionNodes(_currentActionNode);

               // Debug.Log($"Current Action Node: {_currentActionNode.NodeID} - {currentConditionNodes.Count} connected condition nodes found.");
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
                if (currentActionNode is CBTSystemActionNodeSO actionNode)
                {
                    ExecuteActionNode(actionNode);
                }

                CheckConditionNodes();

                yield return new WaitForEndOfFrame();
            }
        }

        private void ExecuteActionNode(CBTSystemActionNodeSO actionNode)
        {
            switch (actionNode.ActionType)
            {
                case CBTActionType.MoveToTarget:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    mover.Resume();
                    mover.SetTarget(combatTarget);

                    break;

                case CBTActionType.LightAttack:

                   // Debug.Log("Performing Light Attack");

                    npc.OnUpdateCombatState?.Invoke(NPCState.Attacking);
                    npc.Attack(combatTarget);

                    break;

                case CBTActionType.CombatStance:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Defensive);

                    npc.ViewLockTargetTransform = combatTarget;

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
            }
        }

        private void CheckConditionNodes()
        {
            if (currentConditionNodes == null)
            {
                Debug.LogError("Error: Condition nodes list is null!");
            }

            if (currentConditionNodes.Count == 0)
            {
                //Debug.LogError("Error: No condition nodes connected to the current action node!");
            }

            foreach (var conditionNode in currentConditionNodes)
            {
                if (CheckConditionNode(conditionNode))
                {

                    // Get the next action node from the condition node
                    CBTSystemNodeSO cbtSystemNode = GetNodeByID(conditionNode.GetConnectedNode()) as CBTSystemActionNodeSO;

                    if (cbtSystemNode == null)
                    {
                        Debug.LogWarning($"No connected node found for condition node {conditionNode.NodeID}");
                        return;
                    }

                    if (cbtSystemNode.GetType() == typeof(CBTSystemActionNodeSO))
                    {
                        // Exit out of the current action node if it is currently executing
                        if (!TryExitActionNode())
                        {
                            return;
                        }

                        // Set the new action node
                        currentActionNode = cbtSystemNode as CBTSystemActionNodeSO;

                        // Break out of the loop, as we only want to execute one action node at a time
                        break;
                    }
                    else
                    {
                        Debug.LogError("Error: Next node is not an action node!");
                    }
                }
            }
        }

        /// <summary>
        /// When exiting/switching the currently preformed action, this method will be called to perform any necessary cleanup or state changes
        /// </summary>
        private bool TryExitActionNode()
        {
            switch (currentActionNode.ActionType)
            {
                case CBTActionType.CombatStance:
                    if (combatStanceCoroutine != null)
                    {
                        StopCoroutine(combatStanceCoroutine);
                        combatStanceCoroutine = null;
                    }

                    npc.ViewLockTargetTransform = null;
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
                    return npc.CurrentHealthPercentage; 

                case CBTConditionType.CheckStamina: 
                    float perc = npc.CurrentStaminaPercentage;
                    return perc;

                case CBTConditionType.TargetInAttackRange:
                    targetDist = Vector3.Distance(transform.position, combatTarget.position);

                    // if the NPC does not have a weapon equipped, we cannot check the attack range
                    if (npc.equippedWeapon == null)
                    {
                        return 0;
                    }

                    // The return type for this is technically a bool, so we return either a 1 or a 0
                    return targetDist <= npc.equippedWeapon.weaponRange ? 1 : 0;

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

        [BoxGroup("Defencive Stance"), SerializeField] private float radiusStep = 1, defensiveRadius;

        [BoxGroup("Defencive Stance"), SerializeField] private float maxAngleStep = 1, minAngleStep = 1;

        [BoxGroup("Defencive Stance"), SerializeField] private float waitDuration = 1;

        private Coroutine combatStanceCoroutine = null;

        private IEnumerator DoCombatStance()
        {
            npc.ViewLockTargetTransform = combatTarget;

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
        private HashSet<CBTSystemConditionNodeSO> GetConnectedConditionNodes(CBTSystemNodeSO node)
        {
            // Get the list of connected nodes
            List<CBTSystemNodeSO> connectedNodes = GetConnectedNodes(node.NextNodeIDs);

            //Debug.Log($"Retrieved {connectedNodes.Count} Nodes");

            // Create the return list of all the condition nodes 
            HashSet<CBTSystemConditionNodeSO> conditionNodes = new();

            // Go through the list of the connected nodes
            foreach (CBTSystemNodeSO connectedNode in connectedNodes)
            {
                // Check if the connected node is a condition node
                if (connectedNode.GetType() == typeof(CBTSystemConditionNodeSO))
                {
                    // Add the condition nodes to the return list 
                    conditionNodes.Add(connectedNode as CBTSystemConditionNodeSO);
                }
            }

            return conditionNodes;
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

