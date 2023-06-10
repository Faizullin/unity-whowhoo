using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer.Player {

    public class PlayerStateData : INetworkSerializable
    {
        public PlayerStateData(ulong playerId, string playerName, Color playerColor)
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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HasDoneFirstAction);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref PlayerColor);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref IsAlive);
        }
    }
}
