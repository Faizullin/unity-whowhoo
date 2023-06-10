using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Multiplayer.Player
{
    public struct PlayerState : INetworkSerializable, IEquatable<PlayerState>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public Color PlayerColor;
        public int Score;
        public bool IsAlive;
        public bool HasDoneFirstAction;

        public PlayerState(ulong clientId, string playerName, Color playerColor, int score, bool isAlive, bool hasDoneFirstAction)
        {
            ClientId = clientId;
            PlayerName = playerName;
            PlayerColor = playerColor;
            Score = score;
            IsAlive = isAlive;
            HasDoneFirstAction = hasDoneFirstAction;
        }

        public PlayerState(PlayerState oldPlayerState)
        {
            ClientId = oldPlayerState.ClientId;
            PlayerName = oldPlayerState.PlayerName;
            PlayerColor = oldPlayerState.PlayerColor;
            Score = oldPlayerState.Score;
            IsAlive = oldPlayerState.IsAlive;
            HasDoneFirstAction = oldPlayerState.HasDoneFirstAction;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsAlive);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref PlayerColor);
            serializer.SerializeValue(ref HasDoneFirstAction);
        }

        public bool Equals(PlayerState other)
        {
            return ClientId == other.ClientId &&
                PlayerName.Equals(other.PlayerName) &&
                IsAlive == other.IsAlive &&
                Score == other.Score &&
                PlayerColor.Equals(other.PlayerColor);
        }
    }
}