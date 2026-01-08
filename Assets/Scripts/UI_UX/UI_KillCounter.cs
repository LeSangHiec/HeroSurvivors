using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_KillCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image killIconImage; // ← MỚI: Sprite icon
    [SerializeField] private TMP_Text killCountText;

    [Header("Display Settings")]
    [SerializeField] private bool showAnimation = true;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Icon Settings")] // ← MỚI
    [SerializeField] private Sprite killIcon; // Icon cho kill counter
    [SerializeField] private bool animateIcon = true; // Animate icon khi kill

    [Header("Color Settings")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color lowKillColor = Color.white;
    [SerializeField] private Color midKillColor = Color.yellow;
    [SerializeField] private Color highKillColor = Color.red;
    [SerializeField] private int midKillThreshold = 50;
    [SerializeField] private int highKillThreshold = 100;

    private int currentKillCount = 0;
    private Vector3 originalTextScale;
    private Vector3 originalIconScale; // ← MỚI

    void Start()
    {
        // Subscribe to event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onKillCountChanged.AddListener(OnKillCountChanged);
        }
      

        // Save original scales
        if (killCountText != null)
        {
            originalTextScale = killCountText.transform.localScale;
        }

        // ← MỚI: Save icon scale
        if (killIconImage != null)
        {
            originalIconScale = killIconImage.transform.localScale;

            // Set icon sprite nếu có
            if (killIcon != null)
            {
                killIconImage.sprite = killIcon;
            }
        }

        UpdateDisplay(0);
    }

    void OnDestroy()
    {
        // Unsubscribe from event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onKillCountChanged.RemoveListener(OnKillCountChanged);
        }
    }

    void OnKillCountChanged(int newKillCount)
    {
        currentKillCount = newKillCount;
        UpdateDisplay(newKillCount);

        if (showAnimation)
        {
            PlayKillAnimation();
        }
    }

    void UpdateDisplay(int killCount)
    {
        if (killCountText == null) return;

        // ← SỬA: Chỉ hiển thị số, không có prefix
        killCountText.text = killCount.ToString();

        if (useColorGradient)
        {
            Color color = GetColorForKillCount(killCount);
            killCountText.color = color;

            // ← MỚI: Đổi màu icon theo kill count
            if (killIconImage != null)
            {
                killIconImage.color = color;
            }
        }
    }

    Color GetColorForKillCount(int killCount)
    {
        if (killCount < midKillThreshold)
        {
            float t = (float)killCount / midKillThreshold;
            return Color.Lerp(lowKillColor, midKillColor, t);
        }
        else if (killCount < highKillThreshold)
        {
            float t = (float)(killCount - midKillThreshold) / (highKillThreshold - midKillThreshold);
            return Color.Lerp(midKillColor, highKillColor, t);
        }
        else
        {
            return highKillColor;
        }
    }

    void PlayKillAnimation()
    {
        if (killCountText == null) return;

        StopAllCoroutines();
        StartCoroutine(ScaleAnimation());
    }

    System.Collections.IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        float halfDuration = animationDuration / 2f;

        // Scale up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            // Animate text
            if (killCountText != null)
            {
                killCountText.transform.localScale = Vector3.Lerp(originalTextScale, originalTextScale * 1.2f, t);
            }

            // ← MỚI: Animate icon
            if (animateIcon && killIconImage != null)
            {
                killIconImage.transform.localScale = Vector3.Lerp(originalIconScale, originalIconScale * 1.3f, t);
            }

            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            // Animate text
            if (killCountText != null)
            {
                killCountText.transform.localScale = Vector3.Lerp(originalTextScale * 1.2f, originalTextScale, t);
            }

            // ← MỚI: Animate icon
            if (animateIcon && killIconImage != null)
            {
                killIconImage.transform.localScale = Vector3.Lerp(originalIconScale * 1.3f, originalIconScale, t);
            }

            yield return null;
        }

        // Reset scales
        if (killCountText != null)
        {
            killCountText.transform.localScale = originalTextScale;
        }

        if (killIconImage != null)
        {
            killIconImage.transform.localScale = originalIconScale;
        }
    }
}