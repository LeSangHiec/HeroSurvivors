using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 50;
        public int maxSize = 100;
        public bool autoExpand = true;
        public int expandStep = 10;
        public Transform poolParent;
    }

    [Header("Pools")]
    [SerializeField] private List<Pool> pools = new List<Pool>();

    // Internal tracking
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolSettings;
    private Dictionary<string, int> activeCount;
    private Dictionary<string, int> totalCreated;

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
        // Debug hotkey
        if (Input.GetKeyDown(KeyCode.P))
        {
            LogPoolStats();
        }
    }

    // ========== INITIALIZATION ==========

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettings = new Dictionary<string, Pool>();
        activeCount = new Dictionary<string, int>();
        totalCreated = new Dictionary<string, int>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"PoolManager: Prefab is null for pool '{pool.poolName}'!");
                continue;
            }

            CreatePoolParent(pool);
            PreSpawnObjects(pool);
            RegisterPool(pool);
        }
    }

    void CreatePoolParent(Pool pool)
    {
        if (pool.poolParent == null)
        {
            GameObject parent = new GameObject($"Pool_{pool.poolName}");
            parent.transform.SetParent(transform);
            pool.poolParent = parent.transform;
        }
    }

    void PreSpawnObjects(Pool pool)
    {
        Queue<GameObject> objectQueue = new Queue<GameObject>();

        for (int i = 0; i < pool.initialSize; i++)
        {
            GameObject obj = CreateNewObject(pool);
            objectQueue.Enqueue(obj);
        }

        poolDictionary.Add(pool.poolName, objectQueue);
    }

    void RegisterPool(Pool pool)
    {
        poolSettings.Add(pool.poolName, pool);
        activeCount.Add(pool.poolName, 0);
        totalCreated.Add(pool.poolName, pool.initialSize);
    }

    GameObject CreateNewObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab, pool.poolParent);
        obj.SetActive(false);
        obj.name = $"{pool.prefab.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        // Add or get PooledObject component
        PooledObject pooledObj = obj.GetComponent<PooledObject>();
        if (pooledObj == null)
        {
            pooledObj = obj.AddComponent<PooledObject>();
        }
        pooledObj.poolName = pool.poolName;

        return obj;
    }

    // ========== SPAWN ==========

    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool '{poolName}' doesn't exist!");
            return null;
        }

        GameObject obj = GetObjectFromPool(poolName);

        if (obj == null)
        {
            Debug.LogError($"Failed to spawn from pool '{poolName}'!");
            return null;
        }

        SetupSpawnedObject(obj, position, rotation);
        activeCount[poolName]++;

        return obj;
    }

    GameObject GetObjectFromPool(string poolName)
    {
        // Try to get from queue
        if (poolDictionary[poolName].Count > 0)
        {
            return poolDictionary[poolName].Dequeue();
        }

        // Queue is empty, try to expand
        Pool pool = poolSettings[poolName];

        if (pool.autoExpand)
        {
            return ExpandPool(poolName);
        }
        else if (activeCount[poolName] < pool.maxSize)
        {
            return CreateAndTrackNewObject(poolName);
        }

        Debug.LogError($"Pool '{poolName}' reached max size ({pool.maxSize})!");
        return null;
    }

    void SetupSpawnedObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
    }

    GameObject ExpandPool(string poolName)
    {
        Pool pool = poolSettings[poolName];

        // Create expandStep objects
        for (int i = 0; i < pool.expandStep - 1; i++)
        {
            GameObject obj = CreateNewObject(pool);
            poolDictionary[poolName].Enqueue(obj);
            totalCreated[poolName]++;
        }

        // Create last object to return
        GameObject returnObj = CreateNewObject(pool);
        totalCreated[poolName]++;

        return returnObj;
    }

    GameObject CreateAndTrackNewObject(string poolName)
    {
        Pool pool = poolSettings[poolName];
        GameObject obj = CreateNewObject(pool);
        totalCreated[poolName]++;
        return obj;
    }

    // ========== DESPAWN ==========

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        PooledObject pooledObj = obj.GetComponent<PooledObject>();

        if (pooledObj == null)
        {
            Debug.LogError($"Object {obj.name} is not pooled!");
            Destroy(obj);
            return;
        }

        string poolName = pooledObj.poolName;

        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool '{poolName}' doesn't exist!");
            Destroy(obj);
            return;
        }

        ResetObject(obj);
        ReturnToPool(obj, poolName);
    }

    void ResetObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;

        // Reset physics
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void ReturnToPool(GameObject obj, string poolName)
    {
        poolDictionary[poolName].Enqueue(obj);
        activeCount[poolName]--;
    }

    // ========== QUERIES ==========

    public PoolInfo GetPoolInfo(string poolName)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            return null;
        }

        return new PoolInfo
        {
            poolName = poolName,
            active = activeCount[poolName],
            available = poolDictionary[poolName].Count,
            totalCreated = totalCreated[poolName],
            autoExpand = poolSettings[poolName].autoExpand,
            maxSize = poolSettings[poolName].maxSize
        };
    }

    // ========== DEBUG ==========

    public void LogPoolStats()
    {
        Debug.Log("=== POOL STATISTICS ===");

        foreach (var kvp in poolDictionary)
        {
            string poolName = kvp.Key;
            PoolInfo info = GetPoolInfo(poolName);

            string status = info.autoExpand ? "Auto" : $"Max:{info.maxSize}";
            Debug.Log($"{poolName}: Active={info.active}, Available={info.available}, Total={info.totalCreated} ({status})");
        }
    }
}

// ========== POOL INFO CLASS ==========

public class PoolInfo
{
    public string poolName;
    public int active;
    public int available;
    public int totalCreated;
    public bool autoExpand;
    public int maxSize;
}