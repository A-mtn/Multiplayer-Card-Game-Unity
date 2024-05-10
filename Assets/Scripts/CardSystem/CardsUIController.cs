using TurnSystem;
using Unity.Netcode;
using UnityEngine;


namespace CardSystem
{
    public class CardsUIController : NetworkBehaviour
    {
        [SerializeField] private TurnManager m_TurnManager;
        [SerializeField] private CardManager m_CardManager;
        
        [SerializeField] private GameObject m_Card;
        [SerializeField] private GameObject m_CardContainer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("I am a client?: " + IsClient);
            if (IsClient)
            {
                Debug.Log("Client connected to state value change event");
                m_TurnManager.currentStateID.OnValueChanged += HandleStateChange;
            }
                
        }

        private void HandleStateChange(int oldState, int newState)
        {
            Debug.Log("State is changed new state: " + newState);
            UpdateClientUI(newState);
        }

        private void UpdateClientUI(int state)
        {
            switch(state)
            {
                case 1:
                    InitializeCards();
                    break;
                case 2:
                    
                    break;
                
                default:
                    Debug.LogWarning($"Unrecognized state: {state}");
                    break;
            }
        }

        private void InitializeCards()
        {

            int clientID = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log("Try to get cards for " + clientID);
            if (clientID == 1)
            {
                RenderCards(m_CardManager.FirstPlayerCardIDs);
            }
            else if (clientID == 2)
            {
                RenderCards(m_CardManager.SecondPlayerCardIDs);
            }
        }

        private void RenderCards(NetworkList<int> cardList)
        {
            if (cardList == null)
            {
                Debug.LogWarning("Card list is empty!!!");
                return;
            }
            
            foreach (Transform child in m_CardContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var id in cardList)
            {
                var cardData = m_CardManager.GetCardDataById(id);
                if (!cardData)
                    continue;
                
                var cardObject = Instantiate(m_Card, m_CardContainer.transform);
        
                var cardUI = cardObject.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCardData(cardData);
                }
            }
        }
    }
}
