using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer_old
{
    public class Tile : NetworkBehaviour
    {
        public Dictionary<Direction, InnerTile> InnerTiles = new Dictionary<Direction, InnerTile>()
        {
        };
        [SerializeField] private int _rowIndex;
        [SerializeField] private int _colIndex;
        [SerializeField] private InnerTile _innerTilePrefab;

        public float TileSize = 4;
        public bool IsFullOfInnerTiles = false;
        public bool HasInnerTiles = true;
        public long PlayerId = -1;
        public int Index;

        private void Awake()
        {
            TileSize = GetComponent<Tile>().GetComponent<BoxCollider2D>().size.x;
        }
        public void CollectInnerTiles()
        {
            InnerTile[] TmpInnerTiles = GetComponentsInChildren<InnerTile>();
            foreach (InnerTile TmpInnerTile in TmpInnerTiles)
            {
                Vector3 TmpInnerTilePostion = TmpInnerTile.transform.position - transform.position;
                if (TmpInnerTilePostion.x > 0)
                {
                    TmpInnerTile.direction = Direction.right;
                }
                else if (TmpInnerTilePostion.x < 0)
                {
                    TmpInnerTile.direction = Direction.left;
                }
                else if (TmpInnerTilePostion.y > 0)
                {
                    TmpInnerTile.direction = Direction.up;
                }
                else if (TmpInnerTilePostion.y < 0)
                {
                    TmpInnerTile.direction = Direction.down;
                }
                if (InnerTiles.ContainsKey(TmpInnerTile.direction))
                {
                    InnerTiles[TmpInnerTile.direction] = TmpInnerTile;
                }
                else
                {
                    InnerTiles.Add(TmpInnerTile.direction, TmpInnerTile);
                }
            }
        }

        public void CheckForFullOfInnerTiles()
        {
            bool TmpAllInnerTilesActive = true;
            foreach (InnerTile innerTile in InnerTiles.Values)
            {
                if (!innerTile.IsSelectedByPlayer)
                {
                    TmpAllInnerTilesActive = false;
                    break;
                }
            }
            IsFullOfInnerTiles = TmpAllInnerTilesActive;
        }

        public Dictionary<Direction, Tile> GetNeighbourTiles()
        {
            Dictionary<Direction, Tile> foundTiles = new Dictionary<Direction, Tile>();
            foreach (Direction key in InnerTiles.Keys)
            {
                foundTiles.Add(key, GetNeighboourTile(key));
            }
            return foundTiles;
        }

        public Dictionary<Direction, Tile> GetNeighbourTilesByDirections(List<Direction> dirs)
        {
            Dictionary<Direction, Tile> foundTiles = new Dictionary<Direction, Tile>();
            foreach (Direction key in dirs)
            {
                var tmp = GetNeighboourTile(key);
                if (tmp != null)
                {
                    foundTiles.Add(key, tmp);
                }
            }
            return foundTiles;
        }

        public Tile GetNeighboourTile(Direction direction)
        {
            Vector2 vectorDirection = Directions.GetVector2FromDirection(direction);
            Vector2 coords = (Vector2)transform.position + vectorDirection * TileSize;
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
            return InnerTiles[direction];
        }
        public Dictionary<Direction, InnerTile> GetInnerTiles()
        {
            return InnerTiles;
        }
        public void ClearInnerTilesSelection()
        {
            if (PlayerId != -1)
            {
                foreach (InnerTile innerTile in InnerTiles.Values)
                {
                    if (innerTile.IsSelectedByPlayer)
                    {
                        innerTile.ClearSelect();
                    }

                }
                PlayerId = -1;
            }

        }

        public void RemoveInnerTile(Direction key)
        {
            if (InnerTiles.ContainsKey(key))
            {
                InnerTiles.Remove(key);
            }
        }
        public void AddInnerTileInDirection(Direction dir)
        {
            Vector2 innerTilePos = transform.position + (Vector3)Directions.GetVector2FromDirection(dir);
            InnerTile innerTile = Instantiate<InnerTile>(_innerTilePrefab, innerTilePos, Quaternion.identity, transform);
            innerTile.direction = dir;
            innerTile.SetParentTile(this);
            var tmp = innerTile.transform.position;
            innerTile.transform.position = tmp;
            innerTile.GetComponent<NetworkObject>().Spawn();
            if (InnerTiles.ContainsKey(dir))
            {
                InnerTiles[dir] = innerTile;
            }
            else
            {
                InnerTiles.Add(dir, innerTile);
            }
        }
    }
}