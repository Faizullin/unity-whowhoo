using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer_old.Lobby.UI
{
    public class LobbyPlayerCard : MonoBehaviour
    {
        [Header("Panels")]
        //[SerializeField] private GameObject waitingForPlayerPanel;
        [SerializeField] private GameObject playerDataPanel;

        [Header("Data Display")]
        [SerializeField] private TextMeshProUGUI playerDisplayNameText;
        //[SerializeField] private Image selectedCharacterImage;
        //[SerializeField] private Toggle isReadyToggle;

        public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
        {
            playerDisplayNameText.SetText(lobbyPlayerState.PlayerName.ToString());
            //playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
            //isReadyToggle.isOn = lobbyPlayerState.IsReady;

            //waitingForPlayerPanel.SetActive(false);
            playerDataPanel.SetActive(true);
        }

        public void DisableDisplay()
        {
            //waitingForPlayerPanel.SetActive(true);
            playerDataPanel.SetActive(false);
        }
    }
}
