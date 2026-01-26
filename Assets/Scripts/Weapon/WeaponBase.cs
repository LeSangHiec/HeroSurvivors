using UnityEngine;
using System.Collections;

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] protected WeaponSO weaponData;

    [Header("References")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected SpriteRenderer weaponSprite;

    [Header("Auto-Targeting")]
    [SerializeField] protected bool enableAutoTargeting = false;
    [SerializeField] protected float targetingRange = 15f;
    [SerializeField] protected float targetingUpdateInterval = 0.2f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected bool showTargetingGizmos = true;

    // Stats
    protected float baseDamage;
    protected float fireRate;
    protected float projectileSpeed;
    protected int projectileCount;
    protected float spreadAngle;
    protected int currentWeaponLevel = 1;

    protected Transform player;
    protected PlayerStats playerStats;
    protected Camera mainCamera;

    protected Transform currentTarget;
    protected float nextTargetUpdateTime = 0f;

    // State
    protected float nextFireTime = 0f;
    protected bool canFire = true;


    protected virtual void Awake()
    {
        mainCamera = Camera.main;

        player = transform.parent;
        while (player != null && player.GetComponent<PlayerStats>() == null)
        {
            player = player.parent;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
        }

        if (weaponSprite == null)
        {
            weaponSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
    }

    protected virtual void Start()
    {
        if (weaponData != null)
        {
            InitializeFromData();
            SubscribeEvents();
        }
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    protected virtual void Update()
    {
        if (enableAutoTargeting)
        {
            UpdateAutoTargeting();
        }
        else
        {
            UpdateRotation(); 
        }

        HandleShooting();
    }


    public virtual void SetWeaponData(WeaponSO data)
    {
        weaponData = data;
        InitializeFromData();
    }

    protected virtual void InitializeFromData()
    {
        if (weaponData == null) return;

        if (WeaponTracker.Instance != null)
        {
            currentWeaponLevel = WeaponTracker.Instance.GetWeaponLevel(weaponData);
            if (currentWeaponLevel == 0) currentWeaponLevel = 1;
        }

        ApplyStats();

        if (weaponSprite != null && weaponData.weaponSprite != null)
        {
            weaponSprite.sprite = weaponData.weaponSprite;
            weaponSprite.transform.localPosition = weaponData.spriteOffset;
        }

        transform.localPosition = weaponData.weaponOffset;
    }

    protected virtual void ApplyStats()
    {
        if (weaponData == null) return;

        baseDamage = weaponData.baseDamage;
        fireRate = weaponData.fireRate;
        projectileSpeed = weaponData.projectileSpeed;
        projectileCount = weaponData.projectileCount;
        spreadAngle = weaponData.spreadAngle;

        int levelBonus = currentWeaponLevel - 1;

        if (levelBonus > 0)
        {
            float multiplier = 1f + weaponData.damagePerLevel * levelBonus;
            baseDamage *= multiplier;

            fireRate = Mathf.Max(0.05f, fireRate - weaponData.fireRatePerLevel * levelBonus);

            projectileSpeed += weaponData.speedPerLevel * levelBonus;

            foreach (int level in weaponData.projectileCountLevels)
            {
                if (currentWeaponLevel >= level)
                {
                    projectileCount++;
                }
            }
        }
    }


    void SubscribeEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onWeaponUpgraded.AddListener(OnWeaponUpgraded);
        }
    }

    void UnsubscribeEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onWeaponUpgraded.RemoveListener(OnWeaponUpgraded);
        }
    }

    void OnWeaponUpgraded(WeaponSO upgradedWeapon, int newLevel)
    {
        if (weaponData == upgradedWeapon)
        {
            currentWeaponLevel = newLevel;
            ApplyStats();
        }
    }


    protected virtual void UpdateAutoTargeting()
    {
        if (Time.time >= nextTargetUpdateTime)
        {
            FindClosestEnemy();
            nextTargetUpdateTime = Time.time + targetingUpdateInterval;
        }

        if (currentTarget != null)
        {
            Enemy enemy = currentTarget.GetComponent<Enemy>();
            if (enemy != null && enemy.IsDead())
            {
                currentTarget = null;
                return;
            }

            RotateToTarget(currentTarget.position);
        }
        
    }

    protected virtual void FindClosestEnemy()
    {
        currentTarget = null;

        if (player == null) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            player.position,
            targetingRange,
            enemyLayer
        );

        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in enemies)
        {
            if (enemyCollider == null) continue;

            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsDead())
            {
                continue; 
            }

            float distance = Vector2.Distance(player.position, enemyCollider.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = enemyCollider.transform;
            }
        }
    }

    protected virtual void RotateToTarget(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (weaponData != null)
        {
            angle += weaponData.rotationOffset;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (weaponSprite != null && player != null)
        {
            weaponSprite.flipY = targetPosition.x < player.position.x;
        }
    }


    protected virtual void UpdateRotation()
    {
        if (mainCamera == null) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (weaponData != null)
        {
            angle += weaponData.rotationOffset;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);

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
        //  Auto mode bắn khi có target, Manual mode  bắn khi nhấn chuột
        if (enableAutoTargeting)
        {
            return currentTarget != null;
        }
        else
        {
            return Input.GetMouseButton(0);
        }
    }

    protected virtual void Fire()
    {
        if (weaponData == null || weaponData.projectilePrefab == null || firePoint == null)
        {
            return;
        }

        for (int i = 0; i < projectileCount; i++)
        {
            SpawnProjectile(i);
        }

        PlayShootEffects();
    }

    protected virtual void SpawnProjectile(int index)
    {
        if (weaponData.projectilePrefab == null || firePoint == null) return;

        float currentSpread = 0f;
        if (projectileCount > 1)
        {
            float step = spreadAngle / (projectileCount - 1);
            currentSpread = -spreadAngle / 2f + step * index;
        }

        Quaternion spreadRotation = Quaternion.Euler(0, 0, currentSpread);
        Vector2 direction = spreadRotation * firePoint.right;

        // Spawn bullet từ pool
        GameObject bulletObj = null;
        string poolName = weaponData.projectilePrefab.name;

        if (PoolManager.Instance != null)
        {
            bulletObj = PoolManager.Instance.Spawn(
                poolName,
                firePoint.position,
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)
            );
        }
        else
        {
            bulletObj = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)
            );
        }

        if (bulletObj == null) return;

        // Set velocity
        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * projectileSpeed;
        }

        // Set damage
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            float totalDamage = CalculateTotalDamage();
            bullet.SetDamage(totalDamage);
            bullet.ResetBullet();
        }
    }

    protected virtual float CalculateTotalDamage()
    {
        float totalDamage = baseDamage;

        if (playerStats != null)
        {
            float playerBaseDamage = playerStats.GetBaseDamage();
            float damageMultiplier = playerStats.GetDamageMultiplier();

            totalDamage = (baseDamage + playerBaseDamage) * damageMultiplier;

            // Crit 
            float critChance = playerStats.GetCritChance();
            bool isCrit = Random.value < critChance;

            if (isCrit)
            {
                totalDamage *= 2f; 
            }
        }

        return totalDamage;
    }

    // ========== EFFECTS ==========

    protected virtual void PlayShootEffects()
    {
        StartCoroutine(MuzzleFlash());

        // Spawn muzzle effect
        if (weaponData != null && weaponData.muzzleEffect != null && firePoint != null)
        {
            Instantiate(weaponData.muzzleEffect, firePoint.position, firePoint.rotation);
        }

        PlayShootSound();
    }

    protected virtual IEnumerator MuzzleFlash()
    {
        if (weaponSprite != null)
        {
            Color originalColor = weaponSprite.color;
            weaponSprite.color = Color.red;
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

    // ========== DEBUG GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        if (!showTargetingGizmos) return;

        // Lấy player position
        Vector3 centerPosition = player != null ? player.position : transform.position;

        if (enableAutoTargeting)
        {
            // Vẽ phạm vi targeting (vòng tròn vàng)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPosition, targetingRange);

            // Vẽ line đến target hiện tại (đỏ)
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, 0.5f);

                // Label
#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    currentTarget.position + Vector3.up,
                    "TARGET",
                    new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } }
                );
#endif
            }
        }
    }

    // ========== PUBLIC GETTERS ==========

    public WeaponSO GetWeaponData() => weaponData;
    public float GetBaseDamage() => baseDamage;
    public float GetTotalDamage() => CalculateTotalDamage();
    public Transform GetCurrentTarget() => currentTarget;
    public bool IsAutoTargeting() => enableAutoTargeting;

    public virtual void SetActive(bool active)
    {
        canFire = active;
        gameObject.SetActive(active);
    }

    public void SetAutoTargeting(bool enabled)
    {
        enableAutoTargeting = enabled;
    }

    public void SetTargetingRange(float range)
    {
        targetingRange = Mathf.Max(0f, range);
    }
}