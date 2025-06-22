using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Tracks key variables across the game
    /// </summary>
    public class GameStateManager : Singleton<GameStateManager>
    {
        [field: SerializeField] Dictionary<string, object> gameState = new Dictionary<string, object>();

        /// <summary>
        /// If a the given key exists in the game state, set it to the given value. Otherwise, add it to the game state with the given value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetGameState(string key, object value)
        {
            if (gameState.ContainsKey(key))
            {
                gameState[key] = value;
            }
            else
            {
                gameState.Add(key, value);
            }

            Debug.Log($"Set game state key {key} to value {value}");
        }

        /// <summary>
        /// Return the value of the given key in the game state as a boolean. If the key does not exist or if it isn't a boolean, return null.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetGameStateBool(string key)
        {
            if (gameState.ContainsKey(key))
            {
                if (gameState[key] is bool)
                {
                    return (bool)gameState[key];
                }
                else
                {
                    Debug.LogError("Key is not a boolean");
                    return false;
                }

            }
            else
            {
                return false;
            }
        }    
    }
}