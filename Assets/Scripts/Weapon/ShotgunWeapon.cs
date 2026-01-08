using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private float recoil = 2f;
    [SerializeField] private bool autoFire = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Fire()
    {
        base.Fire();

        ApplyRecoil();
    }

    void ApplyRecoil()
    {
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 recoilDirection = -firePoint.right;
                playerRb.AddForce(recoilDirection * recoil, ForceMode2D.Impulse);
            }
        }
    }

    protected override void PlayShootSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(weaponData.shootSound, 1.5f);
        }
    }
    protected override bool CanShoot()
    {
        if (autoFire)
        {
            return true; 
        }
        else
        {
            return base.CanShoot(); 
        }
    }
}