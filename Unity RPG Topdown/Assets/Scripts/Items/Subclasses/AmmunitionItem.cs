using UnityEngine;
using Core.Enums;   

namespace Items
{
    [CreateAssetMenu(fileName = "New Ammunition", menuName = "Inventory/Ammunition")]
    public class AmmunitionItem : Item
    {
        public int damage;
        public int ammoCount;

        public WeaponType ammunitionType;
    }
}