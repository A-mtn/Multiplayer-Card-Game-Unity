using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MainGame
{
    public class EndGameUIController : NetworkBehaviour
    {
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private TMP_Text endGameText;

        private int m_player1Soldiers = 3;
        private int m_player2Soldiers = 3;
        public static EndGameUIController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CheckEndGame(int clientID)
        {
            Debug.LogWarning("dead soldier for: " + clientID);
            if (clientID == 0)
            {
                m_player1Soldiers--;
            }
            else if (clientID == 1)
            {
                m_player2Soldiers--;
            }

            Debug.LogWarning("player 1 soldiers: " + m_player1Soldiers + " player 2 soldiers: " + m_player2Soldiers);
            if (m_player1Soldiers == 0)
            {
                ShowEndGameUIClientRpc(0);
            }
            else if (m_player2Soldiers == 0)
            {
                ShowEndGameUIClientRpc(1);
            }
        }
        
        [ClientRpc]
        private void ShowEndGameUIClientRpc(int winningPlayer)
        {
            Debug.LogWarning("player " + winningPlayer + " won!");
            endGamePanel.SetActive(true);
            endGameText.text = $"Client {winningPlayer} Won!";
        }
    }
}