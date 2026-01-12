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
            return true; 
        }
        else
        {
            return base.CanShoot(); 
        }
    }
}