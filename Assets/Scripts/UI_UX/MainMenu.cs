using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuButtonsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject powerUpPanel;
    [SerializeField] private GameObject achievementsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button powerUpButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        ShowMainMenu();
        SetupButtonListeners();
        PlayMenuMusic();
    }

    void Update()
    {
        HandleEscapeKey();
    }

    // ========== INITIALIZATION ==========

    void SetupButtonListeners()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (powerUpButton != null)
        {
            powerUpButton.onClick.AddListener(OnPowerUpButtonClicked);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        }

        if (achievementsButton != null)
        {
            achievementsButton.onClick.AddListener(OnAchievementsButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }

    void PlayMenuMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    // ========== BUTTON CALLBACKS ==========

    void OnStartButtonClicked()
    {
        PlayButtonSound();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogError("SceneController.Instance is null!");
        }
    }

    void OnPowerUpButtonClicked()
    {
        PlayButtonSound();

        if (powerUpPanel != null)
        {
            ShowPowerUpMenu();
        }
    }

    void OnOptionsButtonClicked()
    {
        PlayButtonSound();
        ShowOptionsMenu();
    }

    void OnAchievementsButtonClicked()
    {
        PlayButtonSound();

        if (achievementsPanel != null)
        {
            ShowAchievementsMenu();
        }
    }

    void OnQuitButtonClicked()
    {
        PlayButtonSound();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // ========== MENU NAVIGATION ==========

    public void ShowMainMenu()
    {
        SetPanelActive(menuButtonsPanel, true);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(powerUpPanel, false);
        SetPanelActive(achievementsPanel, false);
    }

    public void ShowOptionsMenu()
    {
        SetPanelActive(menuButtonsPanel, false);
        SetPanelActive(optionsPanel, true);
    }

    public void ShowPowerUpMenu()
    {
        SetPanelActive(menuButtonsPanel, false);
        SetPanelActive(powerUpPanel, true);
    }

    public void ShowAchievementsMenu()
    {
        SetPanelActive(menuButtonsPanel, false);
        SetPanelActive(achievementsPanel, true);
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    // ========== INPUT HANDLING ==========

    void HandleEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed();
        }
    }

    void OnEscapePressed()
    {
        if (IsAnySubPanelActive())
        {
            PlayButtonSound();
            ShowMainMenu();
        }
    }

    bool IsAnySubPanelActive()
    {
        return (optionsPanel != null && optionsPanel.activeSelf) ||
               (powerUpPanel != null && powerUpPanel.activeSelf) ||
               (achievementsPanel != null && achievementsPanel.activeSelf);
    }

    // ========== UTILITY ==========

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}