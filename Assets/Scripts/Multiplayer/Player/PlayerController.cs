using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer.Player
{
    public class PlayerController : NetworkBehaviour
    {

        [SerializeField]
        private NetworkVariable<int> m_playerId =
            new NetworkVariable<int>(-1);

        [SerializeField]
        private NetworkVariable<int> m_score =
           new NetworkVariable<int>(0);

        [SerializeField]
        private string m_playerName = "";

        [SerializeField]
        private Color m_playerColor = Color.white;

        private void Start()
        {
            gameObject.name = $"Player-{OwnerClientId}";
        }

        private void OnEnable()
        {
            m_playerId.OnValueChanged += OnPlayerIdSet;
            m_score.OnValueChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            m_playerId.OnValueChanged -= OnPlayerIdSet;
            m_score.OnValueChanged -= OnScoreChanged;
        }

        private void OnPlayerIdSet(int oldValue, int newValue)
        {
            //CharacterSelectionManager.Instance.SetPlayebleChar(newValue, newValue, IsOwner);

            //if (IsServer)
            //    m_charSelected.Value = newValue;
        }

        // Event call when server changes the network variable
        private void OnScoreChanged(int oldValue, int newValue)
        {
            // If I am not the owner, update the character selection UI
            //if (!IsOwner && HasAcharacterSelected())
            //    CharacterSelectionManager.Instance.SetCharacterUI(m_playerId.Value, newValue);
        }


        IEnumerator HostShutdown()
        {
            // Tell the clients to shutdown
            ShutdownClientRpc();

            // Wait some time for the message to get to clients
            yield return new WaitForSeconds(0.5f);

            // Shutdown server/host
            Shutdown();
        }

        // Shutdown the network session and load the menu scene
        void Shutdown()
        {
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }

        [ClientRpc]
        void ShutdownClientRpc()
        {
            if (IsServer)
                return;

            Shutdown();
        }
    }
}