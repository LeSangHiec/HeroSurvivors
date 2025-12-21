using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float gameDuration = 1800f;
    [SerializeField] private bool startImmediately = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;

    [Header("Events")]
    [SerializeField] private bool pauseGameOnComplete = false;
    [SerializeField] private GameObject victoryPanel;

    [Header("Debug")] // ← THÊM PHẦN NÀY
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private float skipTimeAmount = 60f; // 1 phút = 60 giây

    private float currentTime = 0f;
    private bool isRunning = false;
    private bool isCompleted = false;

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerText is not assigned in Inspector!");
        }

        if (startImmediately)
        {
            StartTimer();
        }

        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning || isCompleted) return;

        currentTime += Time.deltaTime;

        if (currentTime >= gameDuration)
        {
            CompleteTimer();
        }

        UpdateTimerDisplay();

        // ← THÊM: Debug keys
        if (enableDebugKeys)
        {
            HandleDebugKeys();
        }
    }

    // ← THÊM HÀM MỚI
    void HandleDebugKeys()
    {
        // Phím M: Skip 1 phút (Forward)
        if (Input.GetKeyDown(KeyCode.M))
        {
            SkipTime(skipTimeAmount);
        }

        // Phím N: Skip 5 phút (Forward)
        if (Input.GetKeyDown(KeyCode.N))
        {
            SkipTime(skipTimeAmount * 5);
        }

        // Phím B: Rewind 1 phút (Backward)
        if (Input.GetKeyDown(KeyCode.B))
        {
            RewindTime(skipTimeAmount);
        }
    }

    // ← THÊM HÀM MỚI
    public void SkipTime(float seconds)
    {
        if (isCompleted) return;

        currentTime += seconds;

        // Clamp để không vượt quá game duration
        currentTime = Mathf.Min(currentTime, gameDuration);

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        Debug.Log($"<color=cyan>⏩ Skipped {minutes}:{secs:00} forward</color>");

        UpdateTimerDisplay();

        // Check victory
        if (currentTime >= gameDuration)
        {
            CompleteTimer();
        }
    }

    // ← THÊM HÀM MỚI
    public void RewindTime(float seconds)
    {
        if (isCompleted) return;

        currentTime -= seconds;

        // Clamp để không âm
        currentTime = Mathf.Max(currentTime, 0f);

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        Debug.Log($"<color=yellow>⏪ Rewinded {minutes}:{secs:00} backward</color>");

        UpdateTimerDisplay();
    }

    // ← THÊM HÀM MỚI
    public void SetTime(float seconds)
    {
        if (isCompleted) return;

        currentTime = Mathf.Clamp(seconds, 0f, gameDuration);

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int secs = Mathf.FloorToInt(currentTime % 60f);

        Debug.Log($"<color=magenta>⏱️ Time set to {minutes:00}:{secs:00}</color>");

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void CompleteTimer()
    {
        isCompleted = true;
        isRunning = false;
        currentTime = gameDuration;

        UpdateTimerDisplay();
        Debug.Log("30:00 - Victory!");

        if (pauseGameOnComplete)
        {
            Time.timeScale = 0f;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (!isCompleted)
        {
            isRunning = true;
        }
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        isCompleted = false;
        UpdateTimerDisplay();
    }

    public float GetCurrentTime() => currentTime;
    public int GetCurrentMinutes() => Mathf.FloorToInt(currentTime / 60f);
    public int GetCurrentSeconds() => Mathf.FloorToInt(currentTime % 60f);
    public bool IsCompleted() => isCompleted;
    public bool IsRunning() => isRunning;
}