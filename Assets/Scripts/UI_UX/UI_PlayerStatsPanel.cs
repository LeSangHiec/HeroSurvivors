using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerStatsPanel : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject statEntryPrefab;
    [SerializeField] private Transform statsContainer;

    [Header("Stat Configurations")]
    [SerializeField] private List<StatConfig> statConfigs = new List<StatConfig>();

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerController;

    void Start()
    {
        FindPlayerReferences();
        SetupDefaultConfigs();
        CreateStatEntries();

        // ✅ FIX: Update stats after creating entries
        RefreshStats();
    }

    // ✅ NEW: Refresh when panel becomes active
    void OnEnable()
    {
        // Small delay to ensure player is initialized
        Invoke(nameof(RefreshStats), 0.1f);
    }

    // ========== INITIALIZATION ==========

    void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            if (playerStats == null)
            {
                playerStats = playerObj.GetComponent<PlayerStats>();
            }

            if (playerController == null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }

        // ✅ ADD: Debug log
        if (playerStats == null)
        {
            Debug.LogError("UI_PlayerStatsPanel: PlayerStats not found!");
        }
        else
        {
            Debug.Log($"UI_PlayerStatsPanel: PlayerStats found. Max HP: {playerStats.GetMaxHealth()}");
        }
    }

    void SetupDefaultConfigs()
    {
        if (statConfigs.Count == 0)
        {
            statConfigs = new List<StatConfig>
            {
                new StatConfig { statType = StatType.HP, label = "HP" },
                new StatConfig { statType = StatType.MaxHP, label = "Max HP" },
                new StatConfig { statType = StatType.Damage, label = "Damage" },
                new StatConfig { statType = StatType.DamageMultiplier, label = "Damage Multi" },
                new StatConfig { statType = StatType.Speed, label = "Speed" },
                new StatConfig { statType = StatType.AttackSpeed, label = "Attack Speed" },
                new StatConfig { statType = StatType.CritChance, label = "Crit Chance" },
                new StatConfig { statType = StatType.HealthRegen, label = "Regen" }
            };
        }
    }

    void CreateStatEntries()
    {
        if (statEntryPrefab == null || statsContainer == null)
        {
            Debug.LogError("UI_PlayerStatsPanel: Prefab or Container is null!");
            return;
        }

        ClearExistingEntries();
        CreateEntriesFromConfigs();
    }

    void ClearExistingEntries()
    {
        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void CreateEntriesFromConfigs()
    {
        foreach (StatConfig config in statConfigs)
        {
            GameObject entryObj = Instantiate(statEntryPrefab, statsContainer);
            entryObj.name = $"StatEntry_{config.statType}";

            StatEntry entry = entryObj.GetComponent<StatEntry>();
            if (entry != null)
            {
                entry.Setup(config.label, "0"); // Initial value

                if (config.icon != null)
                {
                    entry.SetIcon(config.icon);
                }

                config.entry = entry;
            }
        }
    }

    // ========== REFRESH ==========

    public void RefreshStats()
    {
        if (!FindAndValidatePlayer())
        {
            Debug.LogWarning("UI_PlayerStatsPanel: Cannot refresh - player not found");
            return;
        }

        foreach (StatConfig config in statConfigs)
        {
            UpdateStat(config);
        }
    }

    bool FindAndValidatePlayer()
    {
        // Try to find player if references are null
        if (playerStats == null || playerController == null)
        {
            FindPlayerReferences();
        }

        // Validate
        if (playerStats == null || playerController == null)
        {
            return false;
        }

        return true;
    }

    void UpdateStat(StatConfig config)
    {
        if (config.entry == null) return;

        switch (config.statType)
        {
            case StatType.HP:
                UpdateHP(config.entry);
                break;
            case StatType.MaxHP:
                UpdateMaxHP(config.entry);
                break;
            case StatType.Damage:
                UpdateDamage(config.entry);
                break;
            case StatType.DamageMultiplier:
                UpdateDamageMultiplier(config.entry);
                break;
            case StatType.Speed:
                UpdateSpeed(config.entry);
                break;
            case StatType.AttackSpeed:
                UpdateAttackSpeed(config.entry);
                break;
            case StatType.CritChance:
                UpdateCritChance(config.entry);
                break;
            case StatType.HealthRegen:
                UpdateHealthRegen(config.entry);
                break;
        }
    }

    // ========== STAT UPDATES ==========

    void UpdateHP(StatEntry entry)
    {
        if (playerStats == null) return;

        float current = playerStats.GetCurrentHealth();
        float max = playerStats.GetMaxHealth();
        entry.SetValue($"{current:F0}/{max:F0}");
    }

    void UpdateMaxHP(StatEntry entry)
    {
        if (playerStats == null) return;

        entry.SetValue($"{playerStats.GetMaxHealth():F0}");
    }

    void UpdateDamage(StatEntry entry)
    {
        if (playerStats == null) return;

        entry.SetValue($"{playerStats.GetTotalDamage():F1}");
    }

    void UpdateDamageMultiplier(StatEntry entry)
    {
        if (playerStats == null) return;

        entry.SetValue($"{playerStats.GetDamageMultiplier():F2}x");
    }

    void UpdateSpeed(StatEntry entry)
    {
        if (playerController == null) return;

        entry.SetValue($"{playerController.GetCurrentMoveSpeed():F1}");
    }

    void UpdateAttackSpeed(StatEntry entry)
    {
        if (playerController == null) return;

        entry.SetValue($"{playerController.GetAttackSpeedMultiplier():F2}x");
    }

    void UpdateCritChance(StatEntry entry)
    {
        if (playerStats == null) return;

        entry.SetValue($"{playerStats.GetCritChance() * 100:F0}%");
    }

    void UpdateHealthRegen(StatEntry entry)
    {
        if (playerStats == null) return;

        entry.SetValue($"{playerStats.GetHealthRegen():F1}/s");
    }

    // ========== PUBLIC METHODS ==========

    public void AddStat(StatConfig config)
    {
        if (statConfigs.Contains(config)) return;

        statConfigs.Add(config);
        CreateStatEntries();
        RefreshStats();
    }

    public void RemoveStat(StatType statType)
    {
        StatConfig config = statConfigs.Find(c => c.statType == statType);
        if (config != null)
        {
            statConfigs.Remove(config);
            CreateStatEntries();
            RefreshStats();
        }
    }

    public StatEntry GetStatEntry(StatType statType)
    {
        StatConfig config = statConfigs.Find(c => c.statType == statType);
        return config?.entry;
    }
}

// ========== SUPPORT CLASSES ==========

public enum StatType
{
    HP,
    MaxHP,
    Damage,
    DamageMultiplier,
    Speed,
    AttackSpeed,
    CritChance,
    HealthRegen
}

[System.Serializable]
public class StatConfig
{
    public StatType statType;
    public string label;
    public Sprite icon;

    [HideInInspector]
    public StatEntry entry;
}