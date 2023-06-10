using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Singleplayer.Lobby.UI
{
    public class LobbyPlayerCard : MonoBehaviour
    {
        [Header("Data Display")]
        [SerializeField] private TMP_InputField playerDisplayNameInputField;
        [SerializeField] private Image selectedCharacterImage;
        [SerializeField] private Toggle isReadyToggle;

        public void InitDisplay(Color selectColor)
        {

            playerDisplayNameInputField.text = "";
            selectedCharacterImage.color = selectColor;
            // isReadyToggle.isOn = false;
        }

        public void UpdateDisplay(LobbyPlayerState lobbyPlayerState)
        {
            isReadyToggle.isOn = lobbyPlayerState.IsReady;
        }
    }
}
