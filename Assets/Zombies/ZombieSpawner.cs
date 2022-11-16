using Fusion;
using UnityEngine;

namespace Zombies
{
    public class ZombieSpawner : NetworkBehaviour
    {
        [SerializeField] private Zombie _zombiePrefab;
        [SerializeField] private Transform zombieSpawnPosition;
        [SerializeField] private float spawnerSpeed;
        [SerializeField] private int amountOfZombiesToSpawn;
        [Networked] private TickTimer recallTimeToSpawnZombie { get; set; }

        private bool shouldSpawn;

        public void SetSpawning(bool value)
        {
            if (shouldSpawn == value) return;
            shouldSpawn = value;
            recallTimeToSpawnZombie = TickTimer.CreateFromSeconds(Runner, spawnerSpeed);
        }

        public override void FixedUpdateNetwork()
        {
            if(amountOfZombiesToSpawn <= 0) return;
            if (!Runner.IsServer) return;
            if(!shouldSpawn) return;
            if (!recallTimeToSpawnZombie.ExpiredOrNotRunning(Runner)) return;
            
            recallTimeToSpawnZombie = TickTimer.CreateFromSeconds(Runner, spawnerSpeed);
            Runner.Spawn(_zombiePrefab,
                zombieSpawnPosition.position, zombieSpawnPosition.rotation,
                Object.InputAuthority, (runner, o) =>
                {
                    o.GetComponent<Zombie>().SetTickTimer();
                });
            amountOfZombiesToSpawn--;
        }
    }
}