using UnityEngine;

public class BasicEnemy : Enemy
{
    [Header("Basic Enemy Settings")]
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();

        // Override stats cho BasicEnemy
        maxHealth = 300f;
        currentHealth = maxHealth;
        damage = 20f;
    }

    protected override void Update()
    {
        base.Update();
    }

    // ========== COLLISION DAMAGE ========== ← PHẦN MỚI

    // Damage 1 lần khi bắt đầu chạm
    protected override void OnEnterDamage(GameObject playerObject)
    {
        PlayerStats playerController = playerObject.GetComponent<PlayerStats>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"BasicEnemy hit player for {damage} damage!");
        }
    }

    // Damage liên tục khi đang chạm (với cooldown)
    protected override void OnStayDamage(GameObject playerObject)
    {
        // Attack với cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerStats      playerController = playerObject.GetComponent<PlayerStats>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"BasicEnemy continuous attack for {damage} damage!");
            }

            lastAttackTime = Time.time;
        }
    }
}