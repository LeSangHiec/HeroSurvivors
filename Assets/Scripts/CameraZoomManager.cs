using UnityEngine;
using Unity.Cinemachine; // ← ĐỔI namespace

public class CameraZoomManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera; // ← ĐỔI type
    [SerializeField] private GameTimer gameTimer;

    [Header("Zoom Settings")]
    [SerializeField] private float startOrthographicSize = 5f;
    [SerializeField] private float zoomIncreasePerInterval = 1f;
    [SerializeField] private float zoomInterval = 600f; // 10 phút
    [SerializeField] private float maxOrthographicSize = 15f;
    [SerializeField] private float zoomSmoothSpeed = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private float currentTargetSize;
    private float currentActualSize;
    private int lastZoomLevel = 0;

    void Start()
    {
        // Validate references
        if (cinemachineCamera == null)
        {
            Debug.LogError("CameraZoomManager: Cinemachine Camera reference is missing!");
            return;
        }

        if (gameTimer == null)
        {
            Debug.LogError("CameraZoomManager: Game Timer reference is missing!");
            return;
        }

        // Set initial zoom
        currentTargetSize = startOrthographicSize;
        currentActualSize = startOrthographicSize;

        // ← ĐỔI: Cách set orthographic size
        cinemachineCamera.Lens.OrthographicSize = startOrthographicSize;

        Debug.Log($"Camera Zoom Manager initialized. Start size: {startOrthographicSize}");
    }

    void Update()
    {
        if (cinemachineCamera == null || gameTimer == null) return;

        UpdateZoomLevel();
        SmoothZoom();

        // Debug commands
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PrintDebugInfo();
        }

        if (Input.GetKey(KeyCode.LeftBracket))
        {
            SetZoom(currentTargetSize - 0.1f);
        }

        if (Input.GetKey(KeyCode.RightBracket))
        {
            SetZoom(currentTargetSize + 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetZoom();
        }
    }

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
            Debug.Log($"<color=cyan>CAMERA ZOOM OUT at {minutes} minutes! New size: {currentTargetSize}</color>");
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

            // ← ĐỔI: Cách set orthographic size
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
        Debug.Log("Camera zoom reset!");
    }

    public void SetZoomInstant(float size)
    {
        size = Mathf.Clamp(size, startOrthographicSize, maxOrthographicSize);
        currentTargetSize = size;
        currentActualSize = size;

        var lens = cinemachineCamera.Lens;
        lens.OrthographicSize = size;
        cinemachineCamera.Lens = lens;

        Debug.Log($"Instant zoom set to: {size}");
    }

    // ========== DEBUG ==========

    void PrintDebugInfo()
    {
        Debug.Log("========== CAMERA ZOOM INFO ==========");
        Debug.Log($"Current Actual Size: {currentActualSize:F2}");
        Debug.Log($"Current Target Size: {currentTargetSize:F2}");
        Debug.Log($"Zoom Level: {lastZoomLevel}");
        Debug.Log($"Next Zoom At: {(lastZoomLevel + 1) * zoomInterval / 60f} minutes");
        Debug.Log($"Max Size: {maxOrthographicSize}");
        Debug.Log("======================================");
    }

    // ========== GETTERS ==========

    public float GetCurrentZoom() => currentActualSize;
    public float GetTargetZoom() => currentTargetSize;
    public int GetZoomLevel() => lastZoomLevel;
}