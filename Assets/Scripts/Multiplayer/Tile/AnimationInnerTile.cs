using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer.Tiles
{

    [System.Serializable]
    public class InnerTileData : INetworkSerializable
    {
        public int tileIndex;
        public Direction direction;

        public InnerTileData()
        {
            this.tileIndex = 0;
            this.direction = Direction.up;
        }

        public InnerTileData(int tileIndex, Direction direction)
        {
            this.tileIndex = tileIndex;
            this.direction = direction;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tileIndex);
            serializer.SerializeValue(ref direction);
        }

    }

    [System.Serializable]
    public class AnimationInnerTileData : INetworkSerializable
    {
        public Color SelectColor;
        public Vector3 MoveDirection;
        public ulong PlayerId;
        public InnerTileData FromInnerTileData;
        public InnerTileData ToInnerTileData = new();

        public AnimationInnerTileData()
        {
            this.FromInnerTileData = new InnerTileData(0, Direction.up);
            this.ToInnerTileData = new InnerTileData(0, Direction.up);
            this.SelectColor = Color.black;
            this.MoveDirection = Vector3.up;
            this.PlayerId = 0;
        }

        public AnimationInnerTileData(InnerTile FromInnerTile, InnerTile ToInnerTile, Color SelectColor, Vector3 MoveDirection, ulong PlayerId)
        {
            this.FromInnerTileData = new InnerTileData(FromInnerTile.GetParentTile().Index, FromInnerTile.direction);
            this.ToInnerTileData = new InnerTileData(ToInnerTile.GetParentTile().Index, ToInnerTile.direction);
            this.SelectColor = SelectColor;
            this.MoveDirection = MoveDirection;
            this.PlayerId = PlayerId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref SelectColor);
            serializer.SerializeValue(ref MoveDirection);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref FromInnerTileData);
            serializer.SerializeValue(ref ToInnerTileData);
        }
    }

    public class AnimationInnerTile : MonoBehaviour
    {
        private SpriteRenderer m_spriteRenderer;
        private Rigidbody2D m_rb;
        [SerializeField]
        private float m_moveSpeed = 1f;

        public InnerTile FromInnerTile;
        public InnerTile ToInnerTile;

        public Color SelectColor;
        public Vector3 MoveDirection;
        public ulong PlayerId;

        private void Awake()
        {
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_rb = GetComponent<Rigidbody2D>();
        }

        public void StartMove()
        {
            m_spriteRenderer.color = SelectColor;
            m_rb.velocity = MoveDirection * m_moveSpeed;
        }
    }
}

