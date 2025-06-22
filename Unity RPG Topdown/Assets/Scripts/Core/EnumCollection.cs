namespace Core.Enums
{
    #region Character Enums

    public enum CharacterState { Normal, Attacking, Blocking, Hit, Death }
    public enum ViewDirection { TopLeft, TopRight, BottomLeft, BottomRight }
    public enum AttackDirection { Up, Down, Left, Right, BottomRight, BottomLeft, TopRight, TopLeft }

    #endregion


    public enum ConsumableType
    {
        HealthPotion,
        ManaPotion,
        Food
    }

    public enum WeaponMode
    {
        Slash,
        Swing,
        Thrust,
        TwoHanded,
        Ranged,
        Spell
    }

    public enum WeaponType
    {
        Unarmed,
        Sword,
        LongSword,
        Axe,
        Mace,
        Flail,
        Whip,
        Bow,
        Crossbow,
        SlingShot,
        Dagger,
        Spear,
        Staff,
        Wand,
        Shield,
        Book
    }

    public enum ArmourType
    {
        Head,
        Chest,
        Feet,
        Neck,
        Ring,
    }

    public enum SpellType
    {
        Fire,
        Ice,
        Wind,
        Earth
    }

    public enum Faction
    {
        None,
        Orc,
        Wolf,
        Bat,
        CaveCreatures,
        PlayerFriendly
    }

    public enum StatusEffect
    {
        Bleeding,
        Fear,
        Fire,
        Ice,
        Nature,
        Petrification,
        Poison,
        Shock,
        Sickness,
        Sleep,
        Stun,
        TempSlow
    }

    public enum ActionType
    {
        Talk,
        Loot,
        Use,
        Craft
    }

    public enum QuestTypes
    {
        Kill
    }

    public enum DialogueEndEvent
    {
        None,
        ExitDialogue,
        InitiateCombat,
        OpenShop,
        PlayerRecruitFollower,
        InitiateRomance,
    }

    public enum TabType
    {
        Inventory,
        Equipment,
        Skills,
        Quests,
        Map,
        Magic
    }
}
