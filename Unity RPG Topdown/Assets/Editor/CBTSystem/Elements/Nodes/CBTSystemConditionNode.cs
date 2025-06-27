using GraphSystem.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CBTSystem.Elements.Nodes
{
    using Enumerations;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.Events;


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

        // Node Priority (0 = highest). Colors will cycle through ROYGBIV.
        public int Priority = -1;

        private VisualElement conditionContainer;

        public UnityAction OnPriorityChanged;
        public UnityAction OnNodeHovered;
        public UnityAction OnNodeUnhovered;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
            nodeLabel = "Condition Node";

            conditionContainer = new VisualElement();
            conditionContainer.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(conditionContainer);

            conditionContainer.AddToClassList("cbt-cond");
            conditionContainer.Insert(0, new VisualElement { name = "hdr" });
            conditionContainer.Q("hdr").AddToClassList("cbt-cond__header");


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

        public override void Draw()
        {
            base.Draw();

            if (Priority != -1)
            SetConditionPriority(Priority);

            DrawConditionsUI();
            RefreshExpandedState();
            RefreshPorts();
        }

        public void SetConditionPriority(int newPriority)
        {
            Priority = newPriority;

            if (Priority != -1)
                DrawPriorityButton();
        }

        public void RemoveConditionPriority()
        {
            Priority = -1; // Reset Priority
            var existing = extensionContainer.Q<Button>("Priority-btn");
            if (existing != null) extensionContainer.Remove(existing);
        }

        public void DrawPriorityButton()
        {
            // Remove existing if any
            var existing = extensionContainer.Q<VisualElement>("Priority-btn");
            if (existing != null) extensionContainer.Remove(existing);

            // Create Priority button
            var btn = new Button(() =>
            {
                OnPriorityChanged?.Invoke();

                DrawPriorityButton();
            })
            { name = "Priority-btn", text = "P: " + (Priority + 1)};
            btn.style.width = 48;
            btn.style.height = 24;
            btn.style.backgroundColor = new StyleColor(GetPriorityButtonColor(Priority));
            
            // Increase the font size
            btn.style.fontSize = 14;
            btn.style.color = new StyleColor(GetPriorityTextColor(Priority));

            extensionContainer.Insert(0, btn);
        }

        protected override void AddPorts()
        {
            // Single input, single output
            DisconnectAllPorts();
            inputContainer.Clear();
            outputContainer.Clear();

            var outputPort = this.CreatePort("Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            var inputPort = this.CreatePort("Input", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
            inputContainer.Add(inputPort);
        }

        /* -------------------------------------------------------------------- */
        /*  UI builder                                                          */
        /* -------------------------------------------------------------------- */
        private void DrawConditionsUI()
        {
            // Keep connectors array in sync
            int needed = ConditionEntries.Count - 1;
            while (Connectors.Count < needed) Connectors.Add(LogicalOperator.And);
            while (Connectors.Count > needed) Connectors.RemoveAt(Connectors.Count - 1);

            conditionContainer.Clear();

            /* -- Add / Remove row -------------------------------------------- */
            var btnRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 4 }
            };
            var addBtn = new Button(() =>
            {
                ConditionEntries.Add(
                    new ConditionEntry { ConditionType = CBTConditionType.CheckDistance, Operator = "<=", Value = 1f });
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

            /* -- Each condition row ------------------------------------------ */
            for (int i = 0; i < ConditionEntries.Count; i++)
            {
                int idx = i;

                var row = new VisualElement();
                row.AddToClassList("cbt-cond__row");
                if (IsOdd(idx)) row.AddToClassList("cbt-row--alt");

                /* Connector (AND / OR) before every entry after the first */
                if (idx > 0)
                { 
                    Label pill = new Label(Connectors[idx - 1].ToString().ToUpper());
                    pill.AddToClassList("cbt-pill");
                    pill.RegisterCallback<MouseDownEvent>(_ =>
                    {
                        Connectors[idx - 1] = Connectors[idx - 1] == LogicalOperator.And
                                            ? LogicalOperator.Or
                                            : LogicalOperator.And;
                        pill.text = Connectors[idx - 1].ToString().ToUpper();
                    });
                    row.Add(pill);
                }

                /* move the style assignment here if you need extra padding */
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginTop = 2;

                /* --- Condition-Type popup ----------------------------------- */
                var typeField = new EnumField(ConditionEntries[idx].ConditionType);
                typeField.style.width = 140;
                typeField.RegisterValueChangedCallback(evt =>
                {
                    ConditionEntries[idx].ConditionType = (CBTConditionType)evt.newValue;
                    DrawConditionsUI();          // rebuild row because UI type may change
                });
                row.Add(typeField);

                /* --- Operator popup (hide for boolean) ----------------------- */
                bool isBool = ConditionEntries[idx].IsBooleanCondition();
                if (!isBool)
                {
                    var ops = new List<string> { "<", "<=", ">=", ">", "=" };
                    var opField = new PopupField<string>(ops, ops.IndexOf(ConditionEntries[idx].Operator));
                    opField.style.width = 20;
                    opField.RegisterValueChangedCallback(e => ConditionEntries[idx].Operator = e.newValue);
                    row.Add(opField);
                }
                else
                {
                    ConditionEntries[idx].Operator = "=";  // force equality check for bool
                }

                /* --- Value field (Float or Toggle) --------------------------- */
                if (isBool)
                {
                    // Hide operator + unit
                    // Toggle replaces FloatField
                    Toggle tog = new Toggle { value = ConditionEntries[idx].Value > 0.5f };
                    tog.RegisterValueChangedCallback(e =>
                        ConditionEntries[idx].Value = e.newValue ? 1f : 0f);
                    tog.AddToClassList("cbt-field");
                    row.Add(tog);
                }
                else
                {
                    var valField = new FloatField { value = ConditionEntries[idx].Value };
                    valField.style.width = 20;
                    valField.RegisterValueChangedCallback(e =>
                    {
                        float v = e.newValue;

                        if (ConditionEntries[idx].IsPercentageCondition())
                            v = Mathf.Clamp(v, 0f, 100f);
                        else if (ConditionEntries[idx].ConditionType == CBTConditionType.CheckDistance)
                            v = Mathf.Max(0f, v);

                        ConditionEntries[idx].Value = v;
                        valField.SetValueWithoutNotify(v);   // avoid recursion
                    });
                    row.Add(valField); 
                }

                /* --- Unit label --------------------------------------------- */
                var unitLabel = new Label(GetConditionLabel(ConditionEntries[idx].ConditionType))
                {
                    style = { marginLeft = 4 }
                };
                row.Add(unitLabel);

                conditionContainer.Add(row);


            }
        }

        private string GetConditionLabel(CBTConditionType type)
        {
            return type switch
            {
                CBTConditionType.CheckDistance => "(m)",
                CBTConditionType.CheckHealth => "%HP",
                CBTConditionType.CheckStamina => "%Stam",
                CBTConditionType.HeavySwingChargeProgress => "%Chrg",
                CBTConditionType.TargetRangeCoverage => "%Cov",
                _ => string.Empty,
            };
        }

        bool IsOdd(int i) => (i & 1) == 1;

        private Color GetPriorityButtonColor(int prio)
        {
            switch (prio)
            {
                case 0: return Color.red;
                case 1: return new Color(1f, 0.5f, 0f); // orange
                case 2: return Color.yellow;
                case 3: return Color.green;
                case 4: return Color.cyan;
                case 5: return Color.blue;
                case 6: return new Color(0.5f, 0f, 0.5f); // violet
                default: return Color.white;
            }
        }

        private Color GetPriorityTextColor(int prio)
        {
            switch (prio)
            {
                case 2: return Color.black; // Dark colors for green, cyan, blue
                case 3: return Color.black; // Dark colors for green, cyan, blue
                case 4: return Color.black; // Dark colors for green, cyan, blue
                default: return Color.white;
            }
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
