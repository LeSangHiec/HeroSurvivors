using UnityEngine;

public class EventDebugger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool logAllEvents = true;
    [SerializeField] private bool logEnemyEvents = true;
    [SerializeField] private bool logPlayerEvents = true;
    [SerializeField] private bool logStatsEvents = true;
    [SerializeField] private bool logWaveEvents = true;
    [SerializeField] private bool logCardEvents = true;
    [SerializeField] private bool logWeaponEvents = true;
    [SerializeField] private bool onGameStart = true;
    [SerializeField] private bool onGamePause = true;
    [SerializeField] private bool onGameResume = true;
    [SerializeField] private bool onGameOver = true;

    void Start()
    {
        if (!logAllEvents || GameEvents.Instance == null)
        {
            Debug.LogWarning("EventDebugger disabled or GameEvents not found!");
            return;
        }

        SubscribeToAllEvents();

        Debug.Log("<color=green>========================================</color>");
        Debug.Log("<color=green>EVENT DEBUGGER STARTED</color>");
        Debug.Log("<color=green>All events will be logged to console</color>");
        Debug.Log("<color=green>========================================</color>");
    }

    void SubscribeToAllEvents()
    {
        // ========== ENEMY EVENTS ==========
        if (logEnemyEvents)
        {
            GameEvents.Instance.onEnemyKilled.AddListener(enemy =>
                Debug.Log($"<color=red>[EVENT] Enemy Killed: {enemy.name} (Type: {enemy.GetType().Name})</color>")
            );

            GameEvents.Instance.onEnemySpawned.AddListener(enemy =>
                Debug.Log($"<color=yellow>[EVENT] Enemy Spawned: {enemy.name}</color>")
            );

            GameEvents.Instance.onEnemyDamaged.AddListener(enemy =>
                Debug.Log($"<color=orange>[EVENT] Enemy Damaged: {enemy.name} (HP: {enemy.GetCurrentHealth():F1}/{enemy.GetMaxHealth():F1})</color>")
            );
        }

        // ========== PLAYER EVENTS ==========
        if (logPlayerEvents)
        {
            GameEvents.Instance.onPlayerDeath.AddListener(() =>
                Debug.Log($"<color=red>[EVENT] Player Death!</color>")
            );

            GameEvents.Instance.onLevelUp.AddListener(level =>
                Debug.Log($"<color=cyan>[EVENT] Level Up! New Level: {level}</color>")
            );

            GameEvents.Instance.onXPGained.AddListener(xp =>
                Debug.Log($"<color=green>[EVENT] XP Gained: +{xp}</color>")
            );
        }

        // ========== STATS EVENTS ==========
        if (logStatsEvents)
        {
            GameEvents.Instance.onKillCountChanged.AddListener(count =>
                Debug.Log($"<color=magenta>[EVENT] Kill Count Changed: {count}</color>")
            );

            GameEvents.Instance.onPlayTimeChanged.AddListener(time =>
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                // Log mỗi 10 giây
                if (seconds % 10 == 0)
                {
                    Debug.Log($"<color=cyan>[EVENT] Play Time: {minutes:00}:{seconds:00}</color>");
                }
            });
        }

        // ========== WAVE EVENTS ==========
        if (logWaveEvents)
        {
            GameEvents.Instance.onWaveChanged.AddListener(wave =>
                Debug.Log($"<color=yellow>[EVENT] Wave Changed: Wave {wave + 1}</color>")
            );

            GameEvents.Instance.onEnemyCountChanged.AddListener(count =>
                Debug.Log($"<color=orange>[EVENT] Enemy Count: {count}</color>")
            );
        }

        // ========== CARD EVENTS ==========
        if (logCardEvents)
        {
            GameEvents.Instance.onCardSelected.AddListener(card =>
                Debug.Log($"<color=cyan>[EVENT] Card Selected: {card.cardText}</color>")
            );

            GameEvents.Instance.onCardSelectionShown.AddListener(() =>
                Debug.Log($"<color=yellow>[EVENT] Card Selection Shown (Game Paused)</color>")
            );

            GameEvents.Instance.onCardSelectionHidden.AddListener(() =>
                Debug.Log($"<color=green>[EVENT] Card Selection Hidden (Game Resumed)</color>")
            );
        }

        // ========== WEAPON EVENTS ==========
        if (logWeaponEvents)
        {
            GameEvents.Instance.onWeaponUnlocked.AddListener(weapon =>
                Debug.Log($"<color=cyan>[EVENT] Weapon Unlocked: {weapon.weaponName}</color>")
            );

            // Weapon fired log có thể spam, nên comment out
            // GameEvents.Instance.onWeaponFired.AddListener(weaponIndex => 
            //     Debug.Log($"[EVENT] Weapon Fired: {weaponIndex}")
            // );
        }

        // ========== GAME STATE EVENTS ==========
        GameEvents.Instance.onGameStart.AddListener(() =>
            Debug.Log($"<color=green>[EVENT] Game Started!</color>")
        );

        GameEvents.Instance.onGamePause.AddListener(() =>
            Debug.Log($"<color=yellow>[EVENT] Game Paused</color>")
        );

        GameEvents.Instance.onGameResume.AddListener(() =>
            Debug.Log($"<color=green>[EVENT] Game Resumed</color>")
        );

        GameEvents.Instance.onGameOver.AddListener(() =>
            Debug.Log($"<color=red>[EVENT] Game Over!</color>")
        );
     

        // ========== COMBAT EVENTS ==========
        GameEvents.Instance.onDamageDealt.AddListener(damage =>
        {
            // Log mỗi 100 damage để tránh spam
            if (damage >= 50)
            {
                Debug.Log($"<color=orange>[EVENT] Big Damage Dealt: {damage:F1}</color>");
            }
        });
    }
}