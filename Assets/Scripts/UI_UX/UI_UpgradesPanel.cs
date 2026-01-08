using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_UpgradesPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject upgradeEntryPrefab;
    [SerializeField] private Transform gridContainer;

    [Header("Settings")]
    [SerializeField] private bool autoRefreshOnEnable = true;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void OnEnable()
    {
        if (autoRefreshOnEnable)
        {
            RefreshUpgrades();
        }
    }

    // ========== REFRESH ==========

    public void RefreshUpgrades()
    {
        ClearEntries();

        if (CardTracker.Instance == null)
        {
            return;
        }

        List<CardProgress> pickedCards = CardTracker.Instance.GetAllPickedCards();

        if (pickedCards.Count == 0) return;

        foreach (CardProgress cardProgress in pickedCards)
        {
            SpawnUpgradeEntry(cardProgress);
        }
    }

    void SpawnUpgradeEntry(CardProgress cardProgress)
    {
        if (upgradeEntryPrefab == null || gridContainer == null)
        {
            return;
        }

        GameObject entry = Instantiate(upgradeEntryPrefab, gridContainer);
        spawnedEntries.Add(entry);

        SetupEntryVisuals(entry, cardProgress);
    }

    void SetupEntryVisuals(GameObject entry, CardProgress cardProgress)
    {
        Image iconImage = entry.transform.Find("UpgradeIcon")?.GetComponent<Image>();
        TMP_Text levelText = entry.transform.Find("LevelText")?.GetComponent<TMP_Text>();

        SetupIcon(iconImage, cardProgress);
        SetupLevelText(levelText, cardProgress);
    }

    void SetupIcon(Image iconImage, CardProgress cardProgress)
    {
        if (iconImage == null) return;

        if (cardProgress.card.CardImage != null)
        {
            iconImage.sprite = cardProgress.card.CardImage;
        }
        else
        {
            iconImage.color = GetColorForCardEffect(cardProgress.card.effectType);
        }
    }

    void SetupLevelText(TMP_Text levelText, CardProgress cardProgress)
    {
        if (levelText == null) return;

        levelText.text = $"Lv.{cardProgress.currentLevel}";

        if (cardProgress.currentLevel >= cardProgress.card.maxLevel)
        {
            levelText.color = Color.yellow;
        }
        else
        {
            levelText.color = Color.white;
        }
    }

    void ClearEntries()
    {
        foreach (GameObject entry in spawnedEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }

        spawnedEntries.Clear();
    }

    // ========== HELPER ==========

    Color GetColorForCardEffect(CardEffect effectType)
    {
        switch (effectType)
        {
            case CardEffect.MaxHealth: return new Color(0.2f, 0.8f, 0.2f);
            case CardEffect.Damage: return new Color(0.8f, 0.2f, 0.2f);
            case CardEffect.MoveSpeed: return new Color(0.2f, 0.6f, 0.8f);
            case CardEffect.AttackSpeed: return new Color(0.8f, 0.6f, 0.2f);
            case CardEffect.CritChance: return new Color(0.8f, 0.8f, 0.2f);
            case CardEffect.HealthRegen: return new Color(0.4f, 0.8f, 0.4f);
            case CardEffect.XPGain: return new Color(0.6f, 0.4f, 0.8f);
            default: return Color.white;
        }
    }
}