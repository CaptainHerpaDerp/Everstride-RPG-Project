
namespace UIWindows
{
    public class PlayerHealthBar : ValueBar
    {
        protected override void Start()
        {
            base.Start();

            InitializeValueBar(parentCharacter.HitPointsMax);
        }

        protected override void OnEnable()
        {
            // Listen to all events that will affect the health bar.
            parentCharacter.OnUpdateHealthBar += UpdateValueBar;
            parentCharacter.OnHideHealthBar += HideValueBar;
            parentCharacter.OnShowHealthBar += ShowValueBar;
        }

        protected override void OnDisable()
        {
            // Stop listening to all events that will affect the health bar.
            parentCharacter.OnUpdateHealthBar -= UpdateValueBar;
            parentCharacter.OnHideHealthBar -= HideValueBar;
            parentCharacter.OnShowHealthBar -= ShowValueBar;
        }
    }
}
