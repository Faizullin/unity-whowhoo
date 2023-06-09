using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Singleplayer.Player.UI
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_playerNameText;
        [SerializeField]
        private TextMeshProUGUI m_playerScoreText;

        public void InitDisplay(PlayerState playerState)
        {
            m_playerNameText.text = playerState.PlayerName;
            m_playerNameText.color = playerState.PlayerColor;
            m_playerScoreText.text = playerState.Score.ToString();
            m_playerScoreText.color = playerState.PlayerColor;
        }

        public void UpdateScoreDisplay(int playerScore)
        {
            m_playerScoreText.text = playerScore.ToString();
        }

        public void DisableDisplay()
        {
            m_playerScoreText.color = Color.gray;
            m_playerNameText.color = Color.gray;
        }
    }
}