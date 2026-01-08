using Unity.VisualScripting;
using UnityEngine;

public class RunStats : MonoBehaviour
{
    public static RunStats Instance { get; private set; }

    // ========== STATS ==========

    [Header("Kill Stats")]
    [SerializeField] private int killCount = 0;
    [SerializeField] private int basicEnemyKills = 0;
    [SerializeField] private int fastEnemyKills = 0;
    [SerializeField] private int rangedEnemyKills = 0;
    [SerializeField] private int explosionEnemyKills = 0;

    [Header("Time Stats")]
    [SerializeField] private float playTime = 0f;
    [SerializeField] private bool isRunning = true;

    [Header("Wave Stats")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int highestWave = 0;

    [Header("Damage Stats")]
    [SerializeField] private float totalDamageDealt = 0f;
    [SerializeField] private float totalDamageTaken = 0f;

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
    }

    void Start()
    {
        // ========== SUBSCRIBE TO EVENTS ========== ← QUAN TRỌNG!
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onEnemyKilled.AddListener(OnEnemyKilled);
            GameEvents.Instance.onDamageDealt.AddListener(OnDamageDealt);
            GameEvents.Instance.onDamageTaken.AddListener(OnDamageTaken);
            GameEvents.Instance.onWaveChanged.AddListener(OnWaveChanged);
        }
        else
        {
            Debug.LogError("GameEvents.Instance is null! RunStats will not work!");
        }

        ResetStats();
    }

    void OnDestroy()
    {
        // ========== UNSUBSCRIBE FROM EVENTS ========== ← QUAN TRỌNG!
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.onEnemyKilled.RemoveListener(OnEnemyKilled);
            GameEvents.Instance.onDamageDealt.RemoveListener(OnDamageDealt);
            GameEvents.Instance.onDamageTaken.RemoveListener(OnDamageTaken);
            GameEvents.Instance.onWaveChanged.RemoveListener(OnWaveChanged);
        }
    }

    void Update()
    {
        if (isRunning)
        {
            UpdatePlayTime();
        }

        // Debug key
        if (Input.GetKeyDown(KeyCode.K))
        {
            LogStats();
        }
    }

    // ========== EVENT HANDLERS ==========

    void OnEnemyKilled(Enemy enemy)
    {
        killCount++;

        // Track specific enemy types
        string enemyType = enemy.GetType().Name;

        switch (enemyType)
        {
            case "BasicEnemy":
                basicEnemyKills++;
                break;
            case "FastEnemy":
                fastEnemyKills++;
                break;
            case "RangedEnemy":
                rangedEnemyKills++;
                break;
            case "ExplosionEnemy":
                explosionEnemyKills++;
                break;
        }

        // Trigger UI update event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerKillCountChanged(killCount);
        }

    }

    void OnDamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    void OnDamageTaken(float damage)
    {
        totalDamageTaken += damage;
    }

    void OnWaveChanged(int wave)
    {
        currentWave = wave;

        if (wave > highestWave)
        {
            highestWave = wave;
        }

        Debug.Log($"RunStats: Wave changed to {wave + 1}");
    }

    // ========== TIME TRACKING ==========

    void UpdatePlayTime()
    {
        playTime += Time.deltaTime;

        // Trigger event every second
        if (Mathf.FloorToInt(playTime) != Mathf.FloorToInt(playTime - Time.deltaTime))
        {
            if (GameEvents.Instance != null)
            {
                GameEvents.Instance.TriggerPlayTimeChanged(playTime);
            }
        }
    }

    // ========== CONTROL METHODS ==========

    public void PauseStats()
    {
        isRunning = false;
    }

    public void ResumeStats()
    {
        isRunning = true;
    }

    public void ResetStats()
    {
        killCount = 0;
        basicEnemyKills = 0;
        fastEnemyKills = 0;
        rangedEnemyKills = 0;
        explosionEnemyKills = 0;
        playTime = 0f;
        currentWave = 0;
        totalDamageDealt = 0f;
        totalDamageTaken = 0f;
        isRunning = true;

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.TriggerKillCountChanged(killCount);
            GameEvents.Instance.TriggerPlayTimeChanged(playTime);
        }

    }

    // ========== GETTERS ==========

    public int GetKillCount() => killCount;
    public int GetBasicEnemyKills() => basicEnemyKills;
    public int GetFastEnemyKills() => fastEnemyKills;
    public int GetRangedEnemyKills() => rangedEnemyKills;
    public int GetExplosionEnemyKills() => explosionEnemyKills;
    public float GetPlayTime() => playTime;
    public int GetCurrentWave() => currentWave;
    public int GetHighestWave() => highestWave;
    public float GetTotalDamageDealt() => totalDamageDealt;
    public float GetTotalDamageTaken() => totalDamageTaken;

    public string GetPlayTimeFormatted()
    {
        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public float GetKillsPerMinute()
    {
        if (playTime <= 0) return 0f;
        return killCount / (playTime / 60f);
    }

    public float GetDPS()
    {
        if (playTime <= 0) return 0f;
        return totalDamageDealt / playTime;
    }

    // ========== DEBUG ==========

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;

        if (wave > highestWave)
        {
            highestWave = wave;
        }
    }

    public void AddDamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    public void AddDamageTaken(float damage)
    {
        totalDamageTaken += damage;
    }

    public void LogStats()
    {
        Debug.Log("========== RUN STATS ==========");
        Debug.Log($"Kills: {killCount}");
        Debug.Log($"  - BasicEnemy: {basicEnemyKills}");
        Debug.Log($"  - FastEnemy: {fastEnemyKills}");
        Debug.Log($"  - RangedEnemy: {rangedEnemyKills}");
        Debug.Log($"  - ExplosionEnemy: {explosionEnemyKills}");
        Debug.Log($"Play Time: {GetPlayTimeFormatted()}");
        Debug.Log($"Wave: {currentWave + 1} (Highest: {highestWave + 1})");
        Debug.Log($"Damage Dealt: {totalDamageDealt:F1}");
        Debug.Log($"Damage Taken: {totalDamageTaken:F1}");
        Debug.Log($"Kills/Min: {GetKillsPerMinute():F1}");
        Debug.Log($"DPS: {GetDPS():F1}");
        Debug.Log("==============================");
    }
}
