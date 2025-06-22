using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using GraphSystem.Base;
    using Data.Save;
    using Enumerations;
    using GraphSystem.Base.Utilities;
    using Utilities;
    using Windows;

    public class DialogueSystemMultipleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize((DialogueSystemGraphView)graphView, position);

            DialogueType = DialogueSystemDiagType.MultipleChoice;

            DialogueSystemChoiceSaveData choiceData = new()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            // Adds a button to add a new choice to the node.
            Button addChoiceButton = DialogueSystemElementUtility.CreateButton("Add Choice", () =>
            {
                DialogueSystemChoiceSaveData choiceData = new()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            // Adds the new button to the title container.
            mainContainer.Insert(1, addChoiceButton);

            // Creates a container for the choices of the node.

            foreach (var choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            // RefreshExpandedState() must be called after all elements have been added.
            // RefreshExpandedState() calls RefreshPorts() by itself, so we dont need to call it.
            RefreshExpandedState();
        }

        #region Element Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DialogueSystemChoiceSaveData choiceData = userData as DialogueSystemChoiceSaveData;

            // Assign a unique name to the port based on the choice text or index. This ensures the edge can be connected to the correct port when loading the graph.
            choicePort.portName = $"Choice_{Choices.IndexOf(choiceData)}";

            // Hide the port label
            var portLabel = choicePort.Q<Label>("type");
            if (portLabel != null)
            {
                portLabel.AddToClassList("hidden-port-label");
            }

            Button deleteChoiceButton = DialogueSystemElementUtility.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }     

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                if (Choices.Contains(choiceData))
                {
                    Choices.Remove(choiceData);
                }
                else
                {
                    Debug.LogWarning("Choice could not be found in the list!");
                }


                graphView.RemoveElement(choicePort);

            });

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DialogueSystemElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__text-field__hidden"
                );

            choiceTextField.style.flexDirection = FlexDirection.Column;

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
        #endregion
    }
}

