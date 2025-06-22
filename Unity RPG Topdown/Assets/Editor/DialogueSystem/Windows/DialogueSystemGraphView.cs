using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace DialogueSystem.Windows
{
    using GraphSystem.Base.Windows;
    using Data.Save;
    using Elements;
    using Enumerations;
    using GraphSystem.Base;
    using GraphSystem.Base.ScriptableObjects;

    public class DialogueSystemGraphView : BaseGraphView
    {
        // OVerride the search window property to be of type DialogueSystemSearchWindow
        private DialogueSystemSearchWindow searchWindow;
        protected override BaseSearchWindow baseSearchWindow { get => searchWindow; set => searchWindow = value as DialogueSystemSearchWindow; }
        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemEditorWindow) : base(dialogueSystemEditorWindow)
        {
            ungroupedNodes = new();
            groups = new();
            groupedNodes = new();

            AddManipulators();
        }

        #region Manipulators

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(CreateNodeContextualMenu("Create Node (Single Choice)", DialogueSystemDiagType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Create Node (Multiple Choice)", DialogueSystemDiagType.MultipleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Create Condition Check Node", DialogueSystemDiagType.ConditionCheck));

            this.AddManipulator(CreateGroupContextualMenu());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            RegisterCallback<KeyDownEvent>(OnDuplicateKey);
            RegisterCallback<KeyDownEvent>(OnSaveKey);
            this.RegisterCallback<DragExitedEvent>(HandleDragAndDrop);
        }

        /// <summary>
        /// Adds a menu option to create a node at the mouse position.
        /// </summary>
        /// <returns></returns>
        private IManipulator CreateNodeContextualMenu(string actionTitle, DialogueSystemDiagType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, action: actionEvent => AddElement(CreateNode((int)dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }


        #endregion

        #region Elements Creation

        public override BaseNode CreateNode(int dialogueTypeIndex, Vector2 position, bool shouldDraw = true)
        {
            // Set the dialogue type based on the index
            DialogueSystemDiagType dialogueType = (DialogueSystemDiagType)dialogueTypeIndex;

            Type nodeType = Type.GetType($"DialogueSystem.Elements.DialogueSystem{dialogueType}Node");

            BaseNode node = Activator.CreateInstance(nodeType) as BaseNode;

            node.Initialize(this, position);

            //Debug.Log($"Creating node of type {dialogueType}");

            if (shouldDraw)
            {
                node.Draw();
            }

            AddUngroupedNodes(node);

            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is DialogueSystemGroup))
                {
                    continue;
                }

                DialogueSystemGroup group = selectedElement as DialogueSystemGroup;

                group.AddElement(node);
            }

            return node;
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
                        // We need to determine what type of node the input node is
                       // Debug.Log($"Input {edge.input.node.GetType()}");

                        //Debug.Log($"Output {edge.output.node.GetType()}");

                        // If the node we're connecting from is a dialogue node
                        if (IsDialogueNode(edge.output.node))
                        {
                            // Get the input node, since we can't connect to a condition check node, it will always be a dialogue node
                            DialogueSystemNode inputNode = edge.input.node as DialogueSystemNode;

                            // Get the choice data from the output port (the port that the dialogue choice is connected to)
                            DialogueSystemChoiceSaveData choiceData = edge.output.userData as DialogueSystemChoiceSaveData;

                            if (inputNode == null)
                            {
                                Debug.LogError("The input node is null");
                                continue;
                            }

                            if (choiceData == null)
                            {
                                Debug.LogError("Choice data is null");
                            }

                            // Set the next node ID to the ID of the input node
                            choiceData.NodeID = inputNode.ID;
                        }

                        // Otherwise we can safely assume that the node is a condition check node
                        else
                        {
                            //Debug.Log("Making a connection with the output node being a condition check node");

                            // Get the input node, it will always be a dialogue node
                            DialogueSystemNode inputNode = edge.input.node as DialogueSystemNode;

                            // Get the output node
                            DialogueSystemConditionCheckNode outputNode = edge.output.node as DialogueSystemConditionCheckNode;

                            // Get the connected node ID from the output port
                            string connectedNodeID = inputNode.ID;

                            if (inputNode == null)
                            {
                                Debug.LogError("Next node is null");
                                continue;
                            }

                            if (string.IsNullOrEmpty(connectedNodeID))
                            {
                                Debug.LogError("Connected node ID is null");
                            }

                            // Set the connected node ID to the ID of the input node
                            outputNode.ConnectedNodeID = connectedNodeID;
                        }
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

                        DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData)edge.output.userData;

                        if (choiceData == null)
                        {
                            BaseNode outputNode = (BaseNode)edge.output.node;
                            outputNode = null;
                        }
                        else
                        {
                            choiceData.NodeID = "";
                        }

                    }
                }

                return changes;
            };
        }

        #endregion

        #region Elements Addition

        protected override void AddSearchWindow()
        {
            if (searchWindow != null)
                return;

            searchWindow = ScriptableObject.CreateInstance<DialogueSystemSearchWindow>();

            searchWindow.Initialize(this);

            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }

        #endregion

        #region Utilities

        protected override BaseNode DuplicateNode(BaseNode originalBaseNode)
        {
            DialogueSystemNode originalNode = originalBaseNode as DialogueSystemNode;

            Vector2 newPosition = originalNode.GetPosition().position + new Vector2(20, 20);

            // Create a new node with the same properties as the original
            DialogueSystemNode newNode = CreateNode((int)originalNode.DialogueType, newPosition, shouldDraw: false) as DialogueSystemNode;

            if (newNode.DialogueType == DialogueSystemDiagType.MultipleChoice)
            {
                newNode.Choices = originalNode.Choices;
            }

            newNode.DisconnectAllPorts();

            newNode.Text = originalNode.Text;

            // Copy any other properties from the original node that you want to duplicate

            newNode.Draw(); // Draw the new node

            return newNode;
        }

        private static bool IsDialogueNode(Node node)
        {
            return node.GetType() == typeof(DialogueSystemNode) || node.GetType().IsSubclassOf(typeof(DialogueSystemNode));
        }

        #endregion
    }
}

