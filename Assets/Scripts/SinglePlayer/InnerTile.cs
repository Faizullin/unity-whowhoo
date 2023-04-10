using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class InnerTile : MonoBehaviour
    {
        [SerializeField] public Tile _tile;
        [SerializeField] public SpriteRenderer _spriteRenderer;
        [SerializeField] private AnimationInnerTile _animationInnerTilePrefab;
        public Direction direction;

        public bool IsSelectedByPlayer = false;

        public Color HoverColor = Color.gray;
        public Color SelectedColor = Color.blue;
        public Color OriginalColor;


        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            OriginalColor = _spriteRenderer.color;
            //Invoke(nameof(GetParentComponent), 0.1f);
            GetParentComponent();
        }

        void GetParentComponent()
        {
            _tile = transform.parent.GetComponent<Tile>();
        }

        private void OnMouseDown()
        {
            if (!IsSelectedByPlayer)
            {
                var tmp = this.GetComponent<InnerTile>();
                BoardManager.instance.OnInnerTileClick(tmp);
            }
        }
        private void OnMouseEnter()
        {
            if (!IsSelectedByPlayer)
            {
                _spriteRenderer.color = HoverColor;
            }

        }

        private void OnMouseExit()
        {
            if (!IsSelectedByPlayer)
            {
                _spriteRenderer.color = OriginalColor;
            }

        }


        public void SelectByUser(Player currentPlayer)
        {
            if (!IsSelectedByPlayer && _tile.PlayerId == -1)
            {
                SelectStateByUser(currentPlayer);
                _tile.PlayerId = currentPlayer.playerId;
                currentPlayer.Score++;
                return;
            }
            else if (currentPlayer.playerId == _tile.PlayerId)
            {
                if (IsSelectedByPlayer)
                {
                    var otherInnerTiles = _tile.GetInnerTiles();
                    var keys = new List<Direction>(otherInnerTiles.Keys);
                    var basicKeys = new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left };
                    basicKeys.Remove(direction);
                    keys.Remove(direction);

                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !otherInnerTiles[key].IsSelectedByPlayer)
                        {
                            otherInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                            return;
                        }
                    }
                }
                else
                {
                    SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    return;
                }
            }
            else if (currentPlayer.playerId != _tile.PlayerId)
            {
                var otherInnerTiles = _tile.GetInnerTiles();
                if (IsSelectedByPlayer)
                {
                    var keys = new List<Direction>(otherInnerTiles.Keys);
                    var basicKeys = new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left };

                    foreach (Direction key in keys)
                    {
                        if (otherInnerTiles[key].IsSelectedByPlayer)
                        {
                            BoardManager.instance.Players[_tile.PlayerId].Score--;
                            otherInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                        }
                    }
                    basicKeys.Remove(direction);
                    keys.Remove(direction);
                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !otherInnerTiles[key].IsSelectedByPlayer)
                        {
                            BoardManager.instance.Players[_tile.PlayerId].Score--;
                            otherInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                            _tile.PlayerId = currentPlayer.playerId;
                            return;
                        }
                    }
                    _tile.PlayerId = currentPlayer.playerId;
                }
                else
                {
                    foreach (var innerTile in otherInnerTiles)
                    {
                        if (innerTile.Value.IsSelectedByPlayer)
                        {
                            BoardManager.instance.Players[_tile.PlayerId].Score--;
                            innerTile.Value.SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                        }
                    }
                    SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    _tile.PlayerId = currentPlayer.playerId;
                    return;
                }
            }
            Debug.Log($"Exception undefined select InnerTile");
        }
        public void SelectStateByUser(Player player)
        {
            _spriteRenderer.color = player.playerColor;
            IsSelectedByPlayer = true;
        }

        public void ClearSelect()
        {
            _spriteRenderer.color = OriginalColor;
            IsSelectedByPlayer = false;
        }
    }
}
    

