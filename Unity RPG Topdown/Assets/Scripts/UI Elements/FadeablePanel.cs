using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{
    public class FadeablePanel : MonoBehaviour
    {
        private const float defaultFadeTime = 0.25f;
        private float fadeTime;

        public void FadeIn(Transform elementsGroup, float fadeTime = defaultFadeTime)
        {
            StopAllCoroutines();

            this.fadeTime = fadeTime;
            StartCoroutine(FadeInCR(elementsGroup));
        }

        public void FadeIn(GameObject element, float fadeTime = defaultFadeTime)
        {
            StopAllCoroutines();

            this.fadeTime = fadeTime;
            StartCoroutine(FadeInCR(element.transform));
        }

        public void FadeOut(Transform elementsGroup, float fadeTime = defaultFadeTime)
        {
            StopAllCoroutines();

            this.fadeTime = fadeTime;
            StartCoroutine(FadeOutCR(elementsGroup));
        }

        public void FadeOut(GameObject element, float fadeTime = defaultFadeTime)
        {
            StopAllCoroutines();

            this.fadeTime = fadeTime;
            StartCoroutine(FadeOutCR(element.transform));
        }

        private IEnumerator FadeInCR(Transform elementsGroup)
        {
            Image[] images = elementsGroup.GetComponentsInChildren<Image>();

            elementsGroup.gameObject.SetActive(true);

            // Fade in all inventory elements simultaneously
            float time = 0;
            while (time < fadeTime)
            {
                float fadeAmount = Mathf.Lerp(0, 1, time / fadeTime);

                foreach (Image element in images.ToList())
                {
                    if (element == null)
                    {
                        continue;
                    }

                    Color newColor = element.color;
                    newColor = new Color(newColor.r, newColor.g, newColor.b, fadeAmount);
                    element.color = newColor;
                }

                time += Time.fixedUnscaledDeltaTime;

                yield return null;
            }

            // Fully fade in the elements
            foreach (Image element in images)
            {
                if (element == null)
                {
                    continue;
                }

                // Fully fade in the element
                element.color = new Color(element.color.r, element.color.g, element.color.b, 1);
            }
        }

        private IEnumerator FadeOutCR(Transform elementsGroup)
        {
            Image[] images = elementsGroup.GetComponentsInChildren<Image>();

            // Fade in all inventory elements simultaneously
            float time = 0;
            while (time < fadeTime)
            {
                float fadeAmount = Mathf.Lerp(1, 0, time / fadeTime);

                foreach (Image element in images.ToList())
                {
                    if (element == null)
                    {
                        continue;
                    }

                    Color newColor = element.color;
                    newColor = new Color(newColor.r, newColor.g, newColor.b, fadeAmount);
                    element.color = newColor;
                }

                time += Time.fixedUnscaledDeltaTime;

                yield return null;
            }

            // Fully fade in the elements
            foreach (Image element in images)
            {
                if (element == null)
                {
                    continue;
                }

                // Fully fade in the element
                element.color = new Color(element.color.r, element.color.g, element.color.b, 0);
            }

            // Deactivate the elements
            elementsGroup.gameObject.SetActive(false);
        }
    }
}