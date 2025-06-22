using Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class House : MonoBehaviour
{
    [SerializeField] List<Tilemap> fadingTilemapGroups = new();
    [SerializeField] BuildingTrigger trigger;
    [SerializeField] float fadeDuration = 1.0f; // Adjust this as needed.

    private void Start()
    {
        trigger.onEnter += OnEntryTriggerEnter;
        trigger.onExit += OnEntryTriggerExit;
    }

    private void OnEntryTriggerEnter()
    {
        StartCoroutine(FadeLayers(1.0f, 0.0f));
    }

    private void OnEntryTriggerExit()
    {
        if (gameObject.activeInHierarchy)
        StartCoroutine(FadeLayers(0f, 1.0f));
    }

    private IEnumerator FadeLayers(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            foreach (var tileGroup in fadingTilemapGroups)
            {
                tileGroup.color = new Color(1f, 1f, 1f, Mathf.Lerp(startAlpha, targetAlpha, t));
            }

            yield return null;
        }
        
        // Ensure the final alpha value is exactly the target alpha.
        foreach (var tileGroup in fadingTilemapGroups)
        {
            tileGroup.color = new Color(1f, 1f, 1f, targetAlpha);
        }
    }
}
