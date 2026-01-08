using UnityEngine;
using Unity.Cinemachine;

public class CameraZoomManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private GameTimer gameTimer;

    [Header("Zoom Settings")]
    [SerializeField] private float startOrthographicSize = 5f;
    [SerializeField] private float zoomIncreasePerInterval = 1f;
    [SerializeField] private float zoomInterval = 600f;
    [SerializeField] private float maxOrthographicSize = 15f;
    [SerializeField] private float zoomSmoothSpeed = 2f;

    private float currentTargetSize;
    private float currentActualSize;
    private int lastZoomLevel = 0;

    void Start()
    {
        if (cinemachineCamera == null)
        {
            return;
        }

        if (gameTimer == null)
        {
            return;
        }

        InitializeZoom();
    }

    void Update()
    {
        if (cinemachineCamera == null || gameTimer == null) return;

        UpdateZoomLevel();
        SmoothZoom();
        //HandleDebugInput();
    }

    // ========== INITIALIZATION ==========

    void InitializeZoom()
    {
        currentTargetSize = startOrthographicSize;
        currentActualSize = startOrthographicSize;

        cinemachineCamera.Lens.OrthographicSize = startOrthographicSize;
    }

    // ========== ZOOM MANAGEMENT ==========

    void UpdateZoomLevel()
    {
        float currentTime = gameTimer.GetCurrentTime();
        int newZoomLevel = Mathf.FloorToInt(currentTime / zoomInterval);

        if (newZoomLevel > lastZoomLevel)
        {
            lastZoomLevel = newZoomLevel;
            IncreaseZoom();
        }
    }

    void IncreaseZoom()
    {
        float newSize = startOrthographicSize + (lastZoomLevel * zoomIncreasePerInterval);
        newSize = Mathf.Min(newSize, maxOrthographicSize);

        if (newSize != currentTargetSize)
        {
            currentTargetSize = newSize;

            int minutes = lastZoomLevel * Mathf.FloorToInt(zoomInterval / 60f);
        }
    }

    void SmoothZoom()
    {
        if (Mathf.Abs(currentActualSize - currentTargetSize) > 0.01f)
        {
            currentActualSize = Mathf.Lerp(
                currentActualSize,
                currentTargetSize,
                Time.deltaTime * zoomSmoothSpeed
            );

            var lens = cinemachineCamera.Lens;
            lens.OrthographicSize = currentActualSize;
            cinemachineCamera.Lens = lens;
        }
    }

    // ========== PUBLIC METHODS ==========

    public void SetZoom(float size)
    {
        size = Mathf.Clamp(size, startOrthographicSize, maxOrthographicSize);
        currentTargetSize = size;
    }

    public void ResetZoom()
    {
        currentTargetSize = startOrthographicSize;
        currentActualSize = startOrthographicSize;

        var lens = cinemachineCamera.Lens;
        lens.OrthographicSize = startOrthographicSize;
        cinemachineCamera.Lens = lens;

        lastZoomLevel = 0;
    }

    public void SetZoomInstant(float size)
    {
        size = Mathf.Clamp(size, startOrthographicSize, maxOrthographicSize);
        currentTargetSize = size;
        currentActualSize = size;

        var lens = cinemachineCamera.Lens;
        lens.OrthographicSize = size;
        cinemachineCamera.Lens = lens;
    }

    // ========== DEBUG ==========

    //void HandleDebugInput()
    //{
    //    if (Input.GetKeyDown(KeyCode.Z))
    //    {
    //        PrintDebugInfo();
    //    }

    //    if (Input.GetKey(KeyCode.LeftBracket))
    //    {
    //        SetZoom(currentTargetSize - 0.1f);
    //    }

    //    if (Input.GetKey(KeyCode.RightBracket))
    //    {
    //        SetZoom(currentTargetSize + 0.1f);
    //    }

    //    if (Input.GetKeyDown(KeyCode.Alpha0))
    //    {
    //        ResetZoom();
    //    }
    //}

    //void PrintDebugInfo()
    //{
    //    Debug.Log("========== CAMERA ZOOM INFO ==========");
    //    Debug.Log($"Current Size: {currentActualSize:F2}");
    //    Debug.Log($"Target Size: {currentTargetSize:F2}");
    //    Debug.Log($"Zoom Level: {lastZoomLevel}");
    //    Debug.Log($"Next Zoom: {(lastZoomLevel + 1) * zoomInterval / 60f} min");
    //    Debug.Log($"Max Size: {maxOrthographicSize}");
    //    Debug.Log("======================================");
    //}

    // ========== GETTERS ==========

    public float GetCurrentZoom() => currentActualSize;
    public float GetTargetZoom() => currentTargetSize;
    public int GetZoomLevel() => lastZoomLevel;
}