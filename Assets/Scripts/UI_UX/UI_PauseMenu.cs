using UnityEngine;
using UnityEngine.UI;

public class UI_PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject playerStatsPanel;
    [SerializeField] private GameObject upgradesPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey1 = KeyCode.Escape;
    [SerializeField] private KeyCode pauseKey2 = KeyCode.P;

    private bool isPaused = false;

    void Start()
    {
        // Setup buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);

        // Subscribe events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onGamePause.AddListener(OnGamePause);
            GameEvents.Instance.onGameResume.AddListener(OnGameResume);
        }

        // Hide by default
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onGamePause.RemoveListener(OnGamePause);
            GameEvents.Instance.onGameResume.RemoveListener(OnGameResume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey1) || Input.GetKeyDown(pauseKey2))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        // Don't pause if card selection active
        if (IsCardSelectionActive())
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
            if (GameEvents.Instance != null)
                GameEvents.Instance.TriggerGamePause();
        }
    }

    void ResumeGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
            if (GameEvents.Instance != null)
                GameEvents.Instance.TriggerGameResume();
        }
    }

    void QuitToMainMenu()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
    }

    void OnGamePause()
    {
        isPaused = true;

        if (!IsCardSelectionActive())
        {
            ShowPauseMenu();
        }
    }

    void OnGameResume()
    {
        isPaused = false;
        HidePauseMenu();
    }

    void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Refresh panels
        if (playerStatsPanel != null)
        {
            UI_PlayerStatsPanel stats = playerStatsPanel.GetComponent<UI_PlayerStatsPanel>();
            if (stats != null)
                stats.RefreshStats();
        }

        if (upgradesPanel != null)
        {
            UI_UpgradesPanel upgrades = upgradesPanel.GetComponent<UI_UpgradesPanel>();
            if (upgrades != null)
                upgrades.RefreshUpgrades();
        }
    }

    void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    bool IsCardSelectionActive()
    {
        CardManager cardMgr = FindFirstObjectByType<CardManager>();
        return cardMgr != null && cardMgr.IsCardSelectionActive();
    }
}