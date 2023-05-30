using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    //[SerializeField]
    //private Animator m_menuAnimator;

    [SerializeField]
    private CharacterDataSO[] m_characterDatas;

    //[SerializeField]
    //private AudioClip m_confirmClip;

    private bool m_pressAnyKeyActive = true;
    // private const string k_enterMenuTriggerAnim = "enter_menu";

    [SerializeField]
    private SceneName nextScene = SceneName.SingleplayerCharacterSelectionScene;

    [SerializeField]
    private GameObject[] m_mapButtons;

    [SerializeField]
    private GameObject m_mapListUI;
    [SerializeField]
    private GameObject m_unetListUI;

    [SerializeField]
    private GameObject m_mapLoadButtonPrefab;

    private IEnumerator Start()
    {
        // -- To test with latency on development builds --
        // To set the latency, jitter and packet-loss percentage values for develop builds we need
        // the following code to execute before NetworkManager attempts to connect (changing the
        // values of the parameters as desired).
        //
        // If you'd like to test without the simulated latency, just set all parameters below to zero(0).
        //
        // More information here:
        // https://docs-multiplayer.unity3d.com/netcode/current/tutorials/testing/testing_with_artificial_conditions#debug-builds
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().
            SetDebugSimulatorParameters(
                packetDelay: 50,
                packetJitter: 5,
                dropRate: 3);
#endif

        ClearAllCharacterData();
        UIInit();

        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        // Set the events on the loading manager
        // Doing this because every time the network session ends the loading manager stops
        // detecting the events
        LoadingSceneManager.Instance.Init();
    }

    private void UIInit()
    {
        m_mapListUI.SetActive(false);
        m_unetListUI.SetActive(false);
    }


    private void Update()
    {
        if (m_pressAnyKeyActive)
        {
            if (Input.anyKey)
            {
                TriggerMainMenuTransitionAnimation();

                m_pressAnyKeyActive = false;
            }
        }
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
        if (startHost)
        {
            NetworkManager.Singleton.StartHost();
            LoadingSceneManager.Instance.LoadScene(SceneName.MultiplayerCharacterSelectionScene);
        }
        else
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.SingleplayerCharacterSelectionScene, false);
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
    public void OnClickOpenUnet()
    {
        m_unetListUI.SetActive(!m_unetListUI.activeSelf);
    }
    public void OnClickOpenMapEditor()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.TileMapEditorScene, false);
    }

    public void OnClickHost()
    {
        NetworkManager.Singleton.StartHost();
        // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        LoadingSceneManager.Instance.LoadScene(nextScene);
    }

    public void OnClickJoin()
    {
        // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        StartCoroutine(Join());
    }

    public void OnClickQuit()
    {
        // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        Application.Quit();
    }

    private void ClearAllCharacterData()
    {
        // Clean the all the data of the characters so we can start with a clean slate
        foreach (CharacterDataSO data in m_characterDatas)
        {
            data.EmptyData();
        }
    }

    private void TriggerMainMenuTransitionAnimation()
    {
        // m_menuAnimator.SetTrigger(k_enterMenuTriggerAnim);
        // AudioManager.Instance.PlaySoundEffect(m_confirmClip);
    }

    // We use a coroutine because the server is the one who makes the load
    // we need to make a fade first before calling the start client
    private IEnumerator Join()
    {
        // LoadingFadeEffect.Instance.FadeAll();

        // yield return new WaitUntil(() => LoadingFadeEffect.s_canLoad);
        yield return new WaitUntil(() => true);
        NetworkManager.Singleton.StartClient();
    }
}