using TMPro;
using UnityEngine;

namespace UIElements
{
    public class EquipmentTotals : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI physicalDamageText, physicalResistanceText;

        public void ReloadTotals(float physicalDamage, float physicalResistance)
        {
            physicalDamageText.text = physicalDamage.ToString();
            physicalResistanceText.text = physicalResistance.ToString();
        }
    }
}