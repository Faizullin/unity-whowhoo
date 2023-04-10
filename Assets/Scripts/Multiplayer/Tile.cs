using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class Tile : MonoBehaviour
    {
        public Dictionary<Direction, InnerTile> InnerTiles = new Dictionary<Direction, InnerTile>()
        {
        };
        [SerializeField] private int _rowIndex;
        [SerializeField] private int _colIndex;
        [SerializeField] private InnerTile _innerTilePrefab;

        private int TileSize = 4;
        public bool IsFullOfInnerTiles = false;

        private void Awake()
        {
            CollectInnerTiles();
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
            int tmp = 0;
            foreach (InnerTile innerTile in InnerTiles.Values)
            {
                if (!innerTile.IsSelectedByPlayer)
                {
                    TmpAllInnerTilesActive = false;
                    break;
                }
                tmp++;
            }
            IsFullOfInnerTiles = TmpAllInnerTilesActive;
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
            Debug.Log($"Clear inner tiles for");
            foreach (InnerTile innerTile in InnerTiles.Values)
            {
                innerTile.ClearSelect();
                //_board.Players[PlayerId].Score--;
            }
            //PlayerId = -1;
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

        public Tile GetNeighboourTile(Direction direction)
        {
            Vector2 vectorDirection = Vector2.up;
            switch (direction)
            {
                case Direction.right:
                    vectorDirection = Vector2.right;
                    break;
                case Direction.down:
                    vectorDirection = Vector2.down;
                    break;
                case Direction.left:
                    vectorDirection = Vector2.left;
                    break;
                default:
                    break;
            }
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
    }
}

