using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Newtonsoft.Json;

namespace DialogueSystem.Utilities
{
    using GraphSystem.Base.Data.Save;
    using GraphSystem.Base.ScriptableObjects;
    using Data;
    using Data.Save;
    using DialogueSystem.Enumerations;
    using Elements;
    using GraphSystem.Base;
    using ScriptableObjects;
    using UnityEngine.UIElements;
    using Windows;
    using GraphSystem.Base.Utilities;

    public static class DialogueSystemIOUtility
    {
        private static DialogueSystemGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<BaseNode> nodes;
        private static List<BaseGroup> groups;

        private static Dictionary<string, DialogueSystemDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, BaseNodeSO> createdNodes;

        private static Dictionary<string, BaseGroup> loadedGroups;

        public static void Initialize(DialogueSystemGraphView dsGraphView, string graphName)
        {
            graphView = dsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphName}";

            nodes = new();
            groups = new();

            createdDialogueGroups = new Dictionary<string, DialogueSystemDialogueGroupSO>();
            createdNodes = new();

            loadedGroups = new();
            // loadedNodes = new();
        }

        #region Saving Methods

        public static void Save()
        {
            CreateDefaultFolders();

            GetElementsFromGraphView();

            DialogueSystemGraphSaveDataSO graphData = IOUtilities.CreateAsset<DialogueSystemGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}");

            graphData.Initialize(graphFileName);    

            DialogueSystemDialogueContainerSO dialogueContainer = IOUtilities.CreateAsset<DialogueSystemDialogueContainerSO>(containerFolderPath, graphFileName);

            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            SaveNodesConnections(graphData);

            IOUtilities.SaveAsset(graphData);
            IOUtilities.SaveAsset(dialogueContainer);
        }

        private static void SaveGroups(DialogueSystemGraphSaveDataSO graphData, DialogueSystemDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (BaseGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        /// <summary>
        /// Saves the given group to the main 'save file'.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="graphData"></param>
        private static void SaveGroupToGraph(BaseGroup group, DialogueSystemGraphSaveDataSO graphData)
        {
            BaseGroupSaveData groupData = new()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(BaseGroup group, DialogueSystemDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            IOUtilities.CreateFolder($"{containerFolderPath}/Groups", groupName);
            IOUtilities.CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DialogueSystemDialogueGroupSO dialogueGroup = IOUtilities.CreateAsset<DialogueSystemDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<BaseNodeSO>());

            IOUtilities.SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DialogueSystemGraphSaveDataSO graphData)
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

        //private static void SaveNodes(DialogueSystemGraphSaveDataSO graphData, DialogueSystemDialogueContainerSO dialogueContainer)
        //{
        //    SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
        //    List<string> ungroupedNodeNames = new List<string>();

        //    foreach (BaseNode node in nodes)
        //    {
        //        SaveNodeToGraph(node, graphData);
        //        SaveNodeToScriptableObject(node, dialogueContainer);

        //        if (node.Group != null)
        //        {
        //            groupedNodeNames.AddItem(node.Group.title, node.ID);

        //            continue;
        //        }

        //        ungroupedNodeNames.Add(node.ID);
        //    }

        //    UpdateDialoguesChoicesConnections();

        //    UpdateOldGroupedNodes(groupedNodeNames, graphData);
        //    UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        //}

        private static void SaveNodes(DialogueSystemGraphSaveDataSO graphData, DialogueSystemDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            // Create a list of node save data
            List<BaseNodeSaveData> nodeSaveDataList = new();

            foreach (BaseNode node in nodes)
            {
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node is DialogueSystemNode dialogueNode)
                {
                    //Debug.Log($"Saving node with ID: {dialogueNode.ID} Text: {dialogueNode.Text}");

                    nodeSaveDataList.Add(new DialogueSystemNodeSaveData
                    {
                        ID = dialogueNode.ID,
                        Position = dialogueNode.GetPosition().position,
                        GroupID = dialogueNode.Group?.ID,
                        Text = dialogueNode.Text,
                        DialogueType = dialogueNode.DialogueType,
                        Choices = CloneNodeChoices(dialogueNode.Choices),
                        EventTriggers = CloneNodeTriggers(dialogueNode.EventTriggers),
                    });
                }
                else if (node is DialogueSystemConditionCheckNode conditionCheckNode)
                {
                    nodeSaveDataList.Add(new DialogueSystemConditionCheckNodeSaveData
                    {
                        ID = conditionCheckNode.ID,
                        Position = conditionCheckNode.GetPosition().position,
                        GroupID = conditionCheckNode.Group?.ID,
                        ConditionKey = conditionCheckNode.ConditionKey,
                        ExpectedValue = conditionCheckNode.ExpectedValue,
                        ItemIDField = conditionCheckNode.ItemIDField,
                        ConditionCheckType = conditionCheckNode.ConditionCheckType,
                        ConnectedNodeID = conditionCheckNode.ConnectedNodeID
                    });
                }
                else
                {
                    Debug.LogError($"Unsupported node type: {node.GetType()}");
                }

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.ID);

                    continue;
                }

                ungroupedNodeNames.Add(node.ID);
            }

            // Serialize the nodes list to JSON
            string json = JsonConvert.SerializeObject(nodeSaveDataList, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All, // Preserve type information
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore // Ignore self-referencing properties
            });

            // Save the JSON string into the ScriptableObject
            graphData.SerializedNodes = json;

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);

            // Debug.Log($"Saved {nodeSaveDataList.Count} nodes to JSON.");
        }

        //private static void SaveNodeToGraph(BaseNode node, DialogueSystemGraphSaveDataSO graphData)
        //{
        //    // Find out what type of node we're saving
        //    Debug.Log($"Saving node of type {node.GetType()} to graph");

        //    // Save a dialogue node
        //    if (IsDialogueNode(node))
        //    {
        //        // Cast the node into a dialogue node
        //        DialogueSystemNode dialogueNode = (DialogueSystemNode)node;

        //        // Clone the choices of the node
        //        List<DialogueSystemChoiceSaveData> choices = CloneNodeChoices(dialogueNode.Choices);

        //        // Create a new node save data object and set it's values to the node's values
        //        DialogueSystemNodeSaveData nodeData = new()
        //        {
        //            ID = dialogueNode.ID,
        //            Choices = choices,
        //            Text = dialogueNode.Text,
        //            GroupID = dialogueNode.Group?.ID,
        //            DialogueType = dialogueNode.DialogueType,
        //            Position = dialogueNode.GetPosition().position,
        //            EventTrigger = dialogueNode.EventTrigger,
        //            AssociatedObject = dialogueNode.AssociatedObject,
        //            NextGroupName = dialogueNode.NewGroupNameField
        //        };

        //        graphData.Nodes.Add(nodeData);
        //    }

        //    // Save a check condition node
        //    else if (node.GetType() == typeof(DialogueSystemConditionCheckNode))
        //    {
        //        DialogueSystemConditionCheckNode checkNode = (DialogueSystemConditionCheckNode)node;

        //        DialogueSystemConditionCheckNodeSaveData nodeData = new()
        //        {
        //            ID = checkNode.ID,
        //            ConnectedNodeID = checkNode.ConnectedNodeID,
        //            ConditionKey = checkNode.ConditionKey,
        //            ExpectedValue = checkNode.ExpectedValue,
        //            GroupID = checkNode.Group?.ID,
        //            Position = checkNode.GetPosition().position,
        //        };

        //        graphData.Nodes.Add(nodeData);
        //    }
        //    else
        //    {
        //        Debug.LogError("Node type not recognized!");
        //    }
        //}

        private static void SaveNodeToScriptableObject(BaseNode baseNode, DialogueSystemDialogueContainerSO dialogueContainer)
        {
            Debug.Log($"Saving node of type {baseNode.GetType()} to scriptable object");

            // Save a dialogue node
            if (IsDialogueNode(baseNode))
            {
                // Cast the base node into a dialogue node
                DialogueSystemNode node = (DialogueSystemNode)baseNode;

                DialogueSystemDialogueSO dialogue;

                if (node.Group != null)
                {
                    dialogue = IOUtilities.CreateAsset<DialogueSystemDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.ID);

                    dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
                }
                else
                {
                    dialogue = IOUtilities.CreateAsset<DialogueSystemDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.ID);

                    dialogueContainer.UngroupedDialogues.Add(dialogue);
                }

                dialogue.Initialize(
                    node.ID,
                    node.Text,
                    ConvertNodeChoicesToDialogueChoices(node.Choices),
                    node.DialogueType,
                    node.IsStartingNode(),
                    node.EventTriggers
                );

                createdNodes.Add(node.ID, dialogue);

                IOUtilities.SaveAsset(dialogue);
            }

            // Saving a condition check node
            else if (baseNode.GetType() == typeof(DialogueSystemConditionCheckNode))
            {
                DialogueSystemConditionCheckNode node = (DialogueSystemConditionCheckNode)baseNode;

                DialogueSystemConditionCheckSO conditionCheck;

                if (node.Group != null)
                {
                    conditionCheck = IOUtilities.CreateAsset<DialogueSystemConditionCheckSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.ID);

                    dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], conditionCheck);
                }
                else
                {
                    conditionCheck = IOUtilities.CreateAsset<DialogueSystemConditionCheckSO>($"{containerFolderPath}/Global/Dialogues", node.ID);

                    dialogueContainer.UngroupedDialogues.Add(conditionCheck);
                }

                if (node.ConnectedNode == null)
                {
                    Debug.LogError("Error saving condition check node to SO! Connected node is null.");
                }

                // Need to get the connected node by id.

                foreach (var item in createdNodes)
                {
                    Debug.Log(item.Key);
                }

                conditionCheck.Initialize(
                    node.ConditionKey,
                    node.ExpectedValue,
                    node.ItemIDField,
                    node.ConditionCheckType,
                    node.ConnectedNode
                );

                Debug.Log($"Saving node of type {node.DialogueType}");

                createdNodes.Add(node.ID, conditionCheck);

                IOUtilities.SaveAsset(conditionCheck);
            }
            else
            {
                Debug.LogError("Node type not recognized!");
            }
        }

        private static void SaveNodesConnections(DialogueSystemGraphSaveDataSO graphData)
        {
            graphData.NodeLinks.Clear();

            foreach (Edge edge in graphView.edges.ToList())
            {
                BaseNode outputNode = edge.output.node as BaseNode;
                BaseNode inputNode = edge.input.node as BaseNode;

                if (outputNode == null || inputNode == null)
                {
                    Debug.LogWarning("Edge has invalid nodes.");
                    continue;
                }

                NodeLinkData linkData = new NodeLinkData
                {
                    BaseNodeID = outputNode.ID,
                    TargetNodeID = inputNode.ID,
                    OutputPortName = edge.output.portName,
                    InputPortName = edge.input.portName
                };

                graphData.NodeLinks.Add(linkData);
            }

            Debug.Log($"Saved {graphData.NodeLinks.Count} node connections.");
        }


        #endregion

        private static List<DialogueSystemDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DialogueSystemChoiceSaveData> nodeChoices)
        {
            List<DialogueSystemDialogueChoiceData> dialogueChoices = new List<DialogueSystemDialogueChoiceData>();

            foreach (DialogueSystemChoiceSaveData nodeChoice in nodeChoices)
            {
                DialogueSystemDialogueChoiceData choiceData = new DialogueSystemDialogueChoiceData()
                {
                    Text = nodeChoice.Text,
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        /// <summary>
        /// Ensures that the dialogues' choices are connected to the correct dialogues.
        /// </summary>
        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (BaseNode baseNode in nodes)
            {
                if (IsDialogueNode(baseNode))
                {
                    DialogueSystemNode node = (DialogueSystemNode)baseNode;

                    // Gets this node's dialogue from the created dialogues and casts it to a DialogueSystemDialogueSO
                    DialogueSystemDialogueSO createdNode = createdNodes[node.ID] as DialogueSystemDialogueSO;

                    for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                    {
                        DialogueSystemChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                        if (string.IsNullOrEmpty(nodeChoice.NodeID))
                        {
                            continue;
                        }

                        // Sets the next dialogue of the choice to the correct dialogue
                        createdNode.Choices[choiceIndex].NextDialogue = createdNodes[nodeChoice.NodeID] as DialogueSystemDialogueSO;

                        IOUtilities.SaveAsset(createdNode);
                    }
                }
                else if (baseNode.GetType() == typeof(DialogueSystemConditionCheckNode))
                {
                    DialogueSystemConditionCheckNode node = (DialogueSystemConditionCheckNode)baseNode;

                    if (node == null)
                    {
                        Debug.LogError("Node cast was invalid!");
                    }

                    if (createdNodes == null)
                    {
                        Debug.LogError("Created nodes is null");
                    }

                    if (string.IsNullOrEmpty(node.ID))
                    {
                        Debug.LogError("Node ID is null");
                    }

                    if (!createdNodes.ContainsKey(node.ID))
                    {
                        Debug.LogError("Node ID not found in created nodes");
                        return;
                    }

                    if (createdNodes[node.ID] == null)
                    {
                        Debug.LogError("Node is null");
                    }
                    else
                    {
                        Debug.Log(createdNodes[node.ID].GetType());
                    }

                    DialogueSystemConditionCheckSO conditionCheck = createdNodes[node.ID] as DialogueSystemConditionCheckSO;

                    if (string.IsNullOrEmpty(node.ConnectedNodeID))
                    {
                        continue;
                    }

                    // Sets the connected node of the condition check to the correct dialogue
                    conditionCheck.ConnectedNode = createdNodes[node.ConnectedNodeID];

                    IOUtilities.SaveAsset(conditionCheck);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DialogueSystemGraphSaveDataSO graphData)
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
                        IOUtilities.RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DialogueSystemGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    IOUtilities.RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }


        #region Loading Methods

        public static void Load()
        {
            DialogueSystemGraphSaveDataSO graphData = IOUtilities.LoadAsset<DialogueSystemGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/DialogueSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return;
            }

            //DialogueSystemEditorWindow.UpdateFileName(graphData.FileName);

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

        //private static void LoadNodes(List<BaseNodeSaveData> nodes)
        //{  
        //    if (nodes == null)
        //    {
        //        Debug.LogWarning("Nodes list is null");
        //        return;
        //    } 

        //    foreach (BaseNodeSaveData baseNodeData in nodes)
        //    {
        //        // Since we are saving multiple types of nodes, we need to find out what we're loading
        //        Type nodeType = baseNodeData.GetType();

        //        Debug.Log($"Loading node of type {nodeType}");

        //        if (nodeType == typeof(DialogueSystemNodeSaveData))
        //        {
        //            // Cast the base node into the specific node type
        //            DialogueSystemNodeSaveData nodeData = (DialogueSystemNodeSaveData)baseNodeData;

        //            // Create node from save data
        //            DialogueSystemNode node = graphView.CreateNode((int)nodeData.DialogueType, nodeData.Position, false) as DialogueSystemNode;

        //            node.ID = nodeData.ID;
        //            node.Choices = CloneNodeChoices(nodeData.Choices);
        //            node.Text = nodeData.Text;
        //            node.EventTrigger = nodeData.EventTrigger;
        //            node.AssociatedObject = nodeData.AssociatedObject;
        //            node.NewGroupNameField = nodeData.NextGroupName;

        //            node.Draw();
        //            graphView.AddElement(node);

        //            loadedNodes.Add(node.ID, node);

        //            if (string.IsNullOrEmpty(nodeData.GroupID))
        //            {
        //                // Node is ungrouped, add to ungroupedNodes
        //                graphView.AddUngroupedNodes(node);
        //            }
        //            else
        //            {
        //                // Node is grouped, add to the corresponding group
        //                BaseGroup group = loadedGroups[nodeData.GroupID];
        //                node.Group = group;
        //                group.AddElement(node);
        //            }

        //        }
        //        else if (nodeType == typeof(DialogueSystemConditionCheckNodeSaveData))
        //        {
        //            // Cast the base node into the specific node type
        //            DialogueSystemConditionCheckNodeSaveData nodeData = (DialogueSystemConditionCheckNodeSaveData)baseNodeData;

        //            // Create node from save data
        //            DialogueSystemConditionCheckNode node = graphView.CreateNode((int)DialogueSystemDiagType.ConditionCheck, nodeData.Position, false) as DialogueSystemConditionCheckNode;

        //            node.ID = nodeData.ID;
        //            node.ConnectedNodeID = nodeData.ConnectedNodeID;
        //            node.ConditionKey = nodeData.ConditionKey;
        //            node.ExpectedValue = nodeData.ExpectedValue;

        //            node.Draw();
        //            graphView.AddElement(node);

        //            loadedNodes.Add(node.ID, node);

        //            if (string.IsNullOrEmpty(nodeData.GroupID))
        //            {
        //                // Node is ungrouped, add to ungroupedNodes
        //                graphView.AddUngroupedNodes(node);
        //            }
        //            else
        //            {
        //                // Node is grouped, add to the corresponding group
        //                BaseGroup group = loadedGroups[nodeData.GroupID];
        //                node.Group = group;
        //                group.AddElement(node);
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogError($"Node of type {nodeType} not recognized!");
        //        }
        //    }
        //}

        private static void LoadNodes(DialogueSystemGraphSaveDataSO graphData)
        {
            if (string.IsNullOrEmpty(graphData.SerializedNodes))
            {
                Debug.LogWarning("Serialized nodes data is null or empty.");
                return;
            }

            // Deserialize the nodes list from JSON
            List<BaseNodeSaveData> nodeSaveDataList = JsonConvert.DeserializeObject<List<BaseNodeSaveData>>(graphData.SerializedNodes, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // Restore type information
            });

            if (nodeSaveDataList == null || nodeSaveDataList.Count == 0)
            {
                Debug.LogWarning("No nodes found in the deserialized data.");
                return;
            }

            foreach (BaseNodeSaveData nodeData in nodeSaveDataList)
            {
                //Debug.Log($"Loading node with ID: {nodeData.ID} of type {nodeData.GetType()}");

                if (nodeData is DialogueSystemNodeSaveData dialogueNodeData)
                {
                    DialogueSystemNode dsNode = graphView.CreateNode((int)dialogueNodeData.DialogueType, dialogueNodeData.Position, false) as DialogueSystemNode;

                    //Debug.Log($"Loading node with ID: {dialogueNodeData.ID} Text: {dialogueNodeData.Text}");

                    dsNode.Text = dialogueNodeData.Text;
                    dsNode.Choices = dialogueNodeData.Choices;
                    dsNode.EventTriggers = LoadTriggerData(dialogueNodeData.EventTriggers);
                    dsNode.ID = dialogueNodeData.ID;

                    if (dsNode != null)
                    {
                        dsNode.ID = nodeData.ID;
                        graphView.AddElement(dsNode);

                        if (string.IsNullOrEmpty(nodeData.GroupID))
                        {
                            graphView.AddUngroupedNodes(dsNode);
                        }
                        else if (loadedGroups.TryGetValue(nodeData.GroupID, out var group))
                        {
                            group.AddElement(dsNode);
                        }
                    }

                    dsNode.Draw();
                }
                else if (nodeData is DialogueSystemConditionCheckNodeSaveData conditionCheckNodeData)
                {
                    DialogueSystemConditionCheckNode ccNode = graphView.CreateNode((int)DialogueSystemDiagType.ConditionCheck, conditionCheckNodeData.Position, false) as DialogueSystemConditionCheckNode;

                    ccNode.ConditionKey = conditionCheckNodeData.ConditionKey;
                    ccNode.ExpectedValue = conditionCheckNodeData.ExpectedValue;
                    ccNode.ItemIDField = conditionCheckNodeData.ItemIDField;
                    ccNode.ConditionCheckType = conditionCheckNodeData.ConditionCheckType;
                    ccNode.ConnectedNodeID = conditionCheckNodeData.ConnectedNodeID;

                    if (ccNode != null)
                    {
                        ccNode.ID = nodeData.ID;
                        graphView.AddElement(ccNode);

                        if (string.IsNullOrEmpty(nodeData.GroupID))
                        {
                            graphView.AddUngroupedNodes(ccNode);
                        }
                        else if (loadedGroups.TryGetValue(nodeData.GroupID, out var group))
                        {
                            group.AddElement(ccNode);
                        }
                    }

                    ccNode.Draw();
                }
                else
                {
                    Debug.LogError($"Node of type {nodeData.GetType()} not recognized!");
                }
            }
        }


        /// <summary>
        /// Connects the nodes together based on the loaded data.
        /// </summary>
        private static void LoadNodesConnections(DialogueSystemGraphSaveDataSO graphData)
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
            IOUtilities.CreateFolder("Assets/Editor/DialogueSystem", "Graphs");

            IOUtilities.CreateFolder("Assets", "DialogueSystem");
            IOUtilities.CreateFolder("Assets/DialogueSystem", "Dialogues");

            IOUtilities.CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            IOUtilities.CreateFolder(containerFolderPath, "Global");
            IOUtilities.CreateFolder(containerFolderPath, "Groups");
            IOUtilities.CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(BaseGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is BaseNode node)
                {
                    //Debug.Log($"Adding node of type {node.GetType()}");
                     
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

        #region Utility Functions

        private static List<DialogueSystemEventTriggerSaveData> LoadTriggerData(List<DialogueSystemEventTriggerSaveData> triggerData)
        {
            List<DialogueSystemEventTriggerSaveData> triggers = new();

            foreach (DialogueSystemEventTriggerSaveData trigger in triggerData)
            {
                DialogueSystemEventTriggerSaveData newTriggerData = new()
                {
                    TriggerType = trigger.TriggerType,
                    ScriptableObjectGUID = trigger.ScriptableObjectGUID
                };

                if (!string.IsNullOrEmpty(trigger.ScriptableObjectGUID))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(trigger.ScriptableObjectGUID);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        newTriggerData.TriggerValue = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Missing asset for GUID: {trigger.ScriptableObjectGUID}");
                    }
                }
                else
                {
                    newTriggerData.TriggerValue = trigger.TriggerValue;
                }

                Debug.Log($"Loaded trigger of type {newTriggerData.TriggerType}");

                triggers.Add(newTriggerData);
            }

            return triggers;
        }


        private static List<DialogueSystemEventTriggerSaveData> CloneNodeTriggers(List<DialogueSystemEventTriggerSaveData> nodeTriggers)
        {
            List<DialogueSystemEventTriggerSaveData> triggers = new();

            foreach (DialogueSystemEventTriggerSaveData trigger in nodeTriggers)
            {
                DialogueSystemEventTriggerSaveData newTriggerData = new();

                newTriggerData.TriggerType = trigger.TriggerType;

                if (trigger.TriggerValue is ScriptableObject scriptableObject)
                {
                    string assetPath = AssetDatabase.GetAssetPath(scriptableObject);
                    newTriggerData.ScriptableObjectGUID = AssetDatabase.AssetPathToGUID(assetPath);
                    newTriggerData.TriggerValue = null; 
                }
                else
                {
                    newTriggerData.ScriptableObjectGUID = string.Empty;
                    newTriggerData.TriggerValue = trigger.TriggerValue; 
                }

                triggers.Add(newTriggerData);
            }

            return triggers;
        }


        private static List<DialogueSystemChoiceSaveData> CloneNodeChoices(List<DialogueSystemChoiceSaveData> nodeChoices)
        {
            List<DialogueSystemChoiceSaveData> choices = new List<DialogueSystemChoiceSaveData>();

            foreach (DialogueSystemChoiceSaveData choice in nodeChoices)
            {
                DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        private static bool IsDialogueNode(BaseNode node)
        {
            return node.GetType() == typeof(DialogueSystemNode) || node.GetType().IsSubclassOf(typeof(DialogueSystemNode));
        }

        #endregion
    }

}