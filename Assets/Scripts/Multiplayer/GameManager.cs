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
                AddPlayerCardClientRpc(state.Value);
                return;
            } else if (state.Type == NetworkListEvent<PlayerState>.EventType.Remove || state.Type == NetworkListEvent<PlayerState>.EventType.RemoveAt)
            {
                if (IsServer)
                {
                    var playerState = GetPlayerByClientId(state.Value.ClientId);
                    for (int i = 0; i < playerStates.Count; i++)
                    {
                        if (state.Value.ClientId == playerStates[i].ClientId)
                        {
                            playerStates[i] = new PlayerState(state.Value)
                            {
                                IsAlive = false
                            };
                            return;
                        }
                    }
                }
                // DisablePlayerCardClientRpc(state.Value);
                
            }
            else if (state.Type == NetworkListEvent<PlayerState>.EventType.Value)
            {
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

            UpdatePlayerIsAlive();
            Time.timeScale = 1;
            CurrentGameStatus = GameStatus.RUNNING;
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

        public void OnGameEnd(PlayerState player)
        {
            OnGameEndClientRpc(player);
        }

        [ClientRpc]
        public void OnGameEndClientRpc(PlayerState player)
        {
            CurrentGameStatus = GameStatus.FINISHED;
            Time.timeScale = 0;
            UIManager.Instance.OpenGameEndUI(true, $"Winner {player.PlayerName}({player.Score})");
            BoardManager.Instance.gameObject.SetActive(false);
        }
        public void ResumeGame()
        {
            if (IsServer)
            {
                Time.timeScale = 1;
            }
            UIManager.Instance.OpenPauseUI(false);
            CurrentGameStatus = GameStatus.RUNNING;
        }
        public void LeaveGame()
        {
            ExitToMenu();
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
        public PlayerState? GetPlayerByClientId(ulong clientId)
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

        public void UpdatePlayerHealth(List<PlayerState> newPlayerStates)
        {
            foreach (var item in newPlayerStates)
            {
                for (int i = 0; i < playerStates.Count; i++)
                {
                    if (playerStates[i].ClientId == item.ClientId)
                    {
                        playerStates[i] = new PlayerState(playerStates[i])
                        {
                            HasDoneFirstAction = item.HasDoneFirstAction,
                            Score = item.Score
                        };
                        // UIManager.Instance.UpdatePlayerCard(playerStates[i]);
                    }
                }
            }
        }
        public void UpdatePlayerIsAlive()
        {

            for (int i = 0; i < playerStates.Count; i++)
            {
                if (playerStates[i].IsAlive && playerStates[i].HasDoneFirstAction)
                {
                    if (playerStates[i].Score < 1)
                    {
                        Debug.Log($"Player dies {playerStates[i].PlayerName} ({playerStates[i].ClientId})");
                        playerStates[i] = new PlayerState(playerStates[i])
                        {
                            IsAlive = false
                        };
                    }
                }
            }
        }

        private void HostShutdown()
        {
            ShutdownClientRpc();
        }

        private void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }

        [ClientRpc]
        private void ShutdownClientRpc()
        {
            //if (IsServer)
            //    return;

            Shutdown();
        }

        public void ExitToMenu()
        {
            if (IsServer)
            {
                HostShutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
                LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
            }
        }
    }
}