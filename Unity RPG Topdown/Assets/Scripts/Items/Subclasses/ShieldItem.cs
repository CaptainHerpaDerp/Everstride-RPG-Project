using Sirenix.OdinInspector;
using UnityEngine.U2D.Animation;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Shield", menuName = "Inventory/EquippableItem/Shield")]
    public class ShieldItem : EquippableItem
    {
        [BoxGroup("Shield Attributes")]
        public int shieldPowerMin;

        [BoxGroup("Shield Attributes")]
        public int shieldPowerMax;

        [BoxGroup("Shield Attributes")]
        public SpriteLibraryAsset shieldTrailAsset;
    }
}