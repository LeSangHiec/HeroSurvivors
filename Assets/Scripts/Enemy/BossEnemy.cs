using UnityEngine;
using System.Collections;

public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float shootRange = 15f;

    private float lastShootTime;

    [Header("Spawn Minions Ability")]
    [SerializeField] private bool canSpawnMinions = true;
    [SerializeField] private string minionPoolName = "BasicEnemy";
    [SerializeField] private int minionsPerSpawn = 3;
    [SerializeField] private float minionSpawnInterval = 15f;
    [SerializeField] private float minionSpawnRadius = 3f;
    [SerializeField] private float firstMinionSpawnDelay = 5f;

    private float lastMinionSpawnTime;

    // ❌ REMOVED: Death Animation fields (already in base class Enemy.cs)
    // [Header("Death Animation")]
    // [SerializeField] private float deathAnimationDuration = 2f;
    // [SerializeField] private bool useDeathAnimation = true;

    protected override void Start()
    {
        base.Start();

        // Boss stats
        maxHealth = 1000f;
        currentHealth = maxHealth;
        damage = 50f;
        enemyMoveSpeed = 2f;

        // Initialize minion spawn timer with delay
        lastMinionSpawnTime = Time.time + firstMinionSpawnDelay;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        TryShoot();
        TrySpawnMinions();
    }

    // ========== RANGED ATTACK ==========

    void TryShoot()
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= shootRange && Time.time >= lastShootTime + shootInterval)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        Vector2 direction = (player.transform.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet != null)
        {
            SetupBullet(bullet, direction);
        }
    }

    void SetupBullet(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(damage * 0.5f);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ========== SPAWN MINIONS ABILITY ==========

    void TrySpawnMinions()
    {
        if (!canSpawnMinions) return;
        if (EnemyPoolManager.Instance == null) return;

        if (Time.time >= lastMinionSpawnTime + minionSpawnInterval)
        {
            SpawnMinions();
            lastMinionSpawnTime = Time.time;
        }
    }

    void SpawnMinions()
    {
        Debug.Log($"<color=red>★ BOSS SPAWNING {minionsPerSpawn} MINIONS! ★</color>");

        for (int i = 0; i < minionsPerSpawn; i++)
        {
            Vector3 spawnPos = GetMinionSpawnPosition();
            SpawnMinion(spawnPos);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }
    }

    Vector3 GetMinionSpawnPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * minionSpawnRadius;
        return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }

    void SpawnMinion(Vector3 position)
    {
        GameObject minion = EnemyPoolManager.Instance.SpawnEnemy(
            minionPoolName,
            position,
            Quaternion.identity
        );

        if (minion != null)
        {
            Enemy minionEnemy = minion.GetComponent<Enemy>();
            if (minionEnemy != null)
            {
                minionEnemy.ApplyMultipliers(0.5f, 0.7f, 1f);
            }

            if (deathEffect != null)
            {
                Instantiate(deathEffect, position, Quaternion.identity);
            }
        }
    }

    // ========== MELEE ATTACK ==========

    protected override void OnEnterDamage(GameObject playerObject)
    {
        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    protected override void OnStayDamage(GameObject playerObject)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }

            lastAttackTime = Time.time;
        }
    }

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }

        if (canSpawnMinions)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minionSpawnRadius);
        }
    }
}