using UnityEngine;
using TMPro;

namespace UIElements
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI
            energyText, hungerText, moneyText,
            jobText;

        public void UpdateJobText(string text)
        {
            jobText.text = text;
        }

        public void UpdateStatsText(float energy, float hunger, float money)
        {
            energyText.text = "Energy: " + energy;
            hungerText.text = "Hunger: " + hunger;
            moneyText.text = "Money: " + money;
        }
    }
}
