using System.Collections.Generic;
using UnityEngine;

public class ObstaclePoolManager : MonoBehaviour
{
    public static ObstaclePoolManager Instance { get; private set; }

    [System.Serializable]
    public class ObstaclePool
    {
        public string poolName;
        public GameObject[] prefabVariants;
        public int initialSize = 50;
        public int maxSize = 200;
        public Transform poolParent;
    }

    [Header("Obstacle Pools")]
    [SerializeField] private ObstaclePool rockPool;
    [SerializeField] private ObstaclePool treePool;

    [Header("Settings")]
    [SerializeField] private Transform obstaclesParent;

    // Tracking
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, ObstaclePool> poolSettings;
    private Dictionary<string, int> activeCount;
    private Dictionary<string, List<GameObject>> activeObstacles;

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

    // ========== INITIALIZATION ==========

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettings = new Dictionary<string, ObstaclePool>();
        activeCount = new Dictionary<string, int>();
        activeObstacles = new Dictionary<string, List<GameObject>>();

        CreateObstaclesParent();
        InitializeRockPool();
        InitializeTreePool();
    }

    void CreateObstaclesParent()
    {
        if (obstaclesParent == null)
        {
            GameObject parent = new GameObject("ActiveObstacles");
            parent.transform.SetParent(transform);
            obstaclesParent = parent.transform;
        }
    }

    void InitializeRockPool()
    {
        if (rockPool.prefabVariants != null && rockPool.prefabVariants.Length > 0)
        {
            InitializePool(rockPool);
        }
    }

    void InitializeTreePool()
    {
        if (treePool.prefabVariants != null && treePool.prefabVariants.Length > 0)
        {
            InitializePool(treePool);
        }
    }

    void InitializePool(ObstaclePool pool)
    {
        CreatePoolParent(pool);
        PreSpawnObstacles(pool);
        RegisterPool(pool);
    }

    void CreatePoolParent(ObstaclePool pool)
    {
        if (pool.poolParent == null)
        {
            GameObject parent = new GameObject($"Pool_{pool.poolName}");
            parent.transform.SetParent(transform);
            pool.poolParent = parent.transform;
        }
    }

    void PreSpawnObstacles(ObstaclePool pool)
    {
        Queue<GameObject> objectQueue = new Queue<GameObject>();

        for (int i = 0; i < pool.initialSize; i++)
        {
            GameObject obj = CreateNewObstacle(pool);
            objectQueue.Enqueue(obj);
        }

        poolDictionary.Add(pool.poolName, objectQueue);
    }

    void RegisterPool(ObstaclePool pool)
    {
        poolSettings.Add(pool.poolName, pool);
        activeCount.Add(pool.poolName, 0);
        activeObstacles.Add(pool.poolName, new List<GameObject>());
    }

    GameObject CreateNewObstacle(ObstaclePool pool)
    {
        GameObject prefab = pool.prefabVariants[Random.Range(0, pool.prefabVariants.Length)];
        GameObject obj = Instantiate(prefab, pool.poolParent);
        obj.SetActive(false);
        obj.name = $"{pool.poolName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        return obj;
    }

    // ========== SPAWN ==========

    public GameObject SpawnObstacle(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            return null;
        }

        GameObject obj = GetObstacleFromPool(poolName);
        if (obj == null) return null;

        SetupSpawnedObstacle(obj, position, rotation);
        TrackObstacle(poolName, obj);

        return obj;
    }

    GameObject GetObstacleFromPool(string poolName)
    {
        if (poolDictionary[poolName].Count > 0)
        {
            return poolDictionary[poolName].Dequeue();
        }

        ObstaclePool pool = poolSettings[poolName];

        if (activeCount[poolName] < pool.maxSize)
        {
            return CreateNewObstacle(pool);
        }

        return null;
    }

    void SetupSpawnedObstacle(GameObject obj, Vector3 position, Quaternion rotation)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(obstaclesParent);
        obj.SetActive(true);
    }

    void TrackObstacle(string poolName, GameObject obj)
    {
        activeCount[poolName]++;
        activeObstacles[poolName].Add(obj);
    }

    // ========== DESPAWN ==========

    public void DespawnObstacle(string poolName, GameObject obj)
    {
        if (obj == null) return;

        if (!poolDictionary.ContainsKey(poolName))
        {
            Destroy(obj);
            return;
        }

        ResetObstacle(obj, poolName);
        ReturnToPool(obj, poolName);
    }

    void ResetObstacle(GameObject obj, string poolName)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolSettings[poolName].poolParent);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
    }

    void ReturnToPool(GameObject obj, string poolName)
    {
        poolDictionary[poolName].Enqueue(obj);
        activeCount[poolName]--;
        activeObstacles[poolName].Remove(obj);
    }

    public void DespawnObstacles(string poolName, List<GameObject> obstacles)
    {
        if (obstacles == null) return;

        List<GameObject> copy = new List<GameObject>(obstacles);

        foreach (GameObject obj in copy)
        {
            if (obj != null)
            {
                DespawnObstacle(poolName, obj);
            }
        }
    }

    // ========== QUERIES ==========

    public int GetActiveCount(string poolName)
    {
        return activeCount.ContainsKey(poolName) ? activeCount[poolName] : 0;
    }


   
}