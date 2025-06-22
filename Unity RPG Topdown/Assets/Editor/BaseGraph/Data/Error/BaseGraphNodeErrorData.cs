using GraphSystem.Base;
using System.Collections.Generic;

namespace GraphSystem.Base.Data.Error
{
    public class BaseGraphNodeErrorData
    {
        public BaseGraphErrorData ErrorData { get; set; }
        public List<BaseNode> Nodes { get; set; }

        public BaseGraphNodeErrorData()
        {
            ErrorData = new();
            Nodes = new();
        }
    }
}
