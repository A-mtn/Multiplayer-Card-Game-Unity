using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TurnSystem
{
    public class TurnUIController : NetworkBehaviour
    {
        [SerializeField] private Button m_endTurnButton;
        [SerializeField] private TurnManager m_TurnManager;
        [SerializeField] private TMP_Text m_whoIsText;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("I am a TURN client?: " + IsClient);
            if (IsClient)
            {
                Debug.Log("Client connected to state value change event TURN UI");
                m_TurnManager.currentStateID.OnValueChanged += HandleStateChange;
                m_endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
                m_whoIsText.text = "Client " + NetworkManager.Singleton.LocalClientId;
            }
            else
            {
                m_whoIsText.text = "Server";
            }
        }
        
        private void HandleStateChange(int oldState, int newState)
        {
            Debug.Log("State is changed new state: " + newState);
            UpdateClientUI(newState);
        }

        private void UpdateClientUI(int newState)
        {
            bool shouldShowButton = newState == 1;
            m_endTurnButton.gameObject.SetActive(shouldShowButton);
        }
        
        private void OnEndTurnButtonClicked()
        {
            if (IsClient)
            {
                m_TurnManager.OnEndTurnButtonClicked();
            }
        }
    }
}