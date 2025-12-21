using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 3f; // ← THÊM: Bán kính nổ
    [SerializeField] private float lifetime = 0.5f; // ← THÊM: Thời gian tồn tại
    [SerializeField] private LayerMask damageableLayers; // ← THÊM: Layers có thể bị damage

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;


    private bool hasExploded = false;

    void Start()
    {
        // ← THÊM: Tự động destroy sau lifetime
        Destroy(gameObject, lifetime);

        // ← THÊM: Nổ ngay khi spawn
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // ← THÊM: Play animation
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // ← THÊM: Area damage
        DamageArea();
    }

    void DamageArea()
    {
        // ← SỬA: Damage tất cả trong bán kính
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Damage player
            if (hit.CompareTag("Player"))
            {
                PlayerStats  player = hit.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Debug.Log($"Explosion hit player for {damage} damage!");
                }
            }

            // ← XÓA: KHÔNG damage enemy khác
            // Enemy không nên bị explosion của enemy khác
        }
    }

    // ← THÊM: Public method để set damage
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

   

}