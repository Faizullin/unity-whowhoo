using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace DapperDino.UMT.Lobby.UI
{
    [Serializable]
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public bool IsReady;
        public Color PlayerColor;

        public LobbyPlayerState(ulong clientId, FixedString32Bytes playerName, bool isReady, Color playerColor)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
            PlayerColor = playerColor;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref PlayerColor);
        }

        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                PlayerName.Equals(other.PlayerName) &&
                IsReady == other.IsReady;
        }
    }
}
