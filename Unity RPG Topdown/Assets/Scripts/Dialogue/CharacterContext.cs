using System.Collections.Generic;
using UnityEngine;
using PlayerShopping;
using Core.Enums;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "CharacterContext", menuName = "Custom/CharacterContext")]
    public class CharacterContext : ScriptableObject
    {
        [SerializeField] private TextAsset ContextFile;

        public List<DialogueEndEvent> dialogueEndEvents = new();

        public string characterName;

        public ShopData shopData;

        public string ContextText => (ContextFile == null) ? "Context file is not assigned!" : ContextFile.text;
    }
}