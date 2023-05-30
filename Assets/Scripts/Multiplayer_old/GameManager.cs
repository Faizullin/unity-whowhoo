using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Multiplayer_old
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public GameObject PauseMenuPanel;
        public GameObject GameEndPanel;
        public GameObject StartSettingsPanel;
        public GameObject PlayerStatsPanel;

        public GameObject _playerStatsItemPrefab;
        public TMP_InputField _playerNameInputPrefab;
        public Transform _playerNameInputContainer;
        public Button plusButton;
        private List<TMP_InputField> playerNameInputs = new List<TMP_InputField>();

        public TextMeshProUGUI WinnerText;
        public bool IsPaused;


        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            IsPaused = false;
            //PauseMenuPanel.SetActive(false);
            //GameEndPanel.SetActive(false);
            //OpenStartSettings();
        }

        public void OpenStartSettings()
        {
            IsPaused = true;
            Time.timeScale = 0;
            StartSettingsPanel.SetActive(true);

            plusButton.onClick.AddListener(AddPlayerNameInput);

            Button startButton = StartSettingsPanel.transform.Find("StartButton").GetComponent<Button>();
            startButton.onClick.AddListener(StartGame);
            AddPlayerNameInput();
        }

        public void StartGame()
        {
            //StartSettingsPanel.SetActive(false);
            //List<Player> players = new List<Player>();
            //List<Color> playerColors = new List<Color>()
            //{
            //    Color.blue,
            //    Color.green,
            //    Color.yellow,
            //    Color.red,
            //};
            //int id = 0;
            //foreach (TMP_InputField playerNameInput in playerNameInputs)
            //{
            //    players.Add(new Player(id, playerNameInput.text, playerColors[id]));
            //    id++;
            //}
            //IsPaused = false;
            //Time.timeScale = 1;
            //UpdatePlayerStats(players);
            //BoardManager.instance.StartGame(players);
        }

        public void AddPlayerNameInput()
        {
            TMP_InputField playerNameInputObject = Instantiate<TMP_InputField>(_playerNameInputPrefab, _playerNameInputContainer);
            playerNameInputs.Add(playerNameInputObject);
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
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0 : 1;
            PauseMenuPanel.SetActive(IsPaused);
        }
        public void ResumeGame()
        {
            Time.timeScale = 1;
            PauseMenuPanel.SetActive(false);
            IsPaused = false;
        }

        public void OnGameRestart()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        public void LeaveGame()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}