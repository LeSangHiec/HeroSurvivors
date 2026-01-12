using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Options Back Button")]
    [SerializeField] private Button optionsBackButton;

    private bool isPaused = false;

    void Start()
    {
        SetupButtonListeners();

        // ✅ CRITICAL: Hide all panels at start
        HideAllPanels();
    }

    void Update()
    {
        HandlePauseInput();
    }

    // ========== SETUP ==========

    void SetupButtonListeners()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.AddListener(OnOptionsBackClicked);
        }
    }

    // ========== INPUT HANDLING ==========

    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // If in options, go back to upgrades
                if (IsInOptionsPanel())
                {
                    ShowUpgradesPanel();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    // ========== PAUSE/RESUME ==========

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        ShowUpgradesPanel();

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGamePause();
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        HideAllPanels();

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameResume();
        }
    }

    // ========== PANEL SWITCHING ==========

    void ShowUpgradesPanel()
    {
        // Show pause menu background
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Show upgrades panel
        if (upgradesPanel != null)
        {
            upgradesPanel.SetActive(true);
        }

        // ✅ CRITICAL: Hide options panel
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    void ShowOptionsPanel()
    {
        // Show pause menu background
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // ✅ CRITICAL: Hide upgrades panel
        if (upgradesPanel != null)
        {
            upgradesPanel.SetActive(false);
        }

        // Show options panel
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    void HideAllPanels()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (upgradesPanel != null)
        {
            upgradesPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    // ========== BUTTON CALLBACKS ==========

    void OnResumeClicked()
    {
        PlayButtonSound();

        // ✅ Check if in options panel
        if (IsInOptionsPanel())
        {
            // Go back to upgrades panel
            ShowUpgradesPanel();
        }
        else
        {
            // Resume game
            Resume();
        }
    }

    void OnOptionsClicked()
    {
        PlayButtonSound();
        ShowOptionsPanel();
    }

    void OnOptionsBackClicked()
    {
        PlayButtonSound();
        ShowUpgradesPanel();
    }

    void OnMainMenuClicked()
    {
        PlayButtonSound();
        Time.timeScale = 1f;

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
    }

    void OnQuitClicked()
    {
        PlayButtonSound();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.QuitGame();
        }
    }

    // ========== HELPERS ==========

    bool IsInOptionsPanel()
    {
        return optionsPanel != null && optionsPanel.activeSelf;
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    // ========== PUBLIC GETTERS ==========

    public bool IsPaused() => isPaused;
}