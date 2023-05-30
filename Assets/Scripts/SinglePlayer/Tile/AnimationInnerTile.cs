using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer.Tiles
{
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

