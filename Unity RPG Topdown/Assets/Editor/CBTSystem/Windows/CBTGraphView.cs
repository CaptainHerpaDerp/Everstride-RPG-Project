using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using GraphSystem.Base;
using GraphSystem.Base.Windows;

namespace CBTSystem.Base
{
    using CBTSystem.Elements;
    using CBTSystem.Elements.Nodes;
    using Newtonsoft.Json.Bson;
    using System;
    using System.Collections.Generic;
    using Windows;

    public class CBTGraphView : BaseGraphView
    {        
        private CBTSearchWindow searchWindow;
        protected override BaseSearchWindow baseSearchWindow { get => searchWindow; set => searchWindow = value as CBTSearchWindow; }

        protected CBTSystemNode startingNode;

        public CBTGraphView(BaseGraphEditorWindow editorWindow) : base(editorWindow)
        {
            AddManipulators();
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(CreateNodeContextualMenu("Create Action Node", typeof(CBTSystemActionNode)));
            this.AddManipulator(CreateNodeContextualMenu("Create Condition Node", typeof(CBTSystemConditionNode)));

            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (selection.Count == 1 && selection[0] is CBTSystemNode node)
                {
                    evt.menu.AppendAction("Set Primary Node", action =>
                    {
                        node.IsRootNode = true;

                        // Optionally unset others, update visuals, etc.
                    });
                }
            }));

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        /// <summary>
        /// Adds a menu option to create a node at the mouse position.
        /// </summary>
        /// <returns></returns>
        private IManipulator CreateNodeContextualMenu(string actionTitle, Type nodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, action: actionEvent => AddElement(CreateNode(nodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }

        #region Active Node Visualization

        public void SetActiveNode(string nodeID)
        {
            // Clear previous active node styles
            foreach (var element in graphElements)
            {
                if (element is CBTSystemNode node)
                {
                    // Set all nodes to be the default colour
                    node.SetNodeInactive();
                }
            }
            // Set the new active node style
            if (nodeID != null && FindNodeByID(nodeID) is CBTSystemNode activeNode)
            {
                activeNode.SetNodeActive();
            }
        }

        public void ResetNodeStyles()
        {
            // Clear previous active node styles, bring back the starting node's yellow background
            foreach (var element in graphElements)
            {
                if (element is CBTSystemNode node)
                {
                    node.SetNodeDefault();
                }
            }
        }

        #endregion

        public CBTSystemNode FindNodeByID(string nodeID)
        {
            foreach (var element in graphElements)
            {
                if (element is CBTSystemNode node && node.ID == nodeID)
                {
                    return node;
                }
            }
            return null;
        }

        public List<CBTSystemNode> FindNodesById(List<string> NodeIDs)
        {
            List<CBTSystemNode> foundNodes = new();

            foreach (string id in NodeIDs)
            {
                if (FindNodeByID(id) is CBTSystemNode node)
                {
                    foundNodes.Add(node);
                }
            }

            return foundNodes;
        }


        #region Elements Creation    

        // Replaced by specified type methods
        public override BaseNode CreateNode(int nodeTypeIndex, Vector2 position, bool shouldDraw = true)
        {
            throw new NotImplementedException();
        }

        public BaseNode CreateNode(Type nodeType, Vector2 position, bool shouldDraw = true)
        {
            //Debug.Log($"Creating node of type {nodeType}");

            CBTSystemNode node = Activator.CreateInstance(nodeType) as CBTSystemNode;

            node.Initialize(this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            AddUngroupedNodes(node);

            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is CBTSystemGroup))
                {
                    continue;
                }

                CBTSystemGroup group = selectedElement as CBTSystemGroup;

                group.AddElement(node);
            }

            return node;
        }

        protected override void AddSearchWindow()
        {
            if (searchWindow != null)
                return;

            searchWindow = ScriptableObject.CreateInstance<CBTSearchWindow>();

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
                // Handle new edges
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        CBTSystemNode sourceNode = edge.output.node as CBTSystemNode;
                        CBTSystemNode targetNode = edge.input.node as CBTSystemNode;
                        if (sourceNode != null && targetNode != null)
                        {
                            if (!sourceNode.NextNodeIDs.Contains(targetNode.ID))
                                sourceNode.AddNextNodeID(targetNode.ID);
                        }
                    }
                }

                // Handle removed edges
                if (changes.elementsToRemove != null)
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            CBTSystemNode sourceNode = edge.output.node as CBTSystemNode;
                            CBTSystemNode targetNode = edge.input.node as CBTSystemNode;
                            if (sourceNode != null && targetNode != null)
                            {
                                sourceNode.RemoveNextNodeID(targetNode.ID);
                            }
                        }
                        // Optionally: handle node deletion cleanup here
                    }
                }

                return changes;
            };
        }



        #endregion

        #region Utility Methods

        private static bool IsCBTSystemNode(Node node)
        {
            return node.GetType() == typeof(CBTSystemNode) || node.GetType().IsSubclassOf(typeof(CBTSystemNode));
        }

        protected override BaseNode DuplicateNode(BaseNode originalBaseNode)
        {
            CBTSystemNode originalNode = originalBaseNode as CBTSystemNode;

            Vector2 newPosition = originalNode.GetPosition().position + new Vector2(20, 20);

            if (originalBaseNode.GetType() == typeof(CBTSystemActionNode))
            {
                CBTSystemActionNode originalActionNode = originalBaseNode as CBTSystemActionNode;
                CBTSystemActionNode newActionNode = CreateNode(typeof(CBTSystemActionNode), newPosition) as CBTSystemActionNode;

                newActionNode.ActionType = originalActionNode.ActionType;

                newActionNode.Draw();
                return newActionNode;
            }
            else if (originalBaseNode.GetType() == typeof(CBTSystemConditionNode))
            {
                CBTSystemConditionNode originalConditionNode = originalBaseNode as CBTSystemConditionNode;
                CBTSystemConditionNode newConditionNode = CreateNode(typeof(CBTSystemConditionNode), newPosition) as CBTSystemConditionNode;

                newConditionNode.ConditionEntries = originalConditionNode.ConditionEntries;
                newConditionNode.Connectors = originalConditionNode.Connectors;

                newConditionNode.Draw();
                return newConditionNode;
            }
            else
            {
                Debug.LogError($"Error in duplication: Duplication of node type {originalBaseNode.GetType()} is not supported!");
                return null;
            }
        }

        #endregion

        #region Starting Node

        public void SetStartingNode(CBTSystemNode newStartingNode)
        {
            // Don't do anything if the new starting node is the same as the current one (cause a stack overflow otherwise)
            if (startingNode == newStartingNode)
            {
                return;
            }

            if (startingNode != null)
            {
                startingNode.IsRootNode = false;
            }

            startingNode = newStartingNode;

            if (startingNode != null)
            {
                startingNode.IsRootNode = true;
            }
        }

        #endregion

    }
}
