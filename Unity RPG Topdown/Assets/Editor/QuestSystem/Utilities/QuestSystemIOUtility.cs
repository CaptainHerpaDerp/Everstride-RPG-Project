using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace QuestSystem.Utilities
{
    using GraphSystem.Base.Utilities;
    using Data.Save;
    using Elements;
    using ScriptableObjects;
    using Windows;
    using GraphSystem.Base;
    using GraphSystem.Base.ScriptableObjects;
    using GraphSystem.Base.Data.Save;
    using Newtonsoft.Json;
    using UnityEngine.UIElements;
    using Enumerations;
    public class QuestSystemIOUtility
    {
        private static QuestSystemGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<BaseNode> nodes;
        private static List<BaseGroup> groups;

        private static Dictionary<string, QuestSystemQuestGroupSO> createdQuestGroups;
        private static Dictionary<string, QuestSystemQuestSO> createdQuests;

        private static Dictionary<string, BaseGroup> loadedGroups;

        public static void Initialize(QuestSystemGraphView qsGraphView, string graphName)
        {
            graphView = qsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/QuestSystem/Quests/{graphName}";

            nodes = new();
            groups = new();

            createdQuestGroups = new();
            createdQuests = new();

            loadedGroups = new();
        }

        #region Saving Methods

        public static void Save()
        {
            CreateDefaultFolders();

            GetElementsFromGraphView();

            QuestSystemGraphSaveDataSO graphData = IOUtilities.CreateAsset<QuestSystemGraphSaveDataSO>("Assets/Editor/QuestSystem/Graphs", $"{graphFileName}");

            graphData.Initialize(graphFileName);

            QuestSystemQuestContainerSO questContainer = IOUtilities.CreateAsset<QuestSystemQuestContainerSO>(containerFolderPath, graphFileName);

            questContainer.Initialize(graphFileName);

            SaveGroups(graphData, questContainer);
            SaveNodes(graphData, questContainer);
            SaveNodesConnections(graphData);

            IOUtilities.SaveAsset(graphData);
            IOUtilities.SaveAsset(questContainer);
        }


        private static void SaveGroups(QuestSystemGraphSaveDataSO graphData, QuestSystemQuestContainerSO questContainer)
        {
            List<string> groupNames = new();

            foreach (BaseGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, questContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(BaseGroup group, QuestSystemGraphSaveDataSO graphData)
        {
            BaseGroupSaveData groupData = new()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(BaseGroup group, QuestSystemQuestContainerSO questContainer)
        {
            string groupName = group.title;

            // Create folders for the group
            IOUtilities.CreateFolder($"{containerFolderPath}/Groups", groupName);
            IOUtilities.CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Quests");

            QuestSystemQuestGroupSO questGroup = IOUtilities.CreateAsset<QuestSystemQuestGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            questGroup.Initialize(groupName);

            createdQuestGroups.Add(group.ID, questGroup);

            questContainer.QuestGroups.Add(questGroup, new List<BaseNodeSO>());

            IOUtilities.SaveAsset(questGroup);

            Debug.Log($"Creating group folder: {containerFolderPath}/Groups/{groupName}/Quests");
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, QuestSystemGraphSaveDataSO graphData)
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

        //private static void SaveNodes(QuestSystemGraphSaveDataSO graphData, QuestSystemQuestContainerSO questContainer)
        //{
        //    SerializableDictionary<string, List<string>> groupedNodeNames = new();
        //    List<string> ungroupedNodeNames = new List<string>();

        //    foreach (QuestSystemNode node in nodes)
        //    {
        //        Debug.Log($"Saving Quest: {node.QuestName} with {node.Conditions.Count} conditions");

        //        if (node.Conditions.Count > 0)
        //        {
        //            foreach (QuestSystemConditionSaveData condition in node.Conditions)
        //            {
        //                Debug.Log($"Condition: {condition.Condition} - Value: {condition.ConditionValue}");
        //            }
        //        }

        //        SaveNodeToGraph(node, graphData);
        //        SaveNodeToScriptableObject(node, questContainer);

        //        if (node.Group != null)
        //        {
        //            groupedNodeNames.AddItem(node.Group.title, node.QuestName);

        //            continue;
        //        }

        //        ungroupedNodeNames.Add(node.QuestName);
        //    }

        //    UpdateQuestsConditionsConnections();

        //    UpdateOldGroupedNodes(groupedNodeNames, graphData);
        //    UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        //}

        private static void SaveNodes(QuestSystemGraphSaveDataSO graphData, QuestSystemQuestContainerSO questContainer)
        {
            List<BaseNodeSaveData> nodeSaveDataList = new();
            SerializableDictionary<string, List<string>> groupedNodeNames = new();
            List<string> ungroupedNodeNames = new();

            foreach (BaseNode node in nodes)
            {
                SaveNodeToScriptableObject(node, questContainer);

                if (node is QuestSystemNode questNode)
                {
                    nodeSaveDataList.Add(new QuestSystemNodeSaveData
                    {
                        ID = questNode.ID,
                        Description = questNode.QuestDescription,
                        Position = questNode.GetPosition().position,
                        Conditions = questNode.Conditions,
                        Triggers = questNode.Triggers,
                        GroupID = questNode.Group?.ID,
                        NextNodeID = questNode.NextNodeID,
                        NodeType = questNode.NodeType,
                        NextGroupName = questNode.NewGroupNameField
                    });
                }
                else
                {
                    Debug.LogError($"Unsupported node type: {node.GetType()}");
                }

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

            UpdateQuestsConditionsConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        //private static void SaveNodeToGraph(QuestSystemNode node, QuestSystemGraphSaveDataSO graphData)
        //{
        //    List<QuestSystemConditionSaveData> conditions = CloneNodeConditions(node.Conditions);
        //    List<QuestSystemTriggerSaveData> triggers = CloneNodeTriggers(node.Triggers);

        //    if (node.ID == null)
        //    {
        //        Debug.LogError("The node ID is null.");
        //    }

        //    QuestSystemNodeSaveData nodeData = new()
        //    {
        //        NodeID = node.ID,
        //        Name = node.QuestName,
        //        Description = node.QuestDescription,

        //        Conditions = node.Conditions.Select(condition => new QuestSystemConditionSaveData
        //        {
        //            Condition = condition.Condition,
        //            ConditionValue = condition.ConditionValue
        //        }).ToList(),

        //        Triggers = node.Triggers.Select(trigger => new QuestSystemTriggerSaveData
        //        {
        //            ConditionKey = trigger.ConditionKey,
        //            ConditionValue = trigger.ConditionValue
        //        }).ToList(),

        //        NextNodeID = node.NextNodeID,
        //        GroupID = node.Group?.ID,
        //        Position = node.GetPosition().position,
        //        NodeType = node.NodeType,
        //        NextGroupName = node.NewGroupNameField
        //    };

        //    graphData.Nodes.Add(nodeData);
        //}

        private static void SaveNodeToScriptableObject(BaseNode node, QuestSystemQuestContainerSO questContainer)
        {
            if (node is not QuestSystemNode questNode)
            {
                Debug.Log($"Saving node of type: {node.GetType()} is not supported!");
                return;
            }

            QuestSystemQuestSO quest;

            if (questNode.Group != null)
            {
                quest = IOUtilities.CreateAsset<QuestSystemQuestSO>($"{containerFolderPath}/Groups/{questNode.Group.title}/Quests", questNode.ID);
                questContainer.QuestGroups.AddItem(createdQuestGroups[questNode.Group.ID], quest);
            }
            else
            {
                quest = IOUtilities.CreateAsset<QuestSystemQuestSO>($"{containerFolderPath}/Global/Quests", questNode.ID);
                questContainer.UngroupedQuests.Add(quest);
            }

            quest.Initialize(
                questNode.ID,
                questNode.QuestDescription,
                questNode.NextNodeID,
                ConvertNodeConditionsToQuestConditions(questNode.Conditions),
                ConvertNodeTriggersToQuestTriggers(questNode.Triggers),
                questNode.NodeType,
                questNode.IsStartingNode(),
                questNode.NewGroupNameField
            );

            createdQuests.Add(questNode.ID, quest);

            IOUtilities.SaveAsset(quest);
        }

        private static void SaveNodesConnections(QuestSystemGraphSaveDataSO graphData)
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

        private static void UpdateQuestsConditionsConnections()
        {
            foreach (QuestSystemNode node in nodes)
            {
                QuestSystemQuestSO quest = createdQuests[node.ID];

                for (int conditionIndex = 0; conditionIndex < node.Conditions.Count; ++conditionIndex)
                {
                    QuestSystemConditionSaveData nodecondition = node.Conditions[conditionIndex];

                    if (string.IsNullOrEmpty(nodecondition.ConditionValue))
                    {
                        continue;
                    }

                    IOUtilities.SaveAsset(quest);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, QuestSystemGraphSaveDataSO graphData)
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
                        IOUtilities.RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Quests", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, QuestSystemGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    IOUtilities.RemoveAsset($"{containerFolderPath}/Global/Quests", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        #endregion

        #region Load Methods

        public static void Load()
        {
            QuestSystemGraphSaveDataSO graphData = IOUtilities.LoadAsset<QuestSystemGraphSaveDataSO>("Assets/Editor/QuestSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/QuestSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return; 
            }

            //QuestSystemEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData);
            LoadNodesConnections(graphData);
        }

        private static void LoadGroups(List<BaseGroupSaveData> groups)
        {
            foreach (BaseGroupSaveData groupData in groups)
            {
                BaseGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(QuestSystemGraphSaveDataSO graphData)
        {
            if (string.IsNullOrEmpty(graphData.SerializedNodes)) return;

            var nodeSaveDataList = JsonConvert.DeserializeObject<List<BaseNodeSaveData>>(graphData.SerializedNodes, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            foreach (var nodeData in nodeSaveDataList)
            {
                if (nodeData is QuestSystemNodeSaveData questNodeData)
                {
                    var node = graphView.CreateNode((int)questNodeData.NodeType, questNodeData.Position, false) as QuestSystemNode;

                    node.ID = questNodeData.ID;
                    node.QuestDescription = questNodeData.Description;
                    node.Conditions = questNodeData.Conditions;
                    node.Triggers = questNodeData.Triggers;
                    node.NextNodeID = questNodeData.NextNodeID;

                    graphView.AddElement(node);

                    if (questNodeData.GroupID != null && loadedGroups.TryGetValue(questNodeData.GroupID, out var group))
                    {
                        group.AddElement(node);
                    }

                    node.Draw();
                }
            }
        }

        private static void LoadNodesConnections(QuestSystemGraphSaveDataSO graphData)
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

        private static void CreateDefaultFolders()
        {
            IOUtilities.CreateFolder("Assets/Editor/QuestSystem", "Graphs");

            IOUtilities.CreateFolder("Assets", "QuestSystem");
            IOUtilities.CreateFolder("Assets/QuestSystem", "Quests");

            IOUtilities.CreateFolder("Assets/QuestSystem/Quests", graphFileName);
            IOUtilities.CreateFolder(containerFolderPath, "Global");
            IOUtilities.CreateFolder(containerFolderPath, "Groups");
            IOUtilities.CreateFolder($"{containerFolderPath}/Global", "Quests");
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

        private static List<QuestSystemConditionSaveData> CloneNodeConditions(List<QuestSystemConditionSaveData> nodeConditions)
        {
            List<QuestSystemConditionSaveData> conditions = new List<QuestSystemConditionSaveData>();

            foreach (QuestSystemConditionSaveData condition in nodeConditions)
            {
                QuestSystemConditionSaveData conditionData = new QuestSystemConditionSaveData()
                {
                    Condition = condition.Condition,
                    ConditionValue = condition.ConditionValue
                };

                conditions.Add(conditionData);
            }

            return conditions;
        }

        public static List<QuestSystemTriggerSaveData> CloneNodeTriggers(List<QuestSystemTriggerSaveData> nodeTriggers)
        {
            List<QuestSystemTriggerSaveData> conditions = new();

            foreach (QuestSystemTriggerSaveData trigger in nodeTriggers)
            {
                QuestSystemTriggerSaveData triggerData = new()
                {
                    ConditionKey = trigger.ConditionKey,
                    ConditionValue = trigger.ConditionValue
                };

                conditions.Add(triggerData);
            }

            return conditions;
        }

        #region Utility Methods

        private static List<QuestSystemQuestConditionData> ConvertNodeConditionsToQuestConditions(List<QuestSystemConditionSaveData> nodeconditions)
        {
            List<QuestSystemQuestConditionData> questconditions = new List<QuestSystemQuestConditionData>();

            foreach (QuestSystemConditionSaveData nodecondition in nodeconditions)
            {
                QuestSystemQuestConditionData conditionData = new QuestSystemQuestConditionData()
                {
                    ConditionType = nodecondition.Condition,
                    ConditionValue = nodecondition.ConditionValue
                };

                questconditions.Add(conditionData);
            }

            return questconditions;
        }

        public static List<QuestSystemQuestTriggerData> ConvertNodeTriggersToQuestTriggers(List<QuestSystemTriggerSaveData> nodeTriggers)
        {
            List<QuestSystemQuestTriggerData> conditions = new();

            foreach (QuestSystemTriggerSaveData trigger in nodeTriggers)
            {
                QuestSystemQuestTriggerData triggerData = new()
                {
                    ConditionKey = trigger.ConditionKey,
                    ConditionValue = trigger.ConditionValue
                };

                conditions.Add(triggerData);
            }

            return conditions;
        }


        #endregion
    }
}
