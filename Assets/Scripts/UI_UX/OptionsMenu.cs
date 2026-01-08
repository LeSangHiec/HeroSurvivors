using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    [SerializeField] private TMP_Text masterVolumeLabel;
    [SerializeField] private TMP_Text musicVolumeLabel;
    [SerializeField] private TMP_Text sfxVolumeLabel;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("References")]
    [SerializeField] private MainMenu mainMenu;

    void Start()
    {
        if (mainMenu == null)
        {
            mainMenu = FindFirstObjectByType<MainMenu>();
        }

        SetupSliderListeners();

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        LoadCurrentSettings();
    }

    void OnEnable()
    {
        // Refresh settings when panel is shown
        LoadCurrentSettings();
    }

    void SetupSliderListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    //  LOAD CURRENT SETTINGS 

    void LoadCurrentSettings()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
        }

        UpdateVolumeLabels();
    }

    //  SLIDER CALLBACKS 

    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }

        UpdateVolumeLabels();
    }

    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        UpdateVolumeLabels();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        UpdateVolumeLabels();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    //  BUTTON CALLBACKS

    void OnBackButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Go back to main menu
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
       
    }

    //  UPDATE LABELS

    void UpdateVolumeLabels()
    {
        if (AudioManager.Instance == null) return;

        if (masterVolumeLabel != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetMasterVolume() * 100);
            masterVolumeLabel.text = $"{percentage}%";
        }

        if (musicVolumeLabel != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetMusicVolume() * 100);
            musicVolumeLabel.text = $"{percentage}%";
        }

        if (sfxVolumeLabel != null)
        {
            int percentage = Mathf.RoundToInt(AudioManager.Instance.GetSFXVolume() * 100);
            sfxVolumeLabel.text = $"{percentage}%";
        }
    }
}