using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;

    private float spawnTime;

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        CheckLifetime();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandlePlayerHit(collision);
            return;
        }

        if (collision.CompareTag("Obstacle"))
        {
            HandleObstacleHit(collision);
            return;
        }

        // Ignore enemies
        if (collision.CompareTag("Enemy"))
        {
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

    void HandlePlayerHit(Collider2D collision)
    {
        PlayerStats player = collision.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(damage);
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

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void ResetBullet()
    {
        spawnTime = Time.time;
    }
}