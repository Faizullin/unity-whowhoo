using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer.Player { 

    [System.Serializable]
    public class PlayerData
    {
        public PlayerData(ulong playerId, string playerName, Color playerColor)
        {
            PlayerColor = playerColor;
            PlayerName = playerName;
            PlayerId = playerId;
        }

        public bool HasDoneFirstAction = false;
        public ulong PlayerId;
        public string PlayerName;
        public Color PlayerColor;
        public int Score = 0;
        public bool IsAlive = true;
    }
}
