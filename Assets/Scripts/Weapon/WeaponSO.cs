using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class WeaponSO : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName = "Weapon";
    public Sprite weaponIcon;
    [TextArea(3, 5)]
    public string description;

    [Header("Visual")]
    public Sprite weaponSprite;
    public Vector3 spriteOffset = Vector3.zero;
    public Vector3 weaponOffset = Vector3.zero; // ← THÊM
    public float rotationOffset = 0f;

    [Header("Stats")]
    public float baseDamage = 10f;
    public float fireRate = 0.2f;
    public float projectileSpeed = 15f;
    public int projectileCount = 1;
    public float spreadAngle = 0f;

    [Header("Projectile")]
    public GameObject projectilePrefab;


    [Header("Effects")]
    public GameObject muzzleEffect;

    [Header("Audio")]
    public AudioClip shootSound;

    [Header("Unlock Requirements")]
    public int unlockLevel = 1;
    public bool isStartingWeapon = false;

    [Header("Upgrade System")]
    public int maxWeaponLevel = 5;
    public bool canUpgrade = true;

    [Header("Upgrade Scaling")]
    [Tooltip("% damage increase per level")]
    public float damagePerLevel = 0.1f; // 10% per level

    [Tooltip("Fire rate decrease per level")]
    public float fireRatePerLevel = 0.02f; // -0.02s per level

    [Tooltip("Projectile speed increase per level")]
    public float speedPerLevel = 0.5f; // +0.5 per level

    [Tooltip("Add extra projectile at these level")]
    public int[] projectileCountLevels = new int[] { 3, 5 };
}