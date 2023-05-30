using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Multiplayer.Player;
using Multiplayer.Connection;



namespace Multiplayer.CharacterSelection
{
    public enum ConnectionState : byte
    {
        connected,
        disconnected,
        ready
    }

    // Struct for better serialization on the player connection
    [Serializable]
    public struct PlayerConnectionState
    {
        public ConnectionState playerState;             // State of the player
        public PlayerCharSelection playerObject;        // The NetworkObject of the client use for the disconnection of the client
        public string playerName;                       // The name of the player when spawn
        public ulong clientId;                          // Id of the client
    }

    // Struct for better serialization on the container of the character
    //[Serializable]
    //public struct CharacterContainer
    //{
    //    public Image imageContainer;                    // The image of the character container
    //    public TextMeshProUGUI nameContainer;           // Character name container
    //    public GameObject border;                       // The border of the character container when not ready
    //    public GameObject borderReady;                  // The border of the character container when ready
    //    public GameObject borderClient;                 // Client border of the character container
    //    public Image playerIcon;                        // The background icon of the player (p1, p2)
    //    public GameObject waitingText;                  // The waiting text on the container were no client connected
    //    public GameObject backgroundShip;               // The background of the ship when not ready
    //    public Image backgroundShipImage;               // The image of the ship when not ready
    //    public GameObject backgroundShipReady;          // The background of the ship when ready
    //    public Image backgroundShipReadyImage;          // The image of the ship when ready
    //    public GameObject backgroundClientShipReady;    // Client background of the ship when ready
    //    public Image backgroundClientShipReadyImage;    // Client image of the ship when ready
    //}

    [Serializable]
    public struct CharacterContainer
    {
        public Image imageContainer;                    // The image of the character container
        public TextMeshProUGUI nameContainer;           // Character name container
        public GameObject border;                       // The border of the character container when not ready
        public GameObject borderReady;                  // The border of the character container when ready
        public GameObject borderClient;                 // Client border of the character container
        public Image playerIcon;                        // The background icon of the player (p1, p2)
        public GameObject waitingText;                  // The waiting text on the container were no client connected
        public GameObject backgroundShip;               // The background of the ship when not ready
        public Image backgroundShipImage;               // The image of the ship when not ready
        public GameObject backgroundShipReady;          // The background of the ship when ready
        public Image backgroundShipReadyImage;          // The image of the ship when ready
        public GameObject backgroundClientShipReady;    // Client background of the ship when ready
        public Image backgroundClientShipReadyImage;    // Client image of the ship when ready
    }

    public class CharacterSelectionManager : SingletonNetwork<CharacterSelectionManager>
    {
        public List<CharacterDataSO> charactersData;

        [SerializeField]
        GameObject m_cancelButton;

        [SerializeField]
        float m_timeToStartGame;

        [SerializeField]
        SceneName m_nextScene = SceneName.MultiplayerScene;

        [SerializeField]
        List<PlayerConnectionState> m_playerStates;

        [SerializeField]
        GameObject m_playerPrefab;

        bool m_isTimerOn;
        float m_timer;

        private int maxCharactersNumber = 4;
        [SerializeField]
        private GameObject m_characterContainersListUI;
        private CharacterContainer[] m_charactersContainers;
        [SerializeField]
        private GameObject m_characterContainerPrefab;

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

        [SerializeField]
        private GameObject m_nameInput;

        void Start()
        {
            m_timer = m_timeToStartGame;
        }

        //private void AddCharacterContainerUI(CharacterDataSO data)
        //{
        //    GridLayoutGroup characterContainersListGrid = m_characterContainersListUI.GetComponent<GridLayoutGroup>();
        //    GameObject newCharacterContainer = Instantiate(m_characterContainerPrefab);
        //    newCharacterContainer.transform.SetParent(m_characterContainersListUI.transform, false);

        //    foreach (var inputText in inputTexts)
        //    {
        //        inputText.color = data.color;
        //    }
        //    newCharacterContainer.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(newValue =>
        //    {
        //        data.characterName = newValue;
        //    });
        //    var deleteButton = newCharacterContainer.GetComponentInChildren<Button>();
        //    deleteButton.onClick.AddListener(delegate { RemoveCharacter(data.playerId); });
        //}

        void OnDisable()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnects;
            }
        }

        void StartGame()
        {
            StartGameClientRpc();
            LoadingSceneManager.Instance.LoadScene(m_nextScene);
        }

        [ClientRpc]
        void StartGameClientRpc()
        {
            // LoadingFadeEffect.Instance.FadeAll();
        }

        void RemoveSelectedStates()
        {
            for (int i = 0; i < charactersData.Count; i++)
            {
                charactersData[i].isSelected = false;
            }
        }

        void RemoveReadyStates(ulong clientId, bool disconected)
        {
            for (int i = 0; i < m_playerStates.Count; i++)
            {
                var m_playerState = m_playerStates[i];
                if (m_playerState.playerState == ConnectionState.ready &&
                    m_playerState.clientId == clientId)
                {

                    if (disconected)
                    {
                        m_playerState.playerState = ConnectionState.disconnected;
                        UpdatePlayerStateClientRpc(clientId, i, ConnectionState.disconnected);
                    }
                    else
                    {
                        m_playerState.playerState = ConnectionState.connected;
                        UpdatePlayerStateClientRpc(clientId, i, ConnectionState.connected);
                    }
                }
            }
        }

        [ClientRpc]
        void UpdatePlayerStateClientRpc(ulong clientId, int stateIndex, ConnectionState state)
        {
            if (IsServer)
                return;
            var m_playerState = m_playerStates[stateIndex];
            m_playerState.playerState = state;
            m_playerState.clientId = clientId;
        }

        void StartGameTimer()
        {
            foreach (PlayerConnectionState state in m_playerStates)
            {
                // If a player is connected (not ready)
                if (state.playerState == ConnectionState.connected)
                    return;
            }

            // If all players connected are ready
            m_timer = m_timeToStartGame;
            m_isTimerOn = true;
        }

        void SetNonPlayableChar(int playerId)
        {
            //m_charactersContainers[playerId].imageContainer.sprite = null;
            //m_charactersContainers[playerId].imageContainer.color = new Color(1f, 1f, 1f, 0f);
            //m_charactersContainers[playerId].nameContainer.text = "";
            //m_charactersContainers[playerId].border.SetActive(true);
            //m_charactersContainers[playerId].borderClient.SetActive(false);
            //m_charactersContainers[playerId].borderReady.SetActive(false);
            //m_charactersContainers[playerId].playerIcon.gameObject.SetActive(false);
            //m_charactersContainers[playerId].playerIcon.color = m_playerColor;
            //m_charactersContainers[playerId].backgroundShip.SetActive(false);
            //m_charactersContainers[playerId].backgroundShipReady.SetActive(false);
            //m_charactersContainers[playerId].backgroundClientShipReady.SetActive(false);
            //m_charactersContainers[playerId].waitingText.SetActive(true);
        }

        public bool IsReady(int playerId)
        {
            return charactersData[playerId].isSelected;
        }

        public void SetCharacterUI(int playerId, int characterSelected)
        {
            Debug.Log($"SetCharacterUI {playerId}  ({characterSelected})");
            //m_charactersContainers[playerId].imageContainer.sprite =
            //    charactersData[characterSelected].characterSprite;

            //m_charactersContainers[playerId].backgroundShipImage.sprite =
            //    charactersData[characterSelected].characterShipSprite;

            //m_charactersContainers[playerId].backgroundShipReadyImage.sprite =
            //    charactersData[characterSelected].characterShipSprite;

            //m_charactersContainers[playerId].backgroundClientShipReadyImage.sprite =
            //    charactersData[characterSelected].characterShipSprite;

            //m_charactersContainers[playerId].nameContainer.text =
            //    charactersData[characterSelected].characterName;
            //GridLayoutGroup characterContainersListGrid = m_characterContainersListUI.GetComponent<GridLayoutGroup>();
            //m_characterContainersListUI.transform.GetChild(characterToRemove.playerId);
            //CharacterDataSO characterToUpdate = charactersData.Find(character => character.playerId == playerId);
            //if (characterToUpdate != null)
            //{
            //    m_characterSelectColors[characterToUpdate.color] = true;
            //    charactersData.Remove(characterToRemove);
            //    Destroy(m_characterContainersListUI.transform.GetChild(characterToRemove.playerId - 1).gameObject);
            //}
            //else
            //{
            //    Debug.Log("Player with ID " + playerId + " does not exist.");
            //}

            //foreach (var m_charactersContainers in m_charactersContainers)
            //{

            //    GameObject newCharacterContainer = Instantiate(m_characterContainerPrefab);
            //    newCharacterContainer.transform.SetParent(m_characterContainersListUI.transform, false);
            //    var inputTexts = newCharacterContainer.GetComponentInChildren<TMP_InputField>().GetComponentsInChildren<TextMeshProUGUI>();
            //    foreach (var inputText in inputTexts)
            //    {
            //        inputText.color = data.color;
            //    }
            //    newCharacterContainer.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(newValue =>
            //    {
            //        data.characterName = newValue;
            //    });
            //    var deleteButton = newCharacterContainer.GetComponentInChildren<Button>();
            //    deleteButton.onClick.AddListener(delegate { RemoveCharacter(data.playerId); });
            //}
            // SetCharacterColor(playerId, characterSelected);
        }

        public void SetPlayebleChar(int playerId, int characterSelected, bool isClientOwner)
        {
            SetCharacterUI(playerId, characterSelected);
            //m_charactersContainers[playerId].playerIcon.gameObject.SetActive(true);
            //if (isClientOwner)
            //{
            //    m_charactersContainers[playerId].borderClient.SetActive(true);
            //    m_charactersContainers[playerId].border.SetActive(false);
            //    m_charactersContainers[playerId].borderReady.SetActive(false);
            //    m_charactersContainers[playerId].playerIcon.color = m_clientColor;
            //}
            //else
            //{
            //    m_charactersContainers[playerId].border.SetActive(true);
            //    m_charactersContainers[playerId].borderReady.SetActive(false);
            //    m_charactersContainers[playerId].borderClient.SetActive(false);
            //    m_charactersContainers[playerId].playerIcon.color = m_playerColor;
            //}

            //m_charactersContainers[playerId].backgroundShip.SetActive(true);
            //m_charactersContainers[playerId].waitingText.SetActive(false);
        }

        public ConnectionState GetConnectionState(int playerId)
        {
            if (playerId != -1)
                return m_playerStates[playerId].playerState;

            return ConnectionState.disconnected;
        }

        public void ServerSceneInit(ulong clientId)
        {
            Debug.Log($"ServerSceneInit {clientId}");
            GameObject go =
                NetworkObjectSpawner.SpawnNewNetworkObjectChangeOwnershipToClient(
                    m_playerPrefab,
                    transform.position,
                    clientId,
                    true);


            if (m_playerStates.Count <= maxCharactersNumber)
            {
                var m_playerState = new PlayerConnectionState();
                m_playerState.playerState = ConnectionState.connected;
                m_playerState.playerObject = go.GetComponent<PlayerCharSelection>();
                m_playerState.playerName = go.name;
                m_playerState.clientId = clientId;
                m_playerStates.Add(m_playerState);
            }
            else
            {
                for (int i = 0; i < m_playerStates.Count; i++)
                {
                    var m_playerState = m_playerStates[i];
                    if (m_playerState.playerState == ConnectionState.disconnected)
                    {
                        m_playerState.playerState = ConnectionState.connected;
                        m_playerState.playerObject = go.GetComponent<PlayerCharSelection>();
                        m_playerState.playerName = go.name;
                        m_playerState.clientId = clientId;

                        // Force the exit
                        break;
                    }
                }
            }


            // Sync states to clients
            for (int i = 0; i < m_playerStates.Count; i++)
            {
                var m_playerState = m_playerStates[i];
                if (m_playerState.playerObject != null)
                    PlayerConnectsClientRpc(
                        m_playerState.clientId,
                        i,
                        m_playerState.playerState,
                        m_playerState.playerObject.GetComponent<NetworkObject>());
            }

        }

        [ClientRpc]
        void PlayerConnectsClientRpc(
            ulong clientId,
            int stateIndex,
            ConnectionState state,
            NetworkObjectReference player)
        {
            if (IsServer)
                return;

            if (state != ConnectionState.disconnected)
            {
                var m_playerState = m_playerStates[stateIndex];
                m_playerState.playerState = state;
                m_playerState.clientId = clientId;

                if (player.TryGet(out NetworkObject playerObject))
                    m_playerState.playerObject =
                        playerObject.GetComponent<PlayerCharSelection>();
            }
        }

        public void PlayerDisconnects(ulong clientId)
        {
            if (!ClientConnection.Instance.IsExtraClient(clientId))
                return;

            PlayerNotReady(clientId, isDisconected: true);

            m_playerStates[GetPlayerId(clientId)].playerObject.Despawn();

            // The client disconnected is the host
            if (clientId == 0)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        public void PlayerNotReady(ulong clientId, int characterSelected = 0, bool isDisconected = false)
        {
            int playerId = GetPlayerId(clientId);
            m_isTimerOn = false;
            m_timer = m_timeToStartGame;

            RemoveReadyStates(clientId, isDisconected);

            // Notify clients to change UI
            if (isDisconected)
            {
                PlayerDisconnectedClientRpc(playerId);
            }
            else
            {
                PlayerNotReadyClientRpc(clientId, playerId, characterSelected);
            }
        }

        public int GetPlayerId(ulong clientId)
        {
            for (int i = 0; i <= m_playerStates.Count; i++)
            {
                var m_playerState = m_playerStates[i];
                if (m_playerState.clientId == clientId)
                    return i;
            }

            //! This should never happen
            Debug.LogError("This should never happen");
            return -1;
        }

        // Set the player ready if the player is not selected and check if all player are ready to start the countdown
        public void PlayerReady(ulong clientId, int playerId, int characterSelected)
        {
            if (!charactersData[characterSelected].isSelected)
            {
                PlayerReadyClientRpc(clientId, playerId, characterSelected);

                StartGameTimer();
            }
        }

        // Check if the player has selected the character
        public bool IsSelectedByPlayer(int playerId, int characterSelected)
        {
            return charactersData[characterSelected].playerId == playerId ? true : false;
        }

        [ClientRpc]
        void PlayerReadyClientRpc(ulong clientId, int playerId, int characterSelected)
        {
            //charactersData[characterSelected].isSelected = true;
            //charactersData[characterSelected].clientId = clientId;
            //charactersData[characterSelected].playerId = playerId;

            //var m_playerState = m_playerStates[playerId];
            //m_playerState.playerState = ConnectionState.ready;

            //if (clientId == NetworkManager.Singleton.LocalClientId)
            //{
            //    m_charactersContainers[playerId].backgroundClientShipReady.SetActive(true);
            //    m_charactersContainers[playerId].backgroundShip.SetActive(false);
            //}
            //else
            //{
            //    m_charactersContainers[playerId].border.SetActive(false);
            //    m_charactersContainers[playerId].borderReady.SetActive(true);
            //    m_charactersContainers[playerId].backgroundShip.SetActive(false);
            //    m_charactersContainers[playerId].backgroundShipReady.SetActive(true);
            //}

            //for (int i = 0; i < m_playerStates.Count; i++)
            //{
            //    Only changes the ones on clients that are not selected
            //    m_playerState = m_playerStates[i];
            //    if (m_playerState.playerState == ConnectionState.connected)
            //    {
            //        if (m_playerState.playerObject.CharSelected == characterSelected)
            //        {
            //            SetCharacterColor(i, characterSelected);
            //        }
            //    }
            //}

            // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        }

        [ClientRpc]
        void PlayerNotReadyClientRpc(ulong clientId, int playerId, int characterSelected)
        {
            //charactersData[characterSelected].isSelected = false;
            //charactersData[characterSelected].clientId = 0UL;
            //charactersData[characterSelected].playerId = -1;

            //if (clientId == NetworkManager.Singleton.LocalClientId)
            //{
            //    m_charactersContainers[playerId].borderClient.SetActive(true);
            //    m_charactersContainers[playerId].backgroundClientShipReady.SetActive(false);
            //    m_charactersContainers[playerId].backgroundShip.SetActive(true);
            //}
            //else
            //{
            //    m_charactersContainers[playerId].border.SetActive(true);
            //    m_charactersContainers[playerId].borderReady.SetActive(false);
            //    m_charactersContainers[playerId].borderClient.SetActive(false);
            //    m_charactersContainers[playerId].backgroundShip.SetActive(true);
            //    m_charactersContainers[playerId].backgroundShipReady.SetActive(false);
            //}

            //AudioManager.Instance.PlaySoundEffect(m_cancelClip);
            //for (int i = 0; i < m_playerStates.Count; i++)
            //{
            //    // Only changes the ones on clients that are not selected
            //    var m_playerState = m_playerStates[i];
            //    if (m_playerState.playerState == ConnectionState.connected)
            //    {
            //        if (m_playerState.playerObject.CharSelected == characterSelected)
            //        {
            //            SetCharacterColor(i, characterSelected);
            //        }
            //    }
            //}
        }

        [ClientRpc]
        public void PlayerDisconnectedClientRpc(int playerId)
        {
            SetNonPlayableChar(playerId);

            // All character data unselected
            RemoveSelectedStates();

            var m_playerState = m_playerStates[playerId];
            m_playerState.playerState = ConnectionState.disconnected;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnects;
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
            if (!IsHost) return;
            // SavePlayerData();
            StartGame();
        }
        public void OnClickQuit()
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }

        public Dictionary<ulong, PlayerData> PlayersDataList = new();

        public void SetPlayerName(string newValue, ulong clientId)
        {
            SetPlayerDataServerRpc(clientId, newValue);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerDataServerRpc(ulong clientId, string newValue)
        {
            Debug.Log($"SetPlayerDataServerRpc {clientId} {newValue}");
            if (PlayersDataList.ContainsKey(clientId))
            {
                var playerData = PlayersDataList[clientId];
                playerData.PlayerName = newValue;
                SetPlayerDataClientRpc(clientId, playerData.PlayerName, playerData.PlayerColor);
            }
            else
            {
                var playerData = new PlayerData(clientId, newValue, GetAvailableColor());
                m_characterSelectColors[playerData.PlayerColor] = true;
                PlayersDataList.Add(clientId, playerData);
                SetPlayerDataClientRpc(clientId, playerData.PlayerName, playerData.PlayerColor);
            }
        }

        [ClientRpc]
        public void SetPlayerDataClientRpc(ulong clientId, string newValue, Color playerColor)
        {
            Debug.Log($"SetPlayerDataClientRpc {clientId} {newValue}");
            var playerData = new PlayerData(clientId, newValue, playerColor);
            if (PlayersDataList.ContainsKey(clientId))
            {
                playerData = PlayersDataList[clientId];
                playerData.PlayerName = newValue;
                playerData.PlayerColor = playerColor;
            }
            else
            {
                PlayersDataList.Add(clientId, playerData);
            }
            LobbyUIManager.Instance.UpdatePlayerDataUI(playerData);
        }
    }
}