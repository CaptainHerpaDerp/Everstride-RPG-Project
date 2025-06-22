using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;

namespace DialogueSystem.Elements
{
    using Utilities;
    using Enumerations;
    using Data.Save;
    using Windows;
    using UnityEditor;
    using UnityEditor.Search;
    using GraphSystem.Base.Utilities;
    using GraphSystem.Base;
    using QuestSystem.Utilities;

    public class DialogueSystemNode : BaseNode
    {
        public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DialogueSystemDiagType DialogueType { get; set; }
        public List<DialogueSystemEventTriggerSaveData> EventTriggers { get; set; }
       
        // Creates a container for the choices of the node.
        VisualElement customDataContainer;

      //  public ScriptableObject AssociatedObject { get; set; } // Use ScriptableObject as the associated object type
       // public string ItemIDField { get; set; }

        private ObjectField objectField;
        TextField newGroupTextField;
        TextField itemIDTextField;

        protected DialogueSystemGraphView graphView;
        protected override BaseGraphView baseGraphView { get => graphView; set => graphView = value as DialogueSystemGraphView; }

        private Dictionary<DialogueSystemDiagEvent, List<VisualElement>> eventFieldMap;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            Choices = new List<DialogueSystemChoiceSaveData>();
            Text = "Dialogue Text";

            EventTriggers = new();

            this.graphView = (DialogueSystemGraphView)graphView;
            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            defaultBgColor = mainContainer.style.backgroundColor.value;

            // Initialize customDataContainer before any other methods
            customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(customDataContainer);

            // Initialize event field map and fields
            InitializeEventFieldMap();
        }

        private void InitializeEventFieldMap()
        {
            // Map event triggers to fields
            eventFieldMap = new Dictionary<DialogueSystemDiagEvent, List<VisualElement>>
            {
                { DialogueSystemDiagEvent.SetGroup, new List<VisualElement> { newGroupTextField } },
                { DialogueSystemDiagEvent.RemovePlayerItem, new List<VisualElement> { itemIDTextField } },
                { DialogueSystemDiagEvent.StartQuest, new List<VisualElement> { objectField } },
                { DialogueSystemDiagEvent.ShopOpen, new List<VisualElement> { objectField } }
            };
        }

        public override void Draw()
        {
            CreateIDLabel();

            // Add input port
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            // Add the text field
            Foldout textFoldout = DialogueSystemElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DialogueSystemElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-textfield"
            );

            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);

            // **Event Container**
            VisualElement eventContainer = new VisualElement
            {
                name = "event-container",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 5,
                    marginBottom = 5
                }
            };

            // Add Event Button
            Button addEventButton = DialogueSystemElementUtility.CreateButton("Add Event", () =>
            {
                CreateEventField(eventContainer);
            });

            addEventButton.AddToClassList("ds-node__button");
            eventContainer.Add(addEventButton);

            // Recreate events from saved list (always below the button)
            List<DialogueSystemEventTriggerSaveData> eventsCopy = new(EventTriggers);
            EventTriggers.Clear();

            foreach (var eventSaveData in eventsCopy)
            {
                CreateEventField(eventContainer, eventSaveData.TriggerType, eventSaveData.TriggerValue);
            }

            mainContainer.Add(eventContainer);

            // Refresh containers
            RefreshExpandedState();
            RefreshPorts();
        }


        private void CreateIDLabel()
        {
            // Clear the title container to avoid duplication
            titleContainer.Clear();

            // Create a label for the Node ID
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

            // Allow copying the ID by double-clicking
            titleContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    GUIUtility.systemCopyBuffer = ID;
                    Debug.Log($"Node ID copied to clipboard: {ID}");
                }
            });

            idLabel.AddToClassList("readonly-field");
            titleContainer.Add(idLabel);

            titleContainer.style.justifyContent = Justify.Center;
            titleContainer.style.alignItems = Align.Center;
        }
      
        private void CreateEventField(VisualElement container, DialogueSystemDiagEvent triggerType = DialogueSystemDiagEvent.SetGroup, object triggerValue = null)
        {
            DialogueSystemEventTriggerSaveData newEventTrigger = new()
            {
                TriggerType = triggerType,
                TriggerValue = triggerValue
            };

            EventTriggers.Add(newEventTrigger);

            VisualElement enumContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            EnumField triggerTypeField = new EnumField("", newEventTrigger.TriggerType)
            {
                value = newEventTrigger.TriggerType
            };

            // Create a container to hold the additional field (e.g., int for CollectItem, string for KillTarget)
            VisualElement dynamicFieldContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            triggerTypeField.RegisterValueChangedCallback(evt =>
            {
                newEventTrigger.TriggerType = (DialogueSystemDiagEvent)evt.newValue;

                // Remove the old field
                dynamicFieldContainer.Clear();

                // Add the new field based on the selected Enum value
                VisualElement newField = CreateDynamicField(newEventTrigger.TriggerType, newEventTrigger);
                dynamicFieldContainer.Add(newField);
            });

            // Initialize the first field based on the default Enum value
            VisualElement field = CreateDynamicField(newEventTrigger.TriggerType, newEventTrigger);
            dynamicFieldContainer.Add(field);

            // Add the EnumField and the dynamic field container to the enum container
            enumContainer.Add(triggerTypeField);
            enumContainer.Add(dynamicFieldContainer);

            // Create the remove button to delete the condition
            Button removeConditionButton = QuestSystemElementUtility.CreateButton("X", () =>
            {
                // Remove the condition from the Conditions list
                if (EventTriggers.Contains(newEventTrigger))
                {
                    EventTriggers.Remove(newEventTrigger);
                }
                else
                {
                    Debug.LogWarning($"Condition could not be found in the list!");
                }

                // Remove the visual element
                container.Remove(enumContainer);

                Debug.Log($"Current Conditions Count: {EventTriggers.Count}");
            });

            removeConditionButton.AddToClassList("ds-node__remove-button");
            enumContainer.Add(removeConditionButton);

            container.Add(enumContainer);
        }

        private VisualElement CreateDynamicField(DialogueSystemDiagEvent eventType, DialogueSystemEventTriggerSaveData triggerData)
        {
            VisualElement field = null;

            switch (eventType)
            {
                case DialogueSystemDiagEvent.RemovePlayerItem:
                case DialogueSystemDiagEvent.SetGroup:
                    CreateStringField(ref field, triggerData);
                    break;

                case DialogueSystemDiagEvent.StartQuest:
                case DialogueSystemDiagEvent.ShopOpen:
                    CreateObjectField(ref field, triggerData);
                    break;
            }   

            return field;
        }

        private void CreateStringField (ref VisualElement field, DialogueSystemEventTriggerSaveData triggerData)
        {
            TextField valueField = new("Condition Value");
            valueField.Q<Label>().style.minWidth = 50;  // Min width for the label
            valueField.value = triggerData.TriggerValue as string; // Default value
            field = valueField;

            switch (triggerData.TriggerType)
            {
                case DialogueSystemDiagEvent.RemovePlayerItem:
                    valueField.label = "Item ID";
                    break;

                case DialogueSystemDiagEvent.SetGroup:
                    valueField.label = "Group Name";
                    break;
            }

            valueField.RegisterValueChangedCallback(evt =>
            {
                triggerData.TriggerValue = evt.newValue;
            });
        }

        private void CreateObjectField(ref VisualElement field, DialogueSystemEventTriggerSaveData triggerData)
        {
            ObjectField objectField = new("Associated Object")
            {
                objectType = typeof(ScriptableObject),
                value = triggerData.TriggerValue as ScriptableObject
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                triggerData.TriggerValue = evt.newValue as ScriptableObject;
            });

            field = objectField;
        }

        private void UpdateFieldVisibility()
        {
            //if (eventFieldMap == null)
            //{
            //    InitializeEventFieldMap(); // Ensure the field map is initialized
            //}

            //foreach (var fieldList in eventFieldMap.Values)
            //{
            //    foreach (var field in fieldList)
            //    {
            //        if (field == null) continue; // Skip null fields
            //        field.style.display = DisplayStyle.None; // Hide all fields by default
            //    }
            //}

            //if (eventFieldMap.TryGetValue(EventTrigger, out var visibleFields))
            //{
            //    foreach (var field in visibleFields)
            //    {
            //        if (field == null) continue; // Skip null fields
            //        field.style.display = DisplayStyle.Flex; // Show fields for the current EventTrigger
            //    }
            //}
        }


        private void OnEventTriggerChanged()
        {
            if (eventFieldMap == null)
            {
                InitializeEventFieldMap();
            }

            UpdateFieldVisibility();
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

        #region Utility Methods

        public override void HandleDragAndDrop(DragExitedEvent evt)
        {
            var pickedObject = DragAndDrop.objectReferences.Length > 0 ? DragAndDrop.objectReferences[0] : null;

            foreach (var eventTrigger in EventTriggers)
            {
                if (eventTrigger.TriggerType == DialogueSystemDiagEvent.StartQuest && eventTrigger.TriggerType == DialogueSystemDiagEvent.ShopOpen)
                {
                    if (pickedObject is ScriptableObject scriptableObject)
                    {
                        eventTrigger.TriggerValue = scriptableObject;
                    }
                }
            }
        }

        #endregion
    }
}
