
using Items;

namespace Abstracts
{
    public interface IequipmentHandler{
        void NotifyUpdateWeapon(Item item);
        void NotifyUpdateShield(Item item);

        //void UnequipItem(Item item);
        //void UnequipAllItems();
        //void EquipAllItems();
        //void EquipItem(Item item, int slotIndex);
        //void UnequipItem(int slotIndex);
        //void SwapItems(int slotIndexA, int slotIndexB);
        //void SwapItems(Item itemA, Item itemB);
        //void SwapItems(Item itemA, int slotIndexB);
        //void SwapItems(int slotIndexA, Item itemB);
    }
}



