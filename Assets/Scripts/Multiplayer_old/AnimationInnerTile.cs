using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer_old
{
    public class AnimationInnerTile : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rb;
        public InnerTile FromInnerTile;
        public InnerTile ToInnerTile;

        public float TileSize = 2f;
        public Color SelectColor;
        public Vector3 direction;
        public Vector3 startPosition;
        public ulong PlayerId;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void StartMove()
        {
            float moveSpeed = 1f;
            _spriteRenderer.color = SelectColor;
            _rb.velocity = direction * moveSpeed;
        }
    }
}
