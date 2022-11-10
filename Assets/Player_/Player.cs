using Fusion;
using UnityEngine;

namespace Player_
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Bullet _prefabBall;
        [SerializeField] private Transform spawnBulletTransform;
        
        [Networked] private TickTimer reloadTime { get; set; }
        
        private NetworkCharacterControllerPrototype _cc;
        private Vector3 _forward;
        
        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterControllerPrototype>();
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.direction.Normalize();
                _cc.Move(5 * data.direction * Runner.DeltaTime);

                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;

                if (reloadTime.ExpiredOrNotRunning(Runner))
                {
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                    {
                        reloadTime = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        Runner.Spawn(_prefabBall,
                            spawnBulletTransform.position, spawnBulletTransform.rotation,
                            Object.InputAuthority, (runner, o) =>
                            {
                                // Initialize the Ball before synchronizing it
                                o.GetComponent<Bullet>().Init();
                            });
                    }
                }
            }
        }
    }
}