using UnityEngine;

public class RifleWeapon : WeaponBase
{
    [Header("Rifle Settings")]
    [SerializeField] private bool autoFire = true;

    protected override void Start()
    {
        base.Start();
    }

    // Override CanShoot để auto-fire
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