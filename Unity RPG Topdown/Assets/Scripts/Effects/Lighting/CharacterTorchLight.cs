using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Effects.Lighting
{
    public class CharacterTorchLight : MonoBehaviour
    {
        [SerializeField] private Light2D torchLight;

        [SerializeField] private float flickerSpeed = 0.1f;
        [SerializeField] private float maxIntensity = 1.1f;
        [SerializeField] private float minIntensity = 0.9f;

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(DoTorchLight());
        }

        private IEnumerator DoTorchLight()
        {
            // Every millisecond, change the intensity of the torch light very slightly to mimic a flickering fire effect

            while (true)
            {
                torchLight.intensity = Random.Range(minIntensity, maxIntensity);
                yield return new WaitForSeconds(flickerSpeed);
            }
        }

    }
}
