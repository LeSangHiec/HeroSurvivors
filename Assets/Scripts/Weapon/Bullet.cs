using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    private float damage = 10f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject muzzleEffect;

    private float spawnTime;
    private bool isReturning = false; // ← Prevent double return

    void Awake()
    {
       
    }

    void OnEnable()
    {
        // Reset state
        spawnTime = Time.time;
        isReturning = false;

        // Spawn muzzle effect
        if (muzzleEffect != null)
        {
            Instantiate(muzzleEffect, transform.position, transform.rotation);
        }
    }

    void Update()
    {
        // Auto return after lifetime
        if (!isReturning && Time.time - spawnTime >= lifetime)
        {
            Debug.Log($"<color=orange>Bullet lifetime expired: {Time.time - spawnTime}s</color>");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReturning) return; // ← Already returning

        // Ignore player
        if (collision.CompareTag("Player"))
            return;

        // Hit enemy
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"<color=red>Bullet hit enemy! Damage: {damage}</color>");
            }

            DestroyBullet(collision.transform.position);
            return;
        }

        // Hit obstacle
        if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("<color=gray>Bullet hit obstacle!</color>");
            DestroyBullet(collision.transform.position);
            return;
        }
    }

    void DestroyBullet(Vector3 hitPosition)
    {
        // Spawn hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Return to pool
        
    }

   

    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
    }
}