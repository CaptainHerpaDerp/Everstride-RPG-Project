using GraphSystem.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace CBTSystem.Elements.Nodes
{
    public class CBTSystemUtilitySelectorNode : CBTSystemNode
    {
        private VisualElement conditionContainer;
        public UnityAction OnPriorityChanged;
        public UnityAction OnNodeHovered;
        public UnityAction OnNodeUnhovered;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
            nodeLabel = "Utility Selector";

            conditionContainer = new VisualElement();
            conditionContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(conditionContainer);

            // detect hover start
            this.RegisterCallback<PointerEnterEvent>(evt =>
            {
                OnNodeHovered?.Invoke();
            });

            // detect hover end
            this.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                OnNodeUnhovered?.Invoke();
            });
        }
    }
}
