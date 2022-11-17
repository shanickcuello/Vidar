using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Player_;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zombies;

namespace Spawner
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        [SerializeField] private ZombieSpawner _zombieSpawner;
        [SerializeField] private int amountPlayerToStartGame;
        
        [SerializeField] private Button startHosting;
        [SerializeField] private Button joinGame;
        [SerializeField] private List<Transform> playerSpawnPositions;

        private NetworkRunner _runner;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private bool _mouseButton0;
        private int _amountOfPlayersOnline;

        private List<Player> _players;
        
        private void Update()
        {
            _mouseButton0 = _mouseButton0 | Input.GetMouseButtonUp(0);
        }

        public async void StartGame(GameMode mode, string gameRoom)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = gameRoom,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });}
        
        private void OnGUI()
        {
            
        }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                var networkPlayerObject = CreatePlayers(runner, player);
                IncreasePlayerCounter();
                _spawnedCharacters.Add(player, networkPlayerObject);
                
                UnlockMovementForPlayersWhenNeeded();
            }
        }

        private void UnlockMovementForPlayersWhenNeeded()
        {
            if (_amountOfPlayersOnline >= amountPlayerToStartGame)
            {
                foreach (var playerController in _spawnedCharacters)
                {
                    playerController.Value.gameObject.GetComponent<Player>().SetMovement(true);
                    _zombieSpawner.SetSpawning(true);
                }
            }
        }

        private void IncreasePlayerCounter()
        {
            _amountOfPlayersOnline++;
        }

        private NetworkObject CreatePlayers(NetworkRunner runner, PlayerRef player)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = playerSpawnPositions[UnityEngine.Random.Range(0,playerSpawnPositions.Count)].position;
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            return networkPlayerObject;
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            if (Input.GetKey(KeyCode.W))
                data.direction += Vector3.forward;

            if (Input.GetKey(KeyCode.S))
                data.direction += Vector3.back;

            if (Input.GetKey(KeyCode.A))
                data.direction += Vector3.left;

            if (Input.GetKey(KeyCode.D))
                data.direction += Vector3.right;

            if (_mouseButton0)
                data.buttons |= NetworkInputData.MOUSEBUTTON1;
            _mouseButton0 = false;
            
            input.Set(data);
        }       
        
        
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}