using UnityEngine;

public class BasicEnemy : Enemy
{
    [Header("Basic Enemy Settings")]
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();

        // Override stats
        
    }

    protected override void Update()
    {
        base.Update();
    }

    // ========== COLLISION DAMAGE ==========

    protected override void OnEnterDamage(GameObject playerObject)
    {
        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    protected override void OnStayDamage(GameObject playerObject)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }

            lastAttackTime = Time.time;
        }
    }
}