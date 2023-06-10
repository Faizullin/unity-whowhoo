using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Singleplayer.Player;

namespace Singleplayer.Lobby.UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;
        [SerializeField] private Button startGameButton;
        [SerializeField] private int m_minPlayerEnterNumber = 2;

        private List<LobbyPlayerState> lobbyPlayers = new();

        private void Start()
        {
            var teamsDataList = GetComponent<TeamPicker>().TeamsDataList;
            if (teamsDataList.Count >= lobbyPlayerCards.Length)
            {
                for (int i = 0; i < lobbyPlayerCards.Length; i++)
                {
                    lobbyPlayerCards[i].InitDisplay(teamsDataList[i].TeamColor);
                    lobbyPlayers.Add(new LobbyPlayerState(
                        (ulong)i,
                        "",
                        false,
                        teamsDataList[i].TeamColor
                    ));
                }
            }
        }

        public void ToggleReady(int playerStateIndex)
        {
            if (playerStateIndex >= 0 && playerStateIndex < lobbyPlayerCards.Length)
            {
                lobbyPlayers[playerStateIndex] = new LobbyPlayerState(
                    (ulong)playerStateIndex, 
                    lobbyPlayers[playerStateIndex].PlayerName, 
                    !lobbyPlayers[playerStateIndex].IsReady,
                    lobbyPlayers[playerStateIndex].PlayerColor
                );
                lobbyPlayerCards[playerStateIndex].UpdateDisplay(lobbyPlayers[playerStateIndex]);
            }
        }

        private void StartGame()
        {
            List<LobbyPlayerState> readyPlayers = new();
            var teamsDataList = GetComponent<TeamPicker>().TeamsDataList;
            foreach (var item in lobbyPlayers)
            {
                if (item.IsReady)
                {
                    var newVal = new LobbyPlayerState(item.ClientId, item.PlayerName, item.IsReady, item.PlayerColor);
                    if (newVal.PlayerName.Trim().Length == 0)
                    {
                        newVal.PlayerName = teamsDataList[(int)item.ClientId].TeamName;
                    }
                    readyPlayers.Add(newVal);
                }
            }
            if(readyPlayers.Count < m_minPlayerEnterNumber)
            {
                return;
            }
            PlayerDataManager.Instance.SetLobbyPlayerData(readyPlayers);

            LoadingSceneManager.Instance.LoadScene(SceneName.SingleplayerScene, false);
        }

        public void OnLeaveClicked()
        {
            // GameNetPortal.Instance.RequestDisconnect();
        }

        public void OnStartGameClicked()
        {
            StartGame();
        }

    }
}