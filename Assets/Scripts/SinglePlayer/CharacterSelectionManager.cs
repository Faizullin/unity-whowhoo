using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Singleplayer.Player;
using Multiplayer.Connection;

/*
* Singleton to control the changes on the char sprites and the flow of the scene
*/

namespace Singleplayer.CharacterSelection
{

    public class CharacterSelectionManager : SingletonNetwork<CharacterSelectionManager>
    {
        public List<CharacterDataSO> charactersData;

        [SerializeField]
        GameObject m_readyButton;

        [SerializeField]
        GameObject m_cancelButton;

        [SerializeField]
        float m_timeToStartGame;

        [SerializeField]
        SceneName m_nextScene = SceneName.SingleplayerScene;

        [SerializeField]
        Color m_clientColor;

        [SerializeField]
        Color m_playerColor;

        [SerializeField]
        GameObject m_playerPrefab;

        [Header("Audio clips")]
        [SerializeField]
        AudioClip m_confirmClip;

        [SerializeField]
        AudioClip m_cancelClip;

        bool m_isTimerOn;
        float m_timer;

        private readonly Color k_selectedColor = new Color32(74, 74, 74, 255);

        private int maxCharactersNumber = 4;
        [SerializeField]
        private GameObject m_characterContainersListUI;
        [SerializeField]
        private GameObject m_characterContainerPrefab;

        [SerializeField]
        private GameObject m_titleText;

        private Dictionary<Color, bool> m_characterSelectColors = new Dictionary<Color, bool>()
    {
        { Color.blue, false },
        { Color.green, false },
        { Color.red, false },
        { Color.black, false },
    };
        private Color GetAvailableColor()
        {
            foreach (var key in m_characterSelectColors.Keys)
            {
                if (!m_characterSelectColors[key])
                {
                    return key;
                }
            }
            throw new Exception("Color not found (GetAvailableColor)");
        }

        void Start()
        {
            m_timer = m_timeToStartGame;
            InitUI();
        }

        private void InitUI()
        {
            string mapPath = PlayerPrefs.GetString("MapPath");
            m_titleText.GetComponent<TextMeshProUGUI>().text = mapPath;
            PlayerPrefs.SetString("MapPath", mapPath);
        }

        void Update()
        {
            if (!IsServer)
                return;

            if (!m_isTimerOn)
                return;

            m_timer -= Time.deltaTime;
            if (m_timer <= 0f)
            {
                m_isTimerOn = false;
                StartGame();
            }
        }

        void StartGame()
        {
            LoadingSceneManager.Instance.LoadScene(m_nextScene);
        }

        // Set the players UI button
        public void SetPlayerReadyUIButtons(bool isReady, int characterSelected)
        {
            if (isReady && !charactersData[characterSelected].isSelected)
            {
                m_readyButton.SetActive(false);
                m_cancelButton.SetActive(true);
            }
            else if (!isReady && charactersData[characterSelected].isSelected)
            {
                m_readyButton.SetActive(true);
                m_cancelButton.SetActive(false);
            }
        }

        public void OnClickAddCharacter()
        {
            if (charactersData.Count < maxCharactersNumber)
            {
                var newCharacterDataSO = ScriptableObject.CreateInstance<CharacterDataSO>();
                newCharacterDataSO.characterName = "";
                newCharacterDataSO.color = GetAvailableColor();
                newCharacterDataSO.playerId = charactersData.Count + 1;
                m_characterSelectColors[newCharacterDataSO.color] = true;
                charactersData.Add(newCharacterDataSO);
                AddCharacterContainerUI(newCharacterDataSO);
            }
        }

        private void RemoveCharacter(int playerId)
        {
            CharacterDataSO characterToRemove = charactersData.Find(character => character.playerId == playerId);
            if (characterToRemove != null)
            {
                m_characterSelectColors[characterToRemove.color] = false;
                charactersData.Remove(characterToRemove);
                Destroy(m_characterContainersListUI.transform.GetChild(characterToRemove.playerId - 1).gameObject);
            }
            else
            {
                Debug.Log("Player with ID " + playerId + " does not exist.");
            }
        }

        private void AddCharacterContainerUI(CharacterDataSO data)
        {
            GridLayoutGroup characterContainersListGrid = m_characterContainersListUI.GetComponent<GridLayoutGroup>();
            GameObject newCharacterContainer = Instantiate(m_characterContainerPrefab);
            newCharacterContainer.transform.SetParent(m_characterContainersListUI.transform, false);
            var inputTexts = newCharacterContainer.GetComponentInChildren<TMP_InputField>().GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var inputText in inputTexts)
            {
                inputText.color = data.color;
            }
            newCharacterContainer.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(newValue =>
            {
                data.characterName = newValue;
            });
            var deleteButton = newCharacterContainer.GetComponentInChildren<Button>();
            deleteButton.onClick.AddListener(delegate { RemoveCharacter(data.playerId); });
        }

        private void SavePlayerData()
        {
            Dictionary<ulong, PlayerData> playersData = new();
            foreach (var characterData in charactersData)
            {
                playersData.Add(
                    (ulong)characterData.playerId,
                    new PlayerData(
                        (ulong)characterData.playerId,
                        characterData.characterName,
                        characterData.color
                    )
                );
            }
            PlayerDataManager.Instance.PlayersData = playersData;
        }

        public void OnClickStartGame()
        {
            SavePlayerData();
            LoadingSceneManager.Instance.LoadScene(m_nextScene, false);
        }
        public void OnClickQuit()
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }
    }
}