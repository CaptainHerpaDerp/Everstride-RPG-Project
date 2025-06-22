using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphSystem.Base
{
    using Windows;
    using System.Collections.Generic;
    using GraphSystem.Base.Utilities;
    using System;
    using GraphSystem.Base.Data.Error;
    using System.Linq;

    public abstract class BaseGraphView : GraphView
    {
        protected abstract BaseSearchWindow baseSearchWindow { get; set; }
        protected BaseGraphEditorWindow editorWindow;
        protected MiniMap minimap;

        protected SerializableDictionary<string, BaseGraphNodeErrorData> ungroupedNodes;
        protected SerializableDictionary<string, BaseGraphGroupErrorData> groups;
        protected SerializableDictionary<Group, SerializableDictionary<string, BaseGraphNodeErrorData>> groupedNodes;

        public ScriptableObject AssociatedObject { get; set; } // Use ScriptableObject as the associated object type


        protected int repeatedNames;

        public int RepeatedNames
        {
            get
            {
                return repeatedNames;
            }

            set
            {
                repeatedNames = value;

                if (repeatedNames == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (repeatedNames == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        public BaseGraphView(BaseGraphEditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;

            ungroupedNodes = new();
            groups = new();
            groupedNodes = new();

            AddGridBackground();
            AddSearchWindow();
            AddMinimap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMinimapStyles();
        }

        #region Contextual Menu

        /// <summary>
        /// Creates a contextual menu for creating groups.
        /// </summary>
        /// <returns></returns>
        protected IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", action: actionEvent => CreateGroup("New Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );
            return contextualMenuManipulator;
        }


        #endregion

        #region Overriden Methods

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        #endregion

        #region Elements Creation

        public abstract BaseNode CreateNode(int nodeTypeIndex, Vector2 position, bool shouldDraw = true);

        /// <summary>
        /// Creates a group at the mouse position.
        /// </summary>
        public BaseGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            BaseGroup group = new(title, localMousePosition);

            AddGroup(group);

            AddElement(group);

            foreach (var selectedElement in selection)
            {
                if (!(selectedElement is BaseNode))
                {
                    continue;
                }

                BaseNode node = selectedElement as BaseNode;

                group.AddElement(node);
            }

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            return group;
        }

        protected abstract void AddSearchWindow();

        protected void AddMinimap()
        {
            minimap = new()
            {
                anchored = true
            };

            minimap.SetPosition(new Rect(15, 50, 200, 180));

            Add(minimap);

            minimap.visible = false;
        }

        protected void AddMinimapStyles()
        {
            StyleColor backgroundColor = new(new Color32(23, 23, 30, 255));
            StyleColor borderColor = new(new Color32(51, 51, 51, 255));

            minimap.style.backgroundColor = backgroundColor;
            minimap.style.borderTopColor = borderColor;
            minimap.style.borderRightColor = borderColor;
            minimap.style.borderBottomColor = borderColor;
            minimap.style.borderLeftColor = borderColor;
        }

        protected void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DialogueSystemGraphViewStyles.uss",
                "DialogueSystem/DialogueSystemNodeStyles.uss"
                );
        }

        protected void AddGridBackground()
        {
            // Creates a grid background for the graph view.
            GridBackground gridBackground = new();

            // Sets the size of the grid background to the size of the graph view.
            gridBackground.StretchToParentSize();

            // Sets the position of the grid background to the top left corner of the graph view.
            Insert(0, gridBackground);
        }

        #endregion

        #region Utilities

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            repeatedNames = 0;
        }

        public void ToggleMinimap()
        {
            minimap.visible = !minimap.visible;
        }

        public void DuplicateSelectedNodes()
        {
            List<BaseNode> selectedNodes = selection.OfType<BaseNode>().ToList();

            foreach (var node in selectedNodes)
            {
                BaseNode newNode = DuplicateNode(node) as BaseNode;
                AddElement(newNode);
            }
        }

        protected abstract BaseNode DuplicateNode(BaseNode originalNode);

        #endregion

        #region Keyboard Events
        protected void OnDuplicateKey(KeyDownEvent evt)
        {
            // Check if Ctrl + D is pressed
            if (evt.keyCode == KeyCode.D && evt.modifiers == EventModifiers.Control)
            {
                DuplicateSelectedNodes();
                evt.StopPropagation();
            }
        }
        protected void OnSaveKey(KeyDownEvent evt)
        {
            // Check if Ctrl + S is pressed
            if (evt.keyCode == KeyCode.S && evt.modifiers == EventModifiers.Control)
            {
                editorWindow.Save();
                evt.StopPropagation();
            }
        }

        #endregion

        #region Mouse Events

        // Handle Drag and Drop events
        protected void HandleDragAndDrop(DragExitedEvent evt)
        {
            // Find the node under the mouse
            BaseNode node = GetNodeUnderMouse(evt.mousePosition);

            if (node != null)
            {
                node.HandleDragAndDrop(evt);
            }
        }

        protected BaseNode GetNodeUnderMouse(Vector2 mousePosition)
        {
            // Iterate through the elements under the mouse position
            foreach (var selectedElement in selection)
            {
                if (selectedElement is BaseNode node)
                {
                    return node;
                }
            }

            return null; // No node found under the mouse
        }

        #endregion

        #region Callbacks

        protected abstract void OnGraphViewChanged();

        protected void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(Group);
                Type edgeType = typeof(Edge);

                List<Edge> edgesToDelete = new();
                List<BaseGroup> groupsToDelete = new();
                List<BaseNode> nodesToDelete = new();

                foreach (var element in selection)
                {
                    Debug.Log($"Element type: {element.GetType()}");

                    if (element is BaseNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = element as Edge;
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    bool derivesFromBaseGroup = element.GetType().IsSubclassOf(typeof(BaseGroup));

                    if (element is BaseGroup || derivesFromBaseGroup)
                    {
                        //Debug.Log($"Detected group for deletion: {((BaseGroup)element).title}");
                        groupsToDelete.Add(element as BaseGroup);
                    }
                }

                foreach (var groupToDelete in groupsToDelete)
                {
                    //Debug.Log($"Deleting group: {groupToDelete.title}");

                    List<BaseNode> groupNodes = new();

                    foreach (GraphElement groupElement in groupToDelete.containedElements)
                    {
                        if (groupElement is BaseNode node)
                        {
                            groupNodes.Add(node);
                        }
                    }

                    groupToDelete.RemoveElements(groupNodes);

                    RemoveGroup(groupToDelete);

                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (BaseNode removalNode in nodesToDelete)
                {
                    removalNode.Group?.RemoveElement(removalNode);
                    RemoveUngroupedNode(removalNode);
                    removalNode.DisconnectAllPorts();
                    RemoveElement(removalNode);
                }
            };
        }

        protected void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (var element in elements)
                {
                    if (!(element is BaseNode))
                    {
                        continue;
                    }

                    BaseGroup nodeGroup = group as BaseGroup;
                    BaseNode node = element as BaseNode;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        protected void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (var element in elements)
                {
                    if (!(element is BaseNode))
                    {
                        continue;
                    }

                    BaseNode node = element as BaseNode;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNodes(node);
                }
            };
        }

        protected void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                BaseGroup dialogueSystemGroup = group as BaseGroup;

                dialogueSystemGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dialogueSystemGroup.title))
                {
                    if (!string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        ++RepeatedNames;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        --RepeatedNames;
                    }
                }

                RemoveGroup(dialogueSystemGroup);

                dialogueSystemGroup.OldTitle = dialogueSystemGroup.title;

                AddGroup(dialogueSystemGroup);
            };
        }

        #endregion

        #region Group Methods

        protected void AddGroup(BaseGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                BaseGraphGroupErrorData groupErrorData = new();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<BaseGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;

            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++RepeatedNames;

                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(BaseGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            if (!groups.ContainsKey(oldGroupName))
            {
                Debug.LogWarning($"Group '{oldGroupName}' not found in groups dictionary.");
                return;
            }

            List<BaseGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --RepeatedNames;
                groupsList[0].ResetStyle();
                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }

            //Debug.Log($"Group '{oldGroupName}' removed successfully.");
        }


        public void RemoveGroupedNode(BaseNode node, Group group)
        {
            string nodeName = node.ID.ToLower();

            node.Group = null;

            List<BaseNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --RepeatedNames;

                groupedNodesList[0].ResetStyle();
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        public void AddGroupedNode(BaseNode node, BaseGroup group)
        {
            string nodeName = node.ID.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, BaseGraphNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                BaseGraphNodeErrorData nodeErrorData = new();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<BaseNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++RepeatedNames;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(BaseNode node)
        {
            if (ungroupedNodes == null)
            {
                Debug.LogError("Ungrouped nodes is null");
                return;
            }

            if (node == null)
            {
                Debug.LogError("Node is null");
                return;
            }

            string nodeName = node.ID.ToLower();

            if (string.IsNullOrEmpty(nodeName))
            {
                Debug.LogError("Node name is null or empty");
                return;
            }

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                //Debug.LogError("Node not found in ungrouped nodes");
                return;
            }

            ungroupedNodes[nodeName].Nodes.Remove(node);

            node.ResetStyle();

            if (ungroupedNodes[nodeName].Nodes.Count == 1)
            {
                --RepeatedNames;

                ungroupedNodes[nodeName].Nodes[0].ResetStyle();

                return;
            }

            if (ungroupedNodes[nodeName].Nodes.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        public void AddUngroupedNodes(BaseNode baseNode)
        {
            BaseNode node = baseNode as BaseNode;

            string nodeName = node.ID.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                BaseGraphNodeErrorData nodeErrorData = new();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            List<BaseNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++RepeatedNames;

                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupNode(BaseNode node, BaseGroup group)
        {
            string nodeName = node.ID.ToLower();

            node.Group = null;

            List<BaseNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --RepeatedNames;

                groupedNodesList[0].ResetStyle();
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }


        #endregion
    }
}
