using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace QuestSystem.Windows
{
    using GraphSystem.Base;
    using GraphSystem.Base.Windows;
    using Elements;
    using Enumerations;

    public class QuestSystemSearchWindow : BaseSearchWindow
    {
        public override void Initialize(BaseGraphView graphView)
        {
            QuestSystemGraphView questSystemGraphView = graphView as QuestSystemGraphView;

            this.graphView = questSystemGraphView;

            indentationIcon = new(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeGroupEntry(new GUIContent("Quest Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Objective", indentationIcon))
                {
                    userData = QuestSystemNodeType.Objective,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Quest Groups"), 1),
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
                case QuestSystemNodeType.Objective:
                    {
                        QuestSystemObjectiveNode objectiveNode = (QuestSystemObjectiveNode)graphView.CreateNode((int)QuestSystemNodeType.Objective, localMousePosition);

                        graphView.AddElement(objectiveNode);
                        return true;
                    }

                case Group _:
                    {
                        graphView.CreateGroup("QuestGroup", localMousePosition);

                        return true;
                    }

                default:
                    return false;

            }
        }
    }
}
