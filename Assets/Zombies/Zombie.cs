using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Player_;
using UnityEngine;
using UnityEngine.AI;

namespace Zombies
{
    public class Zombie : NetworkBehaviour
    {
        [SerializeField] private float _speedMovement;
        private NetworkCharacterControllerPrototype _cc;
        private Player target;
        private Transform _targetTransform;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterControllerPrototype>();
        }

        private void Start()
        {
            SetTargetPosition();
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

        private void Update()
        {
            if(target == null || _targetTransform == null) return;
            var directionToMove = _targetTransform.position - transform.position;
            directionToMove.Normalize();
            
            Debug.DrawRay(transform.position, directionToMove);
            _cc.Move( directionToMove * _speedMovement * Runner.DeltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Colisione con algo: " + collision.gameObject.name);
        }

        public void HitZombie(PlayerRef objectInputAuthority)
        {
            Runner.Despawn(Object);
            Debug.Log("Hit Zombie");
        }
    }
}