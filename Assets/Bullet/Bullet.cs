using System;
using Fusion;
using UnityEngine;
using Zombies;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _bulletSpeed;
    [Networked] private TickTimer life { get; set; }
    [SerializeField] private LayerMask _zombieLayer;

    /// <summary>
    /// The timer should be set before the object is spawned, and because Spawned() is called only after a local instance has been created, it should not be used to initialize network state.
    /// Instead, create an Init() method that can be called from the Player and use it to set the life property to be 5 seconds into the future. This is best done with the static helper method CreateFromSeconds() on the TickTimer itself.
    /// </summary>
    public void Init() //call it from player
    {
        life = TickTimer.CreateFromSeconds(Runner, 10.0f);
    }
    
    public override void FixedUpdateNetwork()
    {
        // If the bullet has not hit an asteroid, moves forward.
        if (HasHitZombie() == false) {
            transform.Translate(transform.forward * _bulletSpeed * Runner.DeltaTime, Space.World);
        } 
        else {
            Runner.Despawn(Object);
            return;
        }
        CheckBulletLife();
    }

    private void CheckBulletLife()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Zombie zombie))
        {
            zombie.HitZombie(Object.InputAuthority);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
      
    }

    private bool HasHitZombie()
    {
        var hitZombie = Runner.LagCompensation.Raycast(transform.position, transform.forward * 5, _bulletSpeed,
            Object.InputAuthority, out var hit, _zombieLayer);
        
        Debug.DrawRay(transform.position, transform.forward * 5, Color.red);

        if (hitZombie == false) return false;
        
        var zombie = hit.GameObject.GetComponent<Zombie>();
        zombie.HitZombie(Object.InputAuthority);

        return true;
    }
}
