using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.Player;

namespace TileMapEditor.Tiles
{
    public class InnerTile : MonoBehaviour
    {
        private Tile m_tile;
        public Direction direction;

        private void Awake()
        {
            m_tile = transform.parent.GetComponent<Tile>();
        }

        public Tile GetParentTile()
        {
            return m_tile;
        }
    }
}
    

