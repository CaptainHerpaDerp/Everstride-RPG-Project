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

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);

            nodeLabel = "Action Node";

            actionContainer = new VisualElement();
            actionContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(actionContainer);
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