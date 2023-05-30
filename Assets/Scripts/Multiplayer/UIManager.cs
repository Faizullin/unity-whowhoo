using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Multiplayer.Player;

namespace Multiplayer
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField]
        private GameObject m_pauseMenuUI;
        [SerializeField]
        private GameObject m_gameEndUI;
        [SerializeField]
        private GameObject m_playerStatsUI;
        [SerializeField]
        private GameObject m_pauseButton;

        [SerializeField]
        private GameObject m_playerStatsItemPrefab;

        [SerializeField]
        private GameObject m_winnerText;

        private void Start()
        {
            CloseAllUIPanels();
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
        public void UpdatePlayerStats()
        {
            var playersData = PlayerDataManager.Instance.PlayersData;
            var contentGrid = m_playerStatsUI.GetComponentInChildren<GridLayoutGroup>();
            while (contentGrid.transform.childCount > 0)
            {
                Transform child = contentGrid.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            foreach (var playerData in playersData.Values)
            {
                var playerStatsItem = Instantiate(m_playerStatsItemPrefab);

                playerStatsItem.transform.SetParent(contentGrid.transform, false);
                playerStatsItem.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = playerData.PlayerName;
                playerStatsItem.transform.Find("PlayerScore").GetComponent<TextMeshProUGUI>().text = playerData.Score.ToString();
            }
        }
    }
}