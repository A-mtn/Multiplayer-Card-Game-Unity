using System;
using System.Collections.Generic;
using CardSystem;
using SoldierSystem;
using TurnSystem;
using Unity.Netcode;
using UnityEngine;

namespace MainGame
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] private CardManager m_CardManager;
        [SerializeField] private SoldierManager m_SoldierManager;
        
        public event Action startGame;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
        
        private List<NetworkObjectReference> players = new List<NetworkObjectReference>();
        private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0);

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                Debug.Log("this is a local player on network spawn! " + NetworkObjectId);
                players.Add(NetworkObject);
                ChangePlayerIndexServerRpc();
            }
            if (IsServer)
            {
                Debug.Log("this is server start");
                currentPlayerIndex.OnValueChanged += OnPlayerIndexChanged;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangePlayerIndexServerRpc(ServerRpcParams serverRpcParams = default)
        {
            currentPlayerIndex.Value += 1;
            
            if (currentPlayerIndex.Value == 2)
            {
                Debug.Log("Set event to start the game!");
                SetupInitialSoldiers();
                startGame?.Invoke();
            }
        }
        private void OnPlayerIndexChanged(int previous, int current)
        {
            Debug.Log("current player index changed from: " + previous + " to: " + current);
        }
        
        private void SetupInitialSoldiers() {
            // Example setup for two players
            m_SoldierManager.RegisterPlayerSoldiers(1, new List<int> { 0, 1, 2 });
            m_SoldierManager.RegisterPlayerSoldiers(2, new List<int> { 3, 4, 5 });
        }

        public void SpawnSoldiersForClients()
        {
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                m_SoldierManager.SpawnSoldiersForPlayer(player.ClientId);
            }
        }

    }
}