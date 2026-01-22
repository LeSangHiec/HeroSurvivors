using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";
    public float startTime = 0f;

    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public int[] enemyWeights;

    [Header("Spawn Settings")]
    public int enemiesPerMinute = 30;
    public int maxEnemiesAlive = 50;
    public int minEnemiesAlive = 20; // ← NEW: Minimum enemies to maintain

    [Header("Wave Transition")]
    [Tooltip("Gradually remove old wave enemies when they go off-screen")]
    public bool removeOldEnemiesOffScreen = true;
    [Tooltip("Maximum old enemies allowed to stay")]
    public int maxOldEnemiesAllowed = 20;

    [Header("Difficulty")]
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
}

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private Wave[] waves;

    [Header("Spawn Area")]
    [SerializeField] private Transform player;
    [SerializeField] private float minSpawnDistance = 15f;
    [SerializeField] private float maxSpawnDistance = 20f;

    [Header("Off-Screen Detection")]
    [SerializeField] private float offScreenMargin = 2f;

    [Header("Dynamic Spawning")]
    [SerializeField] private float minSpawnCheckInterval = 1f; // Check every second
    [SerializeField] private bool enableDynamicSpawning = true;

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    [Header("Debug")]
    [SerializeField] private bool showSpawnGizmos = true;
    [SerializeField] private bool showDebugLogs = false;

    // State
    private Wave currentWave;
    private int currentWaveIndex = -1;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Dictionary<GameObject, int> enemyWaveMap = new Dictionary<GameObject, int>();
    private float nextSpawnTime = 0f;
    private float nextMinSpawnCheck = 0f;
    private bool isSpawning = false;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;
    }

    void Start()
    {
        FindPlayer();
        FindGameTimer();
        StartSpawning();
    }

    void Update()
    {
        if (!isSpawning) return;

        UpdateCurrentWave();

        // Normal timed spawning
        if (Time.time >= nextSpawnTime && currentWave != null)
        {
            if (CanSpawnEnemy())
            {
                SpawnEnemy();
                CalculateNextSpawnTime();
            }
        }

        // ✅ NEW: Dynamic spawning to maintain minimum
        if (enableDynamicSpawning && Time.time >= nextMinSpawnCheck)
        {
            CheckMinimumEnemies();
            nextMinSpawnCheck = Time.time + minSpawnCheckInterval;
        }

        CleanupDestroyedEnemies();
        RemoveOldEnemiesOffScreen();
    }

    // ========== INITIALIZATION ==========

    void FindPlayer()
    {
        if (player == null)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
    }

    void FindGameTimer()
    {
        if (gameTimer == null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
        }
    }

    // ========== WAVE MANAGEMENT ==========

    void UpdateCurrentWave()
    {
        if (gameTimer == null) return;

        float currentTime = gameTimer.GetCurrentTime();

        for (int i = waves.Length - 1; i >= 0; i--)
        {
            if (currentTime >= waves[i].startTime)
            {
                if (currentWaveIndex != i)
                {
                    SwitchToWave(i);
                }
                break;
            }
        }
    }

    void SwitchToWave(int waveIndex)
    {
        currentWaveIndex = waveIndex;
        currentWave = waves[waveIndex];

        Debug.Log($"<color=yellow>★ {currentWave.waveName} Started! ★</color>");

        TriggerWaveEvent();
        ResetSpawnTimer();
    }

    void ResetSpawnTimer()
    {
        nextSpawnTime = Time.time;
        nextMinSpawnCheck = Time.time;
    }

    void TriggerWaveEvent()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerWaveChanged(currentWaveIndex);
        }
    }

    // ========== SPAWNING ==========

    bool CanSpawnEnemy()
    {
        int currentWaveEnemyCount = GetCurrentWaveEnemyCount();
        return currentWaveEnemyCount < currentWave.maxEnemiesAlive;
    }

    void SpawnEnemy()
    {
        GameObject enemyPrefab = GetRandomEnemyPrefab();
        if (enemyPrefab == null) return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = CreateEnemy(enemyPrefab, spawnPosition);

        if (enemy == null) return;

        ApplyWaveMultipliers(enemy);
        TrackEnemy(enemy);
        TriggerSpawnEvents();

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>Spawned {enemyPrefab.name}. Current wave enemies: {GetCurrentWaveEnemyCount()}</color>");
        }
    }

    // ✅ NEW: Check and maintain minimum enemy count
    void CheckMinimumEnemies()
    {
        if (currentWave == null) return;

        int currentWaveEnemyCount = GetCurrentWaveEnemyCount();

        if (currentWaveEnemyCount < currentWave.minEnemiesAlive)
        {
            int toSpawn = currentWave.minEnemiesAlive - currentWaveEnemyCount;
            toSpawn = Mathf.Min(toSpawn, currentWave.maxEnemiesAlive - currentWaveEnemyCount);

            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>Low enemy count ({currentWaveEnemyCount}/{currentWave.minEnemiesAlive}). Spawning {toSpawn} enemies...</color>");
            }

            for (int i = 0; i < toSpawn; i++)
            {
                if (CanSpawnEnemy())
                {
                    SpawnEnemy();
                }
            }
        }
    }

    GameObject CreateEnemy(GameObject prefab, Vector3 position)
    {
        if (EnemyPoolManager.Instance != null)
        {
            return EnemyPoolManager.Instance.SpawnEnemy(prefab.name, position, Quaternion.identity);
        }
        else
        {
            return Instantiate(prefab, position, Quaternion.identity);
        }
    }

    void ApplyWaveMultipliers(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.ApplyMultipliers(
                currentWave.enemyHealthMultiplier,
                currentWave.enemyDamageMultiplier,
                currentWave.enemySpeedMultiplier
            );
        }
        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>Applied multipliers to {enemy.name}: " +
                      $"HP x{currentWave.enemyHealthMultiplier}, " +
                      $"DMG x{currentWave.enemyDamageMultiplier}, " +
                      $"SPD x{currentWave.enemySpeedMultiplier}</color>");
        }
    }

    void TrackEnemy(GameObject enemy)
    {
        activeEnemies.Add(enemy);
        enemyWaveMap[enemy] = currentWaveIndex;
    }

    void TriggerSpawnEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerEnemyCountChanged(activeEnemies.Count);
        }
    }

    // ========== OLD ENEMY REMOVAL ==========

    void RemoveOldEnemiesOffScreen()
    {
        if (currentWaveIndex == 0) return;
        if (currentWave == null || !currentWave.removeOldEnemiesOffScreen) return;
        if (mainCamera == null) return;

        int oldEnemyCount = GetOldEnemyCount();

        if (oldEnemyCount <= currentWave.maxOldEnemiesAllowed) return;

        List<GameObject> oldEnemiesOffScreen = GetOldEnemiesOffScreen();

        if (oldEnemiesOffScreen.Count == 0) return;

        GameObject enemyToRemove = oldEnemiesOffScreen[0];
        RemoveEnemy(enemyToRemove);
    }

    int GetOldEnemyCount()
    {
        int count = 0;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null && enemyWaveMap.ContainsKey(enemy))
            {
                if (enemyWaveMap[enemy] < currentWaveIndex)
                {
                    count++;
                }
            }
        }

        return count;
    }

    List<GameObject> GetOldEnemiesOffScreen()
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null) continue;
            if (!enemyWaveMap.ContainsKey(enemy)) continue;

            if (enemyWaveMap[enemy] < currentWaveIndex)
            {
                if (IsOffScreen(enemy.transform.position))
                {
                    result.Add(enemy);
                }
            }
        }

        return result;
    }

    bool IsOffScreen(Vector3 worldPosition)
    {
        if (mainCamera == null) return false;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPosition);

        bool offScreen = viewportPos.x < -offScreenMargin ||
                        viewportPos.x > 1f + offScreenMargin ||
                        viewportPos.y < -offScreenMargin ||
                        viewportPos.y > 1f + offScreenMargin;

        return offScreen;
    }

    void RemoveEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        activeEnemies.Remove(enemy);

        if (enemyWaveMap.ContainsKey(enemy))
        {
            enemyWaveMap.Remove(enemy);
        }

        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.DespawnEnemy(enemy);
        }
        else
        {
            Destroy(enemy);
        }

        TriggerCountChangedEvent();
    }

    int GetCurrentWaveEnemyCount()
    {
        int count = 0;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null && enemyWaveMap.ContainsKey(enemy))
            {
                if (enemyWaveMap[enemy] == currentWaveIndex)
                {
                    count++;
                }
            }
        }

        return count;
    }

    // ========== ENEMY SELECTION ==========

    GameObject GetRandomEnemyPrefab()
    {
        if (currentWave.enemyPrefabs.Length == 0) return null;

        if (currentWave.enemyWeights.Length != currentWave.enemyPrefabs.Length)
        {
            return GetRandomPrefabUnweighted();
        }

        return GetRandomPrefabWeighted();
    }

    GameObject GetRandomPrefabUnweighted()
    {
        return currentWave.enemyPrefabs[Random.Range(0, currentWave.enemyPrefabs.Length)];
    }

    GameObject GetRandomPrefabWeighted()
    {
        int totalWeight = CalculateTotalWeight();
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < currentWave.enemyPrefabs.Length; i++)
        {
            currentWeight += currentWave.enemyWeights[i];
            if (randomValue < currentWeight)
            {
                return currentWave.enemyPrefabs[i];
            }
        }

        return currentWave.enemyPrefabs[0];
    }

    int CalculateTotalWeight()
    {
        int totalWeight = 0;
        foreach (int weight in currentWave.enemyWeights)
        {
            totalWeight += weight;
        }
        return totalWeight;
    }

    // ========== SPAWN POSITION ==========

    Vector3 GetRandomSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );

        return player.position + offset;
    }

    void CalculateNextSpawnTime()
    {
        if (currentWave == null) return;

        float spawnInterval = 60f / currentWave.enemiesPerMinute;
        nextSpawnTime = Time.time + spawnInterval;
    }

    // ========== CLEANUP ==========

    void CleanupDestroyedEnemies()
    {
        int oldCount = activeEnemies.Count;

        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy)
            {
                toRemove.Add(enemy);
            }
        }

        foreach (GameObject enemy in toRemove)
        {
            activeEnemies.Remove(enemy);

            if (enemyWaveMap.ContainsKey(enemy))
            {
                enemyWaveMap.Remove(enemy);
            }
        }

        if (oldCount != activeEnemies.Count)
        {
            TriggerCountChangedEvent();
        }
    }

    void TriggerCountChangedEvent()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerEnemyCountChanged(activeEnemies.Count);
        }
    }

    // ========== PUBLIC METHODS ==========

    public void StartSpawning()
    {
        isSpawning = true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void ClearAllEnemies()
    {
        if (EnemyPoolManager.Instance != null)
        {
            ClearEnemiesWithPool();
        }
        else
        {
            ClearEnemiesWithDestroy();
        }

        activeEnemies.Clear();
        enemyWaveMap.Clear();
    }

    void ClearEnemiesWithPool()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                EnemyPoolManager.Instance.DespawnEnemy(enemy);
            }
        }
    }

    void ClearEnemiesWithDestroy()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
    }

    // ========== GETTERS ==========

    public int GetActiveEnemyCount() => activeEnemies.Count;
    public Wave GetCurrentWave() => currentWave;
    public int GetCurrentWaveIndex() => currentWaveIndex;

    // ========== GIZMOS ==========

    void OnDrawGizmos()
    {
        if (!showSpawnGizmos || player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
    }
}