using GraphSystem.Base.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphSystem.Base.Windows
{
    public abstract class BaseGraphEditorWindow : EditorWindow
    {
        // Tracks which window is currently in focus
        protected static BaseGraphEditorWindow activeWindow;

        protected abstract TextField fileNameTextField { get; set; }
        protected Button minimapButton;
        protected Button saveButton;

        protected virtual string defaultFileName { get; } = "FileName";

        protected virtual void OnFocus()
        {
            activeWindow = this;

            //Debug.Log($"Active window: {activeWindow}");
        }

        protected virtual void OnLostFocus()
        {
            if (activeWindow == this)
            {
                // De-select the filename text field when the window loses focus
                fileNameTextField.Blur();

                activeWindow = null; // Clear active
            }
        }

        public abstract void UpdateFileName(string newName);

        #region Elements Creation

        protected abstract void AddGraphView();

        protected virtual void AddToolbar()
        {
            Toolbar toolbar = new();

            fileNameTextField = BaseGraphSystemElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = BaseGraphSystemElementUtility.CreateButton("Save", () =>
            {
                if (activeWindow != this) return;
                Save();
            });

            Button loadButton = BaseGraphSystemElementUtility.CreateButton("Load", () =>
            {
                if (activeWindow != this) return;
                Load();
            });
            Button clearButton = BaseGraphSystemElementUtility.CreateButton("Clear", () =>
            {
                if (activeWindow != this) return;
                Clear();
            });
            Button resetButton = BaseGraphSystemElementUtility.CreateButton("Reset", () =>
            {
                if (activeWindow != this) return;
                ResetGraph();
            });
            minimapButton = BaseGraphSystemElementUtility.CreateButton("Minimap", () =>
            {
                if (activeWindow != this) return;
                ToggleMinimap();
            });

            toolbar.Add(minimapButton);

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);

            toolbar.Add(clearButton);
            toolbar.Add(resetButton);

            toolbar.Add(loadButton);

            toolbar.AddStyleSheets("DialogueSystem/DialogueSystemToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        protected void AddStyles()
        {
            // Adds the variables stylesheet to list of stylesheets in the graph view.
            rootVisualElement.AddStyleSheets("DialogueSystem/DialogueSystemVariables.uss");
        }

        #endregion

        #region Toolbar Actions

        public abstract void Save();

        #endregion

        #region Utility Methods

        public abstract void EnableSaving();

        public abstract void DisableSaving();

        protected abstract void Clear();

        protected abstract void Load();

        protected abstract void ResetGraph();

        protected abstract void ToggleMinimap();

        protected abstract void ReloadLastLoadedGraph();

        #endregion
    }
}
