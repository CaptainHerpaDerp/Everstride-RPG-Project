using UnityEngine.UIElements;
using UnityEngine;
using GraphSystem.Base;

namespace CBTSystem.Elements.Nodes
{
    using Enumerations;
    using System.Collections.Generic;

    public class CBTSystemActionNode : CBTSystemNode
    {
        private VisualElement actionContainer;

        public CBTActionType ActionType { get; set; }

        Dictionary<CBTSystemConditionNode, int> conditionNodePriorities = new();

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);

            nodeLabel = "Action Node";

            actionContainer = new VisualElement();
            actionContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(actionContainer);
        }

        public override void SetNextNodeIDs(List<string> nextNodeIDs)
        {
            _nextNodeIDs = nextNodeIDs;

            // If we are setting the list, it would be coming from a load, meaning the nodes will have already had priorities set

            List<CBTSystemNode> graphNodes = graphView.FindNodesById(NextNodeIDs);
            List<CBTSystemConditionNode> conditionNodes = new();

            foreach (var node in graphNodes)
            {
                if (node is CBTSystemConditionNode conditionNode)
                {
                    conditionNodes.Add(conditionNode);
                }
            }

            if (conditionNodes.Count == 0)
            {
                Debug.LogWarning("No condition nodes found in the next node IDs.");
                return;
            }

            foreach (var conditionNode in conditionNodes)
            {
                // If the condition node has a priority set, add it to the dictionary
                if (conditionNode.Priority != -1)
                {
                    conditionNodePriorities.Add(conditionNode, conditionNode.Priority);

                    // Listen to its priority change event
                    conditionNode.OnPriorityChanged = () =>
                    {
                        int currentIndex = conditionNodePriorities[conditionNode];
                        int newIndex = (currentIndex + 1) % conditionNodes.Count;
                        conditionNodePriorities[conditionNode] = newIndex;
                        conditionNode.SetConditionPriority(newIndex);
                    };
                }
                else
                {
                    Debug.LogWarning($"Condition node {conditionNode.title} does not have a priority set. It will not be added to the priorities dictionary.");
                }
            }

            RefreshConditionNodePriorities();
        }

        public override void AddNextNodeID(string nextNodeID)
        {
            if (!NextNodeIDs.Contains(nextNodeID))
            {
                NextNodeIDs.Add(nextNodeID);
                RefreshConditionNodePriorities();
            }
        }

        public override void RemoveNextNodeID(string nextNodeID)
        {
            if (NextNodeIDs.Contains(nextNodeID))
            {
                NextNodeIDs.Remove(nextNodeID);
                RefreshConditionNodePriorities();
            }
        }

        public void RefreshConditionNodePriorities()
        {
            List<CBTSystemNode> graphNodes = graphView.FindNodesById(NextNodeIDs);
            List<CBTSystemConditionNode> conditionNodes = new();

            foreach (var node in graphNodes)
            {
                if (node is CBTSystemConditionNode conditionNode)
                {
                    conditionNodes.Add(conditionNode);
                }
            }

            if (conditionNodes.Count == 0)
            {
                Debug.LogWarning("No condition nodes found in the next node IDs.");
                return;
            }

            if (conditionNodes.Count == 1)
            {
                conditionNodes[0].RemoveConditionPriority();
                return;
            }

           // Debug.Log($"Setting condition priorities for {conditionNodes.Count} condition nodes.");

            // Remove any null or invalid condition nodes from the dictionary
            var keysToRemove = new List<CBTSystemConditionNode>();
            foreach (var node in conditionNodePriorities.Keys)
            {
                if (node == null || !conditionNodes.Contains(node))
                {
                    keysToRemove.Add(node);
                }
            }
            foreach (var node in keysToRemove)
            {
                conditionNodePriorities.Remove(node);
            }

            //// Assign priorities in order
            //for (int i = 0; i < conditionNodes.Count; i++)
            //{
            //    var conditionNode = conditionNodes[i];
            //    conditionNodePriorities[conditionNode] = i;
            //    //conditionNode.SetConditionPriority(i);
            //}

            // Ensure OnPriorityChanged is only registered once per node
            foreach (var conditionNode in conditionNodes)
            {
                if (!conditionNodePriorities.ContainsKey(conditionNode))
                {
                    // If the condition node already has a priority, skip it
                    continue;
                }

                // Listen to its priority change event
                conditionNode.OnPriorityChanged = () =>
                {
                    int currentIndex = conditionNodePriorities[conditionNode];
                    int newIndex = (currentIndex + 1) % conditionNodes.Count;
                    conditionNodePriorities[conditionNode] = newIndex;
                    conditionNode.SetConditionPriority(newIndex);
                };

                // When one condition node is hovered, highlight all connected condition nodes to visualize the connections
                conditionNode.OnNodeHovered = () =>
                {
                    HighlightConditionNodes();
                };

                // When the condition node is unhovered, revert the highlights
                conditionNode.OnNodeUnhovered = () =>
                {
                    RevertConditionNodeHighlights();
                };
            }
        }

        /// <summary>
        /// Highlights all condition nodes connected to this action node by setting their background color
        /// </summary>
        protected void HighlightConditionNodes()
        {
            foreach (var conditionNode in conditionNodePriorities.Keys)
            {
                if (conditionNode != null)
                {
                    conditionNode.SetNodeHovered();
                }
            }
        }

        /// <summary>
        /// Resets the highlights of all condition nodes connected to this action node by setting their background color back to default
        /// </summary>
        protected void RevertConditionNodeHighlights()
        {
            foreach (var conditionNode in conditionNodePriorities.Keys)
            {
                if (conditionNode != null)
                {
                    conditionNode.SetNodeDefault();
                }
            }
        }


        #region Node Activation Visualization

        /// <summary>
        /// Sets the node colour to green, indicating it is active. Sets all connected nodes to the check state (indicating their conditions are being checked).
        /// </summary>
        public override void SetNodeActive()
        {
            mainContainer.style.backgroundColor = new StyleColor(Color.green);

            List<CBTSystemNode> connectedNodes = graphView.FindNodesById(NextNodeIDs);

            foreach (var node in connectedNodes)
            {
                if (node is CBTSystemConditionNode conditionNode)
                {
                    conditionNode.SetNodeChecking();
                }
            }

        }

        /// <summary>
        /// Resets the style of this node, and connected condition nodes to its default background color
        /// </summary>
        public override void SetNodeInactive()
        {
            ResetStyle();
        }

        #endregion


        public override void Draw()
        {
            base.Draw();

            // Add custom data fields for the action node: create an enum dropdown
            var actionTypeField = new EnumField("Action Type", ActionType);
            actionTypeField.RegisterValueChangedCallback(evt =>
            {
                // Handle the change (e.g., update the node's data)
                Debug.Log($"Selected Action Type: {evt.newValue}");
                ActionType = (CBTActionType)evt.newValue;   
            });

            actionContainer.Add(actionTypeField);
        }
    }
}