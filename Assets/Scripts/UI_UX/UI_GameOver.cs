using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_GameOver : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Elements - Stats")]
    [SerializeField] private TMP_Text timeValue;
    [SerializeField] private TMP_Text killsValue;
    [SerializeField] private TMP_Text levelValue;
    [SerializeField] private TMP_Text damageValue;

    [Header("UI Elements - Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;

    void Awake()
    {
        SetupButtons();
    }

    void Start()
    {
        SubscribeEvents();

        if (hideOnStart && gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
        CleanupButtons();
    }

    // ========== SETUP ==========

    void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void CleanupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
        }
    }

    void SubscribeEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onGameOver.AddListener(OnGameOver);
        }
    }

    void UnsubscribeEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onGameOver.RemoveListener(OnGameOver);
        }
    }

    // ========== EVENT HANDLERS ==========

    void OnGameOver()
    {
        ShowGameOver();
    }

    // ========== PUBLIC METHODS ==========

    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);
        UpdateAllStats();
        PauseGame();
        PlayGameOverSound();
    }

    public void HideGameOver()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(false);
        ResumeGame();
    }

    // ========== UPDATE STATS ==========

    void UpdateAllStats()
    {
        UpdateTime();
        UpdateKills();
        UpdateLevel();
        UpdateDamage();
    }

    void UpdateTime()
    {
        if (timeValue == null) return;

        if (GameTimer.Instance != null)
        {
            float time = GameTimer.Instance.GetCurrentTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeValue.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            timeValue.text = "00:00";
        }
    }

    void UpdateKills()
    {
        if (killsValue == null) return;

        if (RunStats.Instance != null)
        {
            killsValue.text = RunStats.Instance.GetKillCount().ToString();
        }
        else
        {
            killsValue.text = "0";
        }
    }

    void UpdateLevel()
    {
        if (levelValue == null) return;

        PlayerXP playerXP = FindAnyObjectByType<PlayerXP>();
        if (playerXP != null)
        {
            levelValue.text = playerXP.GetCurrentLevel().ToString();
        }
        else
        {
            levelValue.text = "1";
        }
    }

    void UpdateDamage()
    {
        if (damageValue == null) return;

        if (RunStats.Instance != null)
        {
            float damage = RunStats.Instance.GetTotalDamageDealt();
            damageValue.text = FormatNumber(damage);
        }
        else
        {
            damageValue.text = "0";
        }
    }

    // ========== BUTTON CALLBACKS ==========

    void OnRestartClicked()
    {
        PlayButtonSound();
        ResumeGame();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.ReloadCurrentScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnQuitClicked()
    {
        PlayButtonSound();
        ResumeGame();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
        
    }

    // ========== UTILITY ==========

    void PauseGame()
    {
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    void PlayGameOverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverMusic();
        }
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    string FormatNumber(float number)
    {
        if (number >= 1000000)
        {
            return $"{number / 1000000f:F1}M";
        }
        else if (number >= 1000)
        {
            return $"{number / 1000f:F1}K";
        }
        else
        {
            return $"{number:F0}";
        }
    }
}