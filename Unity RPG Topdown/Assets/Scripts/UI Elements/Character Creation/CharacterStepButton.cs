using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements.CharacterCreation
{
    public class RaceStepButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [BoxGroup("Element References"), SerializeField] private GameObject highlightObject;
        [BoxGroup("Element References"), SerializeField] private TextMeshProUGUI nameText;

        [BoxGroup("Settings"), SerializeField] private Color highlightedColorText;
        [BoxGroup("Settings"), SerializeField] private Color defaultColorText;

        // Debug
        bool toggled;

        private void Start()
        {
            // Set the text to be all lowercase
            nameText.fontStyle = FontStyles.Normal;
            nameText.color = defaultColorText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            toggled = !toggled;

            if (toggled)
            {
                // Set the text to be all uppercase
                nameText.fontStyle = FontStyles.UpperCase;
                nameText.color = highlightedColorText;
            }
            else
            {
                // Set the text to be all lowercase
                nameText.fontStyle = FontStyles.Normal;
                nameText.color = defaultColorText;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightObject.SetActive(true);
            nameText.color = highlightedColorText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightObject.SetActive(false);

            if (!toggled)
                nameText.color = defaultColorText;
        }
    }
}