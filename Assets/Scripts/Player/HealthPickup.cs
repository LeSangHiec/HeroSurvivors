using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float healAmount = 20f;

    [Header("Movement Settings")]
    [SerializeField] private float attractSpeed = 4f;
    [SerializeField] private float attractRange = 3f;
    [SerializeField] private bool autoAttract = true;

    // ✅ THÊM: Bounce Animation
    [Header("Bounce Animation")]
    [SerializeField] private bool enableBounce = true;
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 20f;

    [Header("Effects")]
    [SerializeField] private GameObject collectEffect;

    private PlayerController player;
    private bool isBeingCollected = false;
    private Vector3 startPosition;
    private float bounceTimer = 0f;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // ✅ Lưu vị trí ban đầu cho bounce
        startPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isBeingCollected || player == null) return;

        // ✅ Bounce animation khi CHƯA bị hút
        if (enableBounce && !IsBeingAttracted())
        {
            ApplyBounceAnimation();
        }

        if (autoAttract)
        {
            AttractToPlayer();
        }
    }

    // ✅ THÊM: Kiểm tra xem có đang bị hút không
    bool IsBeingAttracted()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= attractRange;
    }

    // ✅ THÊM: Hiệu ứng nảy lên xuống
    void ApplyBounceAnimation()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;

        float yOffset = Mathf.Sin(bounceTimer) * bounceHeight;

        transform.position = startPosition + new Vector3(0f, yOffset, 0f);
    }

    void AttractToPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attractRange)
        {
            // ✅ Cập nhật startPosition khi bắt đầu hút
            if (!IsBeingAttracted())
            {
                startPosition = transform.position;
            }

            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * attractSpeed * Time.deltaTime;

            // ✅ Cập nhật startPosition liên tục khi đang hút
            startPosition = transform.position;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBeingCollected)
        {
            CollectHealth(collision.gameObject);
        }
    }

    void CollectHealth(GameObject playerObject)
    {
        isBeingCollected = true;

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Heal(healAmount);

            // Spawn effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                // AudioManager.Instance.PlayHealthPickup();
            }
        }

        Destroy(gameObject);
    }

    public void SetHealAmount(float amount)
    {
        healAmount = amount;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractRange);
    }
}