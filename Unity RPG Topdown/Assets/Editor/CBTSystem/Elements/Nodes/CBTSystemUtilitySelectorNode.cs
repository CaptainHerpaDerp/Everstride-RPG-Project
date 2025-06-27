using GraphSystem.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace CBTSystem.Elements.Nodes
{
    public class CBTSystemUtilitySelectorNode : CBTSystemNode
    {
        // Tunables
        public float Temperature = 0.7f;   // 0 = deterministic, 1 = default, 2 = highly random
        public float DecisionInterval = 0.15f;  // seconds between evaluations
        public float MinSwitchScore = 0.05f;  // min utility score to switch
        public float StickyBonus = 0.3f;  // min utility score to switch
        public bool EmergencyOverride = true;  // let Emergency bypass interval/dwell?

        // UI
        private VisualElement settingsFoldout;

        // Events for editor hover-highlight
        public UnityAction OnPriorityChanged;
        public UnityAction OnNodeHovered;
        public UnityAction OnNodeUnhovered;

        public override void Initialize(BaseGraphView graphView, Vector2 position)
        {
            base.Initialize(graphView, position);
            nodeLabel = "Utility Selector";

            // Custom field containers
            settingsFoldout = new Foldout { text = "Selector Settings", value = false };
            settingsFoldout.AddToClassList("ds-node__custom-data-container");
            extensionContainer.Add(settingsFoldout);

            this.RegisterCallback<PointerEnterEvent>(_ => OnNodeHovered?.Invoke());
            this.RegisterCallback<PointerLeaveEvent>(_ => OnNodeUnhovered?.Invoke());
        }

        public override void Draw()
        {
            base.Draw();
            DrawSettingsUI();
        }

        /* ------------------------------------------------------------------ */
        /*  UI helpers                                                         */
        /* ------------------------------------------------------------------ */
        private void DrawSettingsUI()
        {
            settingsFoldout.Clear();

            /* Temperature slider ------------------------------------------------ */
            var tempSlider = new Slider("Temperature", 0f, 2f)
            {
                value = Temperature,
                showInputField = true
            };
            tempSlider.tooltip = "0 = always best action, 1 = balanced mix, 2 = high randomness";
            tempSlider.RegisterValueChangedCallback(e => Temperature = e.newValue);
            settingsFoldout.Add(tempSlider);

            /* Decision interval ------------------------------------------------- */
            var intervalField = new FloatField("Decision Interval (s)")
            {
                value = DecisionInterval,
                tooltip = "How often the selector re-evaluates children."
            };
            intervalField.AddToClassList("inline-field");
            intervalField.RegisterValueChangedCallback(e =>
            {
                DecisionInterval = Mathf.Max(0f, e.newValue);
            });
            settingsFoldout.Add(intervalField);

            /* Min Δ utility ----------------------------------------------------- */
            var deltaField = new FloatField("Min Δ To Switch")
            {
                value = MinSwitchScore,
                tooltip = "Only switch if winner exceeds current by this margin."
            };
            deltaField.AddToClassList("inline-field");
            deltaField.RegisterValueChangedCallback(e =>
            {
                MinSwitchScore = Mathf.Clamp01(e.newValue);
            });
            settingsFoldout.Add(deltaField);

            /* Sticky Bonus ---------------------------------------- */
            var stickyBonusField = new FloatField("Sticky Bonus")
            {
                value = StickyBonus,
                tooltip = "If winner is picked while active, add this bonus to its score."
            };
            stickyBonusField.AddToClassList("inline-field");
            stickyBonusField.RegisterValueChangedCallback(e =>
            {
                StickyBonus = Mathf.Clamp01(e.newValue);
            });
            settingsFoldout.Add(stickyBonusField);

            /* Emergency override toggle ---------------------------------------- */
            var overrideToggle = new Toggle("Emergency Override")
            {
                value = EmergencyOverride,
                tooltip = "If ON, Emergency actions bypass interval & dwell."
            };
            overrideToggle.RegisterValueChangedCallback(e => EmergencyOverride = e.newValue);
            settingsFoldout.Add(overrideToggle);
        }
    }
}
