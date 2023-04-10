using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer;

namespace TileMapEditorScene
{
    public class Tile : MonoBehaviour
    {
        public Dictionary<Direction, InnerTile> InnerTiles = new Dictionary<Direction, InnerTile>()
        {
        };
        [SerializeField] private InnerTile _innerTilePrefab;
        public float TileSize;
        public Color HoverColor = Color.gray;
        public Color OriginalColor;
        public bool HasInnerTiles = true;


        private void Awake()
        {
            TileSize = GetComponent<Tile>().GetComponent<BoxCollider2D>().size.x;
        }

        private void Start()
        {
            InnerTile[] TmpInnerTiles = GetComponentsInChildren<InnerTile>();
            Debug.Log($"TmpInnerTiles {TmpInnerTiles}");
            foreach (InnerTile TmpInnerTile in TmpInnerTiles)
            {
                Vector3 TmpInnerTilePostion = TmpInnerTile.transform.position - transform.position;
                Debug.Log($"TmpInnerTile Index {TmpInnerTile} ({TmpInnerTilePostion})");
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
                InnerTiles.Add(TmpInnerTile.direction, TmpInnerTile);
            }
            Debug.Log(InnerTiles.Count);
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



        public InnerTile GetInnerTileByDirection(Direction direction)
        {
            return InnerTiles[direction];
        }
        public Dictionary<Direction, InnerTile> GetInnerTiles()
        {
            return InnerTiles;
        }


        public void RemoveInnerTile(Direction key)
        {
            if (InnerTiles.ContainsKey(key))
            {
                InnerTiles.Remove(key);
            }
        }

        private Vector2 GetVector2FromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.up:
                    return Vector2.up;
                case Direction.right:
                    return Vector2.right;
                case Direction.down:
                    return Vector2.down;
                case Direction.left:
                    return Vector2.left;
                default:
                    return Vector2.up;
            }
        }

        public void AddInnerTileInDirection(Direction dir)
        {
            Vector2 innerTilePos = transform.position + (Vector3)GetVector2FromDirection(dir);
            //innerTilePos.x -= 1f;
            //innerTilePos.y;
            var innerTile = Instantiate<InnerTile>(_innerTilePrefab, innerTilePos, Quaternion.identity, transform);
            innerTile.direction = dir;
            var tmp = innerTile.transform.position;
            tmp.z = -1;
            innerTile.transform.position = tmp;
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


