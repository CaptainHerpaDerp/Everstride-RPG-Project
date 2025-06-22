using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters;

namespace UIWindows
{
    public abstract class ValueBar : MonoBehaviour
    {
        protected float maxValue;
        protected float currentValue;

        [SerializeField] protected RectTransform borderRectTransform;
        [SerializeField] protected RectTransform foregroundRectTransform;

        protected List<RectTransform> rectElements = new List<RectTransform>();

        protected float fullWidth;
        protected float TargetWidth => currentValue * fullWidth / maxValue;

        [SerializeField] protected float animationSpeed = 1f;

        protected float fadeSpeed = 1f; // Adjust this for fade speed.
        protected float fadeTimer = 5f; // Adjust this for how long the bar stays visible.

        protected Coroutine adjustBarWidthCR;
        protected Coroutine fadeOutCR;

        [SerializeField] protected Character parentCharacter;

        protected virtual void Start()
        {
            if (parentCharacter == null)
            {
                Debug.LogError("Error in Value Bar, parentCharacter is null!");
            }
        }

        protected abstract void OnEnable();

        protected abstract void OnDisable();

        protected virtual void Awake()
        {
            rectElements.Add(borderRectTransform);
            rectElements.Add(foregroundRectTransform);

            fullWidth = foregroundRectTransform.rect.width;
        }

        public virtual void InitializeValueBar(float maxValue)
        {
            this.maxValue = maxValue;
            currentValue = maxValue;

            foregroundRectTransform.sizeDelta = new Vector2(TargetWidth, foregroundRectTransform.rect.height);

           // InitializeValueBar(parentCharacter.HitPointsMax);

            //HideValueBar(); // Initially, hide the Value bar.
        }

        protected virtual IEnumerator AdjustBarWidth(float amount)
        {
            ShowValueBar();

            if (fadeOutCR != null)
            {
                StopCoroutine(fadeOutCR);
            }

            foregroundRectTransform.sizeDelta = new Vector2(TargetWidth, foregroundRectTransform.rect.height);

            if (fadeOutCR != null)
            {
                StopCoroutine(fadeOutCR);
            }

            //fadeOutCR = StartCoroutine(FadeOutValueBar());

            yield break;
        }

        protected IEnumerator FadeOutValueBar()
        {
            yield return new WaitForSeconds(fadeTimer);

            float timer = 0f; // Shared timer for all elements.

            while (timer < 0.25f)
            {
                timer += Time.deltaTime * fadeSpeed;

                foreach (var element in rectElements)
                {
                    // Fade out the Value bar gradually.
                    float startAlpha = element.GetComponent<Image>().color.a;
                    Color newColor = element.GetComponent<Image>().color;
                    newColor.a = Mathf.Lerp(startAlpha, 0f, timer);
                    element.GetComponent<Image>().color = newColor;
                }

                yield return null;
            }

            HideValueBar();
        }


        protected void ShowValueBar()
        {
            foreach (var element in rectElements)
            {
                Color newColor = element.GetComponent<Image>().color;
                newColor.a = 1f; // Fully visible.
                element.GetComponent<Image>().color = newColor;
            }
        }

        protected virtual void HideValueBar()
        {
            foreach (var element in rectElements)
            {
                Color newColor = element.GetComponent<Image>().color;
                newColor.a = 0f; // Fully transparent.
                element.GetComponent<Image>().color = newColor;
            }
        }


        public void UpdateValueBar(float newNPCValue)
        {           
            float amount = newNPCValue - currentValue;

            if (amount == 0)
            {
                return;
            }

            currentValue = Mathf.Clamp(currentValue + amount, 0, maxValue);

            if (adjustBarWidthCR != null)
            {
                StopCoroutine(adjustBarWidthCR);
            }

            adjustBarWidthCR = StartCoroutine(AdjustBarWidth(amount));
        }
    }
}