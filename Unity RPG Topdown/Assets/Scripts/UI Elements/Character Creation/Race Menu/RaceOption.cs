using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements.CharacterCreation.RaceMenu
{
    public class RaceOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [BoxGroup("Visual Components"), SerializeField] private GameObject selectedBorder;
        [BoxGroup("Visual Components"), SerializeField] private GameObject hoverBorder;

        // Debug
        bool selected = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            selected = !selected;
            selectedBorder.SetActive(selected);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverBorder.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverBorder.SetActive(false);
        }
    }
}