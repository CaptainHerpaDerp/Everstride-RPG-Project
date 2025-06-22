using DialogueSystem.Elements;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphSystem.Base
{
    public abstract class BaseNode : Node
    {
        public string ID { get; set; }
        protected abstract BaseGraphView baseGraphView { get; set; }

        protected Color defaultBgColor;

        public string NewGroupNameField;
        public abstract void Initialize(BaseGraphView graphView, Vector2 position);
        public BaseGroup Group { get; set; }
        public abstract void Draw();

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

        public bool IsStartingNode()
        {
            // If this node is of type DialogueSystemConditionCheckNode, return false

            if (this.GetType() == typeof(DialogueSystemConditionCheckNode))
            {
                return false;
            }

            Port inputPort = inputContainer.Children().First() as Port;

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBgColor;
        }

        #endregion

        #region Port Methods

        protected void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                if (port.connections == null)
                {
                    Debug.Log("Connections is null");
                    continue;
                }

                if (port.connections.Count() == 0)
                {
                    Debug.Log("No connections");
                    continue;
                }

                if (baseGraphView == null)
                {
                    Debug.Log("BaseGraphView is null");
                    continue;
                }

                baseGraphView.DeleteElements(port.connections.ToList());
            }
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        protected void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        protected void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        #endregion

        public virtual void HandleDragAndDrop(DragExitedEvent evt)
        {
            Debug.Log("DragExitedEvent");
        }
    }
}