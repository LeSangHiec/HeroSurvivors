using System.ComponentModel;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.InputManagerEntry;

public class EnemyRespawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Respawn Settings")]
    [SerializeField] private float maxDistanceFromPlayer = 35f; // Khoảng cách tối đa
    [SerializeField] private float minSpawnDistance = 18f;      // Khoảng cách spawn tối thiểu
    [SerializeField] private float maxSpawnDistance = 25f;      // Khoảng cách spawn tối đa
    [SerializeField] private float edgeOffset = 2f;             // Offset từ rìa camera

    [Header("Check Settings")]
    [SerializeField] private float checkInterval = 1f;          // Kiểm tra mỗi 1 giây

    [Header("Teleport Options")]
    [SerializeField] private bool onlyTeleportWhenOutsideCamera = true; // Chỉ teleport khi ngoài camera
    [SerializeField] private bool resetHealthOnTeleport = false; // Reset HP về max khi teleport

    private float lastCheckTime;
    private Enemy enemyScript;

    void Start()
    {
        // Lấy Enemy component
        enemyScript = GetComponent<Enemy>();

        // Auto-find player
        if (player == null)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
            else
            {
                Debug.LogError("EnemyRespawner: Player not found!");
            }
        }

        // Auto-find camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Không teleport nếu enemy đã chết
        if (enemyScript != null && enemyScript.IsDead())
        {
            return;
        }

        // Kiểm tra định kỳ
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckAndRespawnEnemy();
            lastCheckTime = Time.time;
        }
    }

    void CheckAndRespawnEnemy()
    {
        if (player == null) return;

        // Tính khoảng cách đến player
        float distance = Vector2.Distance(transform.position, player.position);

        // Kiểm tra điều kiện teleport
        bool shouldTeleport = distance > maxDistanceFromPlayer;

        // Nếu bật option: chỉ teleport khi ngoài camera
        if (onlyTeleportWhenOutsideCamera)
        {
            shouldTeleport = shouldTeleport && IsOutsideCamera();
        }

        if (shouldTeleport)
        {
            TeleportAroundPlayer();
        }
    }

    bool IsOutsideCamera()
    {
        if (mainCamera == null) return true;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // Nếu ngoài viewport (0-1) → outside camera
        return viewportPosition.x < 0 || viewportPosition.x > 1 ||
               viewportPosition.y < 0 || viewportPosition.y > 1 ||
               viewportPosition.z < 0; // Behind camera
    }

    void TeleportAroundPlayer()
    {
        if (player == null) return;

        Vector2 newPosition = GetRandomPositionAroundPlayer();

        transform.position = newPosition;

        // Reset velocity nếu có Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset HP nếu bật option
        if (resetHealthOnTeleport && enemyScript != null)
        {
            float maxHealth = enemyScript.GetMaxHealth();
            // Không thể set trực tiếp vì currentHealth là protected
            // Sẽ heal về full
            float healAmount = maxHealth - enemyScript.GetCurrentHealth();
            if (healAmount > 0)
            {
                // TakeDamage với số âm = heal (nếu Enemy hỗ trợ)
                // Hoặc để nguyên HP
            }
        }

        Debug.Log($"<color=yellow>{gameObject.name} teleported to {newPosition} (Distance was {Vector2.Distance(transform.position, player.position):F1})</color>");
    }

    Vector2 GetRandomPositionAroundPlayer()
    {
        // Random góc (0-360 độ)
        float randomAngle = Random.Range(0f, 360f);

        // Random khoảng cách (minSpawnDistance -> maxSpawnDistance)
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Tính vị trí dựa trên góc và khoảng cách
        Vector2 direction = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );

        Vector2 newPosition = (Vector2)player.position + direction * randomDistance;

        // Đảm bảo spawn ngoài rìa camera
        newPosition = EnsureOutsideCamera(newPosition);

        return newPosition;
    }

    Vector2 EnsureOutsideCamera(Vector2 position)
    {
        if (mainCamera == null) return position;

        // Lấy bounds của camera
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector2 cameraCenter = mainCamera.transform.position;

        // Min/Max bounds của camera (với offset)
        float minX = cameraCenter.x - cameraWidth - edgeOffset;
        float maxX = cameraCenter.x + cameraWidth + edgeOffset;
        float minY = cameraCenter.y - cameraHeight - edgeOffset;
        float maxY = cameraCenter.y + cameraHeight + edgeOffset;

        // Nếu position nằm trong camera → đẩy ra ngoài
        if (position.x > minX && position.x < maxX && position.y > minY && position.y < maxY)
        {
            // Tính hướng từ camera center đến position
            Vector2 direction = (position - cameraCenter).normalized;

            // Đẩy position ra ngoài rìa camera
            float pushDistance = Mathf.Max(cameraWidth, cameraHeight) + edgeOffset;
            position = cameraCenter + direction * pushDistance;
        }

        return position;
    }

    // ========== GIZMOS (Debug visual) ==========

    void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            // Tìm player để vẽ gizmos
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
            else
            {
                return;
            }
        }

        // Vẽ vòng tròn max distance (màu đỏ)
        Gizmos.color = Color.red;
        DrawCircle(player.position, maxDistanceFromPlayer);

        // Vẽ vòng tròn min spawn distance (màu vàng)
        Gizmos.color = Color.yellow;
        DrawCircle(player.position, minSpawnDistance);

        // Vẽ vòng tròn max spawn distance (màu xanh lá)
        Gizmos.color = Color.green;
        DrawCircle(player.position, maxSpawnDistance);

        // Vẽ line từ enemy đến player
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);

        // Hiển thị khoảng cách
        float distance = Vector2.Distance(transform.position, player.position);
        UnityEditor.Handles.Label(
            (transform.position + player.position) / 2f,
            $"Distance: {distance:F1}"
        );
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 50;
        float angleStep = 360f / segments;

        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}
