using CBTSystem.Base;
using GraphSystem.Base;
using UnityEngine;
using UnityEngine.UIElements;

namespace CBTSystem.Elements.Nodes
{
    using GraphSystem.Base.ScriptableObjects;
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;

    public abstract class CBTSystemNode : BaseNode
    {
        // The list of identifiers for the next nodes
        public List<string> NextNodeIDs { get; set; } = new();
        public BaseNodeSO ConnectedNode { get; set; }

        protected CBTGraphView graphView;
        protected override BaseGraphView baseGraphView { get => graphView; set => graphView = value as CBTGraphView; }

        protected string nodeLabel = "Untitled";

        private bool _isRootNode = false;

        public bool IsRootNode
        {
            get => _isRootNode;

            set
            {
                // Tell the base graph view to unset the previous root node
                _isRootNode = value;

                if (_isRootNode)
                {
                    graphView.SetStartingNode(this);
                    mainContainer.style.backgroundColor = new StyleColor(Color.yellow);
                }
                else
                {
                    mainContainer.style.backgroundColor = new StyleColor(defaultBgColor);
                }
            }
        }

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            // Set the unique ID
            ID = System.Guid.NewGuid().ToString();

            // Assign the graph view
            this.graphView = (CBTGraphView)graphView;

            // Set the position
            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            defaultBgColor = mainContainer.style.backgroundColor.value;

            mainContainer.style.backgroundColor = defaultBgColor;

            if (IsRootNode)
            {
                mainContainer.style.backgroundColor = new StyleColor(Color.yellow);
            }
        }

        #region Node Activation Visualization

        /// <summary>
        /// Sets the node colour to green, indicating it is active.
        /// </summary>
        public virtual void SetNodeActive()
        {
            mainContainer.style.backgroundColor = new StyleColor(Color.green);
        }

        /// <summary>
        /// Resets the style of the node to its default background color, showing it is not active (runtime)
        /// </summary>
        public virtual void SetNodeInactive()
        {
            ResetStyle();
        }

        /// <summary>
        /// Completely resets the node style to its default background color, showing it is not active. Reverts to default colours such as showing the starting node (design time)
        /// </summary>
        public void SetNodeDefault()
        {
            if (IsRootNode)
            {
                mainContainer.style.backgroundColor = new StyleColor(Color.yellow);
            }
            else
            {
                ResetStyle();
            }
        }

        /// <summary>
        /// Sets the node colour to orange, showing that its condition is being checked by an active node (runtime).
        /// </summary>
        public void SetNodeChecking()
        {
            mainContainer.style.backgroundColor = new StyleColor(new Color(255, 187, 0, 255));
        }

        #endregion

        public override void Draw()
        {
            CreateIDLabel();

            AddIDLabel();

            AddPorts();
         
            // Refresh containers
            RefreshExpandedState();
            RefreshPorts();
        }

        #region Drawn Elements

        private void AddIDLabel()
        {
            // Add Node ID label to the titleContainer
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

            // Allow copying the ID by double-clicking the titleContainer
            titleContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) // On double-click, copy the ID
                {
                    GUIUtility.systemCopyBuffer = ID;
                    Debug.Log($"Node ID copied to clipboard: {ID}");
                }
            });

            // Add a class for custom styling if needed
            idLabel.AddToClassList("readonly-field");

            // Add the Label to the titleContainer
            titleContainer.Add(idLabel);

            // Apply layout styles to center the label
            titleContainer.style.justifyContent = Justify.Center; // Center horizontally
            titleContainer.style.alignItems = Align.Center; // Center vertically

            mainContainer.Add(titleContainer);
        }

        protected virtual void AddPorts()
        {
            Port outputPort = this.CreatePort("Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputPort);

            Port inputPort = this.CreatePort("Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
        }

        private void CreateIDLabel()
        {
            // Clear the title container to avoid duplication
            titleContainer.Clear();

            // Create a label for the Node ID
            Label idLabel = new Label(nodeLabel)
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


        #endregion

        #region Node Content

        /// <summary>
        /// Based on the node type, create the appropriate label
        /// </summary>
        private void CreateNameLabel()
        {
            titleContainer.Clear();
        }

        #endregion
    }
}