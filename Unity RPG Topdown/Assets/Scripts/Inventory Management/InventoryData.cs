using Core.Enums;
using System;
using System.Collections.Generic;

namespace InventoryManagement
{
    [Serializable]
    public class InventoryData
    {
        public List<string> ItemNames;
        public List<string> ItemIds;
        public List<int> ItemQuantities;
        
        public string EquippedWeaponItemID;
        public string EquippedShieldItemID;

        public Dictionary<ArmourType, string> EquippedArmour = new();
    }
}
