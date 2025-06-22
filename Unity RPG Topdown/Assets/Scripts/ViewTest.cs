using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewTest : MonoBehaviour
{
    [SerializeField] Transform viewerTransform;
    [SerializeField] Transform playerTransform;

    [Range(0f, 1f)]
    public float precision = 0.5f;


    private void OnDrawGizmos()
    {
        Vector2 center = viewerTransform.position;
        Vector2 playerPos = playerTransform.position;
        Vector2 playerLookDirection = playerTransform.right; //x axis

        Vector2 viewDir = (center - playerPos).normalized; // try non normalzied

        float lookness = Vector2.Dot(viewDir, playerLookDirection);

        bool isLooking = lookness >= precision;

        Gizmos.color = isLooking ? Color.green : Color.red;
        Gizmos.DrawLine(playerPos, playerPos + viewDir);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerPos, playerPos + playerLookDirection);

    }
}
