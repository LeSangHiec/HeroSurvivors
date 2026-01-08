using UnityEngine;
using System.Collections;

public class ExplosionEnemy : Enemy
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Warning")]
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private Color warningColor = Color.red;

    private bool hasExploded = false;

    protected override void Start()
    {
        base.Start();

        maxHealth = 15f;
        currentHealth = maxHealth;
        damage = 80f;
        enemyMoveSpeed = 3f;
    }

    protected override void Die()
    {
        if (!hasExploded)
        {
            CreateExplosion();
        }

        base.Die();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(WarningBeforeExplode());
        }
    }

    IEnumerator WarningBeforeExplode()
    {
        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            spriteRenderer.color = warningColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        Explode();
    }

    void FlashWarning()
    {
        StartCoroutine(FlashWarningCoroutine());
    }

    IEnumerator FlashWarningCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = warningColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        CreateExplosion();
        Die();
    }

    void CreateExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Explosion explosionScript = explosion.GetComponent<Explosion>();
            if (explosionScript != null)
            {
                explosionScript.SetDamage(damage);
            }
        }
    }
}