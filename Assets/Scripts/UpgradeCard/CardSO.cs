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
}

public enum CardType
{
    Upgrade,
    Weapon
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
