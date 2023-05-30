using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Multiplayer;
using Multiplayer.Player;

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

        // public static Action<ulong> OnPlayerDefeated;

        [SerializeField]
        private CharacterDataSO[] m_charactersData;

        [SerializeField]
        private PlayerUI[] m_playersUI;

        [SerializeField]
        private GameObject m_deathUI;

        private int m_numberOfPlayerConnected;
        private List<ulong> m_connectedClients = new();
        private List<PlayerShipController> m_playerShips = new();


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
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
            }
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
            Debug.Log($"OnPauseClick isLocalPlayer-{IsLocalPlayer} OwnerClientId-{OwnerClientId} IsOwner-{IsOwner}");
            if (CurrentGameStatus == GameStatus.PAUSED)
            {
                CurrentGameStatus = GameStatus.RUNNING;
            } else if (CurrentGameStatus == GameStatus.RUNNING)
            {
                CurrentGameStatus = GameStatus.PAUSED;
            }
            if (IsHost || IsServer)
            {
                Time.timeScale = CurrentGameStatus == GameStatus.PAUSED ? 0 : 1;
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
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene);
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


        [ClientRpc]
        private void SetPlayerUIClientRpc(int charIndex, string playerShipName)
        {
            // Not optimal, but this is only called one time per ship
            // We do this because we can not pass a GameObject in an RPC
            GameObject playerSpaceship = GameObject.Find(playerShipName);

            PlayerShipController playerShipController =
                playerSpaceship.GetComponent<PlayerShipController>();

            m_playersUI[m_charactersData[charIndex].playerId].SetUI(
                m_charactersData[charIndex].playerId,
                m_charactersData[charIndex].iconSprite,
                m_charactersData[charIndex].iconDeathSprite,
                playerShipController.health.Value,
                m_charactersData[charIndex].darkColor);

            // Pass the UI to the player
            playerShipController.playerUI = m_playersUI[m_charactersData[charIndex].playerId];
        }
        // So this method is called on the server each time a player enters the scene.
        // Because of that, if we create the ship when a player connects we could have a sync error
        // with the other clients because maybe the scene on the client is no yet loaded.
        // To fix this problem we wait until all clients call this method then we create the ships
        // for every client connected 
        public void ServerSceneInit(ulong clientId)
        {
            // Save the clients 
            m_connectedClients.Add(clientId);

            // Check if is the last client
            if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
                return;

            // For each client spawn and set UI
            foreach (var client in m_connectedClients)
            {
                int index = 0;

                foreach (CharacterDataSO data in m_charactersData)
                {
                    if (data.isSelected && data.clientId == client)
                    {
                        GameObject playerSpaceship =
                            NetworkObjectSpawner.SpawnNewNetworkObjectChangeOwnershipToClient(
                                data.spaceshipPrefab,
                                transform.position,
                                data.clientId,
                                true);

                        PlayerShipController playerShipController =
                            playerSpaceship.GetComponent<PlayerShipController>();
                        playerShipController.characterData = data;

                        m_playerShips.Add(playerShipController);
                        SetPlayerUIClientRpc(index, playerSpaceship.name);

                        m_numberOfPlayerConnected++;
                    }

                    index++;
                }
            }
        }
    }
}