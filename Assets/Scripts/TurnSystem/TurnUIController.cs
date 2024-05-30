using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

namespace TurnSystem
{
    public class TurnUIController : NetworkBehaviour
    {
        [SerializeField] private Button m_endTurnButton;
        [SerializeField] private TurnManager m_TurnManager;
        [SerializeField] private TMP_Text m_whoIsText;
        [SerializeField] private TMP_Text m_quantumNumberText;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("I am a TURN client?: " + IsClient);
            if (IsClient)
            {
                Debug.Log("Client connected to state value change event TURN UI");
                m_TurnManager.currentStateID.OnValueChanged += HandleStateChange;
                m_TurnManager.quantumNumber.OnValueChanged += UpdateQuantumText;
                m_endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
                m_whoIsText.text = "Client " + ((int)NetworkManager.Singleton.LocalClientId - 1);
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
            m_endTurnButton.gameObject.SetActive(newState == 2);
            m_quantumNumberText.gameObject.SetActive(newState == 3);
        }

        public void UpdateQuantumText(int oldValue, int newValue)
        {
            m_quantumNumberText.text = newValue.ToString();
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