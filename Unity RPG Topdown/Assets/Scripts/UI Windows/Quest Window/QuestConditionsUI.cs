using Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UIWindows
{
    /// <summary>
    /// Displays Quest Conditions in the UI
    /// </summary>
    public class QuestConditionsUI : Singleton<QuestConditionsUI>
    {
        [SerializeField] private TextMeshProUGUI questPointTextComponentPrefab;

        [SerializeField] private Transform questPointParent;

        private Dictionary<string, TextMeshProUGUI> questNamePairs = new();

        [Header("The color of a quest point that isn't completed")]
        [SerializeField] private Color defaultQuestColor;

        public void ClearQuestPoints()
        {
            foreach (var questPoint in questNamePairs)
            {
                Destroy(questPoint.Value.gameObject);
            }

            questNamePairs.Clear();
        }

        public void AddQuestPoint(string questName)
        {
            TextMeshProUGUI questPointTextComp = Instantiate(questPointTextComponentPrefab, questPointParent);
            questPointTextComp.text = questName;
            questPointTextComp.gameObject.SetActive(true);

            questNamePairs.Add(questName, questPointTextComp);
        }

        public void CompleteQuestPoint(string questName)
        {
            questNamePairs[questName].color = Color.green;
        }

        public void SetQuestPointUncomplete(string questName)
        {
            questNamePairs[questName].color = defaultQuestColor;
        }

        public void FailQuestPoint(string questName)
        {
            questNamePairs[questName].color = Color.red;
        }
    }
}