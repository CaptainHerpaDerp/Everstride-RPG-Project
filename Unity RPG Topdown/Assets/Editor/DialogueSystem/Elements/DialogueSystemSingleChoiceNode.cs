
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    using GraphSystem.Base;
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class DialogueSystemSingleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize((DialogueSystemGraphView)graphView, position);

            DialogueType = DialogueSystemDiagType.SingleChoice;

            DialogueSystemChoiceSaveData choiceData = new()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            // Creates a container for the choices of the node.

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;

                choicePort.portName = choice.Text;

                outputContainer.Add(choicePort);
            }

            // RefreshExpandedState() must be called after all elements have been added.
            // RefreshExpandedState() calls RefreshPorts() by itself, so we dont need to call it.
            RefreshExpandedState();
        }
    }
}
