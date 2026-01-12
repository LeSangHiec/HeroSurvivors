using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    [HideInInspector]
    public string poolName;

    [Header("Auto Despawn")]
    [SerializeField] private bool autoDespawnOnDeath = true;
    [SerializeField] private float despawnDelay = 2f;

    // ✅ THÊM: Method để handle death
    public void OnEnemyDeath()
    {
        if (autoDespawnOnDeath)
        {
            Invoke(nameof(Despawn), despawnDelay);
        }
        else
        {
            Despawn();
        }
    }

    void Despawn()
    {
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.DespawnEnemy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ✅ THÊM: Cancel despawn khi object disabled (optional)
    void OnDisable()
    {
        CancelInvoke();
    }
}