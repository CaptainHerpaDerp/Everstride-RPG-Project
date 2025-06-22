using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// Tracks all characters within a certain radius of the object this script is attached to.
    /// </summary>
    public class CharacterRadiusTracker : MonoBehaviour
    {
        private List<Character> characters = new();

        public List<Character> Characters { get => characters; }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null)
                return;

            if (collision.gameObject.TryGetComponent(out Character outCharacter) && !characters.Contains(outCharacter))
            {
                characters.Add(outCharacter);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null)
                return;

            if (collision.gameObject.TryGetComponent(out Character outCharacter) && characters.Contains(outCharacter))
            {
                characters.Remove(outCharacter);
            }
        }
    }
}