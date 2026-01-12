using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Ranged Settings")]
    [SerializeField] private float optimalDistance = 8f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float circleSpeed = 2f;
    [SerializeField] private bool clockwise = true;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDamage = 15f;
    [SerializeField] private float aimOffset = 0f;

    [Header("Prediction")]
    [SerializeField] private bool usePrediction = true;
    [SerializeField] private float predictionTime = 0.3f;

    private float nextFireTime = 0f;
    private Vector2 circleDirection;
    private float currentAngle;

    protected override void Start()
    {
        base.Start();

        maxHealth = 80f;
        currentHealth = maxHealth;
        damage = 15f;
        enemyMoveSpeed = 3f;

        if (player != null)
        {
            Vector2 directionToPlayer = (transform.position - player.transform.position).normalized;
            currentAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        CircleAroundPlayer();
        FlipEnemy();
        HandleShooting();
    }

    // ========== CIRCLE MOVEMENT ==========

    void CircleAroundPlayer()
    {
        if (player == null) return;

        Vector2 playerPos = player.transform.position;
        Vector2 currentPos = transform.position;
        float distanceToPlayer = Vector2.Distance(currentPos, playerPos);

        Vector2 directionToPlayer = (playerPos - currentPos).normalized;
        Vector2 moveDirection = Vector2.zero;

        if (distanceToPlayer < minDistance)
        {
            moveDirection = -directionToPlayer;
        }
        else if (distanceToPlayer > maxDistance)
        {
            moveDirection = directionToPlayer;
        }
        else
        {
            moveDirection = GetCircleDirection(directionToPlayer);
        }

        if (enableAvoidance)
        {
            moveDirection = ApplyObstacleAvoidance(moveDirection);
        }

        if (rb != null)
        {
            rb.linearVelocity = moveDirection.normalized * enemyMoveSpeed;
        }
    }

    Vector2 GetCircleDirection(Vector2 directionToPlayer)
    {
        Vector2 perpendicularDirection = Vector2.Perpendicular(directionToPlayer);

        if (!clockwise)
        {
            perpendicularDirection = -perpendicularDirection;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float distanceRatio = (distanceToPlayer - optimalDistance) / (maxDistance - optimalDistance);
        distanceRatio = Mathf.Clamp01(distanceRatio);

        Vector2 finalDirection = Vector2.Lerp(perpendicularDirection, directionToPlayer, distanceRatio);

        return finalDirection.normalized;
    }

    // ========== SHOOTING ==========

    void HandleShooting()
    {
        if (player == null || bulletPrefab == null) return;

        if (Time.time < nextFireTime) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > maxDistance * 1.5f) return;

        Shoot();

        nextFireTime = Time.time + fireRate;
    }

    void Shoot()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        Vector2 shootDirection = CalculateShootDirection();

        // ✅ FIX: Always use Instantiate (no pooling)
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(0, 0, Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg)
        );

        if (bullet == null) return;

        SetupBullet(bullet, shootDirection);
    }

    void SetupBullet(GameObject bullet, Vector2 direction)
    {
        // Set velocity
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        // Set damage
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(bulletDamage);
        }
    }

    Vector2 CalculateShootDirection()
    {
        if (player == null) return Vector2.right;

        Vector2 targetPosition = player.transform.position;

        if (usePrediction)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                targetPosition += playerRb.linearVelocity * predictionTime;
            }
        }

        Vector2 direction = (targetPosition - (Vector2)firePoint.position).normalized;

        if (aimOffset > 0)
        {
            float randomOffset = Random.Range(-aimOffset, aimOffset);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += randomOffset;

            direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        }

        return direction;
    }

    // ========== COLLISION (No Melee Damage) ==========

    protected override void OnEnterDamage(GameObject playerObject)
    {
        // Ranged enemy không melee damage
    }

    protected override void OnStayDamage(GameObject playerObject)
    {
        // Ranged enemy không melee damage
    }

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.transform.position, optimalDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.transform.position, minDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, maxDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.transform.position);
    }
}