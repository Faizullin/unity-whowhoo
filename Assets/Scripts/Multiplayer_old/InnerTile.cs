using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


namespace Multiplayer_old
{
    public class InnerTile : NetworkBehaviour
    {
        [SerializeField] private Tile _tile;
        [SerializeField] private SpriteRenderer _spriteRenderer;
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

       

        public void ClearSelect()
        {
            _spriteRenderer.color = OriginalColor;
            IsSelectedByPlayer = false;
        }

        [ServerRpc(RequireOwnership = false)]//(RequireOwnership =false)
        private void ChangeStateServerRpc()
        {
            if (!IsSelectedByPlayer)
            {
                _tile.InnerTiles[direction].IsSelectedByPlayer = true;
                //Debug.Log($"Server CmdChangeState to {_tile.IsFullOfInnerTiles}");
                if (!_tile.IsFullOfInnerTiles)
                {
                    ChangeTileColorClientRpc(_tile.IsFullOfInnerTiles);
                }                
            }
        }
        [ClientRpc]
        private void ChangeTileColorClientRpc(bool IsFullOfInnerTiles)
        {
            //Debug.Log("Client CmdChangeState");
            _spriteRenderer.color = SelectedColor;
            _tile.IsFullOfInnerTiles = IsFullOfInnerTiles;
            IsSelectedByPlayer = true;
        }

        public Tile GetParentTile()
        {
            return _tile;
        }

        public void SetParentTile(Tile newTile)
        {
            _tile = newTile;
        }
    }
}

