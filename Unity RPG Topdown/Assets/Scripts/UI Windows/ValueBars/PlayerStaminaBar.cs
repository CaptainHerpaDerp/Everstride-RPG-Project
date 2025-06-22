
namespace UIWindows
{
    public class PlayerStaminaBar : ValueBar
    {
        protected override void Start()
        {
            base.Start();

            InitializeValueBar(parentCharacter.StaminaMax);
        }

        protected override void OnEnable()
        {
            // Listen to all events that will affect the health bar.
            parentCharacter.OnUpdateStaminaBar += UpdateValueBar;
            parentCharacter.OnHideHealthBar += HideValueBar;
            parentCharacter.OnShowHealthBar += ShowValueBar;
        }

        protected override void OnDisable()
        {
            // Stop listening to all events that will affect the health bar.
            parentCharacter.OnUpdateStaminaBar -= UpdateValueBar;
            parentCharacter.OnHideHealthBar -= HideValueBar;
            parentCharacter.OnShowHealthBar -= ShowValueBar;
        }
    }
}
