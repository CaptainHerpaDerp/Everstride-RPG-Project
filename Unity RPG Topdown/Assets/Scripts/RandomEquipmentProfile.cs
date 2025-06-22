using System.Collections.Generic;
using UnityEngine;
using Items;

[CreateAssetMenu(fileName = "New Equipment Profile", menuName = "Equipment Profile")]
public class RandomEquipmentProfile : ScriptableObject
{
    public string profileName;

    // Helmets
    public List<Item> PossibleHelmets = new();
    public List<int> HelmetLikelihoods = new();

    // Chestplates
    public List<Item> PossibleArmour = new();
    public List<int> ArmourLikelihoods = new();

    // Boots
    public List<Item> PossibleBoots = new();
    public List<int> BootLikelihoods = new();

    // Weapons
    public List<Item> PossibleWeapons = new();
    public List<int> WeaponLikelihoods = new();

    // Misc Items
    public List<Item> PossibleMiscItems = new();
    public List<int> MiscItemLikelihoods = new();

    [Header("The amount of misc items that will be added")]
    public int MiscItemCount;

    public int goldRangeMin, goldRangeMax;
}
