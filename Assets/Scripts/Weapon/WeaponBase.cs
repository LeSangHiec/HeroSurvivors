using UnityEngine;
using System.Collections;

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] protected WeaponSO weaponData;

    [Header("References")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected SpriteRenderer weaponSprite;

    // Stats (from WeaponSO)
    protected float baseDamage;
    protected float fireRate;
    protected float projectileSpeed;
    protected int projectileCount;
    protected float spreadAngle;

    // References
    protected Transform player;
    protected PlayerStats playerStats;
    protected Camera mainCamera;

    // State
    protected float nextFireTime = 0f;
    protected bool canFire = true;

    // ========== UNITY LIFECYCLE ==========

    protected virtual void Awake()
    {
        mainCamera = Camera.main;

        // Find player
        player = transform.parent;
        while (player != null && player.GetComponent<PlayerStats>() == null)
        {
            player = player.parent;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        // Auto-find components
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.LogWarning($"{gameObject.name}: FirePoint not found!");
            }
        }

        if (weaponSprite == null)
        {
            weaponSprite = GetComponentInChildren<SpriteRenderer>();
        }
    }

    protected virtual void Start()
    {
        if (weaponData != null)
        {
            InitializeFromData();
        }
        else
        {
            Debug.LogError($"{gameObject.name}: WeaponSO is not assigned!");
        }
    }

    protected virtual void Update()
    {
        UpdateRotation();
        HandleShooting();
    }

    // ========== INITIALIZATION ==========

    public virtual void SetWeaponData(WeaponSO data)
    {
        weaponData = data;
        InitializeFromData();
    }

    protected virtual void InitializeFromData()
    {
        if (weaponData == null) return;

        baseDamage = weaponData.baseDamage;
        fireRate = weaponData.fireRate;
        projectileSpeed = weaponData.projectileSpeed;
        projectileCount = weaponData.projectileCount;
        spreadAngle = weaponData.spreadAngle;

        if (weaponSprite != null && weaponData.weaponSprite != null)
        {
            weaponSprite.sprite = weaponData.weaponSprite;
            weaponSprite.transform.localPosition = weaponData.spriteOffset;
        }

        // ← THÊM: Apply weapon offset
        transform.localPosition = weaponData.weaponOffset;

        Debug.Log($"<color=cyan>{weaponData.weaponName} initialized</color>");
    }

    // ========== VIRTUAL METHODS (Có code mặc định) ==========

    protected virtual void UpdateRotation()
    {
        if (mainCamera == null) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation offset
        if (weaponData != null)
        {
            angle += weaponData.rotationOffset;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip sprite
        if (weaponSprite != null && player != null)
        {
            weaponSprite.flipY = mousePosition.x < player.position.x;
        }
    }

    protected virtual void HandleShooting()
    {
        if (!canFire) return;

        if (CanShoot() && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    protected virtual bool CanShoot()
    {
        // Default: Hold mouse to shoot
        return Input.GetMouseButton(0);
    }

    protected virtual void Fire()
    {
        if (weaponData == null || weaponData.projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot fire - missing data!");
            return;
        }

        // Spawn projectiles
        for (int i = 0; i < projectileCount; i++)
        {
            SpawnProjectile(i);
        }

        // Play effects
        PlayShootEffects();
    }

    protected virtual void SpawnProjectile(int index)
    {
        // Calculate spread angle
        float angle = 0f;
        if (projectileCount > 1 && spreadAngle > 0f)
        {
            float step = spreadAngle / (projectileCount - 1);
            angle = -spreadAngle / 2f + step * index;
        }

        // Spawn with rotation
        Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);
        GameObject projectile = Instantiate(weaponData.projectilePrefab, firePoint.position, rotation);

        // Set velocity
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = rotation * Vector2.right;
            rb.linearVelocity = direction * projectileSpeed;
        }

        // Set damage
        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            float totalDamage = CalculateTotalDamage();
            bullet.SetDamage(totalDamage);
        }
    }
    // ========== DAMAGE CALCULATION ==========

    protected virtual float CalculateTotalDamage()
    {
        float totalDamage = baseDamage;

        if (playerStats != null)
        {
            float playerBaseDamage = playerStats.GetBaseDamage();
            float damageMultiplier = playerStats.GetDamageMultiplier();

            totalDamage = (baseDamage + playerBaseDamage) * damageMultiplier;
        }

        return totalDamage;
    }

    // ========== EFFECTS ==========

    protected virtual void PlayShootEffects()
    {
        // Muzzle flash
        StartCoroutine(MuzzleFlash());

        // Spawn effect
        if (weaponData != null && weaponData.muzzleEffect != null && firePoint != null)
        {
            Instantiate(weaponData.muzzleEffect, firePoint.position, firePoint.rotation);
        }

        // Play sound
        PlayShootSound();
    }

    protected virtual IEnumerator MuzzleFlash()
    {
        if (weaponSprite != null)
        {
            Color originalColor = weaponSprite.color;
            weaponSprite.color = Color.yellow;
            yield return new WaitForSeconds(0.05f);
            weaponSprite.color = originalColor;
        }
    }

    protected virtual void PlayShootSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponShoot();
        }
    }

    // ========== PUBLIC METHODS ==========

    public WeaponSO GetWeaponData() => weaponData;
    public float GetBaseDamage() => baseDamage;
    public float GetTotalDamage() => CalculateTotalDamage();

    public virtual void SetActive(bool active)
    {
        canFire = active;
        gameObject.SetActive(active);
    }
}