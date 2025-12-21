using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
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
        // Singleton pattern
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
        // Bắt đầu game
        StartGame();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }

    }

    void Update()
    {
        // Pause game với ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // ========== GAME CONTROL ==========

    public void StartGame()
    {
        Debug.Log("Game Started!");

        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;

        // Start các systems
        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }

        if (waveSpawner != null)
        {
            waveSpawner.StartSpawning();
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

    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        Debug.Log("Game Resumed");
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("GAME OVER!");

        // Stop game
        Time.timeScale = 0f;

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
        }
       
    }

    public void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("VICTORY!");

        // Stop game
        Time.timeScale = 0f;

        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
        }
    }
    void OnLevelUp()
    {
        // Show level up menu
        if (GameManager.Instance != null)
        {
            // TODO: GameManager.Instance.ShowLevelUpMenu();
        }

        Debug.Log("TODO: Show upgrade selection menu");
    }

    // ========== PUBLIC GETTERS ==========

    public bool IsPaused() => isPaused;
    public bool IsGameOver() => isGameOver;
    public GameTimer GetGameTimer() => gameTimer;
    public WaveSpawner GetWaveSpawner() => waveSpawner;
    public PlayerController GetPlayer() => player;
}