using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using TMPro;

using DapperDino.UMT.Lobby.Networking;

public class MainMenuManager : MonoBehaviour
{
    private bool m_pressAnyKeyActive = true;

    [SerializeField]
    private GameObject[] m_mapButtons;

    [SerializeField]
    private GameObject m_mapListUI;

    [SerializeField]
    private GameObject m_mapLoadButtonPrefab;

    [SerializeField]
    private TMP_InputField displayNameInputField;

    private IEnumerator Start()
    {
        InitUI();

        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        LoadingSceneManager.Instance.Init();
    }

    private void InitUI()
    {
        m_mapListUI.SetActive(false);
    }

    private void LoadMapList()
    {
        var mapsListContentUI = m_mapListUI.GetComponentInChildren<VerticalLayoutGroup>();
        while (mapsListContentUI.transform.childCount > 0)
        {
            Transform child = mapsListContentUI.transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        string[] mapNames =  LoadingMapManager.GetMapsList();

        foreach (string mapName in mapNames)
        {
            GameObject item = Instantiate(m_mapLoadButtonPrefab);
            item.transform.SetParent(mapsListContentUI.transform, false);
            var itemController = item.GetComponent<MapLoadButtonUIController>();
            itemController.Init(mapName, EditMap, RemoveMap, OnClickLoadMap);
        }
    }

    public void RemoveMap(string filename)
    {
        LoadingMapManager.DeleteMap(filename);
        LoadMapList();
    }

    public void EditMap(string filename)
    {
        PlayerPrefs.SetString("MapPath", filename);
        LoadingSceneManager.Instance.IsEdit = true;
        LoadingSceneManager.Instance.LoadScene(SceneName.TileMapEditorScene, false);
    }

    public void OnClickLoadMap(string map, bool startHost = false)
    {
        PlayerPrefs.SetString("MapPath", map);
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        if (startHost)
        {
            GameNetPortal.Instance.StartHost();
        }
        else
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.SingleplayerLobbyScene, false);
        }
    }

    public void OnClickOpenMapList()
    {
        if(!m_mapListUI.activeSelf)
        {
            LoadMapList();
            m_mapListUI.SetActive(true);
        } else
        {
            m_mapListUI.SetActive(false);
        }
    }

    public void OnClickOpenMapEditor()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.TileMapEditorScene, false);
    }

    public void OnClickStartClient()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        StartCoroutine(Join());
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    // We use a coroutine because the server is the one who makes the load
    // we need to make a fade first before calling the start client
    private IEnumerator Join()
    {

        // yield return new WaitUntil(() => LoadingFadeEffect.s_canLoad);
        yield return new WaitUntil(() => true);
        // NetworkManager.Singleton.StartClient();
        ClientGameNetPortal.Instance.StartClient();
    }
}