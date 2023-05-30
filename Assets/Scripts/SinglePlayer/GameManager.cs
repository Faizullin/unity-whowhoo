using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private void Start()
        {
            SceneInit();
        }

        public void SceneInit()
        {
            CurrentGameStatus = GameStatus.RUNNING;
            StartGame();
        }
        public void StartGame()
        {
            var playersData = PlayerDataManager.Instance.PlayersData;
            if(playersData.Count < 2)
            {
                LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene ,false);
                return;
            }
            UIManager.Instance.CloseAllUIPanels();
            UpdatePlayerStats();
            BoardManager.Instance.StartGame(playersData);
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
            Time.timeScale = CurrentGameStatus == GameStatus.PAUSED ? 0 : 1;
            UIManager.Instance.OpenPauseUI(CurrentGameStatus == GameStatus.PAUSED);

        }
        public void ResumeGame()
        {
            Time.timeScale = 1;
            UIManager.Instance.OpenPauseUI(false);
            CurrentGameStatus = GameStatus.RUNNING;
        }
        public void OnGameEnd(PlayerData player)
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
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }
        public void UpdatePlayerStats()
        {
            PlayersHealthUpdate();
            UIManager.Instance.UpdatePlayerStats();
        }
        private void PlayersHealthUpdate()
        {
            foreach (PlayerData playerData in PlayerDataManager.Instance.PlayersData.Values)
            {
                if (playerData.IsAlive && playerData.HasDoneFirstAction)
                {
                    if (playerData.Score < 1)
                    {
                        Debug.Log($"Player dies {playerData.PlayerName}");
                        playerData.IsAlive = false;
                    }
                }
            }
        }

        public bool CheckForWinner()
        {
            int alivePlayersCount = 0;
            foreach (PlayerData playerData in PlayerDataManager.Instance.PlayersData.Values)
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
                OnGameEnd(BoardManager.Instance.GetCurrentPlayer());
            }
        }
    }
}