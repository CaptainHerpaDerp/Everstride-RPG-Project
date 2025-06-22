using GraphSystem.Base;
using DialogueSystem.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using DialogueSystem.Enumerations;
using System;
using GraphSystem.Base.ScriptableObjects;
using System.Collections.Generic;

namespace DialogueSystem.Elements
{
    /// <summary>
    /// A node that checks for a met condition in the game state manager
    /// </summary>
    public class DialogueSystemConditionCheckNode : BaseNode
    {
        protected DialogueSystemGraphView graphView;
        protected override BaseGraphView baseGraphView { get => graphView; set => graphView = value as DialogueSystemGraphView; }

        public DialogueSystemDiagType DialogueType { get; set; }


        // Hold the key of the condition to check as well as the expected value
        public string ConditionKey { get; set; }
        public bool ExpectedValue { get; set; }
        public string ItemIDField { get; set; }

        public string ConnectedNodeID { get; set; }

        public BaseNodeSO ConnectedNode { get; set; }

        // The type of condition check to perform
        private Dictionary<ConditionCheckType, List<VisualElement>> triggerFieldMap;

        private TextField conditionKeyField;
        private Toggle expectedValueToggle;

        private TextField itemIdField;

        private ConditionCheckType conditionCheckType { get; set; }
        public ConditionCheckType ConditionCheckType
        {
            get => conditionCheckType;
            set
            {
                conditionCheckType = value;
                OnConditionTriggerChanged();
            }
        }

        // Creates a container for the choices of the node.
        VisualElement customDataContainer;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            // Set the graph view
            this.graphView = (DialogueSystemGraphView)graphView;

            // Set the id of the node
            ID = Guid.NewGuid().ToString();

            // Set the position of the node
            SetPosition(new Rect(position, Vector2.zero));

            // The type of the node will always be a condition check
            DialogueType = DialogueSystemDiagType.ConditionCheck;

            // Initialize customDataContainer before any other methods
            customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(customDataContainer);

            InitializeTriggerFieldMap();
            ConditionCheckType = ConditionCheckType.CheckGameState;
        }

        private void InitializeTriggerFieldMap()
        {
            triggerFieldMap = new Dictionary<ConditionCheckType, List<VisualElement>>
            {
                { ConditionCheckType.CheckGameState, new List<VisualElement> { conditionKeyField, expectedValueToggle} },
                { ConditionCheckType.HasItem, new List<VisualElement> { itemIdField } }
            };
        }

        public override void Draw()
        {            
            // Remove the title container from the node
            titleContainer.Remove(titleContainer.ElementAt(0));

            // Add Output Port
            Port outputPort = this.CreatePort("Next Node", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);

            outputPort.userData = ConnectedNode;

            outputContainer.Add(outputPort);

            RefreshExpandedState();

            AddTriggerFields();
        }

        private void AddTriggerFields()
        {
            // Add the trigger type enum field

            EnumField triggerTypeEnumField = new(ConditionCheckType)
            {
                label = "Trigger Type"
            };

            triggerTypeEnumField.RegisterValueChangedCallback(evt =>
            {
                ConditionCheckType = (ConditionCheckType)evt.newValue;
            });
            customDataContainer.Add(triggerTypeEnumField);

            if (conditionKeyField == null)
            {
                // Add a TextField for the condition key
                conditionKeyField = new TextField("Condition Key")
                {
                    value = ConditionKey
                };
                conditionKeyField.RegisterValueChangedCallback(evt =>
                {
                    ConditionKey = evt.newValue;
                });
            }
            customDataContainer.Add(conditionKeyField);

            if (expectedValueToggle == null)
            {
                // Add a Toggle for the expected value
                expectedValueToggle = new Toggle("Expected Value")
                {
                    value = ExpectedValue
                };
                expectedValueToggle.RegisterValueChangedCallback(evt =>
                {
                    ExpectedValue = evt.newValue;
                });
            }
            customDataContainer.Add(expectedValueToggle);

            if (itemIdField == null)
            {
                // Add a TextField for the condition key
                itemIdField = new TextField("Item ID")
                {
                    value = ItemIDField
                };
                itemIdField.RegisterValueChangedCallback(evt =>
                {
                    ItemIDField = evt.newValue;
                });
            }
            customDataContainer.Add(itemIdField);

            InitializeTriggerFieldMap();

            UpdateTriggerFieldVisibility();
        }

        private void UpdateTriggerFieldVisibility()
        {
            if (triggerFieldMap != null)
            {
                InitializeTriggerFieldMap();
            }

            foreach (var triggerField in triggerFieldMap)
            {
                foreach (var field in triggerField.Value)
                {
                    if (field == null) continue; // Skip null fields
                    field.style.display = DisplayStyle.None; // Hide all fields by default
                }
            }


            if (triggerFieldMap.TryGetValue(ConditionCheckType, out var visibleFields))
            {
                foreach (var field in visibleFields)
                {
                    if (field == null) continue; // Skip null fields
                    field.style.display = DisplayStyle.Flex; // Show fields for the current EventTrigger
                }
            }
        }

        private void OnConditionTriggerChanged()
        {
            if (triggerFieldMap != null)
            {
                InitializeTriggerFieldMap();
            }

            UpdateTriggerFieldVisibility();
        }
    }
}

