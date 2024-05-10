using System;
using System.Collections.Generic;
using CardSystem;
using TurnSystem;
using Unity.Netcode;
using UnityEngine;

namespace MainGame
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] private CardManager m_CardManager;
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
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                var client = NetworkManager.ConnectedClients[clientId];
                Debug.Log("this is the client: " + client + " and id: " + clientId);
                if ((int)clientId == 1)
                {
                    m_CardManager.GetInitialCardsForPlayer((int)clientId);
                    Debug.Log("got player 1 card ready: ");
                }
                else if ((int)clientId == 2)
                {
                    m_CardManager.GetInitialCardsForPlayer((int)clientId);
                    Debug.Log("got player 2 card ready: ");
                }
            }

            if (currentPlayerIndex.Value == 2)
            {
                Debug.Log("Set event to start the game!");
                startGame?.Invoke();
            }
        }
        private void OnPlayerIndexChanged(int previous, int current)
        {
            Debug.Log("current player index changed from: " + previous + " to: " + current);
        }

    }
}