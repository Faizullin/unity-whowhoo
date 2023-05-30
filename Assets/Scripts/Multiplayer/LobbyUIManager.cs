using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Multiplayer.Player;

namespace Multiplayer.CharacterSelection
{

    public class LobbyUIManager : SingletonNetwork<LobbyUIManager>
    {
        [SerializeField]
        private GameObject m_playerNameInput;
        [SerializeField]
        private GameObject m_startButton;
        [SerializeField]
        private GameObject m_characterContainersListUI;
        [SerializeField]
        private GameObject m_characterContainerPrefab;
        [SerializeField]
        private GameObject m_titleText;

        // Start is called before the first frame update
        void Start()
        {
            InitUI();
        }

        private void InitUI()
        {
            string mapPath = PlayerPrefs.GetString("MapPath");
            m_titleText.GetComponent<TextMeshProUGUI>().text = mapPath;
            PlayerPrefs.SetString("MapPath", mapPath);
            m_playerNameInput.GetComponent<TMP_InputField>().onValueChanged.AddListener(newValue =>
            {
                Debug.Log($"m_playerNameInput change OwnerClientId-{OwnerClientId} NetworkManager.Singleton.LocalClientId-{NetworkManager.Singleton.LocalClientId} IsLocalPlayer-{IsLocalPlayer} IsOwner-{IsOwner} ==> {newValue}");
                CharacterSelectionManager.Instance.SetPlayerName(newValue, NetworkManager.Singleton.LocalClientId);
            });
            if (!IsServer)
            {
                m_startButton.SetActive(false);
            }
        }

        public void UpdatePlayerDataUI(PlayerData playerData)
        {
            Debug.Log($"UpdatePlayerData {playerData.PlayerName}({playerData.PlayerId}) --> {playerData.PlayerColor} {IsLocalPlayer}");
            m_playerNameInput.GetComponentInChildren<TextMeshProUGUI>().color = playerData.PlayerColor;

            if((int)playerData.PlayerId + 1 <= m_characterContainersListUI.transform.childCount)
            {
                Transform characterContainer = m_characterContainersListUI.transform.GetChild((int)playerData.PlayerId);
                if (characterContainer != null)
                {
                    characterContainer.GetComponentInChildren<TextMeshProUGUI>().text = playerData.PlayerName;
                    characterContainer.GetComponentInChildren<TextMeshProUGUI>().color = playerData.PlayerColor;
                }
                else
                {
                    characterContainer = Instantiate(m_characterContainerPrefab).transform;
                    characterContainer.SetParent(m_characterContainersListUI.transform, false);
                    characterContainer.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerData.PlayerName;
                    characterContainer.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = playerData.PlayerColor;
                }
            }
            else
            {
                var newCharacterContainer = Instantiate(m_characterContainerPrefab).transform;
                newCharacterContainer.SetParent(m_characterContainersListUI.transform, false);
                newCharacterContainer.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerData.PlayerName;
                newCharacterContainer.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = playerData.PlayerColor;
            }
            
            //GridLayoutGroup characterContainersListGrid = m_characterContainersListUI.GetComponent<GridLayoutGroup>();
            //GameObject newCharacterContainer = Instantiate(m_characterContainerPrefab);
            //newCharacterContainer.transform.SetParent(m_characterContainersListUI.transform, false);
            //var inputTexts = newCharacterContainer.GetComponentInChildren<TMP_InputField>().GetComponentsInChildren<TextMeshProUGUI>();
            //foreach (var inputText in inputTexts)
            //{
            //    inputText.color = data.color;
            //}
            //newCharacterContainer.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(newValue =>
            //{
            //    data.characterName = newValue;
            //});
            //var deleteButton = newCharacterContainer.GetComponentInChildren<Button>();
            //deleteButton.onClick.AddListener(delegate { RemoveCharacter(data.playerId); });
        }
    }
}