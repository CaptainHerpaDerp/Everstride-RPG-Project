using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphSystem.Base.Windows
{
    public abstract class BaseSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        protected BaseGraphView graphView;
        protected Texture2D indentationIcon;

        public abstract void Initialize(BaseGraphView graphView);

        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);

        public abstract bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context);
    }
}