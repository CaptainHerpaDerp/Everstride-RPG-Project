using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance { get; private set; }

    public bool IsLocked;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private float maxZoom;

    [SerializeField] private float zoomInterval;

    private float zoomLevel 
    {
        get { return virtualCamera.m_Lens.OrthographicSize; }
        set
        {
            virtualCamera.m_Lens.OrthographicSize = value;
        }
    }

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("Duplicate CameraZoom instance detected, destroying this instance.");
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
            Debug.LogError("No virtual camera assigned/found on CameraZoom script.");

        virtualCamera.m_Lens.OrthographicSize = maxZoom;
    }


    private void Update()
    {
        if (IsLocked)
            return;

        if (Input.mouseScrollDelta.y < 0 && zoomLevel < maxZoom)
        {
            if (zoomLevel + zoomInterval > maxZoom)
                zoomLevel = maxZoom;
            else
                zoomLevel += zoomInterval;
        }
        else if (Input.mouseScrollDelta.y > 0 && zoomLevel > zoomInterval)
        {
                zoomLevel -= zoomInterval;
        }
    }

}
