using UnityEngine;
using UnityEngine.UI;

public class TextObject
{
    public bool active;
    public GameObject textObject;
    public Text text;
    public Vector3 motion;
    public float duration;
    public float lastShown;

    public void Show()
    {
        active = true;
        lastShown = Time.time;
        textObject.SetActive(true);
    }

    public void Hide()
    {
        active = false;
        textObject.SetActive(false);
    }

    public void UpdateFloatingText()
    {
        if (!active) return;

        if(Time.time - lastShown > duration)
        {
            Hide();
        }

        textObject.transform.position += motion * Time.deltaTime;
    }
}
