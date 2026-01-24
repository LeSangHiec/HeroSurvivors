using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float baseMaxHealth;
    [SerializeField] private float currentHealth;

    [Header("Combat Stats")]
    [SerializeField] private float baseDamage ;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float critChance ;

    [Header("Regeneration")]
    [SerializeField] private float healthRegenPerSecond = 0f;
    private float regenTimer = 0f;

    [Header("UI References")]
    [SerializeField] private Image hpBar;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;

    // ✅ THÊM: Damage Flash Settings
    [Header("Damage Flash Effect")]
    [SerializeField] private bool enableDamageFlash = true;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int flashCount = 1; // Số lần nhấp nháy

    private bool isDead = false;
    private Color originalColor; // ✅ THÊM: Lưu màu gốc

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // ✅ THÊM: Lưu màu gốc
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Start()
    {
        baseMaxHealth = maxHealth;
        currentHealth = maxHealth;
        UpdateHpBar();
    }

    void Update()
    {
        if (isDead) return;
        HandleHealthRegen();
    }

    // ========== HEALTH MANAGEMENT ==========

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHpBar();

        // ✅ THÊM: Flash effect khi bị damage
        if (enableDamageFlash)
        {
            StartCoroutine(DamageFlash());
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ✅ THÊM: Coroutine flash đỏ
    System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            // Flash đỏ
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);

            // Về màu gốc
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // Đảm bảo về màu gốc
        spriteRenderer.color = originalColor;
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);

        if (actualHeal > 0)
        {
            currentHealth += actualHeal;
            UpdateHpBar();
        }
    }

    void HandleHealthRegen()
    {
        if (healthRegenPerSecond > 0f)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= 1f)
            {
                Heal(healthRegenPerSecond);
                regenTimer = 0f;
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("★ Player Died! ★");

        StopAllCoroutines();
        ResetVisuals();
        StopMovement();
        SpawnDeathEffect();
        PlayDeathSound();
        TriggerDeathEvents();

        gameObject.SetActive(false);
    }

    void ResetVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor; // ✅ SỬA: Về màu gốc
        }
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void SpawnDeathEffect()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
    }

    void PlayDeathSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDeath();
        }
    }

    void TriggerDeathEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerPlayerDeath();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHealth / maxHealth;
        }
    }

    // ========== STAT UPGRADES ==========

    public void IncreaseMaxHealth(float amount)
    {
        float healthPercentage = currentHealth / maxHealth;
        maxHealth += amount;
        currentHealth = maxHealth * healthPercentage;
        UpdateHpBar();
    }

    public void IncreaseDamage(float percentage)
    {
        damageMultiplier += percentage;
    }

    public void IncreaseCritChance(float amount)
    {
        critChance += amount;
        critChance = Mathf.Min(critChance, 1f);
    }

    public void IncreaseHealthRegen(float amount)
    {
        healthRegenPerSecond += amount;
    }

    // ========== GETTERS ==========

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public float GetBaseDamage() => baseDamage;
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetTotalDamage() => baseDamage * damageMultiplier;
    public float GetCritChance() => critChance;
    public float GetHealthRegen() => healthRegenPerSecond;
    public float GetBaseMaxHealth() => baseMaxHealth;
}