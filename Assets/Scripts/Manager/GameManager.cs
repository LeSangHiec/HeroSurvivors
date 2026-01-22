using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private PlayerController player;

    [Header("UI References")] // ← NEW
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;

    [Header("Game State")]
    private bool isPaused = false;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SubscribeToEvents();
        StartGame();

        // ✅ THÊM: Hide UI at start
        HideAllEndScreens();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // ========== EVENT HANDLING ==========

    void SubscribeToEvents()
    {
        if (GameEvents.Instance == null)
        {
            Debug.LogError("GameEvents.Instance is null!");
            return;
        }

        GameEvents.Instance.onPlayerDeath.AddListener(OnPlayerDeath);
        GameEvents.Instance.onGameOver.AddListener(OnGameOver);
    }

    void UnsubscribeEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onPlayerDeath.RemoveListener(OnPlayerDeath);
            GameEvents.Instance.onGameOver.RemoveListener(OnGameOver);
        }
    }

    // ✅ FIX: Handle player death
    void OnPlayerDeath()
    {
        Debug.Log("GameManager: Player died!");
        GameOver();
    }

    // ✅ FIX: Show Game Over UI
    void OnGameOver()
    {
        Debug.Log("GameManager: Showing Game Over UI");
        ShowGameOver();
    }

    // ========== GAME CONTROL ==========

    public void StartGame()
    {
        Debug.Log("★ Game Started! ★");

        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;

        HideAllEndScreens();

        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }

        if (waveSpawner != null)
        {
            waveSpawner.StartSpawning();
        }

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameStart();
        }
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGamePause();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameResume();
        }
    }

    // ✅ UPDATED: GameOver with UI
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("★ GAME OVER! ★");

        Time.timeScale = 0f;

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
        }

        if (RunStats.Instance != null)
        {
            RunStats.Instance.PauseStats();
        }

        // ✅ Show Game Over UI
        ShowGameOver();

        // Trigger event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameOver();
        }
    }

    // ✅ UPDATED: Victory with UI
    public void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("★ VICTORY! ★");

        Time.timeScale = 0f;

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
        }

        if (RunStats.Instance != null)
        {
            RunStats.Instance.PauseStats();
        }

        // ✅ Show Victory UI
        ShowVictory();

        // Trigger event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameOver();
        }
    }

    // ========== UI MANAGEMENT ========== ← NEW

    void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("Game Over UI shown");
        }
        else
        {
            Debug.LogError("GameManager: Game Over UI is null! Assign it in Inspector.");
        }
    }

    void ShowVictory()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            Debug.Log("Victory UI shown");
        }
        else
        {
            Debug.LogWarning("GameManager: Victory UI is null!");
        }
    }

    void HideAllEndScreens()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (victoryUI != null)
        {
            victoryUI.SetActive(false);
        }
    }

    // ========== RESET ==========

    public void ResetForNewGame()
    {
        gameTimer = null;
        waveSpawner = null;
        player = null;

        isPaused = false;
        isGameOver = false;

        Time.timeScale = 1f;

        HideAllEndScreens();
    }

    public void SetReferences(GameTimer timer, WaveSpawner spawner, PlayerController playerCtrl)
    {
        gameTimer = timer;
        waveSpawner = spawner;
        player = playerCtrl;
    }

    // ========== GETTERS ==========

    public bool IsPaused() => isPaused;
    public bool IsGameOver() => isGameOver;
    public GameTimer GetGameTimer() => gameTimer;
    public WaveSpawner GetWaveSpawner() => waveSpawner;
    public PlayerController GetPlayer() => player;
}
