using Unity.Netcode;
using UnityEngine;

namespace MainGame
{
    public class CameraController:NetworkBehaviour
    {
        [SerializeField] private Transform m_player1CameraTransform;
        [SerializeField] private Transform m_player2CameraTransform;

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                SetupCamera();
            }
        }

        private void SetupCamera()
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            if (localClientId == 1)
            {
                transform.position = m_player1CameraTransform.position;
                transform.rotation = m_player1CameraTransform.rotation;
            }
            else if (localClientId == 2)
            {
                transform.position = m_player2CameraTransform.position;
                transform.rotation = m_player2CameraTransform.rotation;
            }
            else
            {
                Debug.LogError("Unrecognized client ID: " + localClientId);
            }
        }
    }
}