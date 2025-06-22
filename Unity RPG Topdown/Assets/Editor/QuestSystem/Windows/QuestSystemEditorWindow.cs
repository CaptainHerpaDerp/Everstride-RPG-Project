using UnityEditor;
using UnityEngine.UIElements;

namespace QuestSystem.Windows
{
    using GraphSystem.Base.Windows;
    using System.IO;
    using UnityEngine;
    using Utilities;

    public class QuestSystemEditorWindow : BaseGraphEditorWindow
    {
        protected override TextField fileNameTextField { get => questFileNameTextField; set => questFileNameTextField = value; }

        private TextField questFileNameTextField;

        private QuestSystemGraphView graphView;
        protected override string defaultFileName { get; } = "QuestFileName";

        [MenuItem("Window/Quest Graph")]
        public static void ShowExample()
        {
            GetWindow<QuestSystemEditorWindow>("Quest Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();

            ReloadLastLoadedGraph();
        }

        /// <summary>
        /// If we have a stored graph in the editor prefs, reload it after a script reset.
        /// </summary>
        protected override void ReloadLastLoadedGraph()
        {
            string lastLoadedGraph = EditorPrefs.GetString("CurrentLoadedQuestGraph", string.Empty);

            if (!string.IsNullOrEmpty(lastLoadedGraph))
            {
                Debug.Log($"Re-loading last loaded graph: {lastLoadedGraph}");

                Clear();

                QuestSystemIOUtility.Initialize(graphView, lastLoadedGraph);

                try
                {
                    QuestSystemIOUtility.Load();
                    UpdateFileName(lastLoadedGraph);
                }
                catch
                {
                    Debug.LogWarning($"Failed to load graph: {lastLoadedGraph}. It may have been moved or deleted.");
                    EditorPrefs.DeleteKey("CurrentLoadedQuestGraph");
                }
            }
        }

        #region Elements Addition

        /// <summary>
        /// Sets the graph view to the root visual element of the window.
        /// </summary>
        protected override void AddGraphView()
        {
            graphView = new QuestSystemGraphView(this);

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

            if (string.IsNullOrEmpty(questFileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid File Name", "Please enter a valid file name.", "OK");
                return;
            }

            QuestSystemIOUtility.Initialize(graphView, questFileNameTextField.value);
            QuestSystemIOUtility.Save();
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

            string filePath = EditorUtility.OpenFilePanel("Quest Graphs", "Assets/Editor/QuestSystem/Graphs", "asset");

            // Checks if the file path is empty or null.
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            // Clears the current graph
            Clear();

            string graphFileName = Path.GetFileNameWithoutExtension(filePath);
            UpdateFileName(graphFileName);

            QuestSystemIOUtility.Initialize(graphView, graphFileName);
            QuestSystemIOUtility.Load();
             
            // Store the current graph so we can load it on a script reload
            EditorPrefs.SetString("CurrentLoadedQuestGraph", graphFileName);
        }

        protected override void ResetGraph()
        {
            if (activeWindow != this)
                return;

            Clear();

            // Clear the stored graph from EditorPrefs
            EditorPrefs.DeleteKey("CurrentLoadedQuestGraph");

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
            questFileNameTextField.Blur();

            Debug.Log($"Updating file name to: {newName}");

            questFileNameTextField.value = newName;
        }

        #endregion
    }
}
