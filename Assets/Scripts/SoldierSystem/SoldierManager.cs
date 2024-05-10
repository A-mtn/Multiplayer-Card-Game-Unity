using Unity.Netcode;
using UnityEngine;

namespace SoldierSystem
{
    public class SoldierManager : NetworkBehaviour
    {
        [SerializeField] private GameObject soldierPrefabBlue;
        [SerializeField] private GameObject soldierPrefabRed;
        
        public void SpawnInitialPrefabsForPlayers()
        {
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                Debug.Log("Creating soldiers for player " + player.ClientId);
                var zPos = (int)player.ClientId * 3; 
            
                GameObject prefabToSpawn = null;
                switch (player.ClientId)
                {
                    case 1:
                        prefabToSpawn = soldierPrefabBlue;
                        break;
                    case 2:
                        prefabToSpawn = soldierPrefabRed;
                        break;
                    default:
                        Debug.LogError("Unsupported client ID: " + player.ClientId);
                        continue;
                }
            
                for (int i = 0; i < 3; i++)
                {
                    GameObject go = Instantiate(prefabToSpawn, new Vector3(i * 3.0f, 0, zPos), Quaternion.identity);
                    go.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
        
    }
}