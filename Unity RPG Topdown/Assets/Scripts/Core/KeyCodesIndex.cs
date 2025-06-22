using UnityEngine;

namespace Core
{
    /// <summary>
    /// A class that contains all the mappings of the keycodes used in the game.
    /// </summary>
    public static class KC
    {
        #region Movement

        public static KeyCode MoveUp = KeyCode.W;
        public static KeyCode MoveDown = KeyCode.S;
        public static KeyCode MoveLeft = KeyCode.A;
        public static KeyCode MoveRight = KeyCode.D;

        public static KeyCode Sprint = KeyCode.LeftShift;

        #endregion

        #region Combat

        public static KeyCode Attack = KeyCode.Mouse0;
        public static KeyCode Block = KeyCode.Mouse1;
        public static KeyCode SheathKey = KeyCode.R;

        #endregion

        #region Interaction

        public static KeyCode Interact = KeyCode.F;

        #endregion

        #region Inventory Management 

        public static KeyCode OpenInventoryKey = KeyCode.Tab;

        #endregion
    }
}
