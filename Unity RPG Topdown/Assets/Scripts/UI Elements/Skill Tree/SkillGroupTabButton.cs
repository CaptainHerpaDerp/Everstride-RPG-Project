using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.EventSystems;

namespace UIElements.SkillWindow
{
    public class SkillGroupTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [BoxGroup("Element References"), SerializeField] private GameObject highlightObject;
        [BoxGroup("Element References"), SerializeField] private TextMeshProUGUI tabText;

        [BoxGroup("Settings"), SerializeField] private Color highlightedColorText;
        [BoxGroup("Settings"), SerializeField] private Color defaultColorText;

        // Debug
        bool toggled;

        private void Start()
        {
            // Set the text to be all lowercase
            tabText.fontStyle = FontStyles.Normal;
            tabText.color = defaultColorText;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            toggled = !toggled;

            if (toggled)
            {
                // Set the text to be all uppercase
                tabText.fontStyle = FontStyles.UpperCase;
                tabText.color = highlightedColorText;
            }
            else
            {
                // Set the text to be all lowercase
                tabText.fontStyle = FontStyles.Normal;
                tabText.color = defaultColorText;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightObject.SetActive(true);
            tabText.color = highlightedColorText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightObject.SetActive(false);

            if (!toggled)
                tabText.color = defaultColorText;
        }
    }
}