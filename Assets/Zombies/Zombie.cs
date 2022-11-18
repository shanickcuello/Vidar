using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using HP_;
using Player_;
using UnityEngine;
using UnityEngine.AI;

namespace Zombies
{
    public class Zombie : NetworkBehaviour, IHealth
    {
        [SerializeField] private float _speedMovement;
        [SerializeField] private Animator _animator;
        [SerializeField] private float attackRange;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _attackSpeed;

        [Networked] private TickTimer reloadTime { get; set; }

        public bool death;
        private NetworkCharacterControllerPrototype _cc;
        private Player target;
        private Transform _targetTransform;
        private bool _initialized;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterControllerPrototype>();
        }

        private void Start()
        {
            SetTargetPosition();
            _initialized = true;
        }

        private void SetTargetPosition()
        {
            var allPlayers = FindObjectsOfType<Player>();
            var playersAlive = allPlayers.Where(player => player.Alive).ToList();
            if (playersAlive.Count >= 1)
            {
                target = playersAlive[UnityEngine.Random.Range(0, allPlayers.Length)];
                _targetTransform = target.gameObject.transform;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(death) return;
            if(target == null || _targetTransform == null) return;
            var directionToMove = _targetTransform.position - transform.position;
            directionToMove.Normalize();
            
            Debug.DrawRay(transform.position, directionToMove);
            _cc.Move( directionToMove * _speedMovement * Runner.DeltaTime);

            AttackPlayers();
        }

        private void AttackPlayers()
        {
            if (reloadTime.ExpiredOrNotRunning(Runner))
            {
                reloadTime = TickTimer.CreateFromSeconds(Runner, _attackSpeed);

                Runner.LagCompensation.Raycast(transform.position, transform.forward, attackRange,
                    Object.InputAuthority, out var hitInfo, _playerLayer, HitOptions.IncludePhysX);

                float hitInfoDistance = 1;
                var hitPlayer = false;

                if (hitInfo.Distance > 0)
                {
                    hitInfoDistance = hitInfo.Distance;
                }

                if (hitInfo.Hitbox != null)
                {
                    hitPlayer = true;
                }

                if (hitPlayer)
                {
                    Debug.DrawRay(transform.position, transform.forward * attackRange, Color.red, duration:1);
                    if(Object.HasStateAuthority)
                        hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.forward * attackRange, Color.green, duration:1);
                }

            }
           
        }
        
        public void Death()
        {
            _animator.SetTrigger("Death");
            death = true;
            StartCoroutine(Despawn());
        }

        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(3);
            Runner.Despawn(Object);
            Destroy(gameObject);
        }

        public void SetTickTimer()
        {
            if (!_initialized) return;
            reloadTime = TickTimer.CreateFromSeconds(Runner, _attackSpeed);
        }
    }
}