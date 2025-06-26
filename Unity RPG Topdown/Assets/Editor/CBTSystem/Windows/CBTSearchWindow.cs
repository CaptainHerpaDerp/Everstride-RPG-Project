using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CBTSystem.Windows
{
    using CBTSystem.Base;
    using GraphSystem.Base;
    using GraphSystem.Base.Windows;

    public class CBTSearchWindow : BaseSearchWindow
    {
        public override void Initialize(BaseGraphView graphView)
        {
            CBTGraphView cbtGraphView = graphView as CBTGraphView;
            this.graphView = cbtGraphView;

            indentationIcon = new(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
            };

            return searchTreeEntries;
        }


        public override bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {

                default:
                    return false;

            }
        }
    }
}

