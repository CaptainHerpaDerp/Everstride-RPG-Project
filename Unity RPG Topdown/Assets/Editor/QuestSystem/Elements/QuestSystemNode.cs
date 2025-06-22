using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace QuestSystem.Elements
{
    using GraphSystem.Base;
    using Enumerations;
    using GraphSystem.Base.Utilities;
    using QuestSystem.Data.Save;
    using QuestSystem.Windows;
    using System;
    using System.Collections.Generic;
    using Utilities;
    using GraphSystem.Base.ScriptableObjects;

    public class QuestSystemNode : BaseNode
    {
        // The identifier for the next node
        public string NextNodeID { get; set; }
        public BaseNodeSO ConnectedNode { get; set; }
    
        // The description of the node
        public string QuestDescription { get; set; }

        // The type of the node
        public QuestSystemNodeType NodeType { get; set; }

        // A list of the conditions needing to be met to fulfill the node (used by the objective node).
        public List<QuestSystemConditionSaveData> Conditions { get; set; }
        public List<QuestSystemTriggerSaveData> Triggers { get; set; }

        // Keep a reference to the parent graph view so that the node can communicate with it.
        protected QuestSystemGraphView graphView;
        protected override BaseGraphView baseGraphView { get => graphView; set => graphView = value as QuestSystemGraphView; }

        // Track the default background color so that if the node takes an error color, it can be reset.
        private Color defaultBackgroundColor;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            Triggers = new();

            this.graphView = (QuestSystemGraphView)graphView;

            // Set the default background color of the node.
            defaultBackgroundColor = new Color(29f / 255, 29f / 255, 30f / 255);

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        #region Overrided Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", action =>
            {
                DisconnectInputPorts();
            });

            evt.menu.AppendAction("Disconnect Output Ports", action =>
            {
                DisconnectOutputPorts();
            });

            base.BuildContextualMenu(evt);
        }

        #endregion

        public override void Draw()
        {
            // Clear existing elements
            titleContainer.Clear();
 
            AddPorts();

            AddIDLabel();

            // Description Field
            TextField descriptionTextField = QuestSystemElementUtility.CreateTextField(QuestDescription, onValueChanged: callback =>
              {
                  TextField target = callback.target as TextField;
                  QuestDescription = target.value;
              });

            descriptionTextField.multiline = true;
            descriptionTextField.style.height = new StyleLength(StyleKeyword.Auto);

            descriptionTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-textfield"
            );

            mainContainer.Add(descriptionTextField);

            // **Trigger Container**
            VisualElement triggerContainer = new VisualElement
            {
                name = "trigger-container",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 5,
                    marginBottom = 5
                }
            };

            // Add Trigger Button
            Button addTriggerButton = QuestSystemElementUtility.CreateButton("Add Trigger", () =>
            {
                CreateTriggerField(triggerContainer);
            });

            addTriggerButton.AddToClassList("ds-node__button");
            triggerContainer.Add(addTriggerButton);

            // Recreate triggers from saved list
            List<QuestSystemTriggerSaveData> triggersCopy = new(Triggers);
            Triggers.Clear();

            foreach (var triggerSaveData in triggersCopy)
            {
                CreateTriggerField(triggerContainer, triggerSaveData.ConditionKey, triggerSaveData.ConditionValue);
            }

            // Add triggerContainer to the mainContainer
            mainContainer.Add(triggerContainer);

            // Refresh containers
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Drawn Elements

        private void AddPorts()
        {
            Port inputPort = this.CreatePort("Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            Port outputPort = this.CreatePort("Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            outputPort.userData = ConnectedNode;

            outputContainer.Add(outputPort);

            // Add containers to the main layout if not already present
            if (!mainContainer.Contains(inputContainer))
            {
                mainContainer.Add(inputContainer);
            }

            if (!mainContainer.Contains(outputContainer))
            {
                mainContainer.Add(outputContainer);
            }
        }


        private void AddIDLabel()
        {
            // Add Node ID label to the titleContainer
            Label idLabel = new Label(ID)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    flexGrow = 1,
                    color = Color.gray
                }
            };

            // Allow copying the ID by double-clicking the titleContainer
            titleContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) // On double-click, copy the ID
                {
                    GUIUtility.systemCopyBuffer = ID;
                    Debug.Log($"Node ID copied to clipboard: {ID}");
                }
            });

            // Add a class for custom styling if needed
            idLabel.AddToClassList("readonly-field");

            // Add the Label to the titleContainer
            titleContainer.Add(idLabel);

            // Apply layout styles to center the label
            titleContainer.style.justifyContent = Justify.Center; // Center horizontally
            titleContainer.style.alignItems = Align.Center; // Center vertically

            mainContainer.Add(titleContainer);
        }

        private void CreateTriggerField(VisualElement container, string key = "", bool value = false)
        {
            QuestSystemTriggerSaveData newTrigger = new()
            {
                ConditionKey = key,
                ConditionValue = value
            };

            Triggers.Add(newTrigger);

            VisualElement triggerElement = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            TextField keyField = new TextField("Key")
            {
                value = key
            };

            keyField.RegisterValueChangedCallback(evt =>
            {
                newTrigger.ConditionKey = evt.newValue;
            });
            triggerElement.Add(keyField);

            Toggle valueToggle = new Toggle("Value")
            {
                value = value
            };
            valueToggle.RegisterValueChangedCallback(evt =>
            {
                newTrigger.ConditionValue = evt.newValue;
            });
            triggerElement.Add(valueToggle);

            Button removeButton = QuestSystemElementUtility.CreateButton("X", () =>
            {
                Triggers.Remove(newTrigger);
                container.Remove(triggerElement);
            });

            removeButton.AddToClassList("ds-node__remove-button");
            triggerElement.Add(removeButton);

            container.Add(triggerElement);
        }

        #endregion

    }
}