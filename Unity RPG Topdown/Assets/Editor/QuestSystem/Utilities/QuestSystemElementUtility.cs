using QuestSystem.Elements;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace QuestSystem.Utilities
{
    public static class QuestSystemElementUtility
    {
        /// <summary>
        /// Creates a button with the given text and onClick callback, which is called when the button is clicked.
        /// </summary>
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        /// <summary>
        /// Creates a foldout with the given title and collapsed state.
        /// </summary>
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }

        /// <summary>
        /// Creates a text field with the given value and onValueChanged callback, which is called when the value of the text field is changed.
        /// </summary>
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static EnumField CreateConditionEnumField(Enum value = null, EventCallback<ChangeEvent<Enum>> onValueChanged = null)
        {
            EnumField enumField = new EnumField("", value);     

            if (onValueChanged != null)
            {
                enumField.RegisterValueChangedCallback(onValueChanged);
            }

            return enumField;
        }

        /// <summary>
        /// Creates a port with the given name, orientation, direction and capacity.
        /// </summary>
        public static Port CreatePort(this QuestSystemNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

            port.portName = portName;

            return port;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
    }
}

