using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements.CharacterCreation.RaceMenu
{
    public class SkillOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [BoxGroup("Visual Components"), SerializeField] private GameObject hoverBorder;

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