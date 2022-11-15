using Fusion;
using UnityEngine;

namespace Player_
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Bullet _prefabBall;
        [SerializeField] private Transform spawnBulletTransform;
        [SerializeField] private float _playerSpeed;
        [SerializeField] private Animator _animator;
        [Networked] private TickTimer reloadTime { get; set; }
        
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

            Debug.Log($"NetworkInputData.Direction: {networkInputData.direction}");
            
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
                Debug.Log($"CurrentState: {newState}");
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