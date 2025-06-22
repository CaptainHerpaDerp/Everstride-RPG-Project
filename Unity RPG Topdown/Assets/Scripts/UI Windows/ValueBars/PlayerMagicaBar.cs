
namespace UIWindows
{
    public class PlayerMagicaBar : ValueBar
    {
        protected override void Start()
        {
            base.Start();

            InitializeValueBar(parentCharacter.MagicaMax);
        }

        protected override void OnEnable()
        {
            // Listen to all events that will affect the health bar.
            parentCharacter.OnUpdateMagicaBar += UpdateValueBar;
            parentCharacter.OnHideHealthBar += HideValueBar;
            parentCharacter.OnShowHealthBar += ShowValueBar;
        }

        protected override void OnDisable()
        {
            // Stop listening to all events that will affect the health bar.
            parentCharacter.OnUpdateMagicaBar -= UpdateValueBar;
            parentCharacter.OnHideHealthBar -= HideValueBar;
            parentCharacter.OnShowHealthBar -= ShowValueBar;
        }
    }
}
