using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ExplosionEnemy : Enemy // ← SỬA: Đổi tên
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDelay = 0f; // Delay nhỏ trước khi nổ
    [Header("Warning")]
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private Color warningColor = Color.red;

    private bool hasExploded = false; // ← THÊM: Tránh nổ nhiều lần

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
        if (!hasExploded) // ← THÊM: Check tránh nổ 2 lần
        {
            CreateExplosion();
        }

        base.Die();
    }

    // ← SỬA: Dùng OnCollisionEnter2D thay vì OnTriggerEnter2D
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
    private void Explode()
    {
        if (hasExploded) return; // ← Tránh nổ nhiều lần

        hasExploded = true;

        CreateExplosion();

        // ← THÊM: Tự sát ngay lập tức
        Die();
    }

    private void CreateExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // ← THÊM: Set damage cho explosion
            Explosion explosionScript = explosion.GetComponent<Explosion>();
            if (explosionScript != null)
            {
                explosionScript.SetDamage(damage);
            }
        }
    }
}