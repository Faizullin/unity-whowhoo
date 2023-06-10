using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer;
using Singleplayer.Lobby.UI;

namespace Singleplayer.Player {

    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {

        public Dictionary<ulong, PlayerState> PlayersData = new()
        {
            { 1, new PlayerState(1, "P-1", Color.red, 0, true, false) },
            { 2, new PlayerState(2, "P-2", Color.blue, 0, true, false) },
        };

        public List<ulong> GetSortedKeys()
        {
            List<ulong> sortedKeys = new(PlayersData.Keys);
            sortedKeys.Sort();
            return sortedKeys;
        }

        public void SetLobbyPlayerData(List<LobbyPlayerState> lobbyPlayerStates)
        {
            PlayersData.Clear();
            foreach (var item in lobbyPlayerStates)
            {
                PlayersData.Add(item.ClientId, new PlayerState(item.ClientId, item.PlayerName, item.PlayerColor, 0, true, false));
            }
        }
    }
}
