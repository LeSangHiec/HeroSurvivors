using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class StoryIntroManager : MonoBehaviour
{
    public static StoryIntroManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button backgroundButton;

    [Header("Video Background")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private bool playVideoOnStart = true;
    [SerializeField] private float videoVolume = 0.3f;

    [Header("Story Content")]
    [TextArea(5, 10)]
    [SerializeField]
    private string storyContent =
        "Năm 2045...\n\n" +
        "Thế giới đã rơi vào bóng tối.\n\n" +
        "Những sinh vật quái dị xuất hiện từ các cổng địa ngục.\n\n" +
        "Bạn là người sống sót cuối cùng...\n\n" +
        "Nhiệm vụ của bạn là sinh tồn...";

    [Header("Typewriter Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool autoStartOnLoad = true;

    // ✅ THÊM: Control story behavior
    [Header("Story Control")]
    [SerializeField] private bool showOnlyFirstTime = true; // Chỉ show lần đầu
    [SerializeField] private bool resetStoryOnQuit = false; // Reset khi thoát game

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip completeSound;

    private bool isTyping = false;
    private bool isComplete = false;
    private bool hasShown = false;
    private Coroutine typewriterCoroutine;

    // ✅ THÊM: PlayerPrefs key
    private const string STORY_SEEN_KEY = "HasSeenStoryIntro";

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

        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        SetupVideo();
    }

    void Start()
    {
        SetupUI();

        // ✅ THAY ĐỔI: Kiểm tra đã xem story chưa
        if (autoStartOnLoad && ShouldShowStory())
        {
            ShowStory();
        }
        else
        {
            // Skip story - start game ngay
            StartGameDirectly();
        }
    }

    void Update()
    {
        if (storyPanel != null && storyPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                OnScreenClicked();
            }
        }
    }

    // ========== STORY CONTROL ========== ✅ THÊM SECTION MỚI

    bool ShouldShowStory()
    {
        // Nếu không bật "show only first time" → luôn show
        if (!showOnlyFirstTime)
        {
            return true;
        }

        // Kiểm tra đã xem story chưa
        bool hasSeenStory = PlayerPrefs.GetInt(STORY_SEEN_KEY, 0) == 1;

        if (hasSeenStory)
        {
            Debug.Log("<color=yellow>Story already seen - Skipping</color>");
            return false;
        }

        return true;
    }

    void MarkStoryAsSeen()
    {
        PlayerPrefs.SetInt(STORY_SEEN_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("<color=cyan>Story marked as seen</color>");
    }

    public void ResetStoryFlag()
    {
        PlayerPrefs.DeleteKey(STORY_SEEN_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=orange>Story flag reset - Will show next time</color>");
    }

    // ✅ THÊM: Start game trực tiếp (không có story)
    void StartGameDirectly()
    {
        Debug.Log("<color=green>Starting game directly (no story)</color>");

        // Ensure panel is hidden
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        // Resume game immediately
        Time.timeScale = 1f;

        // Trigger game start
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameStart();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    // ========== VIDEO SETUP ==========

    void SetupVideo()
    {
        if (videoPlayer == null) return;

        videoPlayer.Stop();
        videoPlayer.SetDirectAudioVolume(0, videoVolume);
        videoPlayer.Prepare();

        Debug.Log("<color=cyan>Video Player Setup Complete</color>");
    }

    void PlayVideo()
    {
        if (videoPlayer == null || !playVideoOnStart) return;

        videoPlayer.Play();

        Debug.Log("<color=cyan>Video Started</color>");
    }

    void StopVideo()
    {
        if (videoPlayer == null) return;

        videoPlayer.Stop();

        Debug.Log("<color=cyan>Video Stopped</color>");
    }

    // ========== SETUP ==========

    void SetupUI()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(OnScreenClicked);
        }

        if (storyPanel != null && !hasShown)
        {
            storyPanel.SetActive(false);
        }

        if (storyText != null)
        {
            storyText.text = "";
        }
    }

    // ========== SHOW STORY ==========

    public void ShowStory()
    {
        if (hasShown)
        {
            Debug.LogWarning("Story already shown this session");
            return;
        }

        hasShown = true;

        // Pause game
        Time.timeScale = 0f;

        // Show panel
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }

        // Play video
        PlayVideo();

        // Start typewriter
        if (storyText != null)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect());
        }

        Debug.Log("<color=cyan>Story Intro Started</color>");
    }

    // ========== TYPEWRITER EFFECT ==========

    IEnumerator TypewriterEffect()
    {
        isTyping = true;
        isComplete = false;

        storyText.text = "";

        foreach (char letter in storyContent)
        {
            storyText.text += letter;
            PlayTypewriterSound();
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isTyping = false;
        isComplete = true;

        PlayCompleteSound();

        Debug.Log("<color=green>Story Typewriter Complete</color>");
    }

    // ========== CLICK HANDLERS ==========

    void OnScreenClicked()
    {
        if (isTyping)
        {
            CompleteTypewriter();
        }
        else if (isComplete)
        {
            StartGame();
        }
    }

    void OnSkipClicked()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        StartGame();
    }

    void CompleteTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        storyText.text = storyContent;

        isTyping = false;
        isComplete = true;

        PlayCompleteSound();

        Debug.Log("<color=yellow>Story Completed by Click</color>");
    }

    // ========== START GAME ==========

    void StartGame()
    {
        Debug.Log("<color=green>Story Complete - Starting Game</color>");

        // ✅ THÊM: Mark story as seen
        if (showOnlyFirstTime)
        {
            MarkStoryAsSeen();
        }

        // Stop video
        StopVideo();

        // Hide panel
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        // Trigger game start events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerGameStart();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    // ========== AUDIO ==========

    void PlayTypewriterSound()
    {
        if (typewriterSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(typewriterSound);
        }
    }

    void PlayCompleteSound()
    {
        if (completeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(completeSound);
        }
    }

    // ========== ON APPLICATION QUIT ==========

    void OnApplicationQuit()
    {
        // ✅ THÊM: Reset flag khi thoát game nếu bật option
        if (resetStoryOnQuit)
        {
            ResetStoryFlag();
        }
    }

    // ========== PUBLIC METHODS ==========

    public bool IsShowing() => storyPanel != null && storyPanel.activeSelf;
    public bool IsTyping() => isTyping;
    public bool IsComplete() => isComplete;

    public void Reset()
    {
        hasShown = false;
        isTyping = false;
        isComplete = false;

        StopVideo();

        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        if (storyText != null)
        {
            storyText.text = "";
        }
    }

    // ✅ THÊM: Force show story (debug)
    public void ForceShowStory()
    {
        hasShown = false;
        ShowStory();
    }

    // Video controls
    public void SetVideoVolume(float volume)
    {
        if (videoPlayer != null)
        {
            videoPlayer.SetDirectAudioVolume(0, volume);
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void ResumeVideo()
    {
        if (videoPlayer != null && videoPlayer.isPaused)
        {
            videoPlayer.Play();
        }
    }
}