using UnityEngine;

public class FastEnemy : Enemy
{
    [Header("Fast Enemy Settings")]
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        maxHealth = 30f;
        currentHealth = maxHealth;
        damage = 20f;
    }



    protected override void Update()
    {
        base.Update();
    }

    protected override void OnEnterDamage(GameObject playerObject)
    {
        PlayerStats playerController = playerObject.GetComponent<PlayerStats>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
        }
    }

    protected override void OnStayDamage(GameObject playerObject)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerStats playerController = playerObject.GetComponent<PlayerStats>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }

            lastAttackTime = Time.time;
        }
    }
}