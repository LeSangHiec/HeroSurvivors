using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private PlayerController player;

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    DebugWeaponStats();
        //}
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

    void OnPlayerDeath()
    {
        // Handle player death
    }

    void OnGameOver()
    {
        // Handle game over
    }

    // ========== GAME CONTROL ==========

    public void StartGame()
    {
        Debug.Log("★ Game Started! ★");

        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;

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
    }

    public void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("★ VICTORY! ★");

        Time.timeScale = 0f;

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
        }

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameOver();
        }

        if (RunStats.Instance != null)
        {
            RunStats.Instance.PauseStats();
        }
    }

    public void ResetForNewGame()
    {
        gameTimer = null;
        waveSpawner = null;
        player = null;

        isPaused = false;
        isGameOver = false;

        Time.timeScale = 1f;
    }

    public void SetReferences(GameTimer timer, WaveSpawner spawner, PlayerController playerCtrl)
    {
        gameTimer = timer;
        waveSpawner = spawner;
        player = playerCtrl;
    }

    

    public bool IsPaused() => isPaused;
    public bool IsGameOver() => isGameOver;
    public GameTimer GetGameTimer() => gameTimer;
    public WaveSpawner GetWaveSpawner() => waveSpawner;
    public PlayerController GetPlayer() => player;
}