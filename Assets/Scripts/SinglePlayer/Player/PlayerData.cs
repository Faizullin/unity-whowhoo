using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer.Player { 

    [System.Serializable]
    public class PlayerData
    {
        public PlayerData(ulong playerId, string playerName, Color playerColor)
        {
            this.PlayerColor = playerColor;
            this.PlayerName = playerName;
            this.PlayerId = playerId;
        }

        public bool HasDoneFirstAction = false;
        public ulong PlayerId;
        public string PlayerName;
        public Color PlayerColor;
        public int Score = 0;
        public bool IsAlive = true;
    }
}
