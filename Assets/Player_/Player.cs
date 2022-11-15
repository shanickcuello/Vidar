using Fusion;
using UnityEngine;
using Zombies;

namespace Player_
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Bullet _prefabBall;
        [SerializeField] private Transform spawnBulletTransform;
        [SerializeField] private float _playerSpeed;
        [SerializeField] private Animator _animator;
        [Networked] private TickTimer reloadTime { get; set; }
        [SerializeField] LayerMask _zombieLayer;

        private PlayerAnimationStates currentPlayerAnimationState;

        public bool Alive
        {
            get => alive;
        }

        private bool alive = true;
        
        private NetworkCharacterControllerPrototype _cc;
        private Vector3 _forward;
        private bool _canMove;


        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterControllerPrototype>();
            currentPlayerAnimationState = PlayerAnimationStates.Idle;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                if (!_canMove) return;
                UpdateAnim(data);
                data.direction.Normalize();
                _cc.Move(_playerSpeed * data.direction * Runner.DeltaTime);

                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;

                if (reloadTime.ExpiredOrNotRunning(Runner))
                {
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                    {
                        reloadTime = TickTimer.CreateFromSeconds(Runner, 0.5f);


                        Runner.LagCompensation.Raycast(spawnBulletTransform.position, spawnBulletTransform.forward, 500,
                            Object.InputAuthority, out var hitInfo, _zombieLayer, HitOptions.IncludePhysX);

                        float hitInfoDistance = 100;
                        var hitZombie = false;

                        if (hitInfo.Distance > 0)
                        {
                            hitInfoDistance = hitInfo.Distance;
                        }

                        if (hitInfo.Hitbox != null)
                        {
                            Debug.Log($"{Time.time}, {transform.name}, hit hitbox {hitInfo.Hitbox.transform.root.name}");
                            hitZombie = true;
                        }
                        else if (hitInfo.Collider != null)
                        {
                            Debug.Log($"{Time.time}, {transform.name}, hit PhysX {hitInfo.Collider.transform.name}");
                        }

                        if (hitZombie)
                        {
                            Debug.DrawRay(spawnBulletTransform.position, spawnBulletTransform.forward * 500, Color.red, 2);
                            hitInfo.GameObject.GetComponent<Zombie>().HitZombie(Object.InputAuthority);
                        }
                        else
                        {
                            Debug.DrawRay(spawnBulletTransform.position, spawnBulletTransform.forward * 500, Color.green, 2);
                        }


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

        private void UpdateAnim(NetworkInputData networkInputData)
        {
            PlayerAnimationStates newState = PlayerAnimationStates.Idle;
            
            if (networkInputData.direction != Vector3.zero)
            {
                newState = PlayerAnimationStates.Walking;
            }
            else
            {
                newState = PlayerAnimationStates.Idle;
            }

            if (newState != currentPlayerAnimationState)
            {
                currentPlayerAnimationState = newState;
                _animator.SetTrigger(currentPlayerAnimationState.ToString());
            }
        }
        public void SetMovement(bool value)
        {
            _canMove = value;
        }
    }

    public enum PlayerAnimationStates
    {
        Idle,
        Walking,
        Dead
    }
}