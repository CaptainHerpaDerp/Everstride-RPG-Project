using GraphSystem.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CBTSystem.Elements.Nodes
{
    using Enumerations;
    using UnityEditor.Experimental.GraphView;


    /// <summary>
    /// A condition node that can evaluate multiple conditions combined with AND/OR connectors.
    /// </summary>
    public class CBTSystemConditionNode : CBTSystemNode
    {
        // List of conditions to evaluate
        public List<ConditionEntry> ConditionEntries = new List<ConditionEntry>
        {
            new ConditionEntry { ConditionType = CBTConditionType.CheckDistance, Operator = "<=", Value = 1f }
        };

        // Logical connectors between successive conditions
        public List<LogicalOperator> Connectors = new List<LogicalOperator>();

        private VisualElement conditionContainer;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
            nodeLabel = "Condition Node";

            conditionContainer = new VisualElement();
            conditionContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(conditionContainer);
        }

        public override void Draw()
        {
            base.Draw();
            DrawConditionsUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        protected override void AddPorts()
        {
            // Single input, single output
            DisconnectAllPorts();
            inputContainer.Clear();
            outputContainer.Clear();

            var outputPort = this.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            var inputPort = this.CreatePort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
            inputContainer.Add(inputPort);
        }

        private void DrawConditionsUI()
        {
            // Sync connectors count with entries
            int needed = ConditionEntries.Count - 1;
            while (Connectors.Count < needed)
                Connectors.Add(LogicalOperator.And);
            while (Connectors.Count > needed)
                Connectors.RemoveAt(Connectors.Count - 1);

            conditionContainer.Clear();

            // Add/Remove buttons
            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            var addBtn = new Button(() =>
            {
                ConditionEntries.Add(new ConditionEntry { ConditionType = CBTConditionType.CheckDistance, Operator = "<=", Value = 1f });
                DrawConditionsUI();
            })
            { text = "+ Condition" };
            var removeBtn = new Button(() =>
            {
                if (ConditionEntries.Count > 1)
                {
                    ConditionEntries.RemoveAt(ConditionEntries.Count - 1);
                    DrawConditionsUI();
                }
            })
            { text = "- Condition" };
            btnRow.Add(addBtn);
            btnRow.Add(removeBtn);
            conditionContainer.Add(btnRow);

            // Draw each condition with optional connector
            for (int i = 0; i < ConditionEntries.Count; i++)
            {
                int idx = i;
                // Connector before entry
                if (idx > 0)
                {
                    var connField = new PopupField<LogicalOperator>(
                        new List<LogicalOperator> { LogicalOperator.And, LogicalOperator.Or },
                        Connectors[idx - 1]
                    );
                    connField.RegisterValueChangedCallback(evt => Connectors[idx - 1] = evt.newValue);
                    connField.style.width = 60;
                    conditionContainer.Add(connField);
                }

                // Condition row
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 4 } };

                // Unit label
                var label = new Label(GetConditionLabel(ConditionEntries[idx].ConditionType));

                // Value
                var valField = new FloatField { value = ConditionEntries[idx].Value }; 

                // Boolean
                var boolField = new Toggle { value =  true};

                boolField.RegisterValueChangedCallback((evt) =>
                {
                    ConditionEntries[idx].Value = evt.newValue ? 1f : 0f; // Store as 1 or 0
                });

                // Operator field
                var ops = new List<string> { "<", "<=", ">=", ">", "=" };
                int opIndex = ops.IndexOf(ConditionEntries[idx].Operator);
                if (opIndex < 0) opIndex = 1;
                var opField = new PopupField<string>(ops, opIndex);

                // Type
                var typeField = new EnumField(ConditionEntries[idx].ConditionType);
                typeField.RegisterValueChangedCallback(
                    (evt) =>
                    {
                        ConditionEntries[idx].ConditionType = (CBTConditionType)evt.newValue;
                        // Update label based on type change
                        label.text = GetConditionLabel(ConditionEntries[idx].ConditionType);

                        // If we change to a percent-based condition, clamp the value accordingly
                        if (ConditionEntries[idx].ConditionType == CBTConditionType.CheckHealth || ConditionEntries[idx].ConditionType == CBTConditionType.CheckStamina)
                        {
                            valField.value = Mathf.Clamp(valField.value, 0f, 100f);
                        }
                        else if (ConditionEntries[idx].ConditionType == CBTConditionType.CheckDistance)
                        {
                            valField.value = Mathf.Max(valField.value, 0f); // Distance can't be negative
                        }                  
                    });

                typeField.style.width = 120;
                row.Add(typeField);

                // Operator
                opField.RegisterValueChangedCallback(evt => ConditionEntries[idx].Operator = evt.newValue);
                opField.style.width = 40;

                valField.RegisterValueChangedCallback((evt) => {

                    float newValue = evt.newValue;

                    // If the current condition type is a percent value, we want to clamp the value between 0 and 100
                    if (ConditionEntries[idx].ConditionType == CBTConditionType.CheckHealth || ConditionEntries[idx].ConditionType == CBTConditionType.CheckStamina)
                    {
                        newValue = Mathf.Clamp(evt.newValue, 0f, 100f);
                    }
                    else if (ConditionEntries[idx].ConditionType == CBTConditionType.CheckDistance)
                    {
                        newValue = Mathf.Max(evt.newValue, 0f); // Distance can't be negative
                    }
                    else if (ConditionEntries[idx].IsBooleanCondition())
                    {
                        newValue = Mathf.Clamp(evt.newValue, 0f, 1);
                    }

                    ConditionEntries[idx].Value = newValue;   
                    valField.value = newValue;
                });

                valField.style.width = 60;

                  
                row.Add(opField);
                row.Add(valField);
                

                // Unit label
                label.style.marginLeft = 4;
                row.Add(label);

                conditionContainer.Add(row);
            }
        }

        private string GetConditionLabel(CBTConditionType type)
        {
            return type switch
            {
                CBTConditionType.CheckDistance => "(m)",
                CBTConditionType.CheckHealth => "%Health",
                CBTConditionType.CheckStamina => "%Stamina",
                _ => string.Empty,
            };
        } 

        //public override NodeState Evaluate(CombatContext ctx)
        //{
        //    return EvaluateAll(ctx) ? NodeState.Success : NodeState.Failure;
        //}

        //private bool EvaluateAll(CombatContext ctx)
        //{
        //    if (ConditionEntries.Count == 0)
        //        return true;

        //    bool result = CheckSingle(ConditionEntries[0], ctx);
        //    for (int i = 1; i < ConditionEntries.Count; i++)
        //    {
        //        bool next = CheckSingle(ConditionEntries[i], ctx);
        //        var op = Connectors[i - 1];
        //        result = (op == LogicalOp.And) ? (result && next) : (result || next);
        //    }
        //    return result;
        //}

        //private bool CheckSingle(ConditionEntry entry, CombatContext ctx)
        //{
        //    float actual = entry.ConditionType switch
        //    {
        //        CBTConditionType.CheckDistance => Vector2.Distance(ctx.agentPosition, ctx.targetPosition),
        //        CBTConditionType.CheckHealth => ctx.currentHealth / ctx.maxHealth,
        //        CBTConditionType.CheckStamina => ctx.currentStamina / ctx.maxStamina,
        //        _ => 0f,
        //    };

        //    return entry.Operator switch
        //    {
        //        "<" => actual < entry.Value,
        //        "<=" => actual <= entry.Value,
        //        ">" => actual > entry.Value,
        //        ">=" => actual >= entry.Value,
        //        _ => false,
        //    };
        //}
    }
}
