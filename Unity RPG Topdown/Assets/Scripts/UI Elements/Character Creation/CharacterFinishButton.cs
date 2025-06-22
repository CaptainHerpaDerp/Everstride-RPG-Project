using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements.CharacterCreation
{
    public class CharacterFinishButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [BoxGroup("Element References"), SerializeField] private GameObject highlightObject;
        [BoxGroup("Element References"), SerializeField] private TextMeshProUGUI finishText;

        [BoxGroup("Settings"), SerializeField] private Color highlightedColorText;
        [BoxGroup("Settings"), SerializeField] private Color defaultColorText;

        // Debug
        bool toggled;
         
        private void Start()
        {
            // Set the text to be all lowercase
            finishText.fontStyle = FontStyles.Normal;
            finishText.color = defaultColorText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            toggled = !toggled;

            if (toggled)
            {
                // Set the text to be all uppercase
                finishText.fontStyle = FontStyles.UpperCase;
                finishText.color = highlightedColorText;
            }
            else
            {
                // Set the text to be all lowercase
                finishText.fontStyle = FontStyles.Normal;
                finishText.color = defaultColorText;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightObject.SetActive(true);
            finishText.color = highlightedColorText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightObject.SetActive(false);

            if (!toggled)
                finishText.color = defaultColorText;
        }
    }
}

