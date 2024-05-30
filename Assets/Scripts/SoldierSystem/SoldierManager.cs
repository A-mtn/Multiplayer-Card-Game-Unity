using System.Collections.Generic;
using System.Linq;
using CardSystem;
using Unity.Netcode;
using UnityEngine;

namespace SoldierSystem
{
    public class SoldierManager : NetworkBehaviour
    {
        private Dictionary<ulong, List<int>> m_playerSoldiers = new Dictionary<ulong, List<int>>();
        [SerializeField] private GameObject[] m_soldierPrefabs;
        [SerializeField] private CardManager m_cardManager;
        
        public void RegisterPlayerSoldiers(ulong clientId, List<int> soldierIds) {
            if (!m_playerSoldiers.ContainsKey(clientId)) {
                m_playerSoldiers[clientId] = new List<int>();
            }
        
            m_playerSoldiers[clientId].AddRange(soldierIds);
        }
        
        public void SpawnSoldiersForPlayer(ulong clientID) {
            if (m_playerSoldiers.TryGetValue(clientID, out List<int> soldierIds)) {
                foreach (int id in soldierIds)
                {
                    SpawnSoldier(id, clientID);
                }
            }
        }
        
        private void SpawnSoldier(int soldierId, ulong clientID) {
            GameObject prefab = m_soldierPrefabs.FirstOrDefault(p => p.GetComponent<Soldier>().SoldierId == soldierId);
            if (prefab != null)
            {
                var pos = new Vector3(soldierId * 3.0f - ((int)clientID - 1) * 9, 0, (int)clientID * 6);
                GameObject soldierInstance = Instantiate(prefab, pos, Quaternion.identity);
                soldierInstance.GetComponent<NetworkObject>().Spawn();
                Soldier soldier = soldierInstance.GetComponent<Soldier>();
                soldierInstance.name = soldier.Name;
                soldier.SetInitialValues();
                m_cardManager.AddCardToThePlayer(soldier.SoldierCards[0].ID, (int)clientID);
                soldierInstance.tag = "Player" + (clientID-1);
                TagSoldierClientRpc(soldierInstance.GetComponent<NetworkObject>().NetworkObjectId, clientID);
            } else {
                Debug.LogError("No prefab found with Soldier ID: " + soldierId);
            }
        }

        [ClientRpc]
        private void TagSoldierClientRpc(ulong networkObjectId, ulong ownerClientId)
        {
            NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            GameObject soldierObject = networkObject.gameObject;
            ulong localClientID = NetworkManager.Singleton.LocalClientId;

            soldierObject.tag = localClientID == ownerClientId ? "Friend" : "Enemy";
        }
    }
}