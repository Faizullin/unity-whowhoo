using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMapEditor.Tiles
{
    public class Tile : MonoBehaviour
    {
        private Dictionary<Direction, InnerTile> m_innerTiles = new();
        [SerializeField] private InnerTile m_innerTilePrefab;

        private float m_gridSize = 4f;

        public bool IsFullOfInnerTiles = false;
        public bool HasInnerTiles = true;
        public ulong PlayerId = 0;

        public Dictionary<Direction, Tile> GetNeighbourTiles()
        {
            Dictionary<Direction, Tile> foundTiles = new();
            foreach (Direction key in m_innerTiles.Keys)
            {
                foundTiles.Add(key, GetNeighbourTile(key));
            }
            return foundTiles;
        }

        public Dictionary<Direction, Tile> GetNeighbourTilesByDirections(List<Direction> dirs)
        {
            Dictionary<Direction, Tile> foundTiles = new();
            foreach (Direction key in dirs)
            {
                var tmp = GetNeighbourTile(key);
                if (tmp != null)
                {
                    foundTiles.Add(key, tmp);
                }
            }
            return foundTiles;
        }

        public Tile GetNeighbourTile(Direction direction)
        {
            Vector2 vectorDirection = Directions.GetVector2FromDirection(direction);
            Vector2 coords = (Vector2)transform.position + vectorDirection * m_gridSize;
            RaycastHit2D hit = Physics2D.Raycast(coords, Vector2.zero);
            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    return tile;
                }
            }
            return null;
        }

        public InnerTile GetInnerTileByDirection(Direction direction)
        {
            return m_innerTiles[direction];
        }

        public Dictionary<Direction, InnerTile> GetInnerTiles()
        {
            return m_innerTiles;
        }

        public void RemoveInnerTile(Direction key)
        {
            if (m_innerTiles.ContainsKey(key))
            {
                Destroy(m_innerTiles[key].gameObject);
                m_innerTiles.Remove(key);
            }
        }
        public void AddInnerTileInDirection(Direction dir)
        {
            Vector2 innerTilePos = transform.position + (Vector3)Directions.GetVector2FromDirection(dir);
            InnerTile innerTile = Instantiate(m_innerTilePrefab, innerTilePos, Quaternion.identity, transform);
            innerTile.direction = dir;
            var tmp = innerTile.transform.position;
            tmp.z = -1;
            innerTile.transform.position = tmp;
            if (m_innerTiles.ContainsKey(dir))
            {
                Debug.Log($"Warning: AddInnerTileInDirection: InnerTile already sxists ({dir})");
                m_innerTiles[dir] = innerTile;
            }
            else
            {
                m_innerTiles.Add(dir, innerTile);
            }
        }
    }
}
