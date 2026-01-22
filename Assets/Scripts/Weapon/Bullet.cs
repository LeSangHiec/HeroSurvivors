using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    private float damage;
    private bool isCritical = false; // ← NEW

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject critHitEffect; // ← NEW: Special crit effect
    [SerializeField] private GameObject muzzleEffect;

    private float spawnTime;

    void OnEnable()
    {
        spawnTime = Time.time;
        SpawnMuzzleEffect();
    }

    void Update()
    {
        CheckLifetime();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
            return;
        }

        if (collision.CompareTag("Obstacle"))
        {
            HandleObstacleHit(collision);
            return;
        }
    }

    // ========== LIFETIME ==========

    void CheckLifetime()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    // ========== COLLISION HANDLERS ==========

    void HandleEnemyHit(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // ✅ Apply damage (already includes crit multiplier)
            enemy.TakeDamage(damage);
        }

        // ✅ Spawn appropriate hit effect
        if (isCritical && critHitEffect != null)
        {
            SpawnCritHitEffect(collision.transform.position);
        }
        else
        {
            SpawnHitEffect(collision.transform.position);
        }

        ReturnToPool();
    }

    void HandleObstacleHit(Collider2D collision)
    {
        SpawnHitEffect(collision.transform.position);
        ReturnToPool();
    }

    // ========== EFFECTS ==========

    void SpawnMuzzleEffect()
    {
        if (muzzleEffect != null)
        {
            GameObject effect = Instantiate(muzzleEffect, transform.position, transform.rotation);
            Destroy(effect, 1f);
        }
    }

    void SpawnHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    // ✅ NEW: Crit hit effect
    void SpawnCritHitEffect(Vector3 position)
    {
        GameObject effect = Instantiate(critHitEffect, position, Quaternion.identity);
        Destroy(effect, 1.5f);

        // Optional: Screen shake on crit
        // CameraShake.Instance?.Shake(0.1f, 0.2f);
    }

    // ========== POOLING ==========

    void ReturnToPool()
    {
        PooledObject pooledObj = GetComponent<PooledObject>();
        if (pooledObj != null)
        {
            pooledObj.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== PUBLIC METHODS ==========

    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
        isCritical = false; // Reset crit flag
    }

    // ✅ NEW: Set damage with crit info
    public void SetDamage(float damageAmount, bool critical)
    {
        damage = damageAmount;
        isCritical = critical;
    }

    public void ResetBullet()
    {
        spawnTime = Time.time;
        isCritical = false;
    }

    // ✅ NEW: Get if crit
    public bool IsCritical() => isCritical;
}