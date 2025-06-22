using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Windows
{
    using GraphSystem.Base;
    using GraphSystem.Base.Windows;
    using Elements;
    using Enumerations;

    public class DialogueSystemSearchWindow : BaseSearchWindow
    {
        public override void Initialize(BaseGraphView graphView)
        {
            DialogueSystemGraphView dialogueSystemGraphView = graphView as DialogueSystemGraphView;

            this.graphView = dialogueSystemGraphView;

            indentationIcon = new(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = DialogueSystemDiagType.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = DialogueSystemDiagType.MultipleChoice,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            return searchTreeEntries;
        }

        public override bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case DialogueSystemDiagType.SingleChoice:
                    {
                        DialogueSystemSingleChoiceNode singleChoiceNode = (DialogueSystemSingleChoiceNode)graphView.CreateNode((int)DialogueSystemDiagType.SingleChoice, localMousePosition);

                        graphView.AddElement(singleChoiceNode);
                        return true;
                    }

                case DialogueSystemDiagType.MultipleChoice:
                    {
                        DialogueSystemMultipleChoiceNode MultipleChoiceNode = (DialogueSystemMultipleChoiceNode)graphView.CreateNode((int)DialogueSystemDiagType.MultipleChoice, localMousePosition);

                        graphView.AddElement(MultipleChoiceNode);
                        return true;
                    }

                case Group _:
                    {
                        graphView.CreateGroup("DialogueGroup", localMousePosition);

                        return true;
                    }

                default:
                    return false;

            }
        }
    }
}
