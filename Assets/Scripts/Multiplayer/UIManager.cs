using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DapperDino.UMT.Lobby.Networking;
using Multiplayer.Player;
using Multiplayer.UI;

namespace Multiplayer
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField]
        private GameObject m_pauseMenuUI;
        [SerializeField]
        private GameObject m_gameEndUI;
        [SerializeField]
        private GameObject m_playersCardListUI;
        [SerializeField]
        private GameObject m_pauseButton;

        private Dictionary<ulong, PlayerCard> m_playerCards = new();
        [SerializeField]
        private GameObject m_playerCardPrefab;

        [SerializeField]
        private GameObject m_winnerText;

        public void AddPlayerCard(PlayerState playerState)
        {
            GameObject newPlayerCard = Instantiate(m_playerCardPrefab);
            newPlayerCard.transform.SetParent(m_playersCardListUI.GetComponentInChildren<GridLayoutGroup>().transform, false);
            var newPlayerCardScriptComponent = newPlayerCard.GetComponent<PlayerCard>();
            newPlayerCardScriptComponent.InitDisplay(playerState);
            m_playerCards.Add(playerState.ClientId, newPlayerCardScriptComponent);
        }

        public void UpdatePlayerCard(PlayerState playerState, bool addOnNotExist = false)
        {
            // Debug.Log($"UIManager.UpdatePlayerCard {playerState.ClientId}, Score-{playerState.Score}");
            
            if (m_playerCards.ContainsKey(playerState.ClientId))
            {
                if (playerState.IsAlive)
                {
                    m_playerCards[playerState.ClientId].UpdateScoreDisplay(playerState.Score);
                }
                else
                {
                    m_playerCards[playerState.ClientId].UpdateScoreDisplay(playerState.Score);
                }
            }
            // Debug.Log($"UIManager.UpdatePlayerCard -> AddPlayerCard {playerState.ClientId}, Score-{playerState.Score}");
            if (addOnNotExist)
            {
                AddPlayerCard(playerState);
                return;
            }
            // Debug.Log($"Warning: UpdatePlayerCard: Player Card doea not exist for player - {playerState.ClientId}");
        }

        public void CloseAllUIPanels()
        {
            m_pauseMenuUI.SetActive(false);
            m_gameEndUI.SetActive(false);
        }

        public void OpenPauseUI(bool state = true)
        {
            m_pauseMenuUI.SetActive(state);
        }
        public void OpenGameEndUI(bool state = true, string message = "")
        {
            m_gameEndUI.SetActive(state);
            m_pauseMenuUI.SetActive(false);
            m_pauseButton.SetActive(false);
            m_winnerText.GetComponent<TextMeshProUGUI>().text = message;
        }

        public void OnClickPause()
        {
            GameManager.Instance.OnPauseClick();

        }
        public void OnClickLeave()
        {
            GameManager.Instance.LeaveGame();
        }
    }
}