using UnityEngine;

namespace QuestSystem.Data.Error
{
    public class QuestSystemErrorData
    {
        public Color Color { get; set; }

        public QuestSystemErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32((byte)Random.Range(65, 256), (byte)Random.Range(50, 176), (byte)Random.Range(50, 176), 255);
        }
    }
}

