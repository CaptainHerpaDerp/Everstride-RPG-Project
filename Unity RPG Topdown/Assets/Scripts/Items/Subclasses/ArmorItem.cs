using Core.Enums;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Armour", menuName = "Inventory/EquippableItem/Armour")]
    public class ArmourItem : EquippableItem
    {
        [BoxGroup("Armour Attributes")]
        public ArmourType armourType;

        [BoxGroup("Armour Attributes")]
        public float armourDefense;
    }
}