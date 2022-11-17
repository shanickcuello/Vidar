using System.Collections;
using Fusion;
using HP_;
using TMPro;
using UnityEngine;
using Zombies;

namespace Player_
{
    public class Player : NetworkBehaviour, IHealth
    {
        [SerializeField] private Bullet _prefabBall;
        [SerializeField] private Transform spawnBulletTransform;
        [SerializeField] private float _playerSpeed;
        [SerializeField] private Animator _animator;
        [SerializeField] LayerMask _zombieLayer;
        [SerializeField] private Light gunLight;
        [SerializeField] private ParticleSystem gunParticles;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private TextMeshProUGUI nicknameText;
        
        
        [Networked(OnChanged = nameof(OnNicknameChanged))] public NetworkString<_16> nickname { get; set; }
        [Networked(OnChanged = nameof(OnLightChange))] public bool light { get; set; }
        [Networked(OnChanged = nameof(OnParticleValueChange))] public bool particleActive { get; set; }
        [Networked(OnChanged = nameof(OnAudioChange))] public bool audioValue { get; set; }
        
        [Networked(OnChanged = nameof(OnAnimationChange))] public byte animationValue { get; set; }
        [Networked] private TickTimer reloadTime { get; set; }

        
        private NetworkInputData currentData;
        private bool alive = true;
        private NetworkCharacterControllerPrototype _cc;
        private Vector3 _forward;
        private bool _canMove;
        private bool _dead;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickname(string nickname, RpcInfo info = default)
        {
            this.nickname = nickname;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                RPC_SetNickname(PlayerPrefs.GetString("playerNickname"));
            }
        }

        static void OnNicknameChanged(Changed<Player> changed)
        {
           changed.Behaviour.OnNicknameChanged();
        }
        
        private void OnNicknameChanged()
        {
            nicknameText.text = nickname.ToString();
        }
        
        static void OnAnimationChange(Changed<Player> changed)
        {
            byte newAnimationValue = changed.Behaviour.animationValue;
            changed.LoadOld();
            byte oldAudioValue = changed.Behaviour.animationValue;

            if (newAnimationValue != oldAudioValue)
            {
                changed.Behaviour.OnAnimationValueChange(newAnimationValue);
            }
        }
        
        private void OnAnimationValueChange(byte newAnimationValue)
        {
            if (newAnimationValue == 0)
            {
                _animator.SetTrigger(PlayerAnimationStates.Idle.ToString());
            }
            else
            {
                _animator.SetTrigger(PlayerAnimationStates.Walking.ToString());
            }
        }
        
        static void OnAudioChange(Changed<Player> changed)
        {
            bool newAudioValue = changed.Behaviour.audioValue;
            changed.LoadOld();
            bool oldAudioValue = changed.Behaviour.audioValue;

            if (newAudioValue != oldAudioValue)
            {
                changed.Behaviour.OnAudioPlay();
            }
        }
        
        private void OnAudioPlay()
        {
            _audioSource.Play();
        }

        
        static void OnParticleValueChange(Changed<Player> changed)
        {
            bool newParticleValue = changed.Behaviour.particleActive;
            changed.LoadOld();
            bool oldParticleValue = changed.Behaviour.particleActive;

            if (newParticleValue != oldParticleValue)
            {
                changed.Behaviour.OnParticleValueUpdate();
            }
        }
        
        private void OnParticleValueUpdate()
        {
            gunParticles.Play();
        }
        
        static void OnLightChange(Changed<Player> changed)
        {
            bool newLigtValue = changed.Behaviour.light;
            changed.LoadOld();
            bool oldLightValue = changed.Behaviour.light;

            if (newLigtValue != oldLightValue)
            {
                changed.Behaviour.OnLightEnableChange(newLigtValue);
            }
        }

        private void OnLightEnableChange(bool newLigtValue)
        {
            gunLight.enabled = newLigtValue;
            StartCoroutine(UnableGunLight());
        }


        
        public bool Alive
        {
            get => !_dead;
        }
        
        private void Awake()
        {
            _dead = false;
            _cc = GetComponent<NetworkCharacterControllerPrototype>();
            gunLight.enabled = false;
        }
        
        public override void FixedUpdateNetwork()
        {
            
            if (GetInput(out NetworkInputData data))
            {
                if (_dead) return;
                if (!_canMove) return;
                data.direction.Normalize();
                
                if (data.direction != currentData.direction)
                {
                    currentData = data;
                    if (data.direction != Vector3.zero)
                    {
                        animationValue = 1;
                    }
                    else
                    {
                        animationValue = 0;
                    }
                }
                
                _cc.Move(_playerSpeed * data.direction * Runner.DeltaTime);
                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;

                if (reloadTime.ExpiredOrNotRunning(Runner))
                {
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                    {
                        reloadTime = TickTimer.CreateFromSeconds(Runner, 0.1f);


                        Runner.LagCompensation.Raycast(spawnBulletTransform.position, spawnBulletTransform.forward, 500,
                            Object.InputAuthority, out var hitInfo, _zombieLayer, HitOptions.IncludePhysX);

                        light = true;
                        particleActive = !particleActive;
                        audioValue = !audioValue;

                        float hitInfoDistance = 100;
                        var hitZombie = false;

                        if (hitInfo.Distance > 0)
                        {
                            hitInfoDistance = hitInfo.Distance;
                        }

                        if (hitInfo.Hitbox != null)
                        {
                            hitZombie = true;
                        }

                        if (hitZombie)
                        {
                            Debug.DrawRay(spawnBulletTransform.position, spawnBulletTransform.forward * 500, Color.red, 2);
                            
                            if(Object.HasStateAuthority)
                                hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();
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

        private IEnumerator UnableGunLight()
        {
            yield return new WaitForSeconds(0.2f);
            light = false;
        }
        
        public void SetMovement(bool value)
        {
            _canMove = value;
        }

        public void Death()
        {
            _dead = true;
            _animator.SetTrigger(PlayerAnimationStates.Dead.ToString());
            GetBehaviour<Hitbox>().enabled = false;
            GetBehaviour<HitboxRoot>().enabled = false;
        }
    }

    public enum PlayerAnimationStates
    {
        Idle,
        Walking,
        Dead
    }
}