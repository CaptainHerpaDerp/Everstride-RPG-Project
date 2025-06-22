using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextObjectManager : MonoBehaviour
{

    public GameObject textContainer;
    public GameObject textPrefab;

    private List<TextObject> textObjects = new List<TextObject>();

    private void Update()
    {
        foreach(TextObject txt in textObjects)
        {
            txt.UpdateFloatingText();
        }
    }

    public void Show(string text, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        TextObject textObject = GetTextObject();
        textObject.text.text = text;
        textObject.text.fontSize = fontSize;
        textObject.text.color = color;
        textObject.textObject.transform.position = Camera.main.WorldToScreenPoint(position);
        textObject.motion = motion;
        textObject.duration = duration;

        textObject.Show();

    }

    private TextObject GetTextObject()
    {
        TextObject txt = textObjects.Find(t => !t.active);

       if (txt == null)
        {
            txt = new TextObject();
            txt.textObject = Instantiate(textPrefab);
            txt.textObject.transform.SetParent(textContainer.transform);
            txt.text = txt.textObject.GetComponent<Text>();

            textObjects.Add(txt);
        }

        return txt;

    }


}
