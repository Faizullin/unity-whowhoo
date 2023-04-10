using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


namespace Multiplayer
{
    [System.Serializable]
    public class Player : NetworkBehaviour
    {
        public string playerName;
        public Color color; 
        public int score = 0;
        public bool isAlive = true;
        public void OnStartClient()
        {
            // Add the player to the list of players in the BoardManager
            //MultiplayerBoardManager.instance.AddPlayer(this);
        }

        public void OnNetworkDestroy()
        {
            //MultiplayerBoardManager.instance.RemovePlayer(this);
        }
    }

    public class MultiplayerBoardManager : NetworkBehaviour
    {
        public List<Tile> CurrentWaveSessionTiles = new List<Tile>();
        public List<Tile> NextWaveSessionTiles = new List<Tile>();
        public List<AnimationInnerTile> CurrentAnimationInnerTiles;
        [SerializeField] private AnimationInnerTile _animationInnerTilePrefab;


        public static MultiplayerBoardManager instance;
        public string MapFilePath = "Map_2023-02-20_21-22-41.json";
        public bool IsMovementAnimationIsWorking = false;
        public Tile TilePrefab;
        public int WaveCount = 0;
        public int PlayersCount = 0;


        public Tile[] Tiles;

        public int CurrentPlayerTurnIndex;
        public int TileSize = 4;

        private void Awake()
        {
            Tiles = FindObjectsOfType<Tile>();
            instance = this;
        }

        public void CreatePlayers()
        {

        }

        public void StartScanProcedure(Tile tile)
        {
            tile.CheckForFullOfInnerTiles();
            //Debug.Log($"Start StartScanProcedure {tile.IsFullOfInnerTiles}");
            if (!tile.IsFullOfInnerTiles)
            {
                //NextTurn();
            }
            else
            {
                StartFirstWave(tile);
            }
        }

        public void StartFirstWave(Tile fromTile)
        {
            NextWaveSessionTiles.Clear();
            NextWaveSessionTiles.Add(fromTile);
            StartWave();
        }
        public void StartWave()
        {
            CurrentWaveSessionTiles.Clear();
            foreach (var item in NextWaveSessionTiles)
            {
                CurrentWaveSessionTiles.Add(item);
            }
            NextWaveSessionTiles.Clear();
            CurrentAnimationInnerTiles.Clear();
            foreach (Tile tile in CurrentWaveSessionTiles)
            {
                var neighbourTiles = tile.GetNeighbourTiles();
                foreach (var innerTile in tile.InnerTiles)
                {
                    if (neighbourTiles[innerTile.Key] != null)
                    {
                        AnimationInnerTile animInnerTile = GetAnimationInnerTile(tile, innerTile.Value, neighbourTiles[innerTile.Key].GetInnerTileByDirection(GetOppositeDirection(innerTile.Key)));
                        CurrentAnimationInnerTiles.Add(animInnerTile);
                    }
                    
                }
                tile.ClearInnerTilesSelection();
            }
            StartAnimationInnerTileMovement();
        }

        public AnimationInnerTile GetAnimationInnerTile(Tile tileFrom, InnerTile innerTileFrom, InnerTile innerTileTo)
        {
            //Player currentPlayer = GetCurrentPlayer();
            AnimationInnerTile animInnerTile = Instantiate<AnimationInnerTile>(_animationInnerTilePrefab);
            animInnerTile.FromInnerTile = innerTileFrom;
            animInnerTile.ToInnerTile = innerTileTo;// foundTile.GetInnerTileByDirection(foundTile.GetReversedDrection(direction));
            animInnerTile.direction = GetVector2FromDirection(innerTileFrom.direction);
            animInnerTile.transform.position = innerTileFrom.transform.position;
            animInnerTile.SelectColor = Color.green;//currentPlayer.playerColor;
            //animInnerTile.PlayerId = currentPlayer.playerId;
            return animInnerTile;
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
        private Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.right:
                    return Direction.left;
                case Direction.down:
                    return Direction.up;
                case Direction.left:
                    return Direction.right;
                default:
                    return Direction.down;
            }
        }

        public void StartAnimationInnerTileMovement()
        {
            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                //animInnerTile.StartMove();
                var tmp = animInnerTile.GetComponentInChildren<SpriteRenderer>();
                Debug.Log("tmp " + tmp.ToString());
                tmp.color = Color.green;

                float moveSpeed = 5f;
                animInnerTile.GetComponent<Rigidbody2D>().velocity = animInnerTile.direction * moveSpeed;
                animInnerTile.GetComponent<NetworkObject>().Spawn();
            }
            StartCoroutine(TileMovementCoroutine());
        }


        public IEnumerator TileMovementCoroutine()
        {
            float moveTime = 1f;
            yield return new WaitForSeconds(moveTime);

            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                animInnerTile.ToInnerTile.IsSelectedByPlayer = true;
                //animInnerTile.PlayerId = animInnerTile.PlayerId;
            }
            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                Destroy(animInnerTile.gameObject);
            }
            Debug.Log("End move in coroutine");
            DestroyAnimationTilesClientRpc();
        }

        [ClientRpc]
        public void StartAnimationTilesMovementClientRpc(int m)
        {
            //CurrentAnimationInnerTiles = animInnerTiles;
            //Debug.Log("Client ANIM<ATION_INNER_TILES:"+ animInnerTiles.ToString());
            //foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            //{
            //    animInnerTile.StartMove();
            //}
        }

        [ClientRpc]
        public void DestroyAnimationTilesClientRpc()
        {
            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                animInnerTile.ToInnerTile.IsSelectedByPlayer = true;
                //animInnerTile.PlayerId = animInnerTile.PlayerId;
            }
            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                Destroy(animInnerTile.gameObject);
            }
        }

        [ServerRpc]
        public void OnAnimationInnerTileMovementEndServerRpc()
        {
            foreach (AnimationInnerTile animTile in CurrentAnimationInnerTiles)
            {
                if (animTile.IsWorking)
                {
                    return;
                }
            }
            Debug.Log("Animation ends");
        }
    }
}

