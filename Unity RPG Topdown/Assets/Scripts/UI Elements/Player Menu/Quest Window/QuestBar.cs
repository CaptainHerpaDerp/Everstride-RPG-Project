using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UIElements
{
    /// <summary>
    /// Represents the quest bar in the quest menu
    /// </summary>
    public class QuestBar : MonoBehaviour
    {
        [BoxGroup("Components"), SerializeField] private TextMeshProUGUI questNameTextComponent;
        [BoxGroup("Components"), SerializeField] private Button questButton;
        [BoxGroup("Components"), SerializeField] private Sprite questBarDefaultSprite;
        [BoxGroup("Components"), SerializeField] private Sprite questBarSelectedSprite;


        private void Start()
        {
            if (questButton == null && !TryGetComponent(out questButton))
            {
                Debug.LogError("Error in QuestBar: Quest Button not found!");
            }
        }

        public void SetQuestTracked(bool isTracked)
        {
            if (isTracked)
            {
                questButton.image.sprite = questBarSelectedSprite;
            }
            else
            {
                questButton.image.sprite = questBarDefaultSprite;
            }
        }

        // Public Getters
        public string QuestName
        {
            set
            {
                questNameTextComponent.text = value;
            }
        }

        public Button Button
        {
            get
            {
                return questButton;
            }
        }
    }
}
