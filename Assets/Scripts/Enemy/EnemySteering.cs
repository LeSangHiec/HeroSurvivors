using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [Header("Separation Settings")]
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private float separationForce = 2f;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ApplySeparation();
    }

    void ApplySeparation()
    {
        if (rb == null) return;

        // Tìm enemies gần
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            separationRadius,
            enemyLayer
        );

        Vector2 separationDirection = Vector2.zero;
        int count = 0;

        foreach (Collider2D other in nearbyEnemies)
        {
            // Bỏ qua chính mình
            if (other.gameObject == gameObject) continue;

            // Tính hướng đẩy ra
            Vector2 directionAway = (transform.position - other.transform.position).normalized;
            float distance = Vector2.Distance(transform.position, other.transform.position);

            // Càng gần càng đẩy mạnh
            float strength = 1f - (distance / separationRadius);
            separationDirection += directionAway * strength;
            count++;
        }

        if (count > 0)
        {
            separationDirection /= count; // Average
            rb.AddForce(separationDirection * separationForce, ForceMode2D.Force);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}