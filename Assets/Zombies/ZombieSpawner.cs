using System;
using System.Collections.Generic;
using Fusion;
using GameResult;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

namespace Zombies
{
    public class ZombieSpawner : NetworkBehaviour
    {
        [SerializeField] private Zombie _zombiePrefab;
        [SerializeField] private List<Transform> zombieSpawnPosition;
        [SerializeField] private float spawnerSpeed;
        [SerializeField] private int amountOfZombiesToSpawn;
        [Networked] private TickTimer recallTimeToSpawnZombie { get; set; }

        private bool shouldSpawn;
        private bool _win;
        private List<Zombie> _zombies = new List<Zombie>();
        private int _zombiesDeads;
        private int _maxZombiesToSpawn;

        private void Start()
        {
            _maxZombiesToSpawn = amountOfZombiesToSpawn;
        }

        public void SetSpawning(bool value)
        {
            if (shouldSpawn == value) return;
            shouldSpawn = value;
            recallTimeToSpawnZombie = TickTimer.CreateFromSeconds(Runner, spawnerSpeed);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer) return;
            if (amountOfZombiesToSpawn <= 0 && !_win)
            {
                if (_zombiesDeads < _maxZombiesToSpawn) return;
                _win = true;
                RPC_WinGame(true);
            }

            if (!shouldSpawn) return;
            if (!recallTimeToSpawnZombie.ExpiredOrNotRunning(Runner)) return;

            var randomPositionToSpawn = UnityEngine.Random.Range(0, zombieSpawnPosition.Count);

            recallTimeToSpawnZombie = TickTimer.CreateFromSeconds(Runner, spawnerSpeed);
            Runner.Spawn(_zombiePrefab,
                zombieSpawnPosition[randomPositionToSpawn].position,
                zombieSpawnPosition[randomPositionToSpawn].rotation,
                Object.InputAuthority, (runner, instantiated) =>
                {
                    var zombie = instantiated.GetComponent<Zombie>();
                    zombie.SetTickTimer();
                    zombie.SetSpawner(this);
                    _zombies.Add(zombie);
                });
            amountOfZombiesToSpawn--;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_WinGame(bool win, RpcInfo info = default)
        {
            FindObjectOfType<ResultGameUi>().GameOver(win);
        }

        public void AddZombieDead() => _zombiesDeads++;
    }
}