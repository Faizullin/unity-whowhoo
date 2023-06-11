using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.Player;

namespace Singleplayer
{

    public enum GameStatus {
        IDLE,
        RUNNING,
        FINISHED,
        PAUSED,
    };

    public class GameManager : Singleton<GameManager>
    {
        public GameStatus CurrentGameStatus = GameStatus.IDLE;

        [SerializeField]
        private List<PlayerState> playerStates;

        private void Start()
        {
            playerStates = new();
            CurrentGameStatus = GameStatus.RUNNING;
            UIManager.Instance.CloseAllUIPanels();

            var clientIds = PlayerDataManager.Instance.GetSortedKeys();
            foreach (var clientId in clientIds)
            {
                HandleClientConnected(clientId);
            }
            StartGame();
        }

        private void HandleClientConnected(ulong clientId)
        {
            var playerData = PlayerDataManager.Instance.PlayersData[clientId];

            var playerState = new PlayerState(
                clientId,
                playerData.PlayerName,
                playerData.PlayerColor,
                0,
                true,
                false
            );
            playerStates.Add(playerState);
            UIManager.Instance.AddPlayerCard(playerState);
        }

        public void StartGame()
        {
            UpdatePlayerIsAlive();
            Time.timeScale = 1;
            BoardManager.Instance.StartGame();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnPauseClick();
            }
        }

        public void OnGameEnd(PlayerState player)
        {
            CurrentGameStatus = GameStatus.FINISHED;
            UIManager.Instance.OpenGameEndUI(true, $"Winner {player.PlayerName}({player.Score})");
            BoardManager.Instance.gameObject.SetActive(false);
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            UIManager.Instance.OpenPauseUI(false);
            CurrentGameStatus = GameStatus.RUNNING;
        }
        public void OnPauseClick()
        {
            if (CurrentGameStatus == GameStatus.PAUSED)
            {
                CurrentGameStatus = GameStatus.RUNNING;
            }
            else if (CurrentGameStatus == GameStatus.RUNNING)
            {
                CurrentGameStatus = GameStatus.PAUSED;
            }
            Time.timeScale = CurrentGameStatus == GameStatus.PAUSED ? 0 : 1;
            UIManager.Instance.OpenPauseUI(CurrentGameStatus == GameStatus.PAUSED);

        }

        public void LeaveGame()
        {
            Destroy(PlayerDataManager.Instance.gameObject);
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
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
                OnGameEnd((PlayerState)BoardManager.Instance.GetCurrentPlayer());
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
                        UIManager.Instance.UpdatePlayerCard(playerStates[i]);
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
                    // Debug.Log($"UpdatePlayerIsAlive {playerStates[i].PlayerName} Score-{playerStates[i].Score}");
                    if (playerStates[i].Score < 1)
                    {
                        Debug.Log($"Player dies {playerStates[i].PlayerName} ({playerStates[i].ClientId})");
                        playerStates[i] = new PlayerState(
                            playerStates[i].ClientId,
                            playerStates[i].PlayerName.ToString(),
                            playerStates[i].PlayerColor,
                            playerStates[i].Score,
                            false,
                            playerStates[i].HasDoneFirstAction
                        );
                        UIManager.Instance.DisablePlayerCard(playerStates[i]);
                    }
                }
            }
        }
    }
}