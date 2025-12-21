using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 5f;
    private float moveSpeedMultiplier = 1f; // ← THÊM

    [Header("Attack Settings")]
    private float attackSpeedMultiplier = 1f; // ← THÊM

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerStats playerStats;

    private Vector2 moveInput;
    private bool isMoving;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (playerStats != null && playerStats.IsDead()) return;

        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (playerStats != null && playerStats.IsDead()) return;

        if (isMoving)
        {
            // ← SỬA: Áp dụng multiplier
            float currentSpeed = baseMoveSpeed * moveSpeedMultiplier;
            rb.linearVelocity = moveInput * currentSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    public void OnMove(InputValue value)
    {
        if (playerStats != null && playerStats.IsDead()) return;

        moveInput = value.Get<Vector2>();
        isMoving = moveInput.magnitude > 0.1f;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null && playerStats != null)
            {
                playerStats.TakeDamage(enemy.GetDamage() * Time.fixedDeltaTime);
            }
        }
    }

    // ========== UPGRADE METHODS ==========

    public void IncreaseMoveSpeed(float percentage)
    {
        moveSpeedMultiplier += percentage;

        Debug.Log($"<color=cyan>Move Speed increased by {percentage * 100}%! Multiplier: {moveSpeedMultiplier:F2}x</color>");
    }

    public void IncreaseAttackSpeed(float percentage)
    {
        attackSpeedMultiplier += percentage;

        Debug.Log($"<color=magenta>Attack Speed increased by {percentage * 100}%! Multiplier: {attackSpeedMultiplier:F2}x</color>");

        // TODO: Apply to weapon
        // WeaponController weapon = GetComponentInChildren<WeaponController>();
        // if (weapon != null)
        // {
        //     weapon.SetFireRateMultiplier(attackSpeedMultiplier);
        // }
    }

    // ========== PUBLIC GETTERS ==========

    public PlayerStats GetPlayerStats() => playerStats;
    public bool IsDead() => playerStats != null && playerStats.IsDead();
    public float GetCurrentMoveSpeed() => baseMoveSpeed * moveSpeedMultiplier;
    public float GetMoveSpeedMultiplier() => moveSpeedMultiplier;
    public float GetAttackSpeedMultiplier() => attackSpeedMultiplier;
}