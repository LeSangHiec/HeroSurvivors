using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    [HideInInspector]
    public string poolName; // Pool mà enemy này thuộc về

    // Auto-despawn settings (optional)
    [Header("Auto Despawn (Optional)")]
    [SerializeField] private bool autoDespawnWhenDead = true;
    [SerializeField] private float despawnDelay = 0.5f; // Delay sau khi chết

    private Enemy enemyScript;

    void Awake()
    {
        enemyScript = GetComponent<Enemy>();
    }

    void Update()
    {
        // Auto-despawn khi enemy chết
        if (autoDespawnWhenDead && enemyScript != null && enemyScript.IsDead())
        {
            Invoke(nameof(Despawn), despawnDelay);
        }
    }

    public void Despawn()
    {
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.DespawnEnemy(gameObject);
        }
        else
        {
            // Fallback: destroy nếu không có pool manager
            Destroy(gameObject);
        }
    }
}