using Fusion;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _bulletSpeed;
    [Networked] private TickTimer life { get; set; }

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
        transform.position += _bulletSpeed * transform.forward * Runner.DeltaTime;
        CheckBulletLife();
    }

    private void CheckBulletLife()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}
