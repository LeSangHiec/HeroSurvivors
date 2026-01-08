using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [System.Serializable]
    public class EnemyPool
    {
        public string poolName;
        public GameObject enemyPrefab;
        public int initialSize = 20;
        public int maxSize = 50;
        public bool autoExpand = true;
        public int expandStep = 10;
        public Transform poolParent;
    }

    [Header("Enemy Pools")]
    [SerializeField] private List<EnemyPool> enemyPools = new List<EnemyPool>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform defaultSpawnParent;

    // Tracking
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, EnemyPool> poolSettings;
    private Dictionary<string, int> activeCount;
    private Dictionary<string, int> totalCreated;
    private Dictionary<string, List<GameObject>> activeEnemies;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    void Update()
    {
        CleanupNullReferences();
    }

    // ========== INITIALIZATION ==========

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettings = new Dictionary<string, EnemyPool>();
        activeCount = new Dictionary<string, int>();
        totalCreated = new Dictionary<string, int>();
        activeEnemies = new Dictionary<string, List<GameObject>>();

        CreateDefaultSpawnParent();

        foreach (EnemyPool pool in enemyPools)
        {
            if (pool.enemyPrefab == null)
            {
                continue;
            }

            CreatePoolParent(pool);
            PreSpawnEnemies(pool);
            RegisterPool(pool);
        }
    }

    void CreateDefaultSpawnParent()
    {
        if (defaultSpawnParent == null)
        {
            GameObject parent = new GameObject("ActiveEnemies");
            parent.transform.SetParent(transform);
            defaultSpawnParent = parent.transform;
        }
    }

    void CreatePoolParent(EnemyPool pool)
    {
        if (pool.poolParent == null)
        {
            GameObject parent = new GameObject($"Pool_{pool.poolName}");
            parent.transform.SetParent(transform);
            pool.poolParent = parent.transform;
        }
    }

    void PreSpawnEnemies(EnemyPool pool)
    {
        Queue<GameObject> objectQueue = new Queue<GameObject>();

        for (int i = 0; i < pool.initialSize; i++)
        {
            GameObject enemy = CreateNewEnemy(pool);
            objectQueue.Enqueue(enemy);
        }

        poolDictionary.Add(pool.poolName, objectQueue);
    }

    void RegisterPool(EnemyPool pool)
    {
        poolSettings.Add(pool.poolName, pool);
        activeCount.Add(pool.poolName, 0);
        totalCreated.Add(pool.poolName, pool.initialSize);
        activeEnemies.Add(pool.poolName, new List<GameObject>());
    }

    GameObject CreateNewEnemy(EnemyPool pool)
    {
        GameObject enemy = Instantiate(pool.enemyPrefab, pool.poolParent);
        enemy.SetActive(false);
        enemy.name = $"{pool.enemyPrefab.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();
        if (pooledEnemy == null)
        {
            pooledEnemy = enemy.AddComponent<PooledEnemy>();
        }
        pooledEnemy.poolName = pool.poolName;

        return enemy;
    }

    // ========== SPAWN ==========

    public GameObject SpawnEnemy(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            return null;
        }

        GameObject enemy = GetEnemyFromPool(poolName);
        if (enemy == null) return null;

        SetupSpawnedEnemy(enemy, position, rotation);
        TrackEnemy(poolName, enemy);

        return enemy;
    }

    GameObject GetEnemyFromPool(string poolName)
    {
        if (poolDictionary[poolName].Count > 0)
        {
            return poolDictionary[poolName].Dequeue();
        }

        EnemyPool pool = poolSettings[poolName];

        if (pool.autoExpand)
        {
            return ExpandPool(poolName);
        }
        else if (activeCount[poolName] < pool.maxSize)
        {
            return CreateAndTrackNewEnemy(poolName);
        }

        return null;
    }

    void SetupSpawnedEnemy(GameObject enemy, Vector3 position, Quaternion rotation)
    {
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.transform.SetParent(defaultSpawnParent);
        enemy.SetActive(true);

        ResetEnemyState(enemy);
    }

    void TrackEnemy(string poolName, GameObject enemy)
    {
        activeCount[poolName]++;
        activeEnemies[poolName].Add(enemy);
    }

    GameObject ExpandPool(string poolName)
    {
        EnemyPool pool = poolSettings[poolName];

        for (int i = 0; i < pool.expandStep - 1; i++)
        {
            GameObject enemy = CreateNewEnemy(pool);
            poolDictionary[poolName].Enqueue(enemy);
            totalCreated[poolName]++;
        }

        GameObject returnEnemy = CreateNewEnemy(pool);
        totalCreated[poolName]++;

        return returnEnemy;
    }

    GameObject CreateAndTrackNewEnemy(string poolName)
    {
        EnemyPool pool = poolSettings[poolName];
        GameObject enemy = CreateNewEnemy(pool);
        totalCreated[poolName]++;
        return enemy;
    }

    void ResetEnemyState(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.ResetEnemy();
        }

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ========== DESPAWN ==========

    public void DespawnEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();

        if (pooledEnemy == null)
        {
            Destroy(enemy);
            return;
        }

        string poolName = pooledEnemy.poolName;

        if (!poolDictionary.ContainsKey(poolName))
        {
            Destroy(enemy);
            return;
        }

        ResetEnemy(enemy, poolName);
        ReturnToPool(enemy, poolName);
    }

    void ResetEnemy(GameObject enemy, string poolName)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(poolSettings[poolName].poolParent);
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void ReturnToPool(GameObject enemy, string poolName)
    {
        poolDictionary[poolName].Enqueue(enemy);
        activeCount[poolName]--;
        activeEnemies[poolName].Remove(enemy);
    }

    // ========== QUERIES ==========

    public int GetActiveEnemyCount(string poolName)
    {
        return activeCount.ContainsKey(poolName) ? activeCount[poolName] : 0;
    }

    public int GetTotalActiveEnemies()
    {
        int total = 0;
        foreach (var count in activeCount.Values)
        {
            total += count;
        }
        return total;
    }

    public List<GameObject> GetActiveEnemies(string poolName)
    {
        return activeEnemies.ContainsKey(poolName) ? new List<GameObject>(activeEnemies[poolName]) : new List<GameObject>();
    }

    public List<GameObject> GetAllActiveEnemies()
    {
        List<GameObject> allEnemies = new List<GameObject>();
        foreach (var list in activeEnemies.Values)
        {
            allEnemies.AddRange(list);
        }
        return allEnemies;
    }

    // ========== BATCH OPERATIONS ==========

    public void DespawnAllEnemies()
    {
        List<GameObject> allEnemies = GetAllActiveEnemies();

        foreach (GameObject enemy in allEnemies)
        {
            if (enemy != null)
            {
                DespawnEnemy(enemy);
            }
        }
    }

    public void DespawnEnemiesByPool(string poolName)
    {
        if (!activeEnemies.ContainsKey(poolName)) return;

        List<GameObject> enemies = new List<GameObject>(activeEnemies[poolName]);

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                DespawnEnemy(enemy);
            }
        }
    }

    // ========== UTILITY ==========

    void CleanupNullReferences()
    {
        foreach (var kvp in activeEnemies)
        {
            kvp.Value.RemoveAll(enemy => enemy == null);
        }
    }

    // ========== DEBUG ==========

}