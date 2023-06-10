using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DapperDino.UMT.Lobby.Networking;
using DapperDino.UMT.Lobby.UI;

namespace Multiplayer.Lobby.UI
{
    public class LobbyUIManager : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;
        [SerializeField] private Button startGameButton;
        [SerializeField] private int m_minPlayerEnterNumber = 2;

        private NetworkList<LobbyPlayerState> lobbyPlayers;

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
            }

            Debug.Log($"OnNetworkSpawn IsClient-{IsClient} IsServer-{IsServer} IsHost-{IsHost}");
            if (IsServer)
            {
                startGameButton.gameObject.SetActive(true);

                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

                foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                }
            }
            else
            {
                startGameButton.gameObject.SetActive(false);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
        }

        private bool IsEveryoneReady()
        {
            if (lobbyPlayers.Count < m_minPlayerEnterNumber)
            {
                return false;
            }

            foreach (var player in lobbyPlayers)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleClientConnected(ulong clientId)
        {
            var playerData = ServerGameNetPortal.Instance.GetPlayerData(clientId);

            if (!playerData.HasValue) { return; }

            lobbyPlayers.Add(new LobbyPlayerState(
                clientId,
                playerData.Value.PlayerName,
                false,
                Color.white
            ));
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == clientId)
                {
                    lobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    lobbyPlayers[i] = new LobbyPlayerState(
                        lobbyPlayers[i].ClientId,
                        lobbyPlayers[i].PlayerName,
                        !lobbyPlayers[i].IsReady,
                        lobbyPlayers[i].PlayerColor
                    );
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            if (!IsEveryoneReady()) { return; }

            Dictionary<string, PlayerData> newClientData = new() { };
            foreach (var lobbyPlayer in lobbyPlayers)
            {
                var playerData = ServerGameNetPortal.Instance.GetPlayerData(lobbyPlayer.ClientId);
                var playerGuid = ServerGameNetPortal.Instance.GetPlayerGuid(lobbyPlayer.ClientId);

                if (playerData != null && playerGuid != null)
                {
                    newClientData.Add(playerGuid, new PlayerData(playerData.Value.PlayerName, playerData.Value.ClientId, lobbyPlayer.PlayerColor));
                }
            }

            ServerGameNetPortal.Instance.StartGame(newClientData);
        }

        public void OnLeaveClicked()
        {
            GameNetPortal.Instance.RequestDisconnect();
        }

        public void OnReadyClicked()
        {
            ToggleReadyServerRpc();
        }

        public void OnStartGameClicked()
        {
            StartGameServerRpc();
        }

        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
        {            
            for (int i = 0; i < lobbyPlayerCards.Length; i++)
            {
                if (lobbyPlayers.Count > i)
                {
                    lobbyPlayerCards[i].UpdateDisplay(lobbyPlayers[i]);
                }
                else
                {
                    lobbyPlayerCards[i].DisableDisplay();
                }
            }

            if (IsHost)
            {
                startGameButton.interactable = IsEveryoneReady();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerTeamServerRpc(byte newTeamIndex, Color teamColor, ServerRpcParams serverRpcParams = default)
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    lobbyPlayers[i] = new LobbyPlayerState(
                        lobbyPlayers[i].ClientId,
                        lobbyPlayers[i].PlayerName,
                        lobbyPlayers[i].IsReady,
                        teamColor
                    );
                }
            }
        }
    }
}
