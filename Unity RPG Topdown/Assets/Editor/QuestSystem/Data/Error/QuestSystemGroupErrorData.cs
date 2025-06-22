using System.Collections.Generic;

namespace QuestSystem.Data.Error
{
    using Elements;

    public class QuestSystemGroupErrorData
    {
        public QuestSystemErrorData ErrorData { get; set; }
        public List<QuestSystemGroup> Groups { get; set; }

        public QuestSystemGroupErrorData()
        {
            ErrorData = new();
            Groups = new();
        }
    }
}
