﻿using System;
using System.Collections;
using System.Collections.Generic;
using CardSystem;
using CombatSystem;
using MainGame;
using QuantumConnectionSystem;
using SoldierSystem;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

namespace TurnSystem
{
    public class TurnManager : NetworkBehaviour
    {
        private IGameState m_currentState;
        public NetworkVariable<int> currentStateID = new NetworkVariable<int>();
        private int m_PlayersEndedTurn = 0;
        public NetworkVariable<int> quantumNumber = new NetworkVariable<int>();
        
        public static TurnManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public override void OnNetworkSpawn()
        {
            Debug.Log("connect gamemanager event");
            base.OnNetworkSpawn();
            GameManager.Instance.startGame += InitializeGame; 
        }

        private void SetState(IGameState newState)
        {
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
                SetState(new EvaluationState());
                m_currentState.HandleTurn(this);
            }
        }
        
        public void HandleCardPlayOrder(string mostOccurred)
        {
            string adjustedOrder = AdjustMostOccurred(mostOccurred);
            List<(int cardID, int targetID)> firstPlayerPlayedCards = CardManager.Instance.FirstPlayerPlayedCards;
            List<(int cardID, int targetID)> secondPlayerPlayedCards = CardManager.Instance.SecondPlayerPlayedCards;

            List<(int cardID, int targetID)> combinedOrder = new List<(int cardID, int targetID)>();

            int firstPlayerIndex = 0;
            int secondPlayerIndex = 0;

            foreach (char c in adjustedOrder)
            {
                if (c == '0' && firstPlayerIndex < firstPlayerPlayedCards.Count)
                {
                    combinedOrder.Add(firstPlayerPlayedCards[firstPlayerIndex]);
                    firstPlayerIndex++;
                }
                else if (c == '1' && secondPlayerIndex < secondPlayerPlayedCards.Count)
                {
                    combinedOrder.Add(secondPlayerPlayedCards[secondPlayerIndex]);
                    secondPlayerIndex++;
                }
            }

            StartCoroutine(ExecuteCardActionsCoroutine(adjustedOrder, combinedOrder));
        }

        private string AdjustMostOccurred(string mostOccurred)
        {
            int count0 = 0;
            int count1 = 0;
            char[] adjusted = mostOccurred.ToCharArray();

            for (int i = 0; i < adjusted.Length; i++)
            {
                if (adjusted[i] == '0')
                {
                    if (count0 < 3)
                    {
                        count0++;
                    }
                    else
                    {
                        adjusted[i] = '1';
                        count1++;
                    }
                }
                else if (adjusted[i] == '1')
                {
                    if (count1 < 3)
                    {
                        count1++;
                    }
                    else
                    {
                        adjusted[i] = '0';
                        count0++;
                    }
                }

                if (count0 == 3 && count1 == 3)
                {
                    break;
                }
            }

            return new string(adjusted);
        }
        
        private IEnumerator ExecuteCardActionsCoroutine(string adjustedOrder, List<(int cardID, int targetID)> combinedOrder)
        {
            var i = 0;
            var qString = "";
            foreach (var (cardID, targetID) in combinedOrder)
            {
                yield return new WaitForSeconds(1.5f);

                ExecuteCardAction(cardID, targetID);
                CardManager.Instance.RemoveArrow(cardID);
                qString += adjustedOrder[i];    
                quantumNumber.Value = int.Parse(qString);
                i++;
            }
            
            SetState(new CardPlayState());
        }
        
        private void ExecuteCardAction(int cardID, int targetID)
        { 
            var cardData = CardManager.Instance.GetCardDataById(cardID);
            var targetSoldier = CardManager.Instance.FindTargetByID(targetID);

            if (cardData != null && targetSoldier != null)
            {
                if (cardData.Damage > 0)
                {
                    targetSoldier.TakeDamage(new HealthModifier
                    {
                        magnitude = (-1) * cardData.Damage, 
                        isCriticalHit = false, 
                        instigator =  null, 
                        source = null
                    });
                }

                if (cardData.Heal > 0)
                {
                    targetSoldier.TakeDamage(new HealthModifier
                    {
                        magnitude = cardData.Heal, 
                        isCriticalHit = false, 
                        instigator =  null, 
                        source = null
                    });
                }
            }
        }

        public void ClearPlayedCards()
        {
            Debug.Log("Reset played cards!");
            CardManager.Instance.FirstPlayerPlayedCards.Clear();
            CardManager.Instance.SecondPlayerPlayedCards.Clear();
        }
        [ClientRpc]
        private void DisplayCardActionClientRpc(char c)
        {
        }
        
        public void SpawnInitialPrefabsForPlayers()
        {
            GameManager.Instance.SpawnSoldiersForClients();
            SetState(new CardPlayState());
        }
    }
}