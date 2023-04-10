using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

namespace Multiplayer
{
    public class NetworkUIPanel : MonoBehaviour
    {
        [SerializeField] private Button _clientButton;
        // Update is called once per frame
        public void OnStartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void OnStartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void OnStartServer()
        {
            NetworkManager.Singleton.StartServer();
        }
    }
}

