using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 100;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float xpScalingFactor = 1.2f;
    [SerializeField] private float xpGainMultiplier = 1f; // ← THÊM

    [Header("UI References")]
    [SerializeField] private Image xpBarFill;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text levelText;

    [Header("Visual Effects")]
    [SerializeField] private GameObject levelUpEffect;

    void Start()
    {
        UpdateXPBar();
        UpdateLevelText();
    }

    // ========== XP METHODS ==========

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        // ← SỬA: Áp dụng multiplier
        int actualXP = Mathf.RoundToInt(amount * xpGainMultiplier);
        currentXP += actualXP;

        Debug.Log($"Gained {actualXP} XP! (Base: {amount} x {xpGainMultiplier:F2}) Total: {currentXP}/{xpToNextLevel}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayXPCollect();
        }

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateXPBar();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScalingFactor);

        Debug.Log($"<color=yellow>★ LEVEL UP! Now Level {currentLevel} ★</color>");
        Debug.Log($"Next level requires: {xpToNextLevel} XP");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUp();
        }

        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }

        UpdateLevelText();
        OnLevelUp();
    }

    void OnLevelUp()
    {
        // Show card selection
        CardManager cardManager = FindAnyObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.ShowCardSelection();
        }
    }

    // ========== UPGRADE METHODS ==========

    public void IncreaseXPGain(float percentage)
    {
        xpGainMultiplier += percentage;

        Debug.Log($"<color=yellow>XP Gain increased by {percentage * 100}%! Multiplier: {xpGainMultiplier:F2}x</color>");
    }

    // ========== UI UPDATES ==========

    void UpdateXPBar()
    {
        if (xpBarFill != null)
        {
            float fillAmount = (float)currentXP / xpToNextLevel;
            xpBarFill.fillAmount = fillAmount;
        }

        if (xpText != null)
        {
            xpText.text = $"{currentXP} / {xpToNextLevel}";
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"LV {currentLevel}";
        }
    }

    // ========== PUBLIC GETTERS ==========

    public int GetCurrentXP() => currentXP;
    public int GetXPToNextLevel() => xpToNextLevel;
    public int GetCurrentLevel() => currentLevel;
    public float GetXPPercentage() => (float)currentXP / xpToNextLevel;
    public float GetXPGainMultiplier() => xpGainMultiplier;

    // ========== DEBUG ==========

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddXP(50);
            Debug.Log("Debug: Added 50 XP");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            AddXP(xpToNextLevel);
            Debug.Log("Debug: Force level up");
        }
    }
}