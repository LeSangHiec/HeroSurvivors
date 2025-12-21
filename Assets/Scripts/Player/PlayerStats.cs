using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Combat Stats")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float critChance = 0f;

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

    private bool isDead = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHpBar();
    }

    void Update()
    {
        if (isDead) return;

        // Health regeneration
        if (healthRegenPerSecond > 0f)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= 1f)
            {
                Heal(healthRegenPerSecond);
                regenTimer = 0f;
            }
        }

        // Debug keys
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(20f);
            Debug.Log("Debug: Took 20 damage");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(20f);
            Debug.Log("Debug: Healed 20 HP");
        }
    }

    // ========== HEALTH SYSTEM ==========

    public void TakeDamage(float damageAmount)
    {
        // ← SỬA: Chỉ check isDead
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {damageAmount} damage. HP: {currentHealth}/{maxHealth}");

        UpdateHpBar();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);

        if (actualHeal > 0)
        {
            currentHealth += actualHeal;
            UpdateHpBar();

            Debug.Log($"Player healed {actualHeal}. HP: {currentHealth}/{maxHealth}");
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        StopAllCoroutines();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDeath();
        }

        Debug.Log("Player died!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        gameObject.SetActive(false);
    }

    void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHealth / maxHealth;

           
        }
    }

    // ========== UPGRADE METHODS ==========

    public void IncreaseMaxHealth(float amount)
    {
        float healthPercentage = currentHealth / maxHealth;

        maxHealth += amount;
        currentHealth = maxHealth * healthPercentage;

        UpdateHpBar();

        Debug.Log($"<color=green>Max Health increased by {amount}! Now: {maxHealth}</color>");
    }

    public void IncreaseDamage(float percentage)
    {
        damageMultiplier += percentage;

        Debug.Log($"<color=red>Damage increased by {percentage * 100}%! Multiplier: {damageMultiplier:F2}x</color>");
    }

    public void IncreaseCritChance(float amount)
    {
        critChance += amount;
        critChance = Mathf.Min(critChance, 1f);

        Debug.Log($"<color=yellow>Crit Chance increased by {amount * 100}%! Now: {critChance * 100}%</color>");
    }

    public void IncreaseHealthRegen(float amount)
    {
        healthRegenPerSecond += amount;

        Debug.Log($"<color=cyan>Health Regen increased by {amount}/sec! Now: {healthRegenPerSecond}/sec</color>");
    }

    // ========== PUBLIC GETTERS ==========

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public float GetBaseDamage() => baseDamage;
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetTotalDamage() => baseDamage * damageMultiplier;
    public float GetCritChance() => critChance;
    public float GetHealthRegen() => healthRegenPerSecond;
}