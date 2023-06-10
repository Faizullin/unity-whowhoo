using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Multiplayer.UI;
using Multiplayer.Player;
using DapperDino.UMT.Lobby.Networking;

namespace Multiplayer
{

    public enum GameStatus {
        IDLE,
        RUNNING,
        FINISHED,
        PAUSED,
    };

    public class GameManager : SingletonNetwork<GameManager>
    {
        public GameStatus CurrentGameStatus = GameStatus.IDLE;
        [SerializeField]
        private ServerGameNetPortal m_serverGameNetPortal;

        private NetworkList<PlayerState> playerStates;

        private void Awake()
        {
            base.Awake();
            playerStates = new NetworkList<PlayerState>();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"OnNetworkSpawn IsClient-{IsClient}");
            if (IsClient)
            {
                playerStates.OnListChanged += HandlePlayersStateChanged;
                UIManager.Instance.CloseAllUIPanels();
            }

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

                foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                }

                StartGame();
            }
        }

        private void HandlePlayersStateChanged(NetworkListEvent<PlayerState> state)
        {
            if (state.Type == NetworkListEvent<PlayerState>.EventType.Add)
            {
                Debug.Log($"HandlePlayersStateChanged {state.Type} ({playerStates.Count})");
                AddPlayerCardClientRpc(state.Value);
                return;
            } else if (state.Type == NetworkListEvent<PlayerState>.EventType.Remove || state.Type == NetworkListEvent<PlayerState>.EventType.RemoveAt)
            {
                Debug.Log($"HandlePlayersStateChanged {state.Type} ({playerStates.Count})");
                DisablePlayerCardClientRpc(state.Value);
                return;
            }
            else if (state.Type == NetworkListEvent<PlayerState>.EventType.Value)
            {
                Debug.Log($"HandlePlayersStateChanged {state.Type} ({playerStates.Count})");
                UIManager.Instance.UpdatePlayerCard(state.Value);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            // var playerData = TestStartManager.Instance.GetPlayer();
            //Debug.Log($"Main.HandleClientConnected {clientId} --> {playerData.PlayerName}");

            //bool tmpHasPlayerState = false;
            //for (int i = 0; i < playerStates.Count; i++)
            //{
            //    if (playerStates[i].ClientId == clientId)
            //    {
            //        tmpHasPlayerState = true;
            //        break;
            //    }
            //}
            //if (!tmpHasPlayerState)
            //{
            //    playerStates.Add(playerData);
            //}

            var playerData = ServerGameNetPortal.Instance.GetPlayerData(clientId);

            if (!playerData.HasValue) { return; }

            var playerState = new PlayerState(
                clientId,
                playerData.Value.PlayerName,
                playerData.Value.PlayerColor,
                0,
                true,
                false
            );

            playerStates.Add(playerState);
        }

        [ClientRpc]
        private void AddPlayerCardClientRpc(PlayerState playerState)
        {
            UIManager.Instance.AddPlayerCard(playerState);
        }

        [ClientRpc]
        private void DisablePlayerCardClientRpc(PlayerState playerState)
        {
            UIManager.Instance.DisablePlayerCard(playerState);
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].ClientId == clientId)
                {
                    playerStates.RemoveAt(i);
                    break;
                }
            }
        }

        public void StartGame()
        {
            if (!IsServer) return;

            UpdatePlayerStats();
            BoardManager.Instance.StartGame();            
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnPauseClick();
            }
        }

        public void OnPauseClick()
        {
            if (CurrentGameStatus == GameStatus.PAUSED)
            {
                CurrentGameStatus = GameStatus.RUNNING;
            } else if (CurrentGameStatus == GameStatus.RUNNING)
            {
                CurrentGameStatus = GameStatus.PAUSED;
            }
            if (IsHost || IsServer)
            {
                // Time.timeScale = CurrentGameStatus == GameStatus.PAUSED ? 0 : 1;
            }
            
            UIManager.Instance.OpenPauseUI(CurrentGameStatus == GameStatus.PAUSED);
        }

        public void ResumeGame()
        {
            if (IsHost || IsServer)
            {
                Time.timeScale = 1;
            }
            UIManager.Instance.OpenPauseUI(false);
            CurrentGameStatus = GameStatus.RUNNING;
        }
        public void OnGameEnd(PlayerState player)
        {
            CurrentGameStatus = GameStatus.FINISHED;
            UIManager.Instance.OpenGameEndUI(true, $"Winner {player.PlayerName}({player.Score})");
            BoardManager.Instance.gameObject.SetActive(false);
        }
        public void OnGameRestart()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        public void LeaveGame()
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene);
        }
        public void UpdatePlayerStats()
        {
            PlayersHealthUpdate();
        }
        private void PlayersHealthUpdate()
        {
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].IsAlive && playerStates[i].HasDoneFirstAction)
                {
                    if (playerStates[i].Score < 1)
                    {
                        Debug.Log($"Player dies {playerStates[i].PlayerName}");
                        playerStates[i] = new PlayerState(
                            playerStates[i].ClientId,
                            playerStates[i].PlayerName.ToString(),
                            playerStates[i].PlayerColor,
                            playerStates[i].Score,
                            false,
                            playerStates[i].HasDoneFirstAction
                        );
                    }
                }
            }
        }

        public bool CheckForWinner()
        {
            int alivePlayersCount = 0;
            foreach (PlayerState playerData in playerStates)
            {
                if (playerData.IsAlive)
                {
                    alivePlayersCount++;
                }
            }
            return alivePlayersCount > 1;
        }

        public void CheckForWinnerAndRaiseGameEnd()
        {
            if(!CheckForWinner())
            {
                Time.timeScale = 0;
                var currentPlayer = BoardManager.Instance.GetCurrentPlayer();
                if (currentPlayer != null)
                {
                    OnGameEnd(currentPlayer.Value);
                }
            }
        }

        public PlayerState? GetPlayerStateByTurnIndex(ulong clientId)
        {
            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].ClientId == clientId)
                {
                    return playerStates[i];
                }
            }
            return null;
        }

        public List<ulong> GetSortedClientIds()
        {
            List<ulong> result = new();
            foreach (var item in playerStates)
            {
                result.Add(item.ClientId);
            }
            result.Sort();
            return result;
        }

        public void UpdatePlayerStates(PlayerState[] newPlayerStates)
        {
            Debug.Log($"GameManage.UpdatePlayerStates: {newPlayerStates.Length} {playerStates.Count}");
            //playerStates.SetDirty(true);
            foreach (var item in newPlayerStates)
            {
                for (int i = 0; i < playerStates.Count; i++)
                {
                    if (BoardManager.Instance.GetTilePlayerIdFromClientId(playerStates[i].ClientId) == item.ClientId)
                    {
                        playerStates[i] = new PlayerState(playerStates[i].ClientId, playerStates[i].PlayerName.ToString(), playerStates[i].PlayerColor, item.Score, playerStates[i].IsAlive, playerStates[i].HasDoneFirstAction);
                    }
                }
            }
            // playerStates.SetDirty(false);
        }

    }
}