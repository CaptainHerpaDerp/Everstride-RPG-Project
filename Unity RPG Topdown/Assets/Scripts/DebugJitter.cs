using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugJitter : MonoBehaviour
{
    [SerializeField] private float maxStep, radius;

    [SerializeField] private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DoJitter());
    }

    private IEnumerator DoJitter()
    {
        while (true)
        {
           Vector2 diff = (transform.position - target.position);

            float currentAngleDifference = Mathf.Atan2(diff.y, diff.x);

            // Random angle in radians
            float angle = currentAngleDifference + Random.Range(-maxStep, maxStep);

            // Random distance within [radius - maxStep, radius + maxStep]
            float distance = radius + Random.Range(-maxStep, maxStep);

            // Calculate offset from target in XZ plane
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

            // Calculate world position
            Vector3 jitterPosition = target.position + offset;

            // Draw debug line from target to jitter position
            Debug.DrawLine(target.position, jitterPosition, Color.yellow, 0.1f);

            // Wait a short time before next jitter
            yield return new WaitForSeconds(0.1f);
        }
    }
}
