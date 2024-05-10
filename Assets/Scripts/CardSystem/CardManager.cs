using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CardSystem
{
    public class CardManager : NetworkBehaviour
    {

        [SerializeField] private List<CardData> m_allCards;
        public NetworkList<int> FirstPlayerCardIDs;
        public NetworkList<int> SecondPlayerCardIDs;

        private void Awake()
        {
            FirstPlayerCardIDs = new NetworkList<int>();
            SecondPlayerCardIDs = new NetworkList<int>();
        }

        public void GetInitialCardsForPlayer(int clientID)
        {
            if (clientID == 1)
            {
                FirstPlayerCardIDs.Clear();
                FirstPlayerCardIDs.Add(m_allCards[0].ID);
            }
            else if (clientID == 2)
            {
                SecondPlayerCardIDs.Clear();
                SecondPlayerCardIDs.Add(m_allCards[1].ID);
            }
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