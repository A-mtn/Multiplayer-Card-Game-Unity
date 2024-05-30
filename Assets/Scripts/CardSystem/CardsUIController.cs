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
        
        [SerializeField] private GameObject m_ArrowPrefab;
        private Dictionary<int, GameObject> m_Arrows = new Dictionary<int, GameObject>();
        
        private Dictionary<int, System.Action> targetSelectedHandlers = new Dictionary<int, System.Action>();
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

                Debug.LogWarning("Connect to playedCard action !!!");
                m_CardManager.playedCard += UpdatePlayedCardsUI;
                m_CardManager.removedArrow += RemoveArrow;
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
                    ResetUI();
                    break;
                case 2:
                    ResetUI();
                    ClearSelections();
                    break;
                case 3:
                    ResetCardUI();
                    break;
                default:
                    Debug.LogWarning($"Unrecognized state: {state}");
                    break;
            }
        }
        
        private void ResetUI()
        {
            ResetCardUI();
            ClearArrowContainer();
        }

        private void ResetCardUI()
        {
            foreach (Transform child in m_CardContainer.transform)
            {
                var cardUI = child.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.ResetScale();
                }
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
        
        private void OnCardClicked(CardData cardData, CardUI cardUI)
        {
            // Show target selection UI based on card type
            Debug.Log("card clicked on controller");
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
                System.Action handler = () => OnTargetSelected(cardID, soldier.SoldierId, outlineMaterial);
                
                if (!targetSelectedHandlers.ContainsKey(soldier.SoldierId))
                {
                    targetSelectedHandlers[soldier.SoldierId] = handler;
                }
                
                Debug.Log(soldier.Name + " started listening selected event!");
                soldier.selected += targetSelectedHandlers[soldier.SoldierId];
            }
        }
        
        private void OnTargetSelected(int cardID, int targetID, Material outlineMaterial)
        {
            Debug.Log("remove outline from all soldiers!");
            RemoveOutlineFromAllSoldiers(outlineMaterial);

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
            
            if (targetSelectedHandlers.ContainsKey(soldier.SoldierId))
            {
                
                Debug.Log(soldier.Name + " stopped listening selected event!");
                soldier.selected -= targetSelectedHandlers[soldier.SoldierId];
                targetSelectedHandlers.Remove(soldier.SoldierId);
            }
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
            Debug.Log("Catch played card event!!! card:" + cardID + " target: " + targetID);
            // Update local player's played cards UI
            
            var localClientID = (int)NetworkManager.Singleton.LocalClientId;
            if (localClientID == clientID)
            {
                UpdatePlayedCardsUI(m_localPlayerPlayedCardsContainer, cardID, targetID);
            }
            else
            {
                UpdatePlayedCardsUI(m_otherPlayerPlayedCardsContainer, cardID, targetID);
            }
        }
        
        private void UpdatePlayedCardsUI(Transform container, int cardID, int targetID)
        {
           // ClearContainer();
            
            Debug.Log("c: " + cardID + " t: " + targetID);
            var cardData = m_CardManager.GetCardDataById(cardID);
            var targetSoldier = m_CardManager.FindTargetByID(targetID);
            if (cardData != null && targetSoldier != null)
            {
                CreateArrow(cardData, targetSoldier);
            }
            else 
            { 
                Debug.LogWarning("COULD NOT FIND SOLDIER WITH ID " + targetID);
            }
        }

        private void ClearArrowContainer()
        {
            foreach (var arrow in m_Arrows.Values)
            {
                Destroy(arrow);
            }
            m_Arrows.Clear();
        }
        
        private void CreateArrow(CardData cardData, Soldier targetSoldier)
        {
            var sourceSoldier = m_CardManager.FindTargetByID(cardData.SoldierID);
            if (sourceSoldier != null)
            {
                var arrow = Instantiate(m_ArrowPrefab);
                m_Arrows[cardData.ID] = arrow;

                var lineRenderer = arrow.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(1, sourceSoldier.transform.position + Vector3.up * 1.6f);
                    lineRenderer.SetPosition(0, targetSoldier.transform.position + Vector3.up * 1.6f);
                }
            }
        }
        
        public void RemoveArrow(int cardID)
        {
            if (m_Arrows.ContainsKey(cardID))
            {
                Destroy(m_Arrows[cardID]);
                m_Arrows.Remove(cardID);
            }
        }
        
        private void ClearSelections()
        {
            foreach (var soldier in m_CardManager.GetSoldiersByTag("Friend"))
            {
                RemoveOutline(soldier, m_FriendOutlineMaterial);
            }

            foreach (var soldier in m_CardManager.GetSoldiersByTag("Enemy"))
            {
                RemoveOutline(soldier, m_EnemyOutlineMaterial);
            }

            targetSelectedHandlers.Clear();
        }
    }
}
