using GraphSystem.Base;
using System.Collections.Generic;

namespace GraphSystem.Base.Data.Error
{
    public class BaseGraphGroupErrorData
    {
        public BaseGraphErrorData ErrorData { get; set; }
        public List<BaseGroup> Groups { get; set; }

        public BaseGraphGroupErrorData()
        {
            ErrorData = new();
            Groups = new();
        }
    }
}