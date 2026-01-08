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
    [SerializeField] private float xpGainMultiplier = 1f;

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

    void Update()
    {
        HandleDebugInput();
    }

    // ========== XP MANAGEMENT ==========

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        int actualXP = Mathf.RoundToInt(amount * xpGainMultiplier);
        currentXP += actualXP;

        TriggerXPEvents(actualXP);

        int levelsGained = ProcessLevelUps();

        if (levelsGained > 0)
        {
            QueueCardPicks(levelsGained);
        }

        UpdateXPBar();
    }

    void TriggerXPEvents(int actualXP)
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerXPGained(actualXP);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayXPCollect();
        }
    }

    int ProcessLevelUps()
    {
        int levelsGained = 0;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
            levelsGained++;
        }

        return levelsGained;
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScalingFactor);

        Debug.Log($"<color=yellow>★ Level {currentLevel}! ★</color>");

        TriggerLevelUpEvents();
        SpawnLevelUpEffect();
        UpdateLevelText();
    }

    void TriggerLevelUpEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerLevelUp(currentLevel);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUp();
        }
    }

    void SpawnLevelUpEffect()
    {
        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }
    }

    void QueueCardPicks(int count)
    {
        if (CardPickQueue.Instance != null)
        {
            CardPickQueue.Instance.QueueCardPicks(count);
        }
        else
        {
            FallbackCardSelection();
        }
    }

    void FallbackCardSelection()
    {
        CardManager cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.ShowCardSelection();
        }
    }

    // ========== UPGRADES ==========

    public void IncreaseXPGain(float percentage)
    {
        xpGainMultiplier += percentage;
    }

    // ========== UI ==========

    void UpdateXPBar()
    {
        if (xpBarFill != null)
        {
            float fillAmount = (float)currentXP / xpToNextLevel;
            xpBarFill.fillAmount = fillAmount;

            if (GameEvents.Instance != null)
            {
                GameEvents.Instance.TriggerXPBarChanged(fillAmount);
            }
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

    // ========== GETTERS ==========

    public int GetCurrentXP() => currentXP;
    public int GetXPToNextLevel() => xpToNextLevel;
    public int GetCurrentLevel() => currentLevel;
    public float GetXPPercentage() => (float)currentXP / xpToNextLevel;
    public float GetXPGainMultiplier() => xpGainMultiplier;

    // ========== DEBUG ==========

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddXP(50);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            AddXP(xpToNextLevel);
        }
    }
}