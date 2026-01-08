using UnityEngine;
using UnityEngine.Events;

// ========== EVENT DEFINITIONS ==========

[System.Serializable]
public class EnemyEvent : UnityEvent<Enemy> { }

[System.Serializable]
public class EnemyGameObjectEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class CardEvent : UnityEvent<CardSO> { }

[System.Serializable]
public class WeaponEvent : UnityEvent<WeaponSO> { }

// ========== GAME EVENTS MANAGER ==========

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    // ========== ENEMY EVENTS ==========
    [Header("Enemy Events")]
    public EnemyEvent onEnemyKilled = new EnemyEvent();
    public EnemyGameObjectEvent onEnemySpawned = new EnemyGameObjectEvent();
    public EnemyEvent onEnemyDamaged = new EnemyEvent();

    // ========== PLAYER EVENTS ==========
    [Header("Player Events")]
    public IntEvent onLevelUp = new IntEvent();
    public FloatEvent onHealthChanged = new FloatEvent();
    public FloatEvent onMaxHealthChanged = new FloatEvent();
    public UnityEvent onPlayerDeath = new UnityEvent();
    public UnityEvent onPlayerRespawn = new UnityEvent();

    // ========== XP EVENTS ==========
    [Header("XP Events")]
    public IntEvent onXPGained = new IntEvent();
    public FloatEvent onXPBarChanged = new FloatEvent();

    // ========== STATS EVENTS ==========
    [Header("Stats Events")]
    public IntEvent onKillCountChanged = new IntEvent();
    public FloatEvent onPlayTimeChanged = new FloatEvent();

    // ========== WAVE EVENTS ==========
    [Header("Wave Events")]
    public IntEvent onWaveChanged = new IntEvent();
    public IntEvent onEnemyCountChanged = new IntEvent();

    // ========== CARD EVENTS ==========
    [Header("Card Events")]
    public CardEvent onCardSelected = new CardEvent();
    public UnityEvent onCardSelectionShown = new UnityEvent();
    public UnityEvent onCardSelectionHidden = new UnityEvent();

    // ========== WEAPON EVENTS ========== ← FIX: CHỈ 1 HEADER!
    [Header("Weapon Events")]
    public WeaponEvent onWeaponUnlocked = new WeaponEvent();
    public UnityEvent<WeaponSO, int> onWeaponUpgraded = new UnityEvent<WeaponSO, int>(); // ← MOVE LÊN
    public IntEvent onWeaponFired = new IntEvent();

    // ========== GAME STATE EVENTS ==========
    [Header("Game State Events")]
    public UnityEvent onGameStart = new UnityEvent();
    public UnityEvent onGamePause = new UnityEvent();
    public UnityEvent onGameResume = new UnityEvent();
    public UnityEvent onGameOver = new UnityEvent();

    // ========== COMBAT EVENTS ==========
    [Header("Combat Events")]
    public FloatEvent onDamageDealt = new FloatEvent();
    public FloatEvent onDamageTaken = new FloatEvent();

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

    // ========== TRIGGER METHODS ==========

    // Enemy
    public void TriggerEnemyKilled(Enemy enemy)
    {
        onEnemyKilled?.Invoke(enemy);
    }

    public void TriggerEnemySpawned(GameObject enemy)
    {
        onEnemySpawned?.Invoke(enemy);
    }

    public void TriggerEnemyDamaged(Enemy enemy)
    {
        onEnemyDamaged?.Invoke(enemy);
    }

    // Player
    public void TriggerLevelUp(int level)
    {
        onLevelUp?.Invoke(level);
    }

    public void TriggerHealthChanged(float health)
    {
        onHealthChanged?.Invoke(health);
    }

    public void TriggerMaxHealthChanged(float maxHealth)
    {
        onMaxHealthChanged?.Invoke(maxHealth);
    }

    public void TriggerPlayerDeath()
    {
        onPlayerDeath?.Invoke();
    }

    public void TriggerPlayerRespawn()
    {
        onPlayerRespawn?.Invoke();
    }

    // XP
    public void TriggerXPGained(int amount)
    {
        onXPGained?.Invoke(amount);
    }

    public void TriggerXPBarChanged(float fillAmount)
    {
        onXPBarChanged?.Invoke(fillAmount);
    }

    // Stats
    public void TriggerKillCountChanged(int count)
    {
        onKillCountChanged?.Invoke(count);
    }

    public void TriggerPlayTimeChanged(float time)
    {
        onPlayTimeChanged?.Invoke(time);
    }

    // Wave
    public void TriggerWaveChanged(int wave)
    {
        onWaveChanged?.Invoke(wave);
    }

    public void TriggerEnemyCountChanged(int count)
    {
        onEnemyCountChanged?.Invoke(count);
    }

    // Card
    public void TriggerCardSelected(CardSO card)
    {
        onCardSelected?.Invoke(card);
    }

    public void TriggerCardSelectionShown()
    {
        onCardSelectionShown?.Invoke();
    }

    public void TriggerCardSelectionHidden()
    {
        onCardSelectionHidden?.Invoke();
    }

    // Weapon
    public void TriggerWeaponUnlocked(WeaponSO weapon)
    {
        onWeaponUnlocked?.Invoke(weapon);
    }

    public void TriggerWeaponUpgraded(WeaponSO weapon, int newLevel)
    {
        onWeaponUpgraded?.Invoke(weapon, newLevel);
    }

    public void TriggerWeaponFired(int weaponIndex)
    {
        onWeaponFired?.Invoke(weaponIndex);
    }

    // Game State
    public void TriggerGameStart()
    {
        onGameStart?.Invoke();
    }

    public void TriggerGamePause()
    {
        onGamePause?.Invoke();
    }

    public void TriggerGameResume()
    {
        onGameResume?.Invoke();
    }

    public void TriggerGameOver()
    {
        onGameOver?.Invoke();
    }

    // Combat
    public void TriggerDamageDealt(float damage)
    {
        onDamageDealt?.Invoke(damage);
    }

    public void TriggerDamageTaken(float damage)
    {
        onDamageTaken?.Invoke(damage);
    }
}