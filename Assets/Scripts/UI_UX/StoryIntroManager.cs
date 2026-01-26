using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StoryIntroManager : MonoBehaviour
{
    public static StoryIntroManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Button skipButton;

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
    [SerializeField] private float typewriterSpeed = 0.05f; // Giây/ký tự
    [SerializeField] private bool autoStartOnLoad = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip completeSound;

    private bool isTyping = false;
    private bool isComplete = false;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupUI();

        if (autoStartOnLoad)
        {
            ShowStory();
        }
    }

    // ========== SETUP ==========

    void SetupUI()
    {
        // Setup skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        // Hide panel initially
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        // Clear text
        if (storyText != null)
        {
            storyText.text = "";
        }
    }

    // ========== SHOW STORY ==========

    public void ShowStory()
    {
        // Pause game
        Time.timeScale = 0f;

        // Show panel
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }

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

            // Play sound (optional)
            PlayTypewriterSound();

            // Wait (use unscaledDeltaTime because Time.timeScale = 0)
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        // Complete
        isTyping = false;
        isComplete = true;

        PlayCompleteSound();

        // Auto start game after delay
        yield return new WaitForSecondsRealtime(1f);
        StartGame();
    }

    // ========== SKIP ==========

    void OnSkipClicked()
    {
        if (isTyping)
        {
            // Stop typewriter
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            // Show full text
            storyText.text = storyContent;

            isTyping = false;
            isComplete = true;
        }

        // Start game immediately
        StartGame();
    }

    // ========== START GAME ==========

    void StartGame()
    {
        Debug.Log("<color=green>Story Complete - Starting Game</color>");

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

    // ========== PUBLIC METHODS ==========

    public bool IsShowing() => storyPanel != null && storyPanel.activeSelf;
    public bool IsTyping() => isTyping;
    public bool IsComplete() => isComplete;
}