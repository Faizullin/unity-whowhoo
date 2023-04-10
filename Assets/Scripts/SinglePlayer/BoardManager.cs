using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





namespace Singleplayer
{
    [System.Serializable]
    public class Player
    {
        public Player(int playerId, string playerName, Color playerColor)
        {
            this.playerColor = playerColor;
            this.playerName = playerName;
            this.playerId = playerId;
        }
        public int playerId;
        public string playerName;
        public Color playerColor;
        public int Score = 0;
        public bool IsAlive = true;
    }
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager instance;

        public string MapFilePath = "Map_2023-02-20_21-22-41.json";
        public bool IsMovementAnimationIsWorking = false;
        public Tile TilePrefab;
        public int WaveCount = 0;
        public int PlayersCount = 0;

        public List<Tile> CurrentWaveSessionTiles = new List<Tile>();
        public List<Tile> NextWaveSessionTiles = new List<Tile>();
        public List<AnimationInnerTile> CurrentAnimationInnerTiles;
        [SerializeField] private AnimationInnerTile _animationInnerTilePrefab;
        public List<Player> Players;

        public Tile[] Tiles;

        public int CurrentPlayerTurnIndex;
        public float TileSize = 4f;


        private void Awake()
        {
            instance = this;
            CurrentWaveSessionTiles.Clear();
        }
        public void StartGame(List<Player> players)
        {
            Players = players;
            CurrentPlayerTurnIndex = players[0].playerId;
            PlayersCount = players.Count;
            Load(MapFilePath);
        }

        public void OnInnerTileClick(InnerTile innerTile)
        {
            Player currentPlayer = GetCurrentPlayer();
            Tile tile = innerTile._tile;
            if (!IsMovementAnimationIsWorking && currentPlayer.IsAlive)
            {
                if (tile.PlayerId == -1 || tile.PlayerId == currentPlayer.playerId)
                {
                    IsMovementAnimationIsWorking = true;
                    tile.PlayerId = currentPlayer.playerId;
                    innerTile.SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    StartScanProcedure(tile);
                }
            }
        }

        public void OnAnimationInnerTileMovementEnd()
        {
            foreach (AnimationInnerTile animTile in CurrentAnimationInnerTiles)
            {
                animTile.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            Player currentPlayer = GetCurrentPlayer();
            foreach (AnimationInnerTile animTile in CurrentAnimationInnerTiles)
            {
                animTile.ToInnerTile.SelectByUser(currentPlayer);
            }
            foreach (AnimationInnerTile animTile in CurrentAnimationInnerTiles)
            {
                Destroy(animTile.gameObject);
            }
            CurrentAnimationInnerTiles.Clear();

            WaveCount++;
            PlayersStateUpdate();
            GameManager.instance.UpdatePlayerStats(Players);
            bool res = CheckForWinner();
            if (!res)
            {
                GameManager.instance.OnGameEnd(currentPlayer);
            }
            else
            {
                foreach (Tile tile in CurrentWaveSessionTiles)
                {
                    var neighbourTiles = tile.GetNeighbourTiles();
                    foreach (Tile neighbourTile in neighbourTiles.Values)
                    {
                        neighbourTile.CheckForFullOfInnerTiles();
                        if (neighbourTile.IsFullOfInnerTiles)
                        {
                            NextWaveSessionTiles.Add(neighbourTile);
                        }
                    }

                }

                CurrentWaveSessionTiles.Clear();
                if (NextWaveSessionTiles.Count > 0)
                {
                    StartWave();
                }
                else
                {
                    NextTurn();
                }
            }
        }

        public void ClearInnerTilesSelection(List<Tile> tiles)
        {
            foreach (Tile item in tiles)
            {
                item.ClearInnerTilesSelection();
            }
        }



        public void NextTurn()
        {
            bool checkState = false;
            CurrentPlayerTurnIndex = (CurrentPlayerTurnIndex + 1 >= PlayersCount) ? 0 : CurrentPlayerTurnIndex + 1;
            while (!Players[CurrentPlayerTurnIndex].IsAlive)
            {
                if(CurrentPlayerTurnIndex + 1 >= PlayersCount)
                {
                    CurrentPlayerTurnIndex = 0;
                    checkState = true;
                } else {
                    CurrentPlayerTurnIndex = CurrentPlayerTurnIndex + 1;
                    if(checkState)
                    {
                        bool res = CheckForWinner();
                        if (!res)
                        {
                            GameManager.instance.OnGameEnd(GetCurrentPlayer());
                        }
                    }
                }
           
                
            }
            IsMovementAnimationIsWorking = false;
        }

        public bool CheckForWinner()
        {
            int alivePlayersCount = 0;
            foreach (Player player in Players)
            {
                if (player.IsAlive)
                {
                    alivePlayersCount++;
                }
            }
            return alivePlayersCount > 1;
        }

        public void PlayersStateUpdate()
        {
            foreach (Player player in Players)
            {
                if (player.IsAlive)
                {
                    if (player.Score < 1)
                    {
                        Debug.Log($"Player dies {player.playerName}");
                        player.IsAlive = false;
                    }
                }
            }
        }


        public Player GetCurrentPlayer()
        {
            return Players[CurrentPlayerTurnIndex];
        }

        public void StartScanProcedure(Tile tile)
        {
            tile.CheckForFullOfInnerTiles();
            GameManager.instance.UpdatePlayerStats(Players);
            if (!tile.IsFullOfInnerTiles)
            {
                NextTurn();
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
                var innerTiles = tile.GetInnerTiles();
                var neighbourTiles = tile.GetNeighbourTiles();
                foreach (var innerTile in innerTiles)
                {
                    AnimationInnerTile animInnerTile = GetAnimationInnerTile(tile, innerTile.Value, neighbourTiles[innerTile.Key].GetInnerTileByDirection(GetOppositeDirection(innerTile.Key)));
                    CurrentAnimationInnerTiles.Add(animInnerTile);
                }
                tile.ClearInnerTilesSelection();
            }
            StartAnimationInnerTileMovement();
        }

        public void StartAnimationInnerTileMovement()
        {
            foreach (AnimationInnerTile animInnerTile in CurrentAnimationInnerTiles)
            {
                animInnerTile.StartMove();
            }
            StartCoroutine(TileMovementCoroutine());
        }

        public IEnumerator TileMovementCoroutine()
        {
            float moveTime = 2.0f;
            yield return new WaitForSeconds(moveTime);
            OnAnimationInnerTileMovementEnd();
        }


        public AnimationInnerTile GetAnimationInnerTile(Tile tileFrom, InnerTile innerTileFrom, InnerTile innerTileTo)
        {
            Player currentPlayer = GetCurrentPlayer();
            AnimationInnerTile animInnerTile = Instantiate<AnimationInnerTile>(_animationInnerTilePrefab);
            animInnerTile.FromInnerTile = innerTileFrom;
            animInnerTile.ToInnerTile = innerTileTo;// foundTile.GetInnerTileByDirection(foundTile.GetReversedDrection(direction));
            animInnerTile.direction = GetVector2FromDirection(innerTileFrom.direction);
            animInnerTile.transform.position = innerTileFrom.transform.position;
            animInnerTile.SelectColor = currentPlayer.playerColor;
            animInnerTile.PlayerId = currentPlayer.playerId;
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
        public void Load(string filename = "Map.json")
        {
            string mapPath = PlayerPrefs.GetString("MapPath");
            if(mapPath.Trim().Length != 0)
            {
                filename = mapPath + ".json";
            }
            string path = Application.dataPath + "/Maps/" + filename;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                TileDataList tileDataList = JsonUtility.FromJson<TileDataList>(json);
                foreach (TileData tileData in tileDataList.tiles)
                {
                    var newTile = Instantiate(TilePrefab, NormalizePostion(tileData.position),  Quaternion.identity, transform);
                    foreach (Direction key in tileData.directions)
                    {
                        newTile.AddInnerTileInDirection(key);
                    }
                }
            }
            else
            {
                Debug.LogError("File not found: " + path);
            }
        }

        public Vector3 NormalizePostion(Vector3 oldPosition)
        {
            Vector3 normalizedPosition = new Vector2();
            normalizedPosition.x = Mathf.Round(oldPosition.x / TileSize) * TileSize;
            normalizedPosition.y = Mathf.Round(oldPosition.y / TileSize) * TileSize;
            normalizedPosition.z = 0;
            return normalizedPosition;
        }
    }
}
