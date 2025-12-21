using UnityEngine;

public class XPGem : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpValue = 10;

    [Header("Movement Settings")]
    [SerializeField] private float attractSpeed = 5f;
    [SerializeField] private float attractRange = 5f;
    [SerializeField] private bool autoAttract = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 30f; // Tự biến mất sau 30 giây

    private PlayerController player;
    private Vector3 startPosition;
    private bool isBeingCollected = false;

    void Start()
    {
        // Tự động tìm player
        player = FindAnyObjectByType<PlayerController>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        startPosition = transform.position;

        // Tự destroy sau lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isBeingCollected || player == null) return;

        // Bob animation (lên xuống)

        // Attract to player
        if (autoAttract)
        {
            AttractToPlayer();
        }
    }

   

    void AttractToPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Nếu player gần thì hút về
        if (distanceToPlayer <= attractRange)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * attractSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBeingCollected)
        {
            CollectXP(collision.gameObject);
        }
    }

    void CollectXP(GameObject playerObject)
    {
        isBeingCollected = true;

        // ← SỬA: Dùng PlayerXP thay vì PlayerStats
        PlayerXP playerXP = playerObject.GetComponent<PlayerXP>();
        if (playerXP != null)
        {
            playerXP.AddXP(xpValue);
        }

        Debug.Log($"Player collected {xpValue} XP!");

        Destroy(gameObject);
    }

    // Public method để set XP value
    public void SetXPValue(int value)
    {
        xpValue = value;
    }

    // Visualize attract range trong editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRange);
    }
}