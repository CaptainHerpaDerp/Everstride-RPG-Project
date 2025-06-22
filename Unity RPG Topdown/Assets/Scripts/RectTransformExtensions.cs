using UnityEngine;

public static class RectTransformExtensions
{
    public static void SetWidth(this RectTransform rectTransform, float width)
    {
        rectTransform.sizeDelta = new Vector2(width, rectTransform.rect.height);
    }
}
