using CBTSystem.Elements;
using CBTSystem.Enumerations;
using CBTSystem.ScriptableObjects.Nodes;
using Codice.Client.BaseCommands;
using Codice.CM.Client.Differences;
using Core.Enums;
using CustomOdinScripts;
using Items;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Characters.Behaviour
{
       /// <summary>
    /// Manages the AI combat behaviour for a character using a Behaviour Tree system (CBT). Includes Utility behaviour and assessment for combat actions.
    /// </summary>
    public class UtilityAICombatBehaviourManager : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private Mover mover;
        [BoxGroup("Component References"), SerializeField] NPC npc;
        [BoxGroup("Component References"), SerializeField] private CBTSystemContainerSO CBTConfig;
        [BoxGroup("Component References"), SerializeField] private UtilityAIWeightsContainer weights;
        

        [BoxGroup("Combat Stance"), SerializeField] private float combatStanceMoveSpeed;

        [BoxGroup("Combat Stance"), SerializeField] private float radiusStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveRadiusMin = 1.5f;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveRadiusMax = 3f;
        [BoxGroup("Combat Stance"), SerializeField] private float maxAngleStep = 1, minAngleStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float waitDuration = 1;

        [SerializeField, Range(0, 1)] float aggression = 0.35f;
        [SerializeField, Range(0, 1)] float riskAversion = 0.25f;

        [Header("This value determines the minimum duration an action can be carried out before another can be considered")]
       // [BoxGroup("Utility Behaviour"), SerializeField] private float actionMinDwellTime = 0.35f;

        [BoxGroup("Scoring Variables, Combat Stance"), SerializeField] private float lowStaminaFactor = 0.8f; // Factor to increase the score when stamina is low

        public float staminaRetreatThreshold { get; protected set; } = 0.10f; // The stamina percentage at which the NPC will retreat from combat
        public float healthRetreatThreshold { get; protected set; } = 0.15f; // The health percentage at which the NPC will retreat from combat

        [ShowInInspector] protected float _currentDefensiveRadius;

        // A list of the current condition nodes that are connected to the current node
        private List<CBTSystemConditionNodeSO> curConnectedConditionNodes = new();
        private List<CBTSystemActionNodeSO> curConnectedActionNodes = new();
        private List<CBTSystemUtilitySelectorNodeSO> curConnectedUtilityNodes = new();

        private float currentActionStartTime;

        private Transform combatTarget => npc.CombatTarget.transform;

        // The current action node that is being executed
        private CBTSystemActionNodeSO _currentActionNode;

        private Coroutine combatStanceCoroutine = null;
        private Coroutine dodgeRountine = null;

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

                currentActionStartTime = Time.time;

                _currentActionNode = value;

                OnNodeChanged?.Invoke(_currentActionNode.NodeID);

                // Assigns the condition node list, and sorts the nodes by their priority
                curConnectedConditionNodes = GetConnectedConditionNodes(_currentActionNode);
                curConnectedActionNodes = GetConnectedActionNodes(_currentActionNode);
                curConnectedUtilityNodes = GetConnectedUtilityNodes(_currentActionNode);

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
                if (TryCheckConditionNodes(curConnectedConditionNodes)) ;
                if (TryEnterActionNodes(currentActionNode)) ;
                TryEnterUtilityNodes(currentActionNode);
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
                    mover.SetTarget(combatTarget.transform, _currentDefensiveRadius);
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    npc.DoTargetViewLock = true;

                    break;

                case CBTActionType.MoveToAttackRange:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    mover.Resume();
                    mover.SetTarget(combatTarget.transform);
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    npc.DoTargetViewLock = true;

                    break;

                case CBTActionType.CombatStance:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Defensive);

                    npc.DoTargetViewLock = true;

                    mover.Resume();

                    if (combatStanceCoroutine == null)
                    {
                        combatStanceCoroutine = StartCoroutine(DoCombatStance());
                    }

                    break;

                case CBTActionType.DodgeAttack:

                    npc.OnUpdateCombatState?.Invoke(NPCState.TargetBlocking);

                    mover.Resume();

                    if (dodgeRountine == null)
                    {
                        Debug.Log("Dodge");
                        dodgeRountine = StartCoroutine(DoDodgeRoutine());
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
                    mover.Stop();

                    break;

                case CBTActionType.StartHeavyAttack:
                    // Start the heavy attack
                    npc.OnUpdateCombatState?.Invoke(NPCState.Attacking);
                    npc.StartHeavyAttack(combatTarget);
                    mover.Stop();

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
        private bool TryCheckConditionNodes(List<CBTSystemConditionNodeSO> conditionNodes)
        {
            // Simply retrieve the current condition nodes from the current action node
            if (conditionNodes == null || conditionNodes.Count == 0)
            {
                return false;
            }

            // Since all of our condition nodes are sorted by priority, we can simply iterate through them and check if any of the conditions are met
            foreach (var conditionNode in conditionNodes)
            {
                // Evaluate the condition node's conditions, if the condition node is met, we can get the next action node
                if (CheckConditionNode(conditionNode))
                {
                    // Get the next node from the condition node (only one is present)
                    CBTSystemNodeSO cbtSystemNode = GetNodeByID(conditionNode.GetConnectedNode());

                    if (cbtSystemNode == null)
                    {
                        Debug.LogWarning($"No connected node found for condition node {conditionNode.NodeID}");
                        return false;
                    }

                    // If the node is an action node, set it as the active node
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

                    }

                    // If the node is a utility selector node, we can evaluate it and get the next action node
                    else if (cbtSystemNode.GetType() == typeof(CBTSystemUtilitySelectorNodeSO))
                    {
                        // Exit out of the current action node if it is currently executing
                        if (!TryExitCurrentActionNode())
                        {
                            return false;
                        }

                        //Debug.Log($"Evaluating {cbtSystemNode.NodeID}");

                        CBTSystemActionNodeSO newActionNode = GetEvaluateUtilitySelectorNode(cbtSystemNode as CBTSystemUtilitySelectorNodeSO);

                        // From the node who's condition is met, try to enter the utility nodes
                        if (newActionNode != null)
                        {
                            currentActionNode = newActionNode;

                            return true;
                        }
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
        private bool TryEnterActionNodes(CBTSystemNodeSO sourceNode)
        {
            // Get the currently connected utility nodes from the source node
            List<CBTSystemActionNodeSO> connectedActionNodes = GetConnectedActionNodes(sourceNode);

            if (connectedActionNodes == null || connectedActionNodes.Count == 0)
            {
                return false;
            }

            if (connectedActionNodes.Count > 1)
            {
                // If there are multiple action nodes, we can only execute the first one (this should never happen)
                Debug.LogWarning("Multiple action nodes found, executing the first one.");
            }

            CBTSystemActionNodeSO newActionNode = connectedActionNodes[0];

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

        private bool TryEnterUtilityNodes(CBTSystemNodeSO sourceNode)
        {
            // Get the currently connected utility nodes from the source node
            List<CBTSystemUtilitySelectorNodeSO> connectedUtilityNodes = GetConnectedUtilityNodes(sourceNode);

            if (connectedUtilityNodes == null || connectedUtilityNodes.Count == 0)
            {
                return false;
            }

            foreach (var utilityNode in connectedUtilityNodes)
            {
                // For each of the currently connected utility nodes, evaluate the utility selector node and get the next action node
                CBTSystemActionNodeSO newActionNode = GetEvaluateUtilitySelectorNode(utilityNode);

                // If the newly found action node isnt null, and we are able to exit the current action node, we can set the new action node
                if (newActionNode != null && TryExitCurrentActionNode())
                {
                    currentActionNode = newActionNode;
                    return true;
                }

                // If we couldn't find an action node, search through any condition nodes that are connected to the utility node
                List<CBTSystemConditionNodeSO> connectedConditionNodes = GetConnectedConditionNodes(utilityNode);

                // If there are any connected condition nodes, check if any of them are met
                if (connectedConditionNodes.Count > 0)
                {
                    // If any of the condition nodes are met, we can set the current action node to the first connected action node
                    if (TryCheckConditionNodes(connectedConditionNodes))
                    {
                        return true;
                    }
                }
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
                case CBTActionType.MoveToStanceRadius:

                    npc.SetMovementSpeed(npc.defaultMovementSpeed);
                    mover.Stop();

                    return true;

                case CBTActionType.MoveToAttackRange:

                    npc.SetMovementSpeed(npc.defaultMovementSpeed);
                    mover.Stop();

                    return true;

                case CBTActionType.CombatStance:

                    //if (!CheckExitTimeMet())
                    //{
                    //    return false;
                    //}

                    if (combatStanceCoroutine != null)
                    {
                        StopCoroutine(combatStanceCoroutine);
                        combatStanceCoroutine = null;
                    }

                    npc.DoTargetViewLock = false;
                    npc.SetMovementSpeed(combatStanceMoveSpeed);
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

                case CBTActionType.DodgeAttack:

                    bool farEnough = npc.ctx.DistanceToTarget > npc.ctx.TargetAttackRange;

                    if (farEnough)
                    {
                        dodgeRountine = null;

                        return true;
                    }

                    return false;

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
            // Return if the entries in the condition node is null
            if (conditionNode.ConditionEntries == null)
            {
                Debug.LogError($"Condition node {conditionNode.NodeID} is null!");
                return false;
            }

            // Return if there are no entries
            if (conditionNode.ConditionEntries.Count == 0)
            {
                Debug.LogError($"Condition node {conditionNode.NodeID} has no condition entries!");
                return false;
            }

            // Return true if one of the clusters of conditions (seperated by 'ORs') is true
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

                    return targetDist <= (enemyWeapon.weaponRange * 1.1 )? 1 : 0;

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

                // Dynamic defensive radius based on the condition of the npc compared to its combat target

                _currentDefensiveRadius = Mathf.Lerp(defensiveRadiusMin, defensiveRadiusMax, npc.ctx.CurrentHealthPercentage * defensiveRadiusMax);

                float distance = _currentDefensiveRadius + UnityEngine.Random.Range(-radiusStep, radiusStep);

                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;

                Vector3 targetPosition = combatTarget.position + new Vector3(offset.x, offset.y, 0);

                mover.SetTarget(targetPosition);

                // Wait for the mover to reach target
                yield return new WaitUntil(() => mover.AgentArrived());

                yield return new WaitForSeconds(waitDuration);

            }
        }

        private IEnumerator DoDodgeRoutine()
        {
            npc.DoTargetViewLock = true;

            npc.SetMovementSpeed(npc.defaultMovementSpeed);

            Vector3 diff = transform.position - npc.ctx.TargetTransform.position;

            diff.Normalize();

            mover.SetTarget(transform.position + (diff * 10));

            yield return null;
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

        #region Combat Context Evaluation

        private bool CheckExitTimeMet(float actionDwellTime)
        {
            if (Time.time - currentActionStartTime < actionDwellTime)
            {
                Debug.LogWarning("Cannot exit yet, haven't dwelled enough!");
                return false;
            }

            return true;
        }

        public CBTSystemActionNodeSO GetEvaluateUtilitySelectorNode(CBTSystemUtilitySelectorNodeSO utilitySelecorNode)
        {
            // Get all action nodes connected to the utility selector node
            List<CBTSystemActionNodeSO> actionNodes = GetConnectedActionNodes(utilitySelecorNode);

            // Get all the action nodes that can be executed based on the current combat context
            var candidates = actionNodes.Where(node => CanExcecute(node.ActionType, npc.ctx)).ToList();

            if (candidates.Count == 0)
            {
                Debug.Log("No candidates found for utility selector node: " + utilitySelecorNode.NodeID);
                return null;
            }

            var emergencyCandidates = candidates.Where(c => c.GetPriority() == ActionPriority.Emergency).ToList();

            // If the exit time is not met, we cannot set a new action node yet
            if (!CheckExitTimeMet(utilitySelecorNode.DecisionInterval))
            {
                // If we have emergency override on, and there are emergency candidates present, we can still return one of them
                if (utilitySelecorNode.EmergencyOverride && emergencyCandidates.Count >= 0)
                {
                    // Replace the candidates list with the emergency candidates
                    candidates = emergencyCandidates;
                }

                // Otherwise, we can't proceed with any action change
                else
                {
                    Debug.Log($"Exit Time Not Met and No emergency candidates");
                    return null;
                }
            }

            if (candidates.Count == 0)
            {
                Debug.Log("No Candidates");
                return null;
            }

            // Score each of the candidates
            var rawScores = candidates
                .Select(node => EvaluateUtility(node.ActionType, npc.ctx))
                .ToArray();

            // right before you softmax:
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i] == currentActionNode)
                {
                    rawScores[i] += utilitySelecorNode.StickyBonus;  // e.g. +0.2 or +1.0
                    utilitySelecorNode.CurrentActionNodeScore = rawScores[i]; // Update the current action node score
                }

            for (int i = 0; i < candidates.Count; i++)
            {
                //Debug.Log($"{Time.time:f1}s  {candidates[i].ActionType}  score={rawScores[i]:0.00}");
            }

            // Turn the scores into probabilities
            var probabilities = SoftMax(rawScores, utilitySelecorNode.Temperature);

            // Pick one index by weighted random choice, then execute it
            int chosenIndex = Sample(probabilities);
            var chosen = candidates[chosenIndex];

            if (chosen == currentActionNode)
            {
                Debug.LogWarning($"Not switching action, already executing {chosen.ActionType} with score {rawScores[chosenIndex]:0.00}");
                return null;
            }

            // Get the difference in score between the chosen action node and the current action node
            float scoreDifference = rawScores[chosenIndex] - utilitySelecorNode.CurrentActionNodeScore;

            // Should we clamp the score difference to a minimum value?

            if (scoreDifference < 0)
                scoreDifference = 0;

            // If the difference in score doesn't exceed the minimum switch score, we don't switch actions
            if (scoreDifference < utilitySelecorNode.MinSwitchScore)
            {
                Debug.LogWarning($"Not switching action, score difference {scoreDifference:0.00} is below minimum switch score {utilitySelecorNode.MinSwitchScore:0.00}");
                return null;
            }

            // Set the current score to the score of the chosen action node
            utilitySelecorNode.CurrentActionNodeScore = rawScores[chosenIndex];

            return chosen;
        }

        public bool CanExcecute(CBTActionType desiredAction, CombatContext ctx)
        {
            switch (desiredAction)
            {
                case CBTActionType.MoveToStanceRadius:
                    // only if we're outside our combat stance radius
                    return ctx.DistanceToTarget > _currentDefensiveRadius;

                case CBTActionType.MoveToAttackRange:
                    // inside your defensive circle, but still too far to hit
                    return ctx.DistanceToTarget <= _currentDefensiveRadius;

                case CBTActionType.CombatStance:
                    // always valid once in range
                    return ctx.DistanceToTarget <= _currentDefensiveRadius;

                case CBTActionType.LightAttack:
                    // only if we have enough stamina to attack and the target is in range

                    bool inRange = ctx.DistanceToTarget <= ctx.LightAttackRange;

                    bool minStamina = ctx.CurrentStaminaValue >= ctx.LightAttackStaminaCost;

                    return ctx.DistanceToTarget <= ctx.LightAttackRange;

                case CBTActionType.StartHeavyAttack:
                    // only if we have enough stamina to attack and the target is in range
                    return ctx.DistanceToTarget <= ctx.HeavyAttackRange;

                case CBTActionType.ReleaseHeavyAttack:
                    return npc.MinChargeTimeMet();

                case CBTActionType.HoldBlock:
                    return ctx.DistanceToTarget <= ctx.TargetAttackRange;

                case CBTActionType.DodgeAttack:
                    return true;

                default:
                    Debug.LogWarning($"Action {desiredAction} not implemented in action execution check.");
                    return false;
            }
        }

        [BoxGroup("Scoring Modifiers, Move Attack"), SerializeField] private float lastHitThresholdTime = 3;


        // 0 when value ≤ a, 1 when value ≥ b, linear in-between
        float High(float v, float a, float b) => Mathf.Clamp01((v - a) / (b - a));

        // 0 when value ≥ b, 1 when value ≤ a
        float Low(float v, float a, float b) => Mathf.Clamp01((b - v) / (b - a));


        public float EvaluateUtility(CBTActionType action, CombatContext ctx)
        {
            float recentHit = 0;
            float rawHealthDelta = 0;
            float hpDiffFactor = 0;
            float rawStamDelta = 0;
            float stamDiffFactor = 0;
            float stamSufficient = 0;
            float attackRange = 0;
            float attackImminentScore = 0;
            float distanceFactor = 0;
            float staminaLow;
            float rise, fall;

            switch (action)
            {
                case CBTActionType.MoveToStanceRadius:

                    // highest when you’re very far
                    return Mathf.Clamp01((ctx.DistanceToTarget - _currentDefensiveRadius) / _currentDefensiveRadius);

                case CBTActionType.MoveToAttackRange:

                    /**** Considerations ****/

                    /* How is my health, especially relative to the target's health? Can I risk going in for an attack?
                     * Is the target attacking? If so, I should not go in for an attack, as I might get hit.
                     * How is my stamina compared to my enemy's? If it is low, i should probably keep my distance and let my stamina regenerate
                     * How is my health compared to my enemy's? If it is a lot lower, I should definately keep my distance and let my stamina regenerate
                     * If my health is slightly lower than my enemy's but my stamina is much higher, I can probably risk going in for an attack, as I can block or dodge their attacks
                     * How long has it been since I was last hit? If it has been a while, I can be more aggressive and go in for an attack
                     * Is the target holding a heavy attack? If so, I should not go in for an attack, as I might get hit.
                     */



                    //float noRecentHit = High(ctx.TimeSinceLastHit, 3f, 5f);   // 0 → 1
                    //float healthier = High(ctx.CurrentHealthPercentage -
                    //                         ctx.TargetHealthPercentage, 0f, .4f);
                    //float moreStam = High(ctx.CurrentStaminaPercentage -
                    //                         ctx.TargetStaminaPercentage, 0f, .3f);
                    //float staminaOkay = High(ctx.CurrentStaminaPercentage, .3f, .6f);

                    //return noRecentHit * weights.moveToAttack_recentHit +
                    //        healthier * weights.moveToAttack_healthDiff +
                    //        moreStam * weights.moveToAttack_stamDiff +
                    //        staminaOkay * weights.moveToAttack_stamReady;


                    // Take the time since last hit into account (If we have not been hit in a while, we can be more aggressive)
                    float t = Mathf.InverseLerp(3.0f, 5.0f, ctx.TimeSinceLastHit);
                    //  t = 0  (≤3 s) … 1 (≥5 s)
                    recentHit = Mathf.SmoothStep(0f, 1f, t);

                    // The difference of my health to the target's health into consideration (If my health is lower than the target's, I should be more defensive)
                    rawHealthDelta = ctx.CurrentHealthPercentage - ctx.TargetHealthPercentage; // −1 .. +1
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric

                    // The difference of my stamina to the target's stamina into consideration (If my stamina is lower than the target's, I should be more defensive)
                    rawStamDelta = ctx.CurrentStaminaPercentage - ctx.TargetStaminaPercentage;
                    stamDiffFactor = Mathf.Clamp01((rawStamDelta + 1f) * 0.5f);

                    stamSufficient = Mathf.InverseLerp(0.2f, 0.5f, ctx.CurrentStaminaPercentage); // 0.2 is the minimum stamina to attack (score 0), 0.5 will raise the score to 1

                    // *** Target Attacking *** //

                    // If the target is attacking, we should not go in for an attack
                    if (ctx.SeenIncomingAttack > 0f)
                    {
                        // If the target is attacking, we should not go in for an attack
                        return 0f;
                    }

                    return
                        (recentHit * weights.moveToAttack_recentHit +

                        hpDiffFactor * weights.moveToAttack_healthDiff +

                        stamDiffFactor * weights.moveToAttack_stamDiff +

                        stamSufficient * weights.moveToAttack_stamReady) * aggression;

                // Combat stance is where the character will encircle their target, maintaining their distance while being ready to attack or block
                case CBTActionType.CombatStance:

                    /**** Considerations ****/

                    /* 
                     * If our health is lower than our target, we need to be more defensive, and stay in combat stance
                     * If our stamina is lower than our target, we need to be more defensive, and stay in combat stance
                     * If we were recently hit, we might want to back off
                     */

                    //recentHit = Low(ctx.TimeSinceLastHit, 0f, 3f);      // 1 → 0
                    //float weaker = Low(ctx.CurrentHealthPercentage -
                    //                          ctx.TargetHealthPercentage, -.4f, 0f);
                    //float lessStam = Low(ctx.CurrentStaminaPercentage -
                    //                          ctx.TargetStaminaPercentage, -.3f, 0f);
                    //float staminaLow = Low(ctx.CurrentStaminaPercentage, .2f, .5f);

                    //return recentHit * weights.combatStance_recentHit +
                    //        weaker * weights.combatStance_healthDiff +
                    //        lessStam * weights.combatStance_stamDiff +
                    //        staminaLow * weights.combatStance_stamReady;

                    // The more recently we were hit, the more we want to back off
                    recentHit = 1f - Mathf.InverseLerp(0f, lastHitThresholdTime, ctx.TimeSinceLastHit);

                    // The higher the health of the enemy compared to us, the more we want to back off
                    rawHealthDelta = ctx.TargetHealthPercentage - ctx.CurrentHealthPercentage;
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric

                    rawStamDelta = ctx.TargetStaminaPercentage - ctx.CurrentStaminaPercentage;
                    stamDiffFactor = Mathf.Clamp01((rawStamDelta + 1f) * 0.5f);

                    stamSufficient = 1f - Mathf.InverseLerp(0.2f, 0.5f, ctx.CurrentStaminaPercentage);                 

                    return 
                     (recentHit * weights.combatStance_recentHit +

                     hpDiffFactor * weights.combatStance_healthDiff +

                     stamDiffFactor * weights.combatStance_stamDiff +

                     stamSufficient * weights.combatStance_stamReady) * riskAversion;

                //case CBTActionType.InstantBlock:

                //    /**** Considerations ****/

                //    /* 
                //     * The variables to evaluate are if we are relatively close to the target (too far to dodge), 
                //     * if our health is low, we dodging might be too risky
                //     * if our stamina is too low, we might not be able to block the attack (afford the cost)
                //     * if our stamina is at 0 or below 5%, we should not block at all
                //     * if the enemy's stamina is high, and we have both stamina and health to expend, we should block to reduce the enemy's stamina, and fight back later where they can't block anymore
                //     */

                //    //if (ctx.SeenIncomingAttack == 0) return 0f;
                //    //if (ctx.CurrentStaminaPercentage < .1f) return 0f;   // avoid stagger

                //    //float veryClose = Low(ctx.DistanceToTarget, 0f, ctx.TargetAttackRange);
                //    //float stamEnough = High(ctx.CurrentStaminaPercentage, .25f, .5f);

                //    //return veryClose * weights.block_distanceWeight +
                //    //        stamEnough * weights.block_staminaWeight;

                //    // (gated first)
                //    if (ctx.SeenIncomingAttack == 0) return 0f;
                //    if (ctx.CurrentStaminaPercentage < 0.05f) return 0f;     // avoid stagger

                //    float lowDistance = ctx.ValueLow(ctx.DistanceToTarget, 0, ctx.TargetAttackRange); // 1 close, 0 far
                //    float staminaEnough = ctx.ValueHigh(ctx.CurrentStaminaPercentage, 0.25f, 0.5f); // 1 high, 0 low

                //    return
                //         (lowDistance * weights.block_distanceWeight +
                //         staminaEnough * weights.block_staminaWeight) * riskAversion;

                case CBTActionType.HoldBlock:

                    if (ctx.CurrentStaminaPercentage < 0.05f) return 0f;     // avoid stagger

                    // I want to do this if the distance is low (within attack range of target)
                    float lowDistanceHold = ctx.ValueLow(ctx.DistanceToTarget, 0, ctx.TargetAttackRange); // 1 close, 0 far

                    // I want to do this only if my health is quite low
                    float lowHealth = ctx.ValueLow(ctx.CurrentHealthPercentage, 0, 0.3f); // 1 low, 0 high

                    // I want to do this only if my stamina is quite low, but not lower than 10%
                    float lowStamina = ctx.ValueLow(ctx.CurrentStaminaPercentage, 0.25f, 0.4f); // 1 low, 0 high
                    if (ctx.CurrentStaminaPercentage < 0.25f) lowStamina = 0;

                    // I want to do this only if my stamina 

                    return
                       (lowDistanceHold * weights.passiveBlock_lowDistanceWeight +
                        lowHealth * weights.passiveBlock_lowHealthWeight +
                        lowStamina * weights.passiveBlock_lowStaminaWeight) * riskAversion;


                case CBTActionType.DodgeAttack:

                    /**** Considerations ****/

                    /* 
                     * The variables to evaluate are if we are too close to the target (low chance to successfly move out of the way),
                     * depending on the speed of the target weapon's base attack, we might not be able to dodge in time
                     * if the target is performing a heavy attack, we have a higher chance of being able to dodge
                     * the lower our stamina, the higher our favorability to dodge, as we cannot block
                     */

                    //if (ctx.SeenIncomingAttack == 0) return 0f;

                    //float safeZone = High(ctx.DistanceToTarget,
                    //                      ctx.TargetAttackRange * .8f, ctx.TargetAttackRange);
                    //float stamLow = Low(ctx.CurrentStaminaPercentage, 0f, .25f);

                    //return safeZone * weights.dodge_inSafeAreaWeight +
                    //        stamLow * weights.dodge_staminaAbundanceWeight;

                    // (gated first)
                    if (ctx.SeenIncomingAttack == 0) return 0f;

                    if (ctx.TargetHeavyAttackChargePercentage <= 0)
                    {
                        return 0;
                    }

                    // Lerp: Tells you how far along the line between 'a' and 'b' the value is. If the value is at 'a', return 0, if the value is at 'b' return 1.
                    rise = Mathf.Clamp01(Mathf.InverseLerp(ctx.TargetAttackRange * 0.95f, ctx.TargetAttackRange * 1.2f, ctx.DistanceToTarget));

                    // In this inverseLerp, we subtract by 1, becuase we want to fade out the score as the stamina percentage increases (higher = worse)
                    fall = Mathf.Clamp01(1 - Mathf.InverseLerp(ctx.TargetAttackRange * 1.2f, ctx.TargetAttackRange * 1.5f, ctx.DistanceToTarget));

                    // Combine the two lerps into a bump                 
                    float dodgeCurve = Mathf.Min(rise, fall);

                    staminaLow = ctx.ValueLow(ctx.CurrentStaminaPercentage, 0, 0.3f); // 0 lots, 1 low - if we have a lot of stamina, we should just block

                    // Much more favourable when the enemy is performing a heavy attack, as we can dodge out of the way more easily
                    float enemyHeavyAttack = ctx.TargetHeavyAttackChargePercentage > 0 ? 1 : 0; // 1 if heavy attack, 0 if not

                    // Less favourable when our health is low, as dodging might be risky
                    lowHealth = ctx.ValueHigh(ctx.CurrentHealthPercentage, .4f, .7f); // 1 high, 0 low    


                    return

                       (dodgeCurve * weights.dodge_inSafeAreaWeight +
                        staminaLow * weights.dodge_staminaAbundanceWeight +
                        enemyHeavyAttack * weights.dodge_enemyHeavyAttackWeight +
                        lowHealth * weights.dodge_healthAbundantWeight) * riskAversion;

                case CBTActionType.LightAttack:

                    /**** Considerations ****/

                    /* How is my current distance to the enemy, if is just barely within my light attack range, i might wait to wait util i move closer to them, as the chance of hitting them is low.
                     * How is my stamina? If it is on the lower side, a light attack might be smarter than a heavy attack, as it costs less stamina.
                     * 
                     */

                    //if (ctx.DistanceToTarget > ctx.LightAttackRange)
                    //    return 0f;

                    //float close = Low(ctx.DistanceToTarget,
                    //                      ctx.IdealLightAttackRange, ctx.LightAttackRange);
                    //close = close * close * (3f - 2f * close);               // smoothstep

                    //float stamPeak = Mathf.Min(High(ctx.CurrentStaminaPercentage, .25f, .4f),
                    //                             Low(ctx.CurrentStaminaPercentage, .4f, .8f));

                    //float enemyHighHP = High(ctx.TargetHealthPercentage, .2f, .6f);

                    //return close * weights.lightAttack_proximityWeight +
                    //        stamPeak * weights.lightAttack_staminaLightBonus +
                    //        enemyHighHP * weights.lightAttack_lowHealthBonus;

                    // *** Distance to target ***

                    float currentAttackOpt = Mathf.InverseLerp(ctx.IdealLightAttackRange , ctx.LightAttackRange, ctx.DistanceToTarget);

                    // 1 when at ideal range, 0 when at max range
                    float proximity = 1f - currentAttackOpt;

                    // Clamp the value
                    proximity = Mathf.Clamp01(proximity);

                    // Smoothstep for a softer falloff so that it ramps up nicely
                    proximity = proximity * proximity * (3f - 2f * proximity); // This formula gives a smooth transition from 0 to 1

                    // *** Current Stamina ***

                    // The goal is to have a curve that turns on between 25% and 40%, peaks at 40%, then fades out by 75%.
                    
                    // Lerp: Tells you how far along the line between 'a' and 'b' the value is. If the value is at 'a', return 0, if the value is at 'b' return 1.
                    rise = Mathf.Clamp01(Mathf.InverseLerp(0.25f, 0.40f, ctx.CurrentStaminaPercentage));

                    // In this inverseLerp, we subtract by 1, becuase we want to fade out the score as the stamina percentage increases (higher = worse)
                    fall = Mathf.Clamp01(1 - Mathf.InverseLerp(0.40f, 0.75f, ctx.CurrentStaminaPercentage));

                    // Combine the two lerps into a bump
                    // We use m in here because we want to get the lower value of the two lerps, as we want to fade out the score as the stamina percentage increases
                    float staminaLightBonus = Mathf.Min(rise, fall);

                    // *** Current Health Difference***

                    // The higher the health of the enemy compared to us, the more we want to back off
                    rawHealthDelta = ctx.TargetHealthPercentage - ctx.CurrentHealthPercentage;
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric

                    return

                        (proximity * weights.lightAttack_proximityWeight +
                        staminaLightBonus * weights.lightAttack_staminaLightBonus +
                        hpDiffFactor * weights.lightAttack_lowHealthBonus) * aggression;

                case CBTActionType.StartHeavyAttack:

                    /**** Considerations ****/

                    /* How is my current stamina? Higher stamina means I could use some to perform a heavy attack.
                     * How low is the enemy's health? If it is quite low, i should try to finish them off now with light attacks, rather than let them regain health and stamina while i charge for a heavy attack.
                     * How far is the enemy from me? The closer they are, the higher the chance of them getting hit by the heavy attack. Otherwise I might as well just perform a light attack.
                     */

                    if (ctx.CurrentStaminaPercentage < 0.4f)          // hard gate
                        return 0f;

                    //float near = Low(ctx.DistanceToTarget, 0f, ctx.HeavyAttackRange * .6f);
                    //float highStam = High(ctx.CurrentStaminaPercentage, .4f, .8f);
                    //enemyHighHP = High(ctx.TargetHealthPercentage, .15f, .4f);
                    //float myHighHP = High(ctx.CurrentHealthPercentage, .3f, .6f);

                    //return near * weights.heavyAttack_proximityWeight +
                    //        highStam * weights.heavyAttack_highStaminaWeight +
                    //        enemyHighHP * weights.heavyAttack_lowEnemyHealthWeight +
                    //        myHighHP * weights.heavyAttack_lowHealthWeight;

                    // WANT to attack if we have stamina
                    float staminaFactor = ctx.ValueHigh(ctx.CurrentStaminaPercentage, 0.4f, 1f); // 1 high, 0 low

                    // DONT want to attack if the enemy's health is low
                    float enemyHealthFactor = ctx.ValueHigh(ctx.TargetHealthPercentage, .1f, 0.3f); // 1 high, 0 low

                    // WANT to attack if the enemy is close enough                          
                    distanceFactor = ctx.ValueLow(ctx.DistanceToTarget, 0, ctx.HeavyAttackRange * 0.6f); // 1 close, 0 far

                    // DONT want to attack if my health is low, as I might get hit while charging
                    float healthFactor = ctx.ValueHigh(ctx.CurrentHealthPercentage, 0.2f, 0.5f); // 1 high, 0 low

                    // high when we have stamina & are close
                    return

                        (distanceFactor * weights.heavyAttack_proximityWeight +
                        enemyHealthFactor * weights.heavyAttack_lowEnemyHealthWeight +
                        staminaFactor * weights.heavyAttack_highStaminaWeight +
                        healthFactor * weights.heavyAttack_lowHealthWeight) * aggression;


                case CBTActionType.ReleaseHeavyAttack:

                    /**** Considerations ****/

                    /* What is the current stamina drain of the heavy attack?
                     * Is the target blocking? I'd rather them not to be blocking.
                     * What is the current hit coverage area, do i have time to continue charging my heavy attack, or are the enemy about to leave my attack range?
                     * Is the target charging their own heavy attack? If so, I should release mine to interrupt them.
                     * Is the target blocking, and if so, what is their current stamina at? Will the attack i perform stagger them? It'd be better to stagger the opponent rather than let the regain stamina and block my attack.
                     */

                    float currentHoldPerentage = ctx.TargetHeavyAttackChargePercentage;

                    // WANT to release the farther the target is, as we want to hit them before they can escape
                    distanceFactor = Mathf.Clamp01(1 - Mathf.InverseLerp(ctx.TargetAttackRange * 0.8f, ctx.TargetAttackRange, ctx.DistanceToTarget));

                    float enemyChargingHeavyAttack = ctx.TargetHeavyAttackChargePercentage > 0f ? 1f : 0f; // 1 if the enemy is charging a heavy attack, 0 otherwise

                    float enemyBlocking = ctx.TargetBlocking;

                    // only peaks once min‐charge is met, else zero
                    return 
                        (currentHoldPerentage * weights.releaseHeavyAttack_holdPercentWeight +
                        distanceFactor * weights.releaseHeavyAttack_enemyDistanceWeight +
                        enemyChargingHeavyAttack * weights.releaseHeavyAttack_enemyChargingHeavyAttackWeight +
                        enemyBlocking * weights.releaseHeavyAttack_enemyAttackBlockWeight) * aggression;

                default:
                    Debug.LogError($"Action {action} not implemented in utility evaluation.");
                    return 0f;

            }
        }

        static float[] SoftMax(float[] scores, float temperature = 1.0f)
        {
            int n = scores.Length;
            float max = scores.Max();
            float sum = 0f;
            float[] exps = new float[n];

            for (int i = 0; i < n; i++)
            {
                float e = Mathf.Exp((scores[i] - max) / temperature);
                exps[i] = e;
                sum += e;
            }

            for (int i = 0; i < n; i++)
            {
                exps[i] /= sum;
            }

            return exps;
        }

        static int Sample(IList<float> probs)
        {
            float rand = UnityEngine.Random.value; // [0,1)
            float cumulative = 0f;
            for (int i = 0; i < probs.Count; i++)
            {
                cumulative += probs[i];
                if (rand <= cumulative)
                    return i;
            }
            return probs.Count - 1; // fallback due to rounding
        }

        /// <summary>
        /// Helper to check if a score is within 0 and 1 and flags it if it isnt
        /// </summary>
        bool WithinBounds(float score)
        {
            if (score < 0 || score > 1)
            {
                Debug.LogWarning($"Score {score} is out of bounds! It should be between 0 and 1.");
                return false;
            }

            return true;
        }

        float ApplyCompensationToScore(float rawScore, int numConsiderations)
        {
            float modFactor = 1f - (1f / numConsiderations);
            float makeUp = (1f - rawScore) * modFactor; // how much we need to make up for
            return rawScore + (makeUp * rawScore);
        }

        #endregion
    }
}

