using UnityEngine;
using System.Collections.Generic;

public class GarlicWeapon : WeaponBase
{
    [Header("Garlic Settings")]
    [SerializeField] private float damageRadius = 2f; // ← Changed to 2
    [SerializeField] private float damageTickRate = 0.5f;
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject auraEffect;
    [SerializeField] private SpriteRenderer auraSpriteRenderer;
    [SerializeField] private bool scaleVisualWithRadius = true;

    private CircleCollider2D damageCollider;
    private float nextDamageTick = 0f;
    private HashSet<Enemy> enemiesInRange = new HashSet<Enemy>();
    private float baseRadius = 2f; // ← Changed to 2

    // ========== INITIALIZATION ==========

    protected override void Awake()
    {
        base.Awake();
        SetupDamageCollider();

        baseRadius = damageRadius; // Store initial (2f)
    }

    protected override void Start()
    {
        base.Start();
        SetupVisuals();
    }

    void SetupDamageCollider()
    {
        damageCollider = gameObject.AddComponent<CircleCollider2D>();
        damageCollider.isTrigger = true;
        damageCollider.radius = damageRadius;
    }

    void SetupVisuals()
    {
        if (auraSpriteRenderer == null && weaponSprite != null)
        {
            auraSpriteRenderer = weaponSprite;
        }

        if (auraEffect != null)
        {
            auraEffect.SetActive(true);
        }
    }

    protected override void InitializeFromData()
    {
        base.InitializeFromData();

        // ✅ Base radius from weapon data
        if (weaponData != null)
        {
            baseRadius = weaponData.projectileSpeed; // Should be 2 in SO
        }

        UpdateRadiusVisuals();
    }

    // ========== UPDATE ==========

    protected override void Update()
    {
        UpdateRotation();
        HandleDamageTick();
    }

    protected override void UpdateRotation()
    {
        if (weaponSprite != null)
        {
            weaponSprite.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }


    // ========== AURA DAMAGE ==========

    void HandleDamageTick()
    {
        if (Time.time < nextDamageTick) return;

        DamageEnemiesInRange();
        nextDamageTick = Time.time + damageTickRate;
    }

    void DamageEnemiesInRange()
    {
        if (enemiesInRange.Count == 0) return;

        float totalDamage = CalculateTotalDamage();

        List<Enemy> enemiesList = new List<Enemy>(enemiesInRange);

        foreach (Enemy enemy in enemiesList)
        {
            if (enemy == null || enemy.IsDead()) continue;

            enemy.TakeDamage(totalDamage);
            ApplyKnockback(enemy);
            PlayHitEffect(enemy.transform.position);
        }

        PlayDamageSound();
    }

    void ApplyKnockback(Enemy enemy)
    {
        if (player == null) return;

        Vector2 direction = (enemy.transform.position - player.position).normalized;
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

        if (enemyRb != null)
        {
            enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // ========== COLLISION DETECTION ==========

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.IsDead())
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

    // ========== VISUAL EFFECTS ==========

    void PlayHitEffect(Vector3 position)
    {
        if (weaponData != null && weaponData.muzzleEffect != null)
        {
            GameObject effect = Instantiate(weaponData.muzzleEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    void PlayDamageSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponShoot();
        }
    }

    // ========== UPGRADE HANDLING ==========

    protected override void ApplyStats()
    {
        base.ApplyStats();

        // ✅ Calculate new radius: +1 per level
        int levelBonus = currentWeaponLevel - 1;

        // Base radius from weapon data
        float baseRadiusFromData = weaponData != null ? weaponData.projectileSpeed : 2f;

        // Add +1 per level
        damageRadius = baseRadiusFromData + (levelBonus * 1f); // ← Changed to 1f per level

        // Update tick rate
        if (levelBonus > 0)
        {
            damageTickRate = Mathf.Max(0.2f, fireRate - (levelBonus * 0.05f));
        }

        // Update visuals
        UpdateRadiusVisuals();
    }

    // ========== VISUAL UPDATE ==========

    void UpdateRadiusVisuals()
    {
        // Update collider
        if (damageCollider != null)
        {
            damageCollider.radius = damageRadius;
        }

        // Update sprite scale
        if (scaleVisualWithRadius && weaponSprite != null)
        {
            float scaleMultiplier = damageRadius / baseRadius;
            weaponSprite.transform.localScale = Vector3.one * scaleMultiplier;
        }

        // Update aura effect scale (if exists)
        if (scaleVisualWithRadius && auraEffect != null)
        {
            float scaleMultiplier = damageRadius / baseRadius;
            auraEffect.transform.localScale = Vector3.one * scaleMultiplier;
        }
    }

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        if (Application.isPlaying && enemiesInRange.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (Enemy enemy in enemiesInRange)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }
}