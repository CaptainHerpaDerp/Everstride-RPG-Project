using GraphSystem.Base.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace CBTSystem.Windows
{
    using Base;
    using CBTSystem.Utilities;
    using Characters.Behaviour;
    using Core;
    using System.IO;
    using UnityEditor;

    public class CBTEditorWindow : BaseGraphEditorWindow
    {
        protected override TextField fileNameTextField { get => cbtFileTextField; set => cbtFileTextField = value; }
        private TextField cbtFileTextField;

        private CBTGraphView graphView;
        protected override string defaultFileName { get; } = "CBTFileName";

        EventBus eventBus;

        private void OnEnable()
        {
            NPCTestCombatBehaviourTree.OnNodeChanged += HandleNodeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
         
        private void OnDisable()
        {
            NPCTestCombatBehaviourTree.OnNodeChanged -= HandleNodeChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        protected override void OnFocus()
        {
            base.OnFocus();

            string lastLoadedGraph = EditorPrefs.GetString("CurrentLoadedCBTGraph", string.Empty);

            // De-select the filename text field when the window loses focus
            if (fileNameTextField != null)
            {
                fileNameTextField.value = lastLoadedGraph; // Reset to default name
            }
            else
            {
               Debug.LogWarning("File name text field is null. Cannot reset value.");   
            }
        }


        #region Event Callbacks

        private void HandleNodeChanged(string nodeID)
        {
            //Debug.Log($"Node changed: ID={nodeID}");
            if (graphView != null)
            graphView.SetActiveNode(nodeID);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                graphView.ResetNodeStyles();
            }
        }

        #endregion


        [MenuItem("Window/Combat Behaviour Tree Graph")]
        public static void ShowExample()
        {
            GetWindow<CBTEditorWindow>("CBT Graph");
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
            string lastLoadedGraph = EditorPrefs.GetString("CurrentLoadedCBTGraph", string.Empty);

            if (!string.IsNullOrEmpty(lastLoadedGraph))
            {
                Debug.Log($"Re-loading last loaded graph: {lastLoadedGraph}");

                Clear();

                CBTSystemIOUtility.Initialize(graphView, lastLoadedGraph);

                fileNameTextField.value = lastLoadedGraph;

                try
                {
                    CBTSystemIOUtility.Load();
                }
                catch
                {
                    Debug.LogWarning($"Failed to load graph: {lastLoadedGraph}. It may have been moved or deleted.");
                    EditorPrefs.DeleteKey("CurrentLoadedCBTGraph");
                }
            }
        }


        #region Elements Addition

        /// <summary>
        /// Sets the graph view to the root visual element of the window.
        /// </summary>
        protected override void AddGraphView()
        {
            graphView = new CBTGraphView(this);

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

            if (string.IsNullOrEmpty(cbtFileTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid File Name", "Please enter a valid file name.", "OK");
                return;
            }

            CBTSystemIOUtility.Initialize(graphView, cbtFileTextField.value);
            CBTSystemIOUtility.Save();
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

            string filePath = EditorUtility.OpenFilePanel("Quest Graphs", "Assets/Editor/CombatBehaviourTreeSystem/Graphs", "asset");

            // Checks if the file path is empty or null.
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            // Clears the current graph
            Clear();

            string graphFileName = Path.GetFileNameWithoutExtension(filePath);
            UpdateFileName(graphFileName);

            CBTSystemIOUtility.Initialize(graphView, graphFileName);
            CBTSystemIOUtility.Load();

            // Store the current graph so we can load it on a script reload
            EditorPrefs.SetString("CurrentLoadedCBTGraph", graphFileName);
        }

        protected override void ResetGraph()
        {
            if (activeWindow != this)
                return;

            Clear();

            // Clear the stored graph from EditorPrefs
            EditorPrefs.DeleteKey("CurrentLoadedCBTGraph");

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
            cbtFileTextField.Blur();

           // Debug.Log($"Updating file name to: {newName}");

            cbtFileTextField.value = newName;
        }

        #endregion
    }
}
