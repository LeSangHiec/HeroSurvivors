using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ========== SINGLETON ==========

    public static WeaponManager Instance { get; private set; }

    // ========== SETTINGS ==========

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private int maxWeapons = 6;

    [Header("Starting Weapon")]
    [SerializeField] private WeaponSO startingWeapon;
    [SerializeField] private GameObject startingWeaponPrefab;

    // Tracking
    private List<WeaponBase> activeWeapons = new List<WeaponBase>();
    private List<WeaponSO> unlockedWeapons = new List<WeaponSO>();

    // ========== SINGLETON SETUP ==========

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"<color=yellow>Player auto-found: {player.name}</color>");
            }
            else
            {
                Debug.LogError("WeaponManager: Player not found!");
                return;
            }
        }

        // Find or create weapon container
        if (weaponContainer == null)
        {
            weaponContainer = player.Find("WeaponContainer");

            if (weaponContainer == null)
            {
                GameObject container = new GameObject("WeaponContainer");
                container.transform.SetParent(player);
                container.transform.localPosition = Vector3.zero;
                container.transform.localRotation = Quaternion.identity;
                container.transform.localScale = Vector3.one;

                weaponContainer = container.transform;

                Debug.Log($"<color=green>WeaponContainer created under {player.name}</color>");
            }
            else
            {
                Debug.Log($"<color=cyan>WeaponContainer found in {player.name}</color>");
            }
        }
    }

    void Start()
    {
        if (startingWeapon != null && startingWeaponPrefab != null)
        {
            AddWeapon(startingWeapon, startingWeaponPrefab);
        }
    }

    // ========== WEAPON MANAGEMENT ==========

    public bool CanAddWeapon()
    {
        return activeWeapons.Count < maxWeapons;
    }

    public bool HasWeapon(WeaponSO weaponData)
    {
        return unlockedWeapons.Contains(weaponData);
    }

    public void AddWeapon(WeaponSO weaponData, GameObject weaponPrefab)
    {
        if (weaponData == null || weaponPrefab == null)
        {
            Debug.LogError("WeaponManager: Weapon data or prefab is null!");
            return;
        }

        if (HasWeapon(weaponData))
        {
            Debug.LogWarning($"Already has {weaponData.weaponName}!");
            return;
        }

        if (!CanAddWeapon())
        {
            Debug.LogWarning("Max weapons reached!");
            return;
        }

        if (weaponContainer == null)
        {
            Debug.LogError("WeaponManager: Weapon container is null!");
            return;
        }

        // Spawn weapon at container (0, 0, 0)
        GameObject weaponObj = Instantiate(weaponPrefab, weaponContainer);
        weaponObj.name = weaponData.weaponName;

        Debug.Log($"<color=yellow>Weapon spawned: {weaponObj.name}</color>");
        Debug.Log($"<color=yellow>Weapon parent: {weaponObj.transform.parent.name}</color>");
        Debug.Log($"<color=yellow>Weapon position: {weaponObj.transform.position}</color>");
        Debug.Log($"<color=yellow>Weapon active: {weaponObj.activeSelf}</color>");
        // Reset local transform
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        weaponObj.transform.localScale = Vector3.one;

        // Setup weapon with data
        WeaponBase weapon = weaponObj.GetComponent<WeaponBase>();
        if (weapon != null)
        {
            weapon.SetWeaponData(weaponData);
            activeWeapons.Add(weapon);
            unlockedWeapons.Add(weaponData);

            Debug.Log($"<color=cyan>★ EQUIPPED: {weaponData.weaponName} ★</color>");
            Debug.Log($"Active weapons: {activeWeapons.Count}/{maxWeapons}");
        }
        else
        {
            Debug.LogError($"Weapon prefab doesn't have WeaponBase component!");
            Destroy(weaponObj);
        }
    }

    public void RemoveWeapon(WeaponSO weaponData)
    {
        int index = unlockedWeapons.IndexOf(weaponData);

        if (index >= 0 && index < activeWeapons.Count)
        {
            Destroy(activeWeapons[index].gameObject);
            activeWeapons.RemoveAt(index);
            unlockedWeapons.RemoveAt(index);

            Debug.Log($"Removed {weaponData.weaponName}");
        }
    }

    // ========== PUBLIC GETTERS ==========

    public int GetActiveWeaponCount() => activeWeapons.Count;
    public int GetMaxWeapons() => maxWeapons;
    public List<WeaponSO> GetUnlockedWeapons() => new List<WeaponSO>(unlockedWeapons);
    public List<WeaponBase> GetActiveWeapons() => new List<WeaponBase>(activeWeapons);
    public Transform GetPlayer() => player;
} 