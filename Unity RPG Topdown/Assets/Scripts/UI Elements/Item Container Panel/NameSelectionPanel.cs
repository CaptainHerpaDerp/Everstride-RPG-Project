using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameSelectionPanel : MonoBehaviour
{
    [BoxGroup("Components"), SerializeField] private TextMeshProUGUI nameTextComponent;
    [BoxGroup("Components"), SerializeField] private Image selectionPanelImage;

    public string Name
    {
        set
        {
            nameTextComponent.text = value;
        }
    }

    public void SetSelected(bool isSelected)
    {
        selectionPanelImage.enabled = isSelected;
    }

}
