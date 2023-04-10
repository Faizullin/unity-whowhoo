using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


namespace Multiplayer
{
    public class InnerTile : NetworkBehaviour
    {
        public Tile _tile;
        [SerializeField] public SpriteRenderer _spriteRenderer;
        //[SerializeField] private AnimationInnerTile _animationInnerTilePrefab;
        public Direction direction;

        public bool IsSelectedByPlayer = false;

        public Color HoverColor = Color.gray;
        public Color SelectedColor = Color.blue;
        public Color OriginalColor;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            OriginalColor = _spriteRenderer.color;
            _tile = GetComponentInParent<Tile>();
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

        private void OnMouseDown()
        {
            if (!IsSelectedByPlayer)
            {
                Debug.Log($"You clicked on {this}");
                //IsSelectedByPlayer = true;
                ChangeStateServerRpc();
                //MultiplayerBoardManager boardManager = GameObject.FindObjectOfType<MultiplayerBoardManager>();

                //Player currentPlayer = boardManager.GetCurrentPlayer();
                //if (!boardManager.IsMovementAnimationIsWorking && currentPlayer.IsAlive)
                //{

                //    if (_tile.PlayerId == -1 || _tile.PlayerId == currentPlayer.playerId)
                //    {
                //        boardManager.IsMovementAnimationIsWorking = true;
                //        SelectStateByUser(currentPlayer);
                //        _tile.PlayerId = currentPlayer.playerId;
                //        currentPlayer.Score++;
                //        boardManager.StartScanProcedure(_tile);
                //    }
                //}
            }
        }

        [ServerRpc(RequireOwnership = false)]//(RequireOwnership =false)
        private void ChangeStateServerRpc()
        {
            if (!IsSelectedByPlayer)
            {
                _tile.InnerTiles[direction].IsSelectedByPlayer = true;
                MultiplayerBoardManager.instance.StartScanProcedure(_tile);
                Debug.Log($"Server CmdChangeState to {_tile.IsFullOfInnerTiles}");
                if (!_tile.IsFullOfInnerTiles)
                {
                    ChangeTileColorClientRpc(_tile.IsFullOfInnerTiles);
                }                
            }
        }
        [ClientRpc]
        private void ChangeTileColorClientRpc(bool IsFullOfInnerTiles)
        {
            Debug.Log("Client CmdChangeState");
            _spriteRenderer.color = SelectedColor;
            _tile.IsFullOfInnerTiles = IsFullOfInnerTiles;
            IsSelectedByPlayer = true;
        }

        public void ClearSelect()
        {
            _spriteRenderer.color = OriginalColor;
            IsSelectedByPlayer = false;
        }

    }
}

