using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public string poolName;

    [Header("Auto Despawn")]
    [SerializeField] private bool autoDespawn = true;
    [SerializeField] private float despawnTime = 5f;

    private float spawnTime;

    void OnEnable()
    {
        if (autoDespawn)
        {
            spawnTime = Time.time;
        }
    }

    void Update()
    {
        if (autoDespawn && Time.time - spawnTime >= despawnTime)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAutoDespawn(bool enabled, float time = 5f)
    {
        autoDespawn = enabled;
        despawnTime = time;
    }
}