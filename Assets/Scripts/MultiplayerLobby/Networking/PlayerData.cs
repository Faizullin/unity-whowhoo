using UnityEngine;

namespace DapperDino.UMT.Lobby.Networking
{
    public struct PlayerData
    {
        public string PlayerName { get; private set; }
        public ulong ClientId { get; private set; }
        public Color PlayerColor { get; private set; }

        public PlayerData(string playerName, ulong clientId, Color playerColor)
        {
            PlayerName = playerName;
            ClientId = clientId;
            PlayerColor = playerColor;
        }
    }
}
