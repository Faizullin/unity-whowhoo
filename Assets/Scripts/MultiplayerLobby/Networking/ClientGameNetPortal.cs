using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DapperDino.UMT.Lobby.Networking
{
    [RequireComponent(typeof(GameNetPortal))]
    public class ClientGameNetPortal : SingletonPersistent<ClientGameNetPortal>
    {

        public DisconnectReason DisconnectReason { get; private set; } = new DisconnectReason();

        public event Action<ConnectStatus> OnConnectionFinished;

        public event Action OnNetworkTimedOut;

        private GameNetPortal gameNetPortal;

        private void Start()
        {
            gameNetPortal = GetComponent<GameNetPortal>();

            gameNetPortal.OnNetworkReadied += HandleNetworkReadied;
            gameNetPortal.OnConnectionFinished += HandleConnectionFinished;
            gameNetPortal.OnDisconnectReasonReceived += HandleDisconnectReasonReceived;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void OnDestroy()
        {
            if (gameNetPortal == null) { return; }

            gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;
            gameNetPortal.OnConnectionFinished -= HandleConnectionFinished;
            gameNetPortal.OnDisconnectReasonReceived -= HandleDisconnectReasonReceived;

            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void StartClient()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                clientGUID = Guid.NewGuid().ToString(),
                clientScene = SceneManager.GetActiveScene().buildIndex,
                playerName = PlayerPrefs.GetString("PlayerName", "Missing Name")
            });

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }

        private void HandleNetworkReadied()
        {
            if (!NetworkManager.Singleton.IsClient) { return; }

            if (!NetworkManager.Singleton.IsHost)
            {
                gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
            }
        }

        private void HandleUserDisconnectRequested()
        {
            DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
            NetworkManager.Singleton.Shutdown();

            HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

            SceneManager.LoadScene("MainMenuScene");
        }

        private void HandleConnectionFinished(ConnectStatus status)
        {
            if (status != ConnectStatus.Success)
            {
                DisconnectReason.SetDisconnectReason(status);
            }

            OnConnectionFinished?.Invoke(status);
        }

        private void HandleDisconnectReasonReceived(ConnectStatus status)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
            {
                gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;

                if (SceneManager.GetActiveScene().name != "MainMenuScene")
                {
                    if (!DisconnectReason.HasTransitionReason)
                    {
                        DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
                    }

                    SceneManager.LoadScene("MainMenuScene");
                }
                else
                {
                    OnNetworkTimedOut?.Invoke();
                }
            }
        }
    }
}
