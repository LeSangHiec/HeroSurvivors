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

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    [Header("Debug")]
    [SerializeField] private bool showSpawnGizmos = true;

    // State
    private Wave currentWave;
    private int currentWaveIndex = -1;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime = 0f;
    private bool isSpawning = false;

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

        if (Time.time >= nextSpawnTime && currentWave != null)
        {
            SpawnEnemy();
            CalculateNextSpawnTime();
        }

        CleanupDestroyedEnemies();
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

        // Log wave start (kept for feedback)
        Debug.Log($"<color=yellow>★ {currentWave.waveName} Started! ★</color>");

        TriggerWaveEvent();
        CalculateNextSpawnTime();
    }

    void TriggerWaveEvent()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerWaveChanged(currentWaveIndex);
        }
    }

    // ========== SPAWNING ==========

    void SpawnEnemy()
    {
        if (activeEnemies.Count >= currentWave.maxEnemiesAlive)
        {
            return;
        }

        GameObject enemyPrefab = GetRandomEnemyPrefab();
        if (enemyPrefab == null) return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = CreateEnemy(enemyPrefab, spawnPosition);

        if (enemy == null) return;

        ApplyWaveMultipliers(enemy);
        TrackEnemy(enemy);
        TriggerSpawnEvents();
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
    }

    void TrackEnemy(GameObject enemy)
    {
        activeEnemies.Add(enemy);
    }

    void TriggerSpawnEvents()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerEnemyCountChanged(activeEnemies.Count);
        }
    }

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

        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);

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