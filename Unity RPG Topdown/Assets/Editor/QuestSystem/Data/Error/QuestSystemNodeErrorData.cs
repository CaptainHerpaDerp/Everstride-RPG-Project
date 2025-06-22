using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.Data.Error
{
    using Elements;

    public class QuestSystemNodeErrorData
    {
        public QuestSystemErrorData ErrorData { get; set; }
        public List<QuestSystemNode> Nodes { get; set; }

        public QuestSystemNodeErrorData()
        {
            ErrorData = new QuestSystemErrorData();
            Nodes = new List<QuestSystemNode>();
        }
    }
}

