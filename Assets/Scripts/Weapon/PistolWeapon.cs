using UnityEngine;
using UnityEngine.Tilemaps;

public class PistolWeapon : WeaponBase
{
    
    [SerializeField] private bool autoFire = true;

    protected override void Start()
    {
        base.Start();
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