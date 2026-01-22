using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage = 10f;
    [SerializeField] private Image hpBar;

    [Header("Movement")]
    [SerializeField] protected float enemyMoveSpeed = 1f;

    [Header("Obstacle Avoidance")]
    [SerializeField] protected bool enableAvoidance = true;
    [SerializeField] protected float detectionDistance = 1.5f;
    [SerializeField] protected float avoidanceForce = 3f;
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Rigidbody2D rb;

    [Header("Drops")]
    [SerializeField] protected int xpDropAmount = 10;
    [SerializeField] protected GameObject xpGemPrefab;
    [SerializeField] protected GameObject deathEffect;

    [Header("Death Animation")] // ← NEW
    [SerializeField] protected bool useDeathAnimation = false;
    [SerializeField] protected float deathAnimationDuration = 1f;
    [SerializeField] protected string deathAnimationTrigger = "Die";

    [Header("Health Pickup Drop")]
    [SerializeField] protected GameObject healthPickupPrefab;
    [SerializeField] protected float healthDropChance = 0.15f; // tỉ lệ drop
    [SerializeField] protected float healthDropAmount = 150f;

    protected PlayerController player;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        currentHealth = maxHealth;
        UpdateHpBar();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        MoveToPlayer();
        FlipEnemy();
    }


    protected void MoveToPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.transform.position - transform.position).normalized;

        if (enableAvoidance)
        {
            direction = ApplyObstacleAvoidance(direction);
        }

        if (rb != null)
        {
            rb.linearVelocity = direction * enemyMoveSpeed;
        }
        else
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                enemyMoveSpeed * Time.deltaTime
            );
        }
    }

    protected Vector2 ApplyObstacleAvoidance(Vector2 desiredDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            desiredDirection,
            detectionDistance,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            Vector2 avoidanceDirection = Vector2.Perpendicular(hit.normal);
            Vector2 finalDirection = (desiredDirection + avoidanceDirection * avoidanceForce).normalized;
            return finalDirection;
        }

        return desiredDirection;
    }

    protected void FlipEnemy()
    {
        if (player != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = player.transform.position.x < transform.position.x;
        }
    }


    public virtual void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHpBar();
        OnDamaged();

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerEnemyDamaged(this);
            GameEvents.Instance.TriggerDamageDealt(damageAmount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void OnDamaged()
    {
        StartCoroutine(FlashRed());
    }

    protected System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        StopMovement();
        DisableCollider();

        if (useDeathAnimation)
        {
            StartCoroutine(PlayDeathAnimation());
        }
        else
        {
            CompleteDeath();
        }
    }

    protected virtual void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    protected virtual System.Collections.IEnumerator PlayDeathAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(deathAnimationTrigger);
        }

        yield return new WaitForSeconds(deathAnimationDuration);

        CompleteDeath();
    }

    protected virtual void CompleteDeath()
    {
        TriggerDeathEvents();
        SpawnDeathEffect();
        DropLoot();
        HandlePooling();
    }

    void DisableCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    void TriggerDeathEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerEnemyKilled(this);
        }
    }

    void SpawnDeathEffect()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
    }

    void HandlePooling()
    {
        PooledEnemy pooledEnemy = GetComponent<PooledEnemy>();
        if (pooledEnemy != null)
        {
            pooledEnemy.OnEnemyDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== RESET ==========

    public virtual void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;

        UpdateHpBar();
        ResetVisuals();
        ResetPhysics();
        EnableCollider();
    }

    void ResetVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    void ResetPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void EnableCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    // ========== UI ==========

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHealth / maxHealth;
        }
    }

    //  Rơi XP 

    protected virtual void DropLoot()
    {
        DropXP();
        DropHealthPickup();
    }

    void DropXP()
    {
        if (xpGemPrefab != null)
        {
            GameObject xpGem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity);

            XPGem gemScript = xpGem.GetComponent<XPGem>();
            if (gemScript != null)
            {
                gemScript.SetXPValue(xpDropAmount);
            }
        }
    }

    void DropHealthPickup()
    {
        if (healthPickupPrefab == null) return;

        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= healthDropChance)
        {
            Vector3 dropPosition = transform.position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );

            GameObject healthPickup = Instantiate(healthPickupPrefab, dropPosition, Quaternion.identity);

            HealthPickup pickupScript = healthPickup.GetComponent<HealthPickup>();
            if (pickupScript != null)
            {
                pickupScript.SetHealAmount(healthDropAmount);
            }
        }
    }

    //  va chạm 

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnEnterDamage(collision.gameObject);
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnStayDamage(collision.gameObject);
        }
    }

    protected virtual void OnEnterDamage(GameObject playerObject) { }
    protected virtual void OnStayDamage(GameObject playerObject) { }

    //  MULTIPLIERS 

    public void SetHealthMultiplier(float multiplier)
    {
        maxHealth *= multiplier;
        currentHealth = maxHealth;
        UpdateHpBar();
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damage *= multiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        enemyMoveSpeed *= multiplier;
    }

    public void ApplyMultipliers(float health, float dmg, float speed)
    {
        SetHealthMultiplier(health);
        SetDamageMultiplier(dmg);
        SetSpeedMultiplier(speed);
    }

    // ========== GETTERS ==========

    public float GetDamage() => damage;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        if (!enableAvoidance || player == null) return;

        Vector2 direction = (player.transform.position - transform.position).normalized;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, direction * detectionDistance);
    }
}