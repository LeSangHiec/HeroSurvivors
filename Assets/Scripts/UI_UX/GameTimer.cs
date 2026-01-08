using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float gameDuration = 1800f;
    [SerializeField] private bool startImmediately = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;

    [Header("Events")]
    [SerializeField] private bool pauseGameOnComplete = false;
    [SerializeField] private GameObject victoryPanel;

    [Header("Debug")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private float skipTimeAmount = 60f;

    private float currentTime = 0f;
    private bool isRunning = false;
    private bool isCompleted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerText is not assigned!");
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
        HandleDebugInput();
    }

    // ========== TIMER CONTROL ==========

    void CompleteTimer()
    {
        isCompleted = true;
        isRunning = false;
        currentTime = gameDuration;

        UpdateTimerDisplay();

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

    // ========== TIME MANIPULATION ==========

    public void SkipTime(float seconds)
    {
        if (isCompleted) return;

        currentTime += seconds;
        currentTime = Mathf.Min(currentTime, gameDuration);

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        UpdateTimerDisplay();

        if (currentTime >= gameDuration)
        {
            CompleteTimer();
        }
    }

    public void RewindTime(float seconds)
    {
        if (isCompleted) return;

        currentTime -= seconds;
        currentTime = Mathf.Max(currentTime, 0f);

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        UpdateTimerDisplay();
    }

    public void SetTime(float seconds)
    {
        if (isCompleted) return;

        currentTime = Mathf.Clamp(seconds, 0f, gameDuration);
        UpdateTimerDisplay();
    }

    // ========== UI ==========

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public float GetCurrentTime() => currentTime;
    public int GetCurrentMinutes() => Mathf.FloorToInt(currentTime / 60f);
    public int GetCurrentSeconds() => Mathf.FloorToInt(currentTime % 60f);
    public bool IsCompleted() => isCompleted;
    public bool IsRunning() => isRunning;

    //  DEBUG 

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void HandleDebugInput()
    {
        if (!enableDebugKeys) return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            SkipTime(skipTimeAmount);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SkipTime(skipTimeAmount * 5);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            RewindTime(skipTimeAmount);
        }
    }
}