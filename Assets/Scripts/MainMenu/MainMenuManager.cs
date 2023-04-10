using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        public Button[] maps;
        public Transform MapListPanel;

        public GameObject _mapLoadButtonPrefab;
        private string _mapsDirectory = "Maps";


        private void Awake()
        {
            MapListPanel.gameObject.SetActive(false);
        }

        public void OpenMapListEvent()
        {
            LoadMapListPanel();
            MapListPanel.gameObject.SetActive(!MapListPanel.gameObject.activeSelf);
        }

        public void OpenUnetListEvent()
        {
            //ServerListPanel.gameObject.SetActive(!ServerListPanel.gameObject.activeSelf);
        }

        public void LoadMap(string mapName)
        {
            PlayerPrefs.SetString("MapPath", mapName);
            SceneManager.LoadScene("SinglePlayerScene", LoadSceneMode.Single);
        }
        public void LoadMapListPanel()
        {
            GridLayoutGroup mapsListContentPanel = MapListPanel.GetComponentInChildren<GridLayoutGroup>();
            while (mapsListContentPanel.transform.childCount > 0)
            {
                Transform child = mapsListContentPanel.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
            if (!Directory.Exists(_mapsDirectory))
            {
                Debug.Log($"Directory for Maps does not exist!");
                Directory.CreateDirectory(_mapsDirectory);
            }
            string[] mapNames = GetMapNames();
            
            foreach (string mapName in mapNames)
            {
                GameObject button = Instantiate(_mapLoadButtonPrefab) as GameObject;
                button.transform.SetParent(mapsListContentPanel.transform, false);
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Map: " + mapName;
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { LoadMap(mapName); });
            }
        }

        public void StartSimpleGame()
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            //GameManager.gameType = GameType.Simple;
        }

        public void StartNetworkGame()
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            //GameManager.gameType = GameType.Network;
        }

        public void OpenMapEditor()
        {
            SceneManager.LoadScene("TileMapEditorScene", LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Application.Quit();
        }


        public void OnMutiplayerPanelOpenClick()
        {
            PopulateServerList();
        }
        void PopulateServerList()
        {
            //foreach (GameObject item in ServerListItems)
            //{
            //    Destroy(item);
            //}
            //ServerListItems.Clear();
            //string[] serverIPs = ScanForActiveServers();

            //// Instantiate a server list item for each active server found
            //foreach (string ip in serverIPs)
            //{
            //    GameObject newItem = Instantiate(ServerListItemPrefab, ServerListPanel);
            //    //newItem.GetComponentInChildren<TextMeshPro>().text = "Server " + ip; // Set the server name label to the IP address
            //    ServerListItems.Add(newItem);
            //}
        }

        public string[] GetMapNames()
        {
            string mapsFolder = Application.dataPath + "/Maps";
            string[] mapPaths = Directory.GetFiles(mapsFolder, "*.json");
            string[] mapNames = new string[mapPaths.Length];
            for (int i = 0; i < mapPaths.Length; i++)
            {
                mapNames[i] = Path.GetFileNameWithoutExtension(mapPaths[i]);
            }

            return mapNames;
        }
    }
}

