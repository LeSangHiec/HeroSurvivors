using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private float recoil = 2f;
    [SerializeField] private bool autoFire = false;

    protected override void Start()
    {
        base.Start();
        Debug.Log("<color=orange>★ Shotgun equipped! ★</color>");
    }

    // Override Fire() để thêm recoil
    protected override void Fire()
    {
        base.Fire(); // Gọi Fire() mặc định (bắn đạn)

        // Thêm recoil effect
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

    // Override sound để to hơn
    protected override void PlayShootSound()
    {
        if (AudioManager.Instance != null)
        {
            // Shotgun to hơn pistol
            AudioManager.Instance.PlaySFX(weaponData.shootSound, 1.5f);
        }
    }
    protected override bool CanShoot()
    {
        if (autoFire)
        {
            return true; // Luôn bắn (full auto)
        }
        else
        {
            return base.CanShoot(); // Giữ chuột
        }
    }
}