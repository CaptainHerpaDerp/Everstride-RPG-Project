using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace DialogueSystem.Windows
{
    using GraphSystem.Base.Windows;
    using UnityEngine;
    using Utilities;

    public class DialogueSystemEditorWindow : BaseGraphEditorWindow
    {
        protected override TextField fileNameTextField { get => dialogueFileNameTextField; set => dialogueFileNameTextField = value; }

        private TextField dialogueFileNameTextField;


        protected DialogueSystemGraphView graphView;
        protected override string defaultFileName { get; } = "DialogueFileName";

        [MenuItem("Window/Dialogue Graph")]
        public static void ShowExample()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolbar();
            AddStyles(); 

            // Reload the last loaded graph
            ReloadLastLoadedGraph();
        }

        /// <summary>
        /// If we have a stored graph in the editor prefs, reload it after a script reset.
        /// </summary>
        protected override void ReloadLastLoadedGraph()
        {
            string lastLoadedGraph = EditorPrefs.GetString("CurrentLoadedDialogueGraph", string.Empty);

            if (!string.IsNullOrEmpty(lastLoadedGraph))
            {
                Debug.Log($"Re-loading last loaded graph: {lastLoadedGraph}");

                Clear();

                DialogueSystemIOUtility.Initialize(graphView, lastLoadedGraph);

                try
                {
                    DialogueSystemIOUtility.Load();
                }
                catch
                {
                    Debug.LogWarning($"Failed to load graph: {lastLoadedGraph}. It may have been moved or deleted.");
                    EditorPrefs.DeleteKey("CurrentLoadedDialogueGraph");
                }
            }
        }


        #region Elements Addition

        /// <summary>
        /// Sets the graph view to the root visual element of the window.
        /// </summary>
        protected override void AddGraphView()
        {
            graphView = new DialogueSystemGraphView(this);

            // Sets the size of the graph view to the size of the window.
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        #endregion

        #region Toolbar Actions

        public override void Save()
        {
            if (activeWindow != this)
                return;

            if (string.IsNullOrEmpty(dialogueFileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid File Name", "Please enter a valid file name.", "OK");
                return;
            }

            DialogueSystemIOUtility.Initialize(graphView, dialogueFileNameTextField.value);
            DialogueSystemIOUtility.Save();

        }

        #endregion

        #region Utility Methods

        public override void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public override void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
             
        protected override void Clear()
        {
            graphView.ClearGraph();
        }

        protected override void Load()
        {
            if (activeWindow != this)
                return;

            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

            // Checks if the file path is empty or null.
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            // Clears the current graph
            Clear();

            string graphFileName = Path.GetFileNameWithoutExtension(filePath);

            DialogueSystemIOUtility.Initialize(graphView, graphFileName);
            DialogueSystemIOUtility.Load();

            UpdateFileName(graphFileName);

            // Store the current graph so we can load it on a script reload
            EditorPrefs.SetString("CurrentLoadedDialogueGraph", graphFileName);
        }

        protected override void ResetGraph()
        {
            if (activeWindow != this)
                return;

            Clear();

            // Clear the stored graph from EditorPrefs
            EditorPrefs.DeleteKey("CurrentLoadedDialogueGraph");

            UpdateFileName(defaultFileName);
        }


        protected override void ToggleMinimap()
        {
            if (activeWindow != this)
                return;

            graphView.ToggleMinimap();
            minimapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        public override void UpdateFileName(string newName)
        {
            if (activeWindow != this)
            {
                Debug.LogWarning("Active window is not this window. Cannot update file name.");
                return;
            }

            // De-select the filename text field so that the user value is not taken into account
            dialogueFileNameTextField.Blur();

            Debug.Log($"Updating file name to: {newName}");

            dialogueFileNameTextField.value = newName;
        }

        #endregion
    }
}
