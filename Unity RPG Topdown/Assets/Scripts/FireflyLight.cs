using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireflyLight : MonoBehaviour
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private float fadeInTime;
    [SerializeField] private float lightLifetime;

    private float targetIntensity;

    void OnEnable()
    {
        targetIntensity = light2D.intensity;

        StartCoroutine(StartLightFadeIn());
    }

    private IEnumerator StartLightFadeIn()
    {
        light2D.intensity = 0;

        while (light2D.intensity < targetIntensity)
        {
            light2D.intensity += Time.deltaTime / fadeInTime;
            yield return null;
        }

        StartCoroutine(StartLightFadeOut());
    }

    private IEnumerator StartLightFadeOut()
    {
        yield return new WaitForSeconds(lightLifetime);

        while (light2D.intensity > 0)
        {
            light2D.intensity -= Time.deltaTime / fadeInTime;
            yield return null;
        }

        StartCoroutine(StartLightFadeIn());
    }
}
