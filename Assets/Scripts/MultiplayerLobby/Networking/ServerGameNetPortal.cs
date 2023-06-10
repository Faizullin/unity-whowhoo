using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DapperDino.UMT.Lobby.Networking
{
    public class ServerGameNetPortal : SingletonPersistent<ServerGameNetPortal>
    {
        [Header("Settings")]
        [SerializeField] private int maxPlayers = 4;

        [Header("Clients Data")]
        [SerializeField]
        private Dictionary<string, PlayerData> clientData;
        [SerializeField]
        private Dictionary<ulong, string> clientIdToGuid;
        [SerializeField]
        private Dictionary<ulong, int> clientSceneMap;
        [SerializeField]
        private bool gameInProgress;

        private const int MaxConnectionPayload = 1024;

        private GameNetPortal gameNetPortal;

        private void Start()
        {
            gameNetPortal = GetComponent<GameNetPortal>();
            gameNetPortal.OnNetworkReadied += HandleNetworkReadied;

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

            clientData = new Dictionary<string, PlayerData>();
            clientIdToGuid = new Dictionary<ulong, string>();
            clientSceneMap = new Dictionary<ulong, int>();
        }

        private void OnDestroy()
        {
            if (gameNetPortal == null) { return; }

            gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;

            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        }

        public string GetPlayerGuid(ulong clientId)
        {
            if (clientIdToGuid.TryGetValue(clientId, out string clientGuid))
            {
                return clientGuid;
            }
            else
            {
                Debug.LogWarning($"No client guid found for client id: {clientId}");
            }

            return null;
        }
        public PlayerData? GetPlayerData(ulong clientId)
        {
           var clientGuid =  GetPlayerGuid(clientId);
            if (clientGuid != null)
            {
                if (clientData.TryGetValue(clientGuid, out PlayerData playerData))
                {
                    return playerData;
                }
                else
                {
                    Debug.LogWarning($"No player data found for client id: {clientId}");
                }
            }
            return null;
        }

        public void StartGame(Dictionary<string, PlayerData> newClientData = null)
        {
            gameInProgress = true;

            if (newClientData != null)
            {
                clientData = newClientData;
            }

            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerScene", LoadSceneMode.Single);
        }

        public void EndRound()
        {
            gameInProgress = false;

            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerLobbyScene", LoadSceneMode.Single);
        }

        private void HandleNetworkReadied()
        {
            if (!NetworkManager.Singleton.IsServer) { return; }

            gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            gameNetPortal.OnClientSceneChanged += HandleClientSceneChanged;

            NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerLobbyScene", LoadSceneMode.Single);

            if (NetworkManager.Singleton.IsHost)
            {
                clientSceneMap[NetworkManager.Singleton.LocalClientId] = SceneManager.GetActiveScene().buildIndex;
            }
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            clientSceneMap.Remove(clientId);

            if (clientIdToGuid.TryGetValue(clientId, out string guid))
            {
                clientIdToGuid.Remove(clientId);

                if (clientData[guid].ClientId == clientId)
                {
                    clientData.Remove(guid);
                }
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
                gameNetPortal.OnClientSceneChanged -= HandleClientSceneChanged;
            }
        }

        private void HandleClientSceneChanged(ulong clientId, int sceneIndex)
        {
            clientSceneMap[clientId] = sceneIndex;
        }

        private void HandleUserDisconnectRequested()
        {
            HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

            NetworkManager.Singleton.Shutdown();

            ClearData();

            SceneManager.LoadScene("MainMenuScene");
        }

        private void HandleServerStarted()
        {
            if (!NetworkManager.Singleton.IsHost) { return; }

            string clientGuid = Guid.NewGuid().ToString();
            string playerName = PlayerPrefs.GetString("PlayerName", "Missing Name");

            clientData.Add(clientGuid, new PlayerData(playerName, NetworkManager.Singleton.LocalClientId, Color.white));
            clientIdToGuid.Add(NetworkManager.Singleton.LocalClientId, clientGuid);
        }

        private void ClearData()
        {
            clientData.Clear();
            clientIdToGuid.Clear();
            clientSceneMap.Clear();

            gameInProgress = false;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var clientId = request.ClientNetworkId;
            var connectionData = request.Payload;

            Debug.Log($"ApprovalCheck {connectionData.Length > MaxConnectionPayload} ({clientId == NetworkManager.Singleton.LocalClientId})");

            if (connectionData.Length > MaxConnectionPayload)
            {
                response.Approved = false;
                return;
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                response.Approved = false;
                return;
            }

            string payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            ConnectStatus gameReturnStatus = ConnectStatus.Success;

            //if (clientData.ContainsKey(connectionPayload.clientGUID))
            //{
            //    ulong oldClientId = clientData[connectionPayload.clientGUID].ClientId;
            //    StartCoroutine(WaitToDisconnectClient(oldClientId, ConnectStatus.LoggedInAgain));
            //}
            
            if (gameInProgress)
            {
                gameReturnStatus = ConnectStatus.GameInProgress;
            }
            else if (clientData.Count >= maxPlayers)
            {
                gameReturnStatus = ConnectStatus.ServerFull;
            }

            Debug.Log($"ApprovalCheck {gameReturnStatus == ConnectStatus.Success} ({clientId})");
            if (gameReturnStatus == ConnectStatus.Success)
            {
                clientSceneMap[clientId] = connectionPayload.clientScene;
                clientIdToGuid[clientId] = connectionPayload.clientGUID;
                clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId, Color.white);
            }

            response.Approved = true;

            gameNetPortal.ServerToClientConnectResult(clientId, gameReturnStatus);

            if (gameReturnStatus != ConnectStatus.Success)
            {
                StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
            }
        }

        private IEnumerator WaitToDisconnectClient(ulong clientId, ConnectStatus reason)
        {
            gameNetPortal.ServerToClientSetDisconnectReason(clientId, reason);

            yield return new WaitForSeconds(0);

            KickClient(clientId);
        }

        private void KickClient(ulong clientId)
        {
            NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (networkObject != null)
            {
                networkObject.Despawn(true);
            }

            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
}
