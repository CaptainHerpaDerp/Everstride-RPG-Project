using UnityEngine;

namespace GraphSystem.Base.Data.Error
{

    public class BaseGraphErrorData
    {
        public Color Color { get; set; }

        public BaseGraphErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            //Color = new Color32(
            //    (byte)Random.Range(65, 256),
            //    (byte)Random.Range(50, 176),
            //    (byte)Random.Range(50, 176),
            //    255
            //);

            Color = new Color32(
                213,
                6,
                6,
                255
                );
        }
    }
}