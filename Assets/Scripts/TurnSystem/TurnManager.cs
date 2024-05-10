using System;
using System.Collections.Generic;
using MainGame;
using QuantumConnectionSystem;
using SoldierSystem;
using Unity.Netcode;
using UnityEngine;

namespace TurnSystem
{
    public class TurnManager : NetworkBehaviour
    {
        private IGameState m_currentState;
        public NetworkVariable<int> currentStateID = new NetworkVariable<int>();
        private int m_PlayersEndedTurn = 0;
        [SerializeField] private SoldierManager soldierManager;
        
        public override void OnNetworkSpawn()
        {
            Debug.Log("connect gamemanager event");
            base.OnNetworkSpawn();
            GameManager.Instance.startGame += InitializeGame; 
        }

        private void SetState(IGameState newState)
        {
            Debug.Log("Changing state!!! Old State is: " + currentStateID.Value+ " New state is: " + newState.GetID()) ;
            m_currentState?.OnExit(this);
            m_currentState = newState;
            currentStateID.Value = newState.GetID();
            m_currentState?.OnEnter(this);
        }

        private void InitializeGame()
        {
            Debug.Log("initializing new game!!");
            SetState(new GameStartState());
        }
        
        
        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.startGame -= InitializeGame;
            }
        }
        
        public void OnEndTurnButtonClicked()
        {
            EndTurnServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        void EndTurnServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
        {
            m_PlayersEndedTurn++;
            Debug.Log("TURN ENDED FOR PLAYER: " + clientId);
            if (m_PlayersEndedTurn == 2)
            {
                m_PlayersEndedTurn = 0;
                QuantumConnector.Instance.ConnectToQuantum();
                m_currentState.HandleTurn(this);
            }
        }
        
        public void SpawnInitialPrefabsForPlayers()
        {
            soldierManager.SpawnInitialPrefabsForPlayers();
        }
    }
}