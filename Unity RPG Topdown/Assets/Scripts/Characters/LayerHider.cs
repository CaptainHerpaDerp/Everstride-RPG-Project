using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Characters.Visual
{
    public class LayerHider : MonoBehaviour
    {
        [SerializeField] float fadeDuration = 1.0f; // Adjust this as needed.
        [SerializeField] float delayAfterFade = 0.1f; // Adjust this as needed.

        Tilemap tileMap;
        Color originalColor;

        private void Start()
        {
            tileMap = GetComponent<Tilemap>();
            originalColor = tileMap.color;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() == null) return;

            StartCoroutine(FadeLayers(1.0f, 0.0f));
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() == null) return;

            StartCoroutine(FadeLayers(0f, 1.0f));
        }

        private IEnumerator FadeLayers(float startAlpha, float targetAlpha)
        {
            float elapsedTime = 0f;
            Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, startAlpha);
            Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);

                tileMap.color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

            tileMap.color = targetColor;

            // Wait for a short delay to prevent the sudden jump in opacity.
            yield return new WaitForSeconds(delayAfterFade);

            // Do something after the fade is completed (if needed).
        }
    }
}