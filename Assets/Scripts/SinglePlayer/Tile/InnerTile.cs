using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.Player;

namespace Singleplayer.Tiles
{
    public class InnerTile : MonoBehaviour
    {
        private Tile m_tile;
        private SpriteRenderer m_spriteRenderer;
        public Direction direction;

        public bool IsSelectedByPlayer = false;

        public Color HoverColor = Color.gray;
        public Color OriginalColor;


        private void Awake()
        {
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            OriginalColor = m_spriteRenderer.color;
            m_tile = transform.parent.GetComponent<Tile>();
        }

        private void OnMouseDown()
        {
            if (!IsSelectedByPlayer)
            {
                var tmp = this.GetComponent<InnerTile>();
                BoardManager.Instance.OnInnerTileClick(tmp, m_tile);
            }
        }
        private void OnMouseEnter()
        {
            if (!IsSelectedByPlayer)
            {
                m_spriteRenderer.color = HoverColor;
            }
        }

        private void OnMouseExit()
        {
            if (!IsSelectedByPlayer)
            {
                m_spriteRenderer.color = OriginalColor;
            }
        }

        public Tile GetParentTile()
        {
            return m_tile;
        }

        public void SelectStateByUser(PlayerData player)
        {
            m_spriteRenderer.color = player.PlayerColor;
            IsSelectedByPlayer = true;
        }

        public void ClearSelect()
        {
            m_spriteRenderer.color = OriginalColor;
            IsSelectedByPlayer = false;
        }
    }
}
    

