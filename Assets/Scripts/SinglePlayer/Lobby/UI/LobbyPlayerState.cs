using System;
using Unity.Collections;
using UnityEngine;

namespace Singleplayer.Lobby.UI
{
    [Serializable]
    public struct LobbyPlayerState : IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;
        public String PlayerName;
        public bool IsReady;
        public Color PlayerColor;

        public LobbyPlayerState(ulong clientId, String playerName, bool isReady, Color playerColor)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
            PlayerColor = playerColor;
        }

        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                PlayerName.Equals(other.PlayerName) &&
                IsReady == other.IsReady;
        }
    }
}
