using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private int maxWeapons = 6;

    [Header("Starting Weapon")]
    [SerializeField] private WeaponSO startingWeapon;
    [SerializeField] private GameObject startingWeaponPrefab;

    private List<WeaponBase> activeWeapons = new List<WeaponBase>();
    private List<WeaponSO> unlockedWeapons = new List<WeaponSO>();

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

        FindPlayer();
        CreateWeaponContainer();
    }

    void Start()
    {
        if (startingWeapon != null && startingWeaponPrefab != null)
        {
            AddWeapon(startingWeapon, startingWeaponPrefab);
        }
    }

    // ========== INITIALIZATION ==========

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            
        }
    }

    void CreateWeaponContainer()
    {
        if (player == null) return;

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
            }
        }
    }

    // ========== WEAPON MANAGEMENT ==========

    public void AddWeapon(WeaponSO weaponData, GameObject weaponPrefab)
    {
        if (!ValidateWeapon(weaponData, weaponPrefab)) return;
        if (HasWeapon(weaponData)) return;
        if (!CanAddWeapon()) return;

        GameObject weaponObj = InstantiateWeapon(weaponPrefab, weaponData);
        if (weaponObj == null) return;

        SetupWeapon(weaponObj, weaponData);
    }

    bool ValidateWeapon(WeaponSO weaponData, GameObject weaponPrefab)
    {
        if (weaponData == null || weaponPrefab == null)
        {
            return false;
        }

        if (weaponContainer == null)
        {
            return false;
        }

        return true;
    }

    GameObject InstantiateWeapon(GameObject weaponPrefab, WeaponSO weaponData)
    {
        GameObject weaponObj = Instantiate(weaponPrefab, weaponContainer);
        weaponObj.name = weaponData.weaponName;
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        weaponObj.transform.localScale = Vector3.one;

        return weaponObj;
    }

    void SetupWeapon(GameObject weaponObj, WeaponSO weaponData)
    {
        WeaponBase weapon = weaponObj.GetComponent<WeaponBase>();

        if (weapon != null)
        {
            weapon.SetWeaponData(weaponData);
            activeWeapons.Add(weapon);
            unlockedWeapons.Add(weaponData);

            TriggerWeaponEvents(weaponData);
            RegisterWeapon(weaponData);

        }
        else
        {
            Destroy(weaponObj);
        }
    }

    void TriggerWeaponEvents(WeaponSO weaponData)
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerWeaponUnlocked(weaponData);
        }
    }

    void RegisterWeapon(WeaponSO weaponData)
    {
        if (WeaponTracker.Instance != null)
        {
            WeaponTracker.Instance.RegisterWeapon(weaponData);
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
        }
    }

    // ========== QUERIES ==========

    public bool CanAddWeapon() => activeWeapons.Count < maxWeapons;
    public bool HasWeapon(WeaponSO weaponData) => unlockedWeapons.Contains(weaponData);
    public int GetActiveWeaponCount() => activeWeapons.Count;
    public int GetMaxWeapons() => maxWeapons;
    public List<WeaponSO> GetUnlockedWeapons() => new List<WeaponSO>(unlockedWeapons);
    public List<WeaponBase> GetActiveWeapons() => new List<WeaponBase>(activeWeapons);
    public Transform GetPlayer() => player;
}