using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;

namespace UIElements
{
    /// <summary>
    /// Represents a single objective in a quest
    /// </summary>
    public class QuestObjective : MonoBehaviour
    {
        [BoxGroup("Components"), SerializeField] private TextMeshProUGUI objectiveTextComponent;
        [BoxGroup("Components"), SerializeField] private GameObject checkBoxObject;

        [BoxGroup("Text Colors"), SerializeField] private Color defaultTextColor = new Color(1, 1, 1);
        [BoxGroup("Text Colors"), SerializeField] private Color completedTextColor = new Color(1, 1, 1, 0.5f);

        public string ObjectiveText
        {
            get => objectiveTextComponent.text;
            set => objectiveTextComponent.text = value;
        }

        public void SetObjectiveComplete(bool isComplete)
        {
            checkBoxObject.SetActive(isComplete);

            if (isComplete)
            {
                objectiveTextComponent.color = completedTextColor;
            }
            else
            {
                objectiveTextComponent.color = defaultTextColor;
            }
        }
    }
}
