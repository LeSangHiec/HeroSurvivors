using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private bool hasExploded = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        PlayAnimation();
        DamageArea();
    }

    void PlayAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
    }

    void DamageArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                DamagePlayer(hit);
            }
        }
    }

    void DamagePlayer(Collider2D playerCollider)
    {
        PlayerStats player = playerCollider.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}