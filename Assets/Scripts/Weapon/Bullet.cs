using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    private float damage;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
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
        // Ignore player
        if (collision.CompareTag("Player"))
        {
            return;
        }

        // Hit enemy
        if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
            return;
        }

        // Hit obstacle
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
            enemy.TakeDamage(damage);
        }

        SpawnHitEffect(collision.transform.position);
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
    }

    public void ResetBullet()
    {
        spawnTime = Time.time;
    }
}