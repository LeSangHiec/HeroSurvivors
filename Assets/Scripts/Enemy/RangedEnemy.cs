using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Ranged Settings")]
    [SerializeField] private float optimalDistance = 8f;        // Khoảng cách lý tưởng từ player
    [SerializeField] private float minDistance = 6f;            // Khoảng cách tối thiểu
    [SerializeField] private float maxDistance = 10f;           // Khoảng cách tối đa
    [SerializeField] private float circleSpeed = 2f;            // Tốc độ đi vòng
    [SerializeField] private bool clockwise = true;             // Đi theo chiều kim đồng hồ

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;               // Bắn mỗi 1 giây
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDamage = 15f;
    [SerializeField] private float aimOffset = 0f;              // Độ sai lệch ngắm (0 = chính xác)

    [Header("Prediction")]
    [SerializeField] private bool usePrediction = true;         // Ngắm trước vị trí player
    [SerializeField] private float predictionTime = 0.3f;       // Thời gian dự đoán

  

    private float nextFireTime = 0f;
    private Vector2 circleDirection;
    private float currentAngle;

    protected override void Start()
    {
        base.Start();

        // Override stats cho RangedEnemy
        maxHealth = 80f;
        currentHealth = maxHealth;
        damage = 15f;
        enemyMoveSpeed = 3f;

        // Khởi tạo góc ban đầu
        if (player != null)
        {
            Vector2 directionToPlayer = (transform.position - player.transform.position).normalized;
            currentAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        }

       
    }

    protected override void Update()
    {
        if (isDead) return;

        // ← SỬA: Dùng custom movement thay vì MoveToPlayer()
        CircleAroundPlayer();
        FlipEnemy();

        // Bắn đạn
        HandleShooting();

        
    }

    // ========== CIRCLE MOVEMENT ==========

    void CircleAroundPlayer()
    {
        if (player == null) return;

        Vector2 playerPos = player.transform.position;
        Vector2 currentPos = transform.position;
        float distanceToPlayer = Vector2.Distance(currentPos, playerPos);

        // Tính hướng đến player
        Vector2 directionToPlayer = (playerPos - currentPos).normalized;

        // Quyết định hành động dựa trên khoảng cách
        Vector2 moveDirection = Vector2.zero;

        if (distanceToPlayer < minDistance)
        {
            // Quá gần → Lùi ra
            moveDirection = -directionToPlayer;
        }
        else if (distanceToPlayer > maxDistance)
        {
            // Quá xa → Tiến vào
            moveDirection = directionToPlayer;
        }
        else
        {
            // Trong khoảng lý tưởng → Đi vòng
            moveDirection = GetCircleDirection(directionToPlayer);
        }

        // Apply movement với obstacle avoidance
        if (enableAvoidance)
        {
            moveDirection = ApplyObstacleAvoidance(moveDirection);
        }

        // Move
        if (rb != null)
        {
            rb.linearVelocity = moveDirection.normalized * enemyMoveSpeed;
        }
    }

    Vector2 GetCircleDirection(Vector2 directionToPlayer)
    {
        // Tính hướng vuông góc (perpendicular) để đi vòng
        Vector2 perpendicularDirection = Vector2.Perpendicular(directionToPlayer);

        // Đảo hướng nếu đi ngược chiều kim đồng hồ
        if (!clockwise)
        {
            perpendicularDirection = -perpendicularDirection;
        }

        // Blend giữa hướng vào player và hướng đi vòng
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float distanceRatio = (distanceToPlayer - optimalDistance) / (maxDistance - optimalDistance);
        distanceRatio = Mathf.Clamp01(distanceRatio);

        // Nếu đang ở optimal distance → đi vòng 100%
        // Nếu xa hơn → blend với hướng vào player
        Vector2 finalDirection = Vector2.Lerp(perpendicularDirection, directionToPlayer, distanceRatio);

        return finalDirection.normalized;
    }

    // ========== SHOOTING ==========

    void HandleShooting()
    {
        if (player == null || bulletPrefab == null) return;

        // Check cooldown
        if (Time.time < nextFireTime) return;

        // Check khoảng cách (chỉ bắn khi trong range)
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > maxDistance * 1.5f) return; // Không bắn nếu quá xa

        // Shoot
        Shoot();

        // Update next fire time
        nextFireTime = Time.time + fireRate;
    }

    void Shoot()
    {
        if (firePoint == null)
        {
            firePoint = transform; // Fallback
        }

        // Tính hướng bắn
        Vector2 shootDirection = CalculateShootDirection();

        // Spawn bullet
        GameObject bullet = null;

        // Kiểm tra có dùng PoolManager không
        if (PoolManager.Instance != null)
        {
            string poolName = bulletPrefab.name;
            bullet = PoolManager.Instance.Spawn(
                poolName,
                firePoint.position,
                Quaternion.Euler(0, 0, Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg)
            );
        }
        else
        {
            bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.Euler(0, 0, Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg)
            );
        }

        if (bullet == null) return;

        // Setup bullet velocity
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletSpeed;
        }

        // Setup bullet damage
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



    protected override void OnEnterDamage(GameObject playerObject)
    {

    }

    protected override void OnStayDamage(GameObject playerObject)
    {
    }


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