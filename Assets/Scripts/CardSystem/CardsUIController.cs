using System.Collections.Generic;
using SoldierSystem;
using TMPro;
using TurnSystem;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;


namespace CardSystem
{
    public class CardsUIController : NetworkBehaviour
    {
        [SerializeField] private TurnManager m_TurnManager;
        [SerializeField] private CardManager m_CardManager;
        
        [SerializeField] private GameObject m_Card;
        [SerializeField] private GameObject m_CardContainer;

        [SerializeField] private Material m_EnemyOutlineMaterial;
        [SerializeField] private Material m_FriendOutlineMaterial;
        
        [SerializeField] private Transform m_localPlayerPlayedCardsContainer;
        [SerializeField] private Transform m_otherPlayerPlayedCardsContainer;
        [SerializeField] private GameObject m_playedCardInfoPrefab;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("I am a client?: " + IsClient);
            
            if (IsClient)
            {
                var clientID = (int)NetworkManager.Singleton.LocalClientId;
                Debug.Log("Client connected to state value change event");
                m_TurnManager.currentStateID.OnValueChanged += HandleStateChange;
                switch (clientID)
                {
                    case 1:
                        m_CardManager.FirstPlayerCardIDs.OnListChanged += OnCardsChanged;
                        break;
                    case 2:
                        m_CardManager.SecondPlayerCardIDs.OnListChanged += OnCardsChanged;
                        break;
                }

                m_CardManager.playedCard += UpdatePlayedCardsUI;
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
                    break;
                case 2:
                    
                    break;
                
                default:
                    Debug.LogWarning($"Unrecognized state: {state}");
                    break;
            }
        }

        private void RenderCards(NetworkList<int> cardList)
        {
            if (cardList == null || cardList.Count == 0)
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
                Debug.Log("Trying to render card: " + id);
                var cardData = m_CardManager.GetCardDataById(id);
                if (!cardData)
                {
                    Debug.Log("NO card data with id: " + id);
                    continue;  
                } 
                
                var cardObject = Instantiate(m_Card, m_CardContainer.transform);
                
                var cardUI = cardObject.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCardData(cardData);
                    cardUI.clicked += OnCardClicked;
                }
            }
        }
        
        private void OnCardClicked(CardData cardData)
        {
            // Show target selection UI based on card type
            if (cardData.Damage > 0)
            {
                HighlightTargets("Enemy", cardData.ID, m_EnemyOutlineMaterial);
            }
            else if (cardData.Heal > 0)
            {
                HighlightTargets("Friend", cardData.ID, m_FriendOutlineMaterial);
            }
        }
        
        private void HighlightTargets(string tag, int cardID, Material outlineMaterial)
        {
            List<Soldier> soldiers = m_CardManager.GetSoldiersByTag(tag);
            foreach (var soldier in soldiers)
            {
                Renderer renderer = soldier.GetComponent<Renderer>();
                if (renderer != null)
                {
                    List<Material> materials = new List<Material>(renderer.materials) { outlineMaterial };
                    renderer.materials = materials.ToArray();
                } 
                soldier.selected += () => OnTargetSelected(cardID, soldier.SoldierId, outlineMaterial);
            }
        }
        
        private void OnTargetSelected(int cardID, int targetID, Material outlineMaterial)
        {
            // Remove outline from all soldiers
            Debug.Log("remove outline from all soldiers!");
            RemoveOutlineFromAllSoldiers(outlineMaterial);

            // Send selected card and target to server
            var clientID = (int)NetworkManager.Singleton.LocalClientId;
            SelectCardOnServerRpc(clientID, cardID, targetID);
        }

        private void RemoveOutlineFromAllSoldiers(Material outlineMaterial)
        {
            foreach (var soldier in m_CardManager.GetSoldiersByTag("Friend"))
            {
                RemoveOutline(soldier, outlineMaterial);
            }

            foreach (var soldier in m_CardManager.GetSoldiersByTag("Enemy"))
            {
                RemoveOutline(soldier, outlineMaterial);
            }
        }
        
        private void RemoveOutline(Soldier soldier, Material outlineMaterial)
        {
            Renderer renderer = soldier.GetComponent<Renderer>();
            if (renderer != null)
            {
                List<Material> materials = new List<Material>(renderer.materials);
                foreach (var mat in renderer.materials)
                {
                    if (mat.name.Contains(outlineMaterial.name))
                    {
                        materials.Remove(mat);
                    }
                }
                renderer.materials = materials.ToArray();
            }
            soldier.selected -= () => OnTargetSelected(0, soldier.SoldierId, outlineMaterial);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SelectCardOnServerRpc(int clientID, int cardID, int targetID)
        {
            m_CardManager.SelectCard(clientID, cardID, targetID);
        }
        
        private void OnCardsChanged(NetworkListEvent<int> changeEvent)
        {
            var clientID = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log("Cards changed for client: " + clientID);
            switch (clientID)
            {
                case 1:
                    RenderCards(m_CardManager.FirstPlayerCardIDs);
                    break;
                case 2:
                    RenderCards(m_CardManager.SecondPlayerCardIDs);
                    break;
            }
        }

        public void UpdatePlayedCardsUI(int clientID, int cardID, int targetID)
        {
            Debug.Log("Catch played card event!!!");
            // Update local player's played cards UI
            
            var localClientID = (int)NetworkManager.Singleton.LocalClientId;
            if (localClientID == clientID)
            {
                UpdatePlayedCardsUI(m_localPlayerPlayedCardsContainer, cardID, targetID);
            }
            else
            {
                // Update other player's played cards UI
                UpdatePlayedCardsUI(m_otherPlayerPlayedCardsContainer, cardID, targetID);
            }
        }

        private void UpdatePlayedCardsUI(Transform container, int cardID, int targetID)
        {
            // Clear previous entries
            /*foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }*/

            Debug.Log("c: " + cardID + " t: " + targetID);
            var cardData = m_CardManager.GetCardDataById(cardID);
            var targetSoldier = m_CardManager.FindTargetByID(targetID);
            if (cardData != null && targetSoldier != null)
            {
                var entry = Instantiate(m_playedCardInfoPrefab, container);
                var text = entry.GetComponentInChildren<TMP_Text>();
                Soldier soldier = targetSoldier.GetComponent<Soldier>();
                text.text = $"Card: {cardData.Name}, Target: {soldier.Name}";
            }
            else
            {
                Debug.LogWarning("COULD NOT FOUND SOLDIER WITH ID " + cardID);
            }
        }
    }
}
