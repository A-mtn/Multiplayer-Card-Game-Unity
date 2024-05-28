using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TurnSystem;

namespace QuantumConnectionSystem
{
    public class QuantumConnector : MonoBehaviour
    {
        public static QuantumConnector Instance { get; private set; }
        const string URL = "http://localhost:8487/run-circuit";
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void ConnectToQuantum()
        {
            string exampleCircuit = "{\"qubits\": 2, \"circuit\": \"data\"}";
            StartCoroutine(GetRequest(exampleCircuit));
        }

        IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(URL))
            {
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        ProcessQuantumResult(webRequest.downloadHandler.text);
                        break;
                }
            }
        } 
        
        private void ProcessQuantumResult(string jsonResult)
        {
            QuantumResult result = JsonUtility.FromJson<QuantumResult>(jsonResult);
            TurnManager.Instance.HandleCardPlayOrder(result.mostOccurred);
        }
        
        [System.Serializable]
        public class QuantumResult
        {
            public bool success;
            public string mostOccurred;
            public int count;
        }
    }
}