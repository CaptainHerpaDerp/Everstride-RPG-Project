using System;

namespace GraphSystem.Base.Data.Save
{
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeID; // ID of the source node
        public string TargetNodeID; // ID of the target node
        public string OutputPortName; // Name of the output port
        public string InputPortName; // Name of the input port
    }
}