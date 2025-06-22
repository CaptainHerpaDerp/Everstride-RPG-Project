using UnityEngine;

namespace GraphSystem.Base.Utilities
{
    public class GraphService<TGraphView, TGraphData>
        where TGraphView : BaseGraphView
        where TGraphData : BaseGraphSaveData, new()
    {
        private TGraphView graphView;

        public void Initialize(TGraphView view)
        {
            graphView = view;

            Debug.Log("Graph Service initialized");
        }

        public TGraphData SaveGraph()
        {
            // Save logic here
            return new TGraphData();
        }

        public void LoadGraph(TGraphData graphData)
        {
            // Load logic here
        }
    }
}
