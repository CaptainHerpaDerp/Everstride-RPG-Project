using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace QuestSystem.Windows
{
    using Enumerations;
    using System;
    using Elements;
    using GraphSystem.Base;
    using GraphSystem.Base.Windows;

    public class QuestSystemGraphView : BaseGraphView
    {
        private QuestSystemSearchWindow searchWindow;
        protected override BaseSearchWindow baseSearchWindow { get => searchWindow; set => searchWindow = value as QuestSystemSearchWindow; }

        public QuestSystemGraphView(QuestSystemEditorWindow questSystemEditorWindow) : base(questSystemEditorWindow)
        {
            AddManipulators();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());

            // Add selection dragger before rectangle selector
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Objective", QuestSystemNodeType.Objective));

            this.AddManipulator(CreateGroupContextualMenu());

            RegisterCallback<KeyDownEvent>(OnDuplicateKey);
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, QuestSystemNodeType nodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(

                menuEvent => menuEvent.menu.AppendAction("Add Objective Node", actionEvent => AddElement(CreateNode((int)nodeType, actionEvent.eventInfo.localMousePosition)))
                );

            return contextualMenuManipulator;
        }

        #region Elements Creation    

        public override BaseNode CreateNode(int nodeTypeIndex, Vector2 position, bool shouldDraw = true)
        {
            // Set the node type to the index of the node type
            QuestSystemNodeType questType = (QuestSystemNodeType)nodeTypeIndex;

            Type nodeType = Type.GetType($"QuestSystem.Elements.QuestSystem{questType}Node");

            Debug.Log($"Creating node of type {nodeType}");

            QuestSystemNode node = Activator.CreateInstance(nodeType) as QuestSystemNode;

            node.Initialize(this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            AddUngroupedNodes(node);

            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is QuestSystemGroup))
                {
                    continue;
                }

                QuestSystemGroup group = selectedElement as QuestSystemGroup;

                group.AddElement(node);
            }

            return node;
        }

        protected override void AddSearchWindow()
        {
            if (searchWindow != null)
                return;

            searchWindow = ScriptableObject.CreateInstance<QuestSystemSearchWindow>();

            searchWindow.Initialize(this);

            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }


        #endregion

        #region Callbacks

        protected override void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        if (edge == null)
                        {
                            Debug.LogWarning("Edge is null");
                            continue;
                        }

                        if (edge.output == null || edge.input == null)
                        {
                            Debug.LogWarning("Edge output or input is null");
                            continue;
                        }

                        //// Check if outputPort.userData is set to the source node
                        //if (edge.output.userData == null)
                        //{
                        //    Debug.LogWarning("Edge output user data is null");
                        //    continue;
                        //}

                        QuestSystemNode sourceNode = null, targetNode = null;

                        if (IsQuestNode(edge.output.node as BaseNode))
                        {
                            sourceNode = edge.output.node as QuestSystemNode;
                        }
                        else
                        {
                            Debug.LogWarning("Output user data is not of type QuestSystemNode");
                            continue;
                        }

                        if (IsQuestNode(edge.input.node as BaseNode))
                        {
                            targetNode = edge.input.node as QuestSystemNode;
                        }
                        else
                        {
                            Debug.LogWarning("Input node is not of type QuestSystemNode");
                            continue;
                        }
                        
                        Debug.Log($"Connecting {sourceNode.QuestDescription} to {targetNode.QuestDescription}");
                        sourceNode.NextNodeID = targetNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge)element;

                        // Check if outputPort.userData is set to the source node
                        if (edge.output.userData == null ||!IsQuestNode(edge.output.userData as BaseNode))
                        {
                            Debug.LogWarning("Edge output user data is null or not of type QuestSystemNode");
                            continue;
                        }

                        //// Remove references to the target node in the source node's conditions
                        //QuestSystemConditionSaveData conditionToRemove = sourceNode.Conditions
                        //    .FirstOrDefault(cond => cond.ConditionValue == (edge.input.node as QuestSystemNode)?.ID);

                        //if (conditionToRemove != null)
                        //{
                        //    sourceNode.Conditions.Remove(conditionToRemove);
                        //    Debug.Log($"Removed condition linking {sourceNode.QuestName} to {(edge.input.node as QuestSy  stemNode)?.QuestName}");
                        //}
                    }
                }

                return changes;
            };
        }


        #endregion

        #region Utility Methods

        private static bool IsQuestNode(Node node)
        {
            return node.GetType() == typeof(QuestSystemNode) || node.GetType().IsSubclassOf(typeof(QuestSystemNode));
        }

        protected override BaseNode DuplicateNode(BaseNode originalBaseNode)
        {
            QuestSystemNode originalNode = originalBaseNode as QuestSystemNode;

            Vector2 newPosition = originalNode.GetPosition().position + new Vector2(20, 20);

            int nodeIndex = (int)originalNode.NodeType;

            // Create a new node with the same properties as the original
            QuestSystemNode newNode = CreateNode(nodeIndex, newPosition) as QuestSystemNode;

            if (newNode.NodeType == QuestSystemNodeType.Objective)
            {
                foreach (var condition in ((QuestSystemObjectiveNode)originalNode).Conditions)
                {
                    ((QuestSystemObjectiveNode)newNode).Conditions.Add(condition);
                }
            }

            // Copy any other properties from the original node that you want to duplicate

            newNode.Draw(); // Draw the new node

            return newNode;
        }

        #endregion
    }
}