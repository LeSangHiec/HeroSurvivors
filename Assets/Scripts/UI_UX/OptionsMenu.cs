using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UniversalOptionsPanel : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    [SerializeField] private TMP_Text masterVolumeLabel;
    [SerializeField] private TMP_Text musicVolumeLabel;
    [SerializeField] private TMP_Text sfxVolumeLabel;

    [Header("Mute Toggles (Optional)")]
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("Back Navigation")]
    [SerializeField] private UnityEvent onBackButtonClicked;

    [Header("Settings")]
    [SerializeField] private bool playSoundOnSliderChange = true;
    [SerializeField] private float soundPreviewDelay = 0.1f;

    private float lastPreviewTime;
    private bool listenersSetup = false; // ← THÊM: Track listener state

    void Start()
    {
        SetupListeners();
        LoadCurrentSettings();
    }

    void OnEnable()
    {
        // ✅ FIX: Setup listeners if not already done
        if (!listenersSetup)
        {
            SetupListeners();
        }

        LoadCurrentSettings();
    }

    void OnDisable()
    {
        // ❌ KHÔNG remove listeners nữa
        // RemoveListeners(); // ← XÓA DÒNG NÀY
    }

    // ========== SETUP ==========

    void SetupListeners()
    {
        // ✅ THÊM: Prevent duplicate listeners
        if (listenersSetup) return;

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

        if (musicMuteToggle != null)
        {
            musicMuteToggle.onValueChanged.AddListener(OnMusicMuteChanged);
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        listenersSetup = true; // ✅ Mark as setup
    }

    void RemoveListeners()
    {
        if (!listenersSetup) return;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        if (musicMuteToggle != null)
        {
            musicMuteToggle.onValueChanged.RemoveListener(OnMusicMuteChanged);
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }

        listenersSetup = false;
    }

    void OnDestroy()
    {
        // ✅ THÊM: Cleanup when destroyed
        RemoveListeners();
    }

    // ========== LOAD SETTINGS ==========

    void LoadCurrentSettings()
    {
        if (AudioManager.Instance == null) return;

        // ✅ THÊM: Temporarily remove listeners to prevent triggering callbacks
        bool wasSetup = listenersSetup;
        if (wasSetup)
        {
            RemoveListeners();
        }

        LoadVolumeSettings();
        LoadMuteSettings();
        UpdateAllLabels();

        // ✅ Re-add listeners
        if (wasSetup)
        {
            SetupListeners();
        }
    }

    void LoadVolumeSettings()
    {
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
    }

    void LoadMuteSettings()
    {
        if (musicMuteToggle != null)
        {
            musicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
        }
    }

    // ========== VOLUME CALLBACKS ==========

    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }

        UpdateVolumeLabel(masterVolumeLabel, value);
    }

    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        UpdateVolumeLabel(musicVolumeLabel, value);
    }

    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        UpdateVolumeLabel(sfxVolumeLabel, value);
        PlayPreviewSound();
    }

    // ========== MUTE CALLBACKS ==========

    void OnMusicMuteChanged(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMuteMusic(isMuted);
        }
    }

    void OnSFXMuteChanged(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMuteSFX(isMuted);
        }
    }

    // ========== BACK BUTTON ==========

    void OnBackClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        onBackButtonClicked?.Invoke();
    }

    // ========== UI UPDATES ==========

    void UpdateAllLabels()
    {
        if (AudioManager.Instance == null) return;

        UpdateVolumeLabel(masterVolumeLabel, AudioManager.Instance.GetMasterVolume());
        UpdateVolumeLabel(musicVolumeLabel, AudioManager.Instance.GetMusicVolume());
        UpdateVolumeLabel(sfxVolumeLabel, AudioManager.Instance.GetSFXVolume());
    }

    void UpdateVolumeLabel(TMP_Text label, float value)
    {
        if (label != null)
        {
            label.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    void PlayPreviewSound()
    {
        if (!playSoundOnSliderChange) return;
        if (AudioManager.Instance == null) return;

        if (Time.unscaledTime - lastPreviewTime > soundPreviewDelay)
        {
            AudioManager.Instance.PlayButtonClick();
            lastPreviewTime = Time.unscaledTime;
        }
    }
}