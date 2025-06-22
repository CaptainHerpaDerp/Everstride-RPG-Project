using UnityEngine;
using UnityEngine.UI;

public class ExpandableBackground : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private float padding = 10f;

    private void UpdateBackgroundSize()
    {
        int optionCount = layoutGroup.transform.childCount;
        float totalHeight = layoutGroup.padding.top + layoutGroup.padding.bottom + padding;

        for (int i = 0; i < optionCount; i++)
        {
            RectTransform child = layoutGroup.transform.GetChild(i) as RectTransform;
            totalHeight += LayoutUtility.GetPreferredHeight(child) + layoutGroup.spacing;
        }

        backgroundRectTransform.sizeDelta = new Vector2(backgroundRectTransform.sizeDelta.x, totalHeight);
    }

    private void OnEnable()
    {
        UpdateBackgroundSize();
    }

    public void AddResponseOption(Transform responseOption)
    {
        responseOption.SetParent(layoutGroup.transform, false);
        UpdateBackgroundSize();
    }
}
