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
    [RequireComponent(typeof(ScoreEvaluatorUtility))]
    public class UtilityAICombatBehaviourManager : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private Mover mover;
        [BoxGroup("Component References"), SerializeField] NPC npc;
        [BoxGroup("Component References"), SerializeField] private CBTSystemContainerSO CBTConfig;
        [BoxGroup("Component References"), SerializeField] private ScoreEvaluatorUtility scoreEval;

        [BoxGroup("Combat Stance"), SerializeField] private float combatStanceMoveSpeed;

        [BoxGroup("Combat Stance"), SerializeField] private float radiusStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveRadiusMin = 1.5f;
        [BoxGroup("Combat Stance"), SerializeField] private float defensiveRadiusMax = 3f;
        [BoxGroup("Combat Stance"), SerializeField] private float maxAngleStep = 1, minAngleStep = 1;
        [BoxGroup("Combat Stance"), SerializeField] private float waitDuration = 1;

        [Header("This value determines the minimum duration an action can be carried out before another can be considered")]
       // [BoxGroup("Utility Behaviour"), SerializeField] private float actionMinDwellTime = 0.35f;

        [BoxGroup("Scoring Variables, Combat Stance"), SerializeField] private float lowStaminaFactor = 0.8f; // Factor to increase the score when stamina is low

        [BoxGroup("Retreat"), SerializeField] private float retreatTimeMax = 5;
        [BoxGroup("Retreat"), SerializeField] private float dodgeTimeMax = 1.5f;
        private float _currentRetreatTime, _currentDodgeTime;

        [ShowInInspector] public float currentDefensiveRadius { get; protected set; }

        // A list of the current condition nodes that are connected to the current node
        private List<CBTSystemConditionNodeSO> curConnectedConditionNodes = new();
        private List<CBTSystemActionNodeSO> curConnectedActionNodes = new();
        private List<CBTSystemUtilitySelectorNodeSO> curConnectedUtilityNodes = new();

        private float currentActionStartTime;

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
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    mover.Resume();
                    mover.SetTarget(combatTarget.transform, currentDefensiveRadius);

                    npc.DoTargetViewLock = true;

                    break;

                case CBTActionType.MoveToAttackRange:

                    npc.OnUpdateCombatState?.Invoke(NPCState.Moving);
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    mover.Resume();
                    mover.SetTarget(combatTarget.transform);

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


                case CBTActionType.DodgeAttack:

                    _currentDodgeTime += Time.deltaTime;

                    npc.OnUpdateCombatState?.Invoke(NPCState.TargetBlocking);
                    mover.Resume();
                    npc.DoTargetViewLock = true;
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    Vector3 diffA = transform.position - npc.ctx.TargetTransform.position;

                    diffA.Normalize();

                    mover.SetTarget(transform.position + (diffA));

                    break; 

                case CBTActionType.Retreat:

                    _currentRetreatTime += Time.deltaTime;

                    npc.OnUpdateCombatState?.Invoke(NPCState.TargetBlocking);
                    mover.Resume();
                    npc.DoTargetViewLock = false;
                    npc.SetMovementSpeed(npc.defaultMovementSpeed);

                    Vector3 diffB = transform.position - npc.ctx.TargetTransform.position;

                    diffB.Normalize();

                    mover.SetTarget(transform.position + (diffB));

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

                // For each of the currently connected utility nodes, evaluate the utility selector node and get the next action node
                CBTSystemActionNodeSO newActionNode = GetEvaluateUtilitySelectorNode(utilityNode);

                // If the newly found action node isnt null, and we are able to exit the current action node, we can set the new action node
                if (newActionNode != null && TryExitCurrentActionNode())
                {
                    currentActionNode = newActionNode;
                    return true;
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

                    bool dodgedLongEnough = _currentDodgeTime >= dodgeTimeMax;

                    bool farEnough = npc.ctx.DistanceToTarget > npc.ctx.TargetAttackRange * 1.2f;
                    
                    if (farEnough == false)
                    Debug.Log("Cant exit, not far enough");

                    if (farEnough || dodgedLongEnough)
                    {
                        _currentDodgeTime = 0;
                        return true;
                    }

                    return false;

                case CBTActionType.Retreat:

                    bool ranFarEnough = npc.ctx.DistanceToTarget > npc.ctx.TargetAttackRange;

                    bool ranLongEnough = _currentRetreatTime >= retreatTimeMax;

                    if (ranFarEnough == false)  
                        Debug.Log("Cant exit, not far enough");

                    if (ranFarEnough || ranLongEnough)
                    {
                        _currentRetreatTime = 0;
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

        #region Action Coroutines

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

                currentDefensiveRadius = Mathf.Lerp(defensiveRadiusMin, defensiveRadiusMax, npc.ctx.CurrentHealthPercentage * defensiveRadiusMax);

                float distance = currentDefensiveRadius + UnityEngine.Random.Range(-radiusStep, radiusStep);

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

        #region Combat Context Evaluation

        private bool CheckExitTimeMet(float actionDwellTime)
        {
            if (Time.time - currentActionStartTime < actionDwellTime)
            {
                return false;
            }

            return true;
        }

        public bool CanExcecute(CBTActionType desiredAction, CombatContext ctx)
        {
            switch (desiredAction)
            {
                case CBTActionType.MoveToStanceRadius:
                    // only if we're outside our combat stance radius
                    return ctx.DistanceToTarget > currentDefensiveRadius;

                case CBTActionType.MoveToAttackRange:
                    // inside your defensive circle, but still too far to hit
                    return ctx.DistanceToTarget <= currentDefensiveRadius;

                case CBTActionType.CombatStance:
                    // always valid once in range
                    return ctx.DistanceToTarget <= currentDefensiveRadius;

                case CBTActionType.LightAttack:
                    // only if we have enough stamina to attack and the target is in range

                    bool inRange = ctx.DistanceToTarget <= ctx.LightAttackRange;

                    bool minStamina = ctx.CurrentStaminaValue >= ctx.LightAttackStaminaCost;

                    return minStamina && ctx.DistanceToTarget <= ctx.LightAttackRange;

                case CBTActionType.StartHeavyAttack:
                    // only if we have enough stamina to attack and the target is in range
                    return ctx.DistanceToTarget <= ctx.HeavyAttackRange;

                case CBTActionType.ReleaseHeavyAttack:
                    return npc.MinChargeTimeMet();

                case CBTActionType.HoldBlock:
                    inRange = ctx.DistanceToTarget <= ctx.TargetAttackRange * 1.2f; // 20% more than the attack range

                    return true;

                case CBTActionType.DodgeAttack:

                    inRange = ctx.DistanceToTarget <= ctx.TargetAttackRange * 1.2f; // 20% more than the attack range

                    return inRange;

                case CBTActionType.Retreat:
                    return true;

                default:
                    Debug.LogWarning($"Action {desiredAction} not implemented in action execution check.");
                    return false;
            }
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
                .Select(node => scoreEval.EvaluateUtility(node.ActionType, npc.ctx))
                .ToArray();

            // right before you softmax:
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i] == currentActionNode)
                {
                    rawScores[i] += utilitySelecorNode.StickyBonus;  // e.g. +0.2 or +1.0
                    utilitySelecorNode.CurrentActionNodeScore = rawScores[i]; // Update the current action node score
                }

            // Turn the scores into probabilities
            var probabilities = ScoreEvaluatorUtility.SoftMax(rawScores, utilitySelecorNode.Temperature);

            // Pick one index by weighted random choice, then execute it
            int chosenIndex = ScoreEvaluatorUtility.Sample(probabilities);
            var chosen = candidates[chosenIndex];

            if (chosen == currentActionNode)
            {
                //Debug.LogWarning($"Not switching action, already executing {chosen.ActionType} with score {rawScores[chosenIndex]:0.00}");
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

        #endregion
    }
}

