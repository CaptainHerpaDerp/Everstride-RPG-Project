using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Newtonsoft.Json;

namespace CBTSystem.Utilities
{
    using GraphSystem.Base.Data.Save;
    using GraphSystem.Base.ScriptableObjects;
    using GraphSystem.Base;
    using GraphSystem.Base.Utilities;
    using Base;
    using CBTSystem.Data.Save.Nodes;
    using CBTSystem.Elements.Nodes;
    using UnityEngine.UIElements;
    using System.IO;
    using CBTSystem.ScriptableObjects.Nodes;
    using CBTSystem.ScriptableObjects;

    /// <summary>
    /// Utility class for handling IO operations for the Combat Behaviour Tree System.
    /// </summary>
    public static class CBTSystemIOUtility
    {
        private static CBTGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<BaseNode> nodes;
        private static List<BaseGroup> groups;

        private static Dictionary<string, CBTSystemGroupSO> createdGroups;
        private static Dictionary<string, CBTSystemNodeSO> createdNode;

        private static Dictionary<string, BaseNodeSO> createdNodes;

        private static Dictionary<string, BaseGroup> loadedGroups;

        public static void Initialize(CBTGraphView cbtGraphView, string graphName)
        {
            graphView = cbtGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/CBTSystem/CBTConfigs/{graphName}";

            nodes = new();
            groups = new();

            createdGroups = new();
            createdNodes = new();

            loadedGroups = new();
        }

        #region Save Methods

        public static void Save()
        {
            Debug.Log("Saving");

            CreateDefaultFolders();

            GetElementsFromGraphView();

            BaseGraphSaveDataSO graphData = IOUtilities.CreateAsset<BaseGraphSaveDataSO>("Assets/Editor/CBTSystem/Graphs", $"{graphFileName}");

            graphData.Initialize(graphFileName);

            CBTSystemContainerSO containerSO = IOUtilities.CreateAsset<CBTSystemContainerSO>(containerFolderPath, graphFileName);

            containerSO.Initialize(graphFileName);

            SaveGroups(graphData, containerSO);
            SaveNodes(graphData, containerSO);
            SaveNodesConnections(graphData);

            IOUtilities.SaveAsset(graphData);
            IOUtilities.SaveAsset(containerSO);
        }

        #endregion

        #region Load Methods

        public static void Load()
        {
            BaseGraphSaveDataSO graphData = IOUtilities.LoadAsset<BaseGraphSaveDataSO>("Assets/Editor/CBTSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/CombatBehaviourTreeSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return;
            }

            LoadGroups(graphData.Groups);
            LoadNodes(graphData);
            LoadNodesConnections(graphData);
        }


        #endregion

        #region Group Saving

        private static void SaveGroups(BaseGraphSaveDataSO graphData, CBTSystemContainerSO container)
        {
            List<string> groupNames = new();

            foreach (BaseGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, container);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, BaseGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    IOUtilities.RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveGroupToGraph(BaseGroup group, BaseGraphSaveDataSO graphData)
        {
            BaseGroupSaveData groupData = new()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(BaseGroup group, CBTSystemContainerSO container)
        {
            string groupName = group.title;

            // Create folders for the group
            IOUtilities.CreateFolder($"{containerFolderPath}/Groups", groupName);
            IOUtilities.CreateFolder($"{containerFolderPath}/Groups/{groupName}", "CBTConfigs");

            CBTSystemGroupSO groupSO = IOUtilities.CreateAsset<CBTSystemGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            groupSO.Initialize(groupName);

            createdGroups.Add(group.ID, groupSO);

            container.Groups.Add(groupSO, new List<CBTSystemNodeSO>());

            IOUtilities.SaveAsset(groupSO);

            Debug.Log($"Creating group folder: {containerFolderPath}/Groups/{groupName}/CBTConfigs");
        }

        #endregion

        #region Node Saving 

        private static void SaveNodes(BaseGraphSaveDataSO graphData, CBTSystemContainerSO nodeContainer)
        {
            List<BaseNodeSaveData> nodeSaveDataList = new();
            SerializableDictionary<string, List<string>> groupedNodeNames = new();
            List<string> ungroupedNodeNames = new();

            foreach (BaseNode node in nodes)
            {
                SaveNodeToScriptableObject(node, nodeContainer);
                SaveNodeData(node, ref nodeSaveDataList);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.ID);
                }
                else
                {
                    ungroupedNodeNames.Add(node.ID);
                }
            }

            // Serialize node save data to JSON
            string json = JsonConvert.SerializeObject(nodeSaveDataList, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            graphData.SerializedNodes = json;

            UpdateNodeConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeData(BaseNode node, ref List<BaseNodeSaveData> nodeSaveDataList)
        {
            if (node is CBTSystemActionNode actionNode)
            {
                nodeSaveDataList.Add(GetActionSaveDataFromNode(actionNode));
            }
            else if (node is CBTSystemConditionNode conditionNode)
            {
                nodeSaveDataList.Add(GetConditionSaveDataFromNode(conditionNode));
            }
            else
            {
                Debug.LogError($"Saving node of type: {node.GetType()} is not supported!");
            }
        }

        private static void SaveNodeToScriptableObject(BaseNode node, CBTSystemContainerSO containerSO)
        {
            if (node is CBTSystemActionNode actionNode)
            {
                SaveActionNodeSO(actionNode, containerSO);
            }
            else if (node is CBTSystemConditionNode conditionNode)
            {
                SaveConditionNodeSO(conditionNode, containerSO);
            }
            else
            {
                Debug.LogError($"Saving node of type: {node.GetType()} is not supported!");
            }
        }

        private static void SaveNodesConnections(BaseGraphSaveDataSO graphData)
        {
            graphData.NodeLinks.Clear();

            foreach (Edge edge in graphView.edges.ToList())
            {
                BaseNode outputNode = edge.output.node as BaseNode;
                BaseNode inputNode = edge.input.node as BaseNode;

                if (outputNode == null || inputNode == null) continue;

                NodeLinkData linkData = new()
                {
                    BaseNodeID = outputNode.ID,
                    TargetNodeID = inputNode.ID,
                    OutputPortName = edge.output.portName,
                    InputPortName = edge.input.portName
                };

                graphData.NodeLinks.Add(linkData);
            }
        }

        private static void UpdateNodeConnections()
        {
            //foreach (CBTSystemNode node in nodes)
            //{
            //    BaseNodeSO quest = createdNodes[node.ID];

            //    for (int conditionIndex = 0; conditionIndex < node.Conditions.Count; ++conditionIndex)
            //    {
            //        QuestSystemConditionSaveData nodecondition = node.Conditions[conditionIndex];

            //        if (string.IsNullOrEmpty(nodecondition.ConditionValue))
            //        {
            //            continue;
            //        }

            //        IOUtilities.SaveAsset(quest);
            //    }
            //}
        }

        #endregion

        #region Nose Data Save Types

        public static CBTSystemActionNodeSaveData GetActionSaveDataFromNode(CBTSystemActionNode actionNode)
        {
           return new CBTSystemActionNodeSaveData()
           {
                ID = actionNode.ID,
                Position = actionNode.GetPosition().position,
                GroupID = actionNode.Group?.ID,
                NextNodeIDs = actionNode.NextNodeIDs,
                ActionType = actionNode.ActionType,
                IsRootNode = actionNode.IsRootNode
           };
        }

        public static CBTSystemConditionNodeSaveData GetConditionSaveDataFromNode(CBTSystemConditionNode conditionNode)
        {
            return new CBTSystemConditionNodeSaveData()
            {
                ID = conditionNode.ID,
                Position = conditionNode.GetPosition().position,
                GroupID = conditionNode.Group?.ID,
                NextNodeIDs = conditionNode.NextNodeIDs,

                ConditionEntries = conditionNode.ConditionEntries,
                Connectors = conditionNode.Connectors,

                IsRootNode = conditionNode.IsRootNode
            };
        }

        #endregion

        #region Node SO Save Types

        public static void SaveActionNodeSO(CBTSystemActionNode node, CBTSystemContainerSO containerSO)
        {
            string nodePath = node.Group != null
                ? $"{containerFolderPath}/Groups/{node.Group.title}/Nodes"
                : $"{containerFolderPath}/Global/Nodes";

            // Ensure the directory exists before creating the asset
            if (!Directory.Exists(nodePath))
            {
                Directory.CreateDirectory(nodePath);
            }

            var nodeSO = IOUtilities.CreateAsset<CBTSystemActionNodeSO>(nodePath, node.ID);

            Debug.Log($"Saving action node with {node.NextNodeIDs.Count} connections");

            nodeSO.Initialize(
                   node.ID,
                   node.NextNodeIDs,
                   node.IsRootNode,
                   node.ActionType
            );

            if (node.Group != null)
            {
                containerSO.Groups.AddItem(createdGroups[node.Group.ID], nodeSO);
            }
            else
            {
                containerSO.UngroupedNodes.Add(nodeSO);
            }

            createdNodes.Add(node.ID, nodeSO);
            IOUtilities.SaveAsset(nodeSO);
        }

        public static void SaveConditionNodeSO(CBTSystemConditionNode conditionNode, CBTSystemContainerSO containerSO)
        {
            string nodePath = conditionNode.Group != null
                ? $"{containerFolderPath}/Groups/{conditionNode.Group.title}/Nodes"
                : $"{containerFolderPath}/Global/Nodes";

            // Ensure the directory exists before creating the asset
            if (!Directory.Exists(nodePath))
            {
                Directory.CreateDirectory(nodePath);
            }

            var nodeSO = IOUtilities.CreateAsset<CBTSystemConditionNodeSO>(nodePath, conditionNode.ID);

            nodeSO.Initialize(
                   conditionNode.ID,
                   conditionNode.NextNodeIDs,
                   conditionNode.IsRootNode,
                   conditionNode.ConditionEntries,
                   conditionNode.Connectors
            );

            if (conditionNode.Group != null)
            {
                containerSO.Groups.AddItem(createdGroups[conditionNode.Group.ID], nodeSO);
            }
            else
            {
                containerSO.UngroupedNodes.Add(nodeSO);
            }

            createdNodes.Add(conditionNode.ID, nodeSO);
            IOUtilities.SaveAsset(nodeSO);
        }


        #endregion

        #region Group Loading

        private static void LoadGroups(List<BaseGroupSaveData> groups)
        {
            foreach (BaseGroupSaveData groupData in groups)
            {
                BaseGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        #endregion

        #region Node Loading 

        private static void LoadNodes(BaseGraphSaveDataSO graphData)
        {
            if (string.IsNullOrEmpty(graphData.SerializedNodes)) return;

            var nodeSaveDataList = JsonConvert.DeserializeObject<List<BaseNodeSaveData>>(graphData.SerializedNodes, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            foreach (var nodeData in nodeSaveDataList)
            {
                if (nodeData is CBTSystemActionNodeSaveData actionNodeData)
                {
                    CBTSystemActionNode newActionNode = LoadActionNode(actionNodeData);

                    graphView.AddElement(newActionNode);

                    if (actionNodeData.GroupID != null && loadedGroups.TryGetValue(actionNodeData.GroupID, out var group))
                    {
                        group.AddElement(newActionNode);
                    }

                    newActionNode.Draw();
                }
                else if (nodeData is CBTSystemConditionNodeSaveData conditionNodeData)
                {
                    CBTSystemConditionNode newConditionNode = LoadConditionNode(conditionNodeData);

                    graphView.AddElement(newConditionNode);

                    if (conditionNodeData.GroupID != null && loadedGroups.TryGetValue(conditionNodeData.GroupID, out var group))
                    {
                        group.AddElement(newConditionNode);
                    }

                    newConditionNode.Draw();
                }
                else
                {
                    Debug.LogError($"Error: No load logic found for the node of type {nodeData.GetType()}");
                }
            }
        }

        private static void LoadNodesConnections(BaseGraphSaveDataSO graphData)
        {
            foreach (NodeLinkData linkData in graphData.NodeLinks)
            {
                // Find the source and target nodes directly in the graph
                BaseNode sourceNode = graphView.nodes.ToList()
                    .OfType<BaseNode>()
                    .FirstOrDefault(node => node.ID == linkData.BaseNodeID);

                BaseNode targetNode = graphView.nodes.ToList()
                    .OfType<BaseNode>()
                    .FirstOrDefault(node => node.ID == linkData.TargetNodeID);

                if (sourceNode == null)
                {
                    Debug.LogWarning($"Source node with ID {linkData.BaseNodeID} not found.");
                    continue;
                }

                if (targetNode == null)
                {
                    Debug.LogWarning($"Target node with ID {linkData.TargetNodeID} not found.");
                    continue;
                }

                // Create a connection between the nodes
                Port outputPort = sourceNode.outputContainer.Query<Port>()
                    .Where(port => port.portName == linkData.OutputPortName)
                    .First();
                Port inputPort = targetNode.inputContainer.Query<Port>()
                    .Where(port => port.portName == linkData.InputPortName)
                    .First();

                if (outputPort != null && inputPort != null)
                {
                    Edge edge = outputPort.ConnectTo(inputPort);
                    graphView.AddElement(edge);
                }
                else
                {
                    Debug.LogWarning($"Failed to connect nodes {sourceNode.ID} -> {targetNode.ID}: Ports not found.");
                }
            }
        }

        #endregion

        #region Node Load Types

        /// <summary>
        /// Creates and returns a new action node loaded from the information from the provided action node save data object
        /// </summary>
        /// <param name="actionNodeSaveData"></param>
        /// <returns></returns>
        public static CBTSystemActionNode LoadActionNode(CBTSystemActionNodeSaveData actionNodeSaveData)
        {
            CBTSystemActionNode actionNode = graphView.CreateNode(typeof(CBTSystemActionNode), actionNodeSaveData.Position, false) as CBTSystemActionNode;

            actionNode.ID = actionNodeSaveData.ID;
            actionNode.NextNodeIDs = actionNodeSaveData.NextNodeIDs;
            actionNode.IsRootNode = actionNodeSaveData.IsRootNode;

            Debug.Log($"Loadng node of type {actionNodeSaveData.ActionType}");

            actionNode.ActionType = actionNodeSaveData.ActionType;

            return actionNode;
        }

        /// <summary>
        /// Creates and returns a new condition node loaded from the information from the provided condition node save data object
        /// </summary>
        /// <param name="conditionNodeSaveData"></param>
        /// <returns></returns>
        public static CBTSystemConditionNode LoadConditionNode(CBTSystemConditionNodeSaveData conditionNodeSaveData)
        {
            CBTSystemConditionNode conditionNode = graphView.CreateNode(typeof(CBTSystemConditionNode), conditionNodeSaveData.Position, false) as CBTSystemConditionNode;

            conditionNode.ID = conditionNodeSaveData.ID;
            conditionNode.NextNodeIDs = conditionNodeSaveData.NextNodeIDs;
            conditionNode.ConditionEntries = conditionNodeSaveData.ConditionEntries;
            conditionNode.Connectors = conditionNodeSaveData.Connectors;
            conditionNode.IsRootNode = conditionNodeSaveData.IsRootNode;

            return conditionNode;
        }

        #endregion

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, BaseGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNodeNames != null && graphData.OldGroupNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        IOUtilities.RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Nodes", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, BaseGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    IOUtilities.RemoveAsset($"{containerFolderPath}/Global/Nodes", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void CreateDefaultFolders()
        {
            IOUtilities.CreateFolder("Assets/Editor/CBTSystem", "Graphs");

            IOUtilities.CreateFolder("Assets", "CBTSystem");
            IOUtilities.CreateFolder("Assets/CBTSystem", "CBTConfigs");

            IOUtilities.CreateFolder("Assets/CBTSystem/CBTConfigs", graphFileName);
            IOUtilities.CreateFolder(containerFolderPath, "Global");
            IOUtilities.CreateFolder(containerFolderPath, "Groups");
            IOUtilities.CreateFolder($"{containerFolderPath}/Global", "Nodes");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(BaseGroup);

            if (graphView == null)
            {
                Debug.LogError("The graph view is not set.");
                return;
            }

            if (graphView.graphElements == null)
            {
                Debug.LogError("The graph view does not contain any elements.");
                return;
            }

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is BaseNode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    BaseGroup group = (BaseGroup)graphElement;

                    groups.Add(group);

                    return;
                }
            });
        }
    }
}

