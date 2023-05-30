using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Multiplayer;
using UnityEngine.SceneManagement;

namespace Multiplayer.Player {

    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {

        public Dictionary<ulong, PlayerData> PlayersData = new()
        {
            { 1, new PlayerData(1, "Player 1", Color.red) },
            { 2, new PlayerData(2, "Player 2", Color.blue) },
        };

        public List<ulong> GetSortedKeys()
        {
            List<ulong> sortedKeys = new(PlayersData.Keys);
            sortedKeys.Sort();
            return sortedKeys;
        }
    }
}
