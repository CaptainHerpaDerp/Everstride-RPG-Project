using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using System.Collections;

namespace UIElements
{ 
    public class SkillPointsText : MonoBehaviour
    {
        [BoxGroup("Visual Components"), SerializeField] private TextMeshProUGUI skillPointsText;

        [BoxGroup("Settings"), SerializeField] private float fadeDuration = 0.5f;
        [BoxGroup("Settings"), SerializeField] private float minAlpha;

        private void OnEnable()
        {
            StartCoroutine(FadeTextCR());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator FadeTextCR()
        {
            float elapsedTime = 0;
            bool fadingOut = false;

            while (elapsedTime <= fadeDuration)
            {
                elapsedTime += Time.deltaTime;

                float alpha = 0;

                if (fadingOut)
                {
                    alpha = Mathf.Lerp(1, minAlpha, elapsedTime / fadeDuration);

                    if (alpha <= minAlpha)
                    {
                        fadingOut = false;
                        elapsedTime = 0;
                    }
                }
                else
                {
                    alpha = Mathf.Lerp(minAlpha, 1, elapsedTime / fadeDuration);

                    if (alpha >= 1)
                    {
                        fadingOut = true;
                        elapsedTime = 0;
                    }
                }

                skillPointsText.color = new Color(skillPointsText.color.r, skillPointsText.color.g, skillPointsText.color.b, alpha);

                yield return null;
            }
        }
    }
}