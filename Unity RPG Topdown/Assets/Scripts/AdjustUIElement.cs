using UnityEngine;

public class AdjustUIElement : MonoBehaviour
{
    public RectTransform uiElement; // Reference to your UI element.
    public Vector2 targetPosition;   // Desired position for your UI element.

    void Start()
    {
        // Check the current screen resolution.
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Calculate the position adjustment based on the reference resolution (e.g., 1920x1080).
        float xAdjustment = screenWidth / 1920f; // Adjust based on your reference width.
        float yAdjustment = screenHeight / 1080f; // Adjust based on your reference height.

        // Apply the adjustment to the target position.
        Vector2 adjustedPosition = new Vector2(targetPosition.x * xAdjustment, targetPosition.y * yAdjustment);

        // Set the UI element's position.
        uiElement.anchoredPosition = adjustedPosition;
    }
}
