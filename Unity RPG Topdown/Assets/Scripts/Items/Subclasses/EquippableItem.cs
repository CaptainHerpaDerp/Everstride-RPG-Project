using Sirenix.OdinInspector;
using UnityEngine.U2D.Animation;
using UnityEngine;

namespace Items
{
    public abstract class EquippableItem : Item
    {
        [BoxGroup("Visual Attributes")]
        public SpriteLibraryAsset itemLibraryAsset;

        [BoxGroup("Visual Attributes")]
        public bool isColoured;

        [ShowIf("isColoured"), BoxGroup("Visual Attributes")]
        [SerializeField] public Color32 itemColor;
    }
}