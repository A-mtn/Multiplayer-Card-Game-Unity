using System;
using System.Collections.Generic;
using SoldierSystem;
using Unity.Netcode;
using UnityEngine;

namespace CardSystem
{
    public class CardManager : NetworkBehaviour
    {
        [SerializeField] private List<CardData> m_allCards;
        public NetworkList<int> FirstPlayerCardIDs;
        public NetworkList<int> SecondPlayerCardIDs;
        private List<(int cardID, int targetID)> m_firstPlayerSelectedCards = new List<(int cardID, int targetID)>();
        private List<(int cardID, int targetID)> m_secondPlayerSelectedCards = new List<(int cardID, int targetID)>();

        public List<(int cardID, int targetID)> FirstPlayerPlayedCards { get; private set; } = new List<(int cardID, int targetID)>();
        public List<(int cardID, int targetID)> SecondPlayerPlayedCards { get; private set; } = new List<(int cardID, int targetID)>();

        public Action<int, int, int> playedCard;
        public Action<int> removedArrow;
        public static CardManager Instance { get; private set; }
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
            FirstPlayerCardIDs = new NetworkList<int>();
            SecondPlayerCardIDs = new NetworkList<int>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                FirstPlayerCardIDs.OnListChanged += OnFirstPlayerCardsChanged;
                SecondPlayerCardIDs.OnListChanged += OnSecondPlayerCardsChanged;
            }
            
        }

        public void AddCardToThePlayer(int cardID, int clientID)
        {
            if (IsServer)
            {
                if (clientID == 1)
                {
                    FirstPlayerCardIDs.Add(cardID);
                }
                else if (clientID == 2)
                {
                    SecondPlayerCardIDs.Add(cardID);
                }
            }
        }
        
        public List<Soldier> GetSoldiersByTag(string tag)
        {
            List<Soldier> soldiers = new List<Soldier>();
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                Soldier soldier = networkObject.GetComponent<Soldier>();
                if (soldier != null && soldier.CompareTag(tag))
                {
                    soldiers.Add(soldier);
                }
            }
            return soldiers;
        }
        
        public Soldier FindTargetByID(int targetID)
        {
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                Soldier soldier = networkObject.GetComponent<Soldier>();
                if (soldier != null && soldier.SoldierId == targetID)
                {
                    return soldier;
                }
            }
            return null;
        }
        
        public void SelectCard(int clientID, int cardID, int targetID)
        {
            if (IsServer)
            {
                if (clientID == 1)
                {
                    Debug.Log("first player card count: " + FirstPlayerPlayedCards.Count);
                    if (FirstPlayerPlayedCards.Count < 3)
                    {
                        Debug.Log("ADD TO FIRST PLAYER CARDS: " + cardID + " t: " + targetID);
                        FirstPlayerPlayedCards.Add((cardID, targetID));
                    }
                }
                else if (clientID == 2)
                {
                    if (SecondPlayerPlayedCards.Count < 3)
                    {
                        Debug.Log("ADD TO SECOND PLAYER CARDS: " + cardID + " t: " + targetID);
                        SecondPlayerPlayedCards.Add((cardID, targetID));
                    }
                }
                UpdatePlayedCardsClientRpc(clientID, cardID, targetID);
            }
        }
        
        private void OnFirstPlayerCardsChanged(NetworkListEvent<int> changeEvent)
        {
            Debug.Log("First player cards updated.");
            // Handle logic for when the first player's cards change.
        }

        private void OnSecondPlayerCardsChanged(NetworkListEvent<int> changeEvent)
        {
            Debug.Log("Second player cards updated.");
            // Handle logic for when the second player's cards change.
        }
              
        public void RemoveCardForSoldier(int soldierID)
        {
            if (IsServer)
            {
                CardData cardData = m_allCards.Find(card => card.SoldierID == soldierID);
                if (cardData != null)
                {
                    FirstPlayerCardIDs.Remove(cardData.ID);
                    SecondPlayerCardIDs.Remove(cardData.ID);
                }
            }
        }
        
        public int GetSoldierClient(int soldierID)
        {
            CardData cardData = m_allCards.Find(card => card.SoldierID == soldierID);
            if (FirstPlayerCardIDs.Contains(cardData.ID))
            {
                return 0;
            }
            else if (SecondPlayerCardIDs.Contains(cardData.ID))
            {
                return 1;
            }

            return 99;
        }
        
        [ClientRpc]
        private void UpdatePlayedCardsClientRpc(int clientID, int cardID, int targetID)
        {
            playedCard?.Invoke(clientID, cardID, targetID);
        }

        public void RemoveArrow(int cardID)
        {
            RemoveArrowClientRpc(cardID);
        }
        
        [ClientRpc]
        private void RemoveArrowClientRpc(int cardID)
        {
            removedArrow?.Invoke(cardID);
        }
        
        public CardData GetCardDataById(int cardId)
        {
            foreach (CardData card in m_allCards)
            {
                if (card.ID == cardId)
                {
                    return card;
                }
            }
            return null;
        }
    }
}
