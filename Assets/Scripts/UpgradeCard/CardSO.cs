using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardSO : ScriptableObject
{
    [Header("Card Info")]
    public string cardText;
    public Sprite CardImage;

    [Header("Card Type")]
    public CardType cardType;

    [Header("Upgrade Type")]
    public CardEffect effectType;

    [Header("Upgrade Values")]
    public float effectValue;

    [Header("Weapon Type")]
    public WeaponSO weaponData;
    public GameObject weaponPrefab; // ← Prefab của weapon

    [Header("Requirements")]
    public int unlockLevel;
    public bool isUnique;

    [Header("Level System")]
    public int maxLevel = 5; // Max level cho card này
    public bool canStack = true; // Có thể pick nhiều lần không?

    [Header("Weapon Upgrade")]
    [Tooltip("Weapon to upgrade (for WeaponUpgrade card type)")]
    public WeaponSO targetWeapon;
}

public enum CardType
{
    Upgrade,
    Weapon,
    WeaponUpgrade
}

public enum CardEffect
{
    None,
    MaxHealth,
    MoveSpeed,
    Damage,
    AttackSpeed,
    CritChance,
    HealthRegen,
    XPGain
}
