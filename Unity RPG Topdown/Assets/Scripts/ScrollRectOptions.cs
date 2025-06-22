using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollRectOptions : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;

    private void Start()
    {
        // Get the Scroll Rect component attached to this GameObject.
        scrollRect = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("mark");

        // Disable the Scroll Rect's dragging when a drag begins.
        scrollRect.velocity = Vector2.zero;
        scrollRect.StopMovement();
        scrollRect.enabled = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable the Scroll Rect's dragging when the drag ends.
        scrollRect.enabled = true;
    }

}
