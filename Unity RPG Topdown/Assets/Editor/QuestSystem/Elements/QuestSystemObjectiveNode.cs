using UnityEngine;
using UnityEngine.UIElements;

namespace QuestSystem.Elements
{
    using Enumerations;
    using Windows;
    using Utilities;
    using QuestSystem.Data.Save;
    using System.Collections.Generic;
    using GraphSystem.Base;

    public class QuestSystemObjectiveNode : QuestSystemNode
    {
        public override void Initialize(BaseGraphView questSystemGraphView, Vector2 position)
        {
            base.Initialize((QuestSystemGraphView)questSystemGraphView, position);

            NodeType = QuestSystemNodeType.Objective;

            Conditions = new();
        }

        public override void Draw()
        {
            base.Draw();

            // Add Condition Button
            Button addConditionButton = QuestSystemElementUtility.CreateButton("Add Condition", () =>
            {
                CreateConditionField();
            });

            addConditionButton.AddToClassList("ds-node__button");
            mainContainer.Add(addConditionButton);

            // Copy the current conditions to avoid modifying the original list
            List<QuestSystemConditionSaveData> conditionsCopy = new(Conditions);

            // Clear the current conditions
            Conditions.Clear();

            // Re-create the conditions (they will be added to the Conditions list automatically)
            foreach (var conditionSaveData in conditionsCopy)
            {
                CreateConditionField(conditionSaveData.Condition, value : conditionSaveData.ConditionValue);
            }

            RefreshExpandedState();
        }

        private void CreateConditionField(QuestCondition condition = QuestCondition.CollectItem, string value = "")
        {
            // Create a new container for the condition
            VisualElement enumContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row } // Horizontal layout
            };

            mainContainer.Add(enumContainer);

            // Add the new condition to the Conditions list
            QuestSystemConditionSaveData newCondition = new() { Condition = condition, ConditionValue = value };

          
            Conditions.Add(newCondition);
          

            // Create the EnumField for selecting the condition type
            EnumField conditionEnumField = new EnumField("", newCondition.Condition)
            {
                value = newCondition.Condition // Initialize value
            };

            // Create a container to hold the additional field (e.g., int for CollectItem, string for KillTarget)
            VisualElement dynamicFieldContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            conditionEnumField.RegisterValueChangedCallback(evt =>
            {
                newCondition.Condition = (QuestCondition)evt.newValue;

                // Remove the old field
                dynamicFieldContainer.Clear();

                // Add the new field based on the selected Enum value
                VisualElement newField = CreateDynamicField(newCondition.Condition, newCondition);
                dynamicFieldContainer.Add(newField);
            });

            // Initialize the first field based on the default Enum value
            VisualElement field = CreateDynamicField(newCondition.Condition, newCondition);
            dynamicFieldContainer.Add(field);

            // Add the EnumField and the dynamic field container to the enum container
            enumContainer.Add(conditionEnumField);
            enumContainer.Add(dynamicFieldContainer);

            // Create the remove button to delete the condition
            Button removeConditionButton = QuestSystemElementUtility.CreateButton("X", () =>
            {
                // Remove the condition from the Conditions list
                if (Conditions.Contains(newCondition))
                {
                    Conditions.Remove(newCondition);
                    Debug.Log($"Condition removed successfully: {newCondition.Condition}");
                }
                else
                {
                    Debug.LogWarning($"Condition could not be found in the list!");
                }

                // Remove the visual element
                mainContainer.Remove(enumContainer);

                Debug.Log($"Current Conditions Count: {Conditions.Count}");
            });

            removeConditionButton.AddToClassList("ds-node__remove-button");
            enumContainer.Add(removeConditionButton);
        }

        private VisualElement CreateDynamicField(QuestCondition condition, QuestSystemConditionSaveData conditionData)
        {
            VisualElement field = null;

            TextField conditionField = new TextField("Condition Value");
            conditionField.Q<Label>().style.minWidth = 50;  // Min width for the label
            conditionField.value = conditionData.ConditionValue; // Default value
            field = conditionField;

            switch (condition)
            {
                case QuestCondition.CollectItem:
                    conditionField.label = "Item ID";
                    break;

                case QuestCondition.KillTarget:
                    conditionField.label = "NPC ID";
                    break;

                case QuestCondition.PressKey:
                    conditionField.label = "Key Code";
                    break;

                case QuestCondition.ActivateNPCDialogueStep:
                    conditionField.label = "Diag Node Name";
                    break;

                // Add more cases for other enum types as needed
                default:
                    // Fallback case, can be an empty field or a label indicating unsupported type
                    Label unsupportedField = new Label("Unsupported condition");
                    field = unsupportedField;
                    break;
            }

            conditionField.RegisterValueChangedCallback(evt =>
            {
                conditionData.ConditionValue = evt.newValue;
                //Debug.Log($"Updated Value to: {conditionData.ConditionValue}");
            });

            return field;
        }
    }
}