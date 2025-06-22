using System.Collections;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Core
{
    public static class Utils
    {
        /// <summary>
        /// Waits a given period of time and executes an action
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator WaitDurationAndExecute(float duration, Action action)
        {
            yield return new WaitForSeconds(duration);
            action.Invoke();
        }

        public static float EaseInOut(float t)
        {
            return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        }

        #region Particle Effects

        /// <summary>
        /// Plays a particle effect along will all of its child particle effects
        /// </summary>
        /// <param name="particleSystem"></param>
        public static void PlayParticleEffect(ParticleSystem particleSystem)
        {
            particleSystem.Play();

            foreach (Transform child in particleSystem.transform)
            {
                // Ignore the child if it's not active
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                if (child.TryGetComponent(out ParticleSystem childParticleSystem))
                {
                    childParticleSystem.Play();
                }

                if (child.TryGetComponent(out Light childLight))
                {
                    childLight.enabled = true;
                }
            }
        }

        public static void PlayParticleEffect(Transform particleSystemTransform)
        {
            if (particleSystemTransform.TryGetComponent(out ParticleSystem particleSystem))
            {
                PlayParticleEffect(particleSystem);
            }
            else
            {
                Debug.LogError("No particle system found on transform!");
            }
        }

        /// <summary>
        /// Stops a particle effect along will all of its child particle effects
        /// </summary>
        /// <param name="particleSystem"></param>
        public static void StopParticleEffect(ParticleSystem particleSystem)
        {
            particleSystem.Stop();

            foreach (Transform child in particleSystem.transform)
            {
                // Ignore the child if it's not active
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                if (child.TryGetComponent(out ParticleSystem childParticleSystem))
                {
                    childParticleSystem.Stop();
                }

                if (child.TryGetComponent(out Light childLight))
                {
                    childLight.enabled = false;
                }
            }
        }


        public static void StopParticleEffect(Transform particleSystemTransform)
        {
            if (particleSystemTransform.TryGetComponent(out ParticleSystem particleSystem))
            {
                StopParticleEffect(particleSystem);
            }
            else
            {
                Debug.LogError("No particle system found on transform!");
            }
        }

        #endregion

        #region NavMesh

        public static Vector3 SampleNavMesh(Vector3 position, float maxDistance = 1)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                return Vector3.zero;
            }
        }

        #endregion

        public static bool TryCast<T>(object obj, out T result)
        {
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        #region UI Methods

        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float fadeTime = 0.1f, bool fadingIn = true, Action afterFadeAction = null)
        {
            if (fadingIn)
            {
                // Fade in all inventory elements simultaneously
                float timer = 0f;
                while (timer < fadeTime)
                {
                    // Get the alpha for the current time
                    float alpha = Mathf.Lerp(0f, 1f, timer / fadeTime);

                    canvasGroup.alpha = alpha;

                    timer += Time.fixedUnscaledDeltaTime;
                    yield return new WaitForFixedUpdate();
                }

                // Set the alpha to 1 to ensure that the canvas group is fully visible
                canvasGroup.alpha = 1f;

                // Make the canvas group interactable
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                if (afterFadeAction != null)
                {
                    afterFadeAction();
                }
            }

            else
            {
                // Fade out inventory elements simultaneously
                float timer = 0f;
                while (timer < fadeTime)
                {
                    // Get the alpha for the current time
                    float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

                    canvasGroup.alpha = alpha;

                    timer += Time.fixedUnscaledDeltaTime;
                    yield return new WaitForFixedUpdate();
                }

                // Set the alpha to 0 to ensure that the canvas group has fully faded out
                canvasGroup.alpha = 0f;

                // Make the canvas group non-interactable
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                if (afterFadeAction != null)
                {
                    afterFadeAction();
                }
            }
        }

        #endregion
    }
}