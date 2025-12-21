using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";
    public float startTime = 0f; // Thời gian bắt đầu wave (giây)

    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Danh sách enemy có thể spawn
    public int[] enemyWeights; // Tỷ lệ spawn (càng cao càng hay spawn)

    [Header("Spawn Settings")]
    public int enemiesPerMinute = 30; // Số enemy spawn mỗi phút
    public int maxEnemiesAlive = 50; // Giới hạn enemy cùng lúc

    [Header("Difficulty")]
    public float enemyHealthMultiplier = 1f; // Nhân HP
    public float enemyDamageMultiplier = 1f; // Nhân damage
    public float enemySpeedMultiplier = 1f; // Nhân speed
}

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private Wave[] waves;

    [Header("Spawn Area")]
    [SerializeField] private Transform player;
    [SerializeField] private float minSpawnDistance = 15f; // Spawn xa player tối thiểu
    [SerializeField] private float maxSpawnDistance = 20f; // Spawn xa player tối đa

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    [Header("Debug")]
    [SerializeField] private bool showSpawnGizmos = true;

    // Private variables
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
        // Tự động tìm player nếu chưa gắn
        if (player == null)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        // Tự động tìm GameTimer nếu chưa gắn
        if (gameTimer == null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
        }

        StartSpawning();
    }

    void Update()
    {
        if (!isSpawning) return;

        // Update wave dựa trên thời gian
        UpdateCurrentWave();

        // Spawn enemies
        if (Time.time >= nextSpawnTime && currentWave != null)
        {
            SpawnEnemy();
            CalculateNextSpawnTime();
        }

        // Cleanup null enemies
        CleanupDestroyedEnemies();
    }

    void UpdateCurrentWave()
    {
        if (gameTimer == null) return;

        float currentTime = gameTimer.GetCurrentTime();

        // Tìm wave phù hợp với thời gian hiện tại
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

        Debug.Log($"<color=yellow>WAVE CHANGED: {currentWave.waveName}</color>");
        Debug.Log($"Enemies/min: {currentWave.enemiesPerMinute}, Max alive: {currentWave.maxEnemiesAlive}");

        CalculateNextSpawnTime();
    }

    void SpawnEnemy()
    {
        // Check giới hạn enemies
        if (activeEnemies.Count >= currentWave.maxEnemiesAlive)
        {
            return;
        }

        // Chọn enemy ngẫu nhiên theo weight
        GameObject enemyPrefab = GetRandomEnemyPrefab();
        if (enemyPrefab == null)
        {
            Debug.LogWarning("No enemy prefab found in current wave!");
            return;
        }

        // Tính vị trí spawn
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Apply multipliers
        ApplyWaveMultipliers(enemy);

        // Add vào list
        activeEnemies.Add(enemy);
    }

    GameObject GetRandomEnemyPrefab()
    {
        if (currentWave.enemyPrefabs.Length == 0) return null;

        // Nếu không có weights, chọn random bình thường
        if (currentWave.enemyWeights.Length != currentWave.enemyPrefabs.Length)
        {
            return currentWave.enemyPrefabs[Random.Range(0, currentWave.enemyPrefabs.Length)];
        }

        // Weighted random selection
        int totalWeight = 0;
        foreach (int weight in currentWave.enemyWeights)
        {
            totalWeight += weight;
        }

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

    Vector3 GetRandomSpawnPosition()
    {
        if (player == null)
        {
            return Vector3.zero;
        }

        // Random góc
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Random khoảng cách
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Tính vị trí
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );

        return player.position + offset;
    }

    void ApplyWaveMultipliers(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript == null) return;

        // Apply multipliers
        enemyScript.ApplyMultipliers(
            currentWave.enemyHealthMultiplier,
            currentWave.enemyDamageMultiplier,
            currentWave.enemySpeedMultiplier
        );
    }

    void CalculateNextSpawnTime()
    {
        if (currentWave == null) return;

        // Tính interval dựa trên enemies per minute
        float spawnInterval = 60f / currentWave.enemiesPerMinute;
        nextSpawnTime = Time.time + spawnInterval;
    }

    void CleanupDestroyedEnemies()
    {
        // Remove null references (enemies đã chết)
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("Wave spawner started!");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Wave spawner stopped!");
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    // Visualize spawn area
    void OnDrawGizmos()
    {
        if (!showSpawnGizmos || player == null) return;

        // Min spawn distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);

        // Max spawn distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
    }

    // Public getters
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public Wave GetCurrentWave() => currentWave;
    public int GetCurrentWaveIndex() => currentWaveIndex;
}