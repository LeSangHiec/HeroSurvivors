using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponProgress
{
    public WeaponSO weapon;
    public int currentLevel;

    public WeaponProgress(WeaponSO weapon)
    {
        this.weapon = weapon;
        this.currentLevel = 1;
    }
}

public class WeaponTracker : MonoBehaviour
{
    public static WeaponTracker Instance { get; private set; }

    [Header("Weapon Progress")]
    [SerializeField] private List<WeaponProgress> unlockedWeapons = new List<WeaponProgress>();

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

    // ========== WEAPON MANAGEMENT ==========

    public void RegisterWeapon(WeaponSO weapon)
    {
        if (GetWeaponProgress(weapon) == null)
        {
            unlockedWeapons.Add(new WeaponProgress(weapon));
        }
    }

    public void UpgradeWeapon(WeaponSO weapon)
    {
        WeaponProgress progress = GetWeaponProgress(weapon);

        if (progress == null)
        {
            return;
        }

        if (progress.currentLevel < weapon.maxWeaponLevel)
        {
            progress.currentLevel++;


            TriggerUpgradeEvent(weapon, progress.currentLevel);
        }

    }

    void TriggerUpgradeEvent(WeaponSO weapon, int level)
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerWeaponUpgraded(weapon, level);
        }
    }

    // ========== QUERIES ==========

    public WeaponProgress GetWeaponProgress(WeaponSO weapon)
    {
        return unlockedWeapons.Find(w => w.weapon == weapon);
    }

    public int GetWeaponLevel(WeaponSO weapon)
    {
        WeaponProgress progress = GetWeaponProgress(weapon);
        return progress != null ? progress.currentLevel : 0;
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        return GetWeaponProgress(weapon) != null;
    }

    public bool IsWeaponMaxLevel(WeaponSO weapon)
    {
        WeaponProgress progress = GetWeaponProgress(weapon);
        return progress != null && progress.currentLevel >= weapon.maxWeaponLevel;
    }

    public List<WeaponProgress> GetAllWeapons()
    {
        return new List<WeaponProgress>(unlockedWeapons);
    }

    // ========== UTILITY ==========

    public void ResetWeapons()
    {
        unlockedWeapons.Clear();
    }
}