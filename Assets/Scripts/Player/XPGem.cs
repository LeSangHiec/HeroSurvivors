using UnityEngine;

public class XPGem : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpValue = 20;

    [Header("Movement Settings")]
    [SerializeField] private float attractSpeed = 5f;
    [SerializeField] private float attractRange = 5f;
    [SerializeField] private bool autoAttract = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 30f; 

    private PlayerController player;
    private Vector3 startPosition;
    private bool isBeingCollected = false;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        startPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isBeingCollected || player == null) return;


        // Attract to player
        if (autoAttract)
        {
            AttractToPlayer();
        }
    }

    void AttractToPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

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
        Destroy(gameObject);
    }

    public void SetXPValue(int value)
    {
        xpValue = value;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRange);
    }
}