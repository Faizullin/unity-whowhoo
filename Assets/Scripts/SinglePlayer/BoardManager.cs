using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.Tiles;
using Singleplayer.Player;

namespace Singleplayer
{
    public class BoardManager : Singleton<BoardManager>
    {
        [SerializeField]
        private string m_mapFilePath = "Map_2023-05-27_13-58-56.json";
        public bool IsMovementAnimationIsWorking = false;
        
        public int WaveCount = 0;
        public int PlayersCount = 0;
        public ulong CurrentPlayerTurnIndex;

        public List<Tile> CurrentWaveSessionTiles = new();
        public List<Tile> NextWaveSessionTiles = new();

        [SerializeField] private Tile m_tilePrefab;
        [SerializeField] private AnimationInnerTile m_animationInnerTilePrefab;
        [SerializeField] private List<AnimationInnerTile> m_currentAnimationInnerTiles;

        public void StartGame(Dictionary<ulong, PlayerData> playersData)
        {
            CurrentWaveSessionTiles.Clear();
            CurrentPlayerTurnIndex = PlayerDataManager.Instance.GetSortedKeys()[0];
            PlayersCount = playersData.Count;
            Load(m_mapFilePath);
        }

        public void OnInnerTileClick(InnerTile innerTile, Tile parentTile)
        {
            PlayerData currentPlayer = GetCurrentPlayer();
            if(!currentPlayer.IsAlive)
            {
                Debug.Log($"Warning: OnInnerTileClick: Player {currentPlayer} is not alive");
                return;
            }
            if (!IsMovementAnimationIsWorking)
            {
                if (parentTile.PlayerId == 0 || parentTile.PlayerId == currentPlayer.PlayerId)
                {
                    IsMovementAnimationIsWorking = true;
                    parentTile.PlayerId = currentPlayer.PlayerId;
                    innerTile.SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    if(!currentPlayer.HasDoneFirstAction)
                    {
                        currentPlayer.HasDoneFirstAction = true;
                    }
                    StartScanProcedure(parentTile);
                }
            }
        }

        public bool AttemptSelectByUserProcedure(InnerTile innerTile)
        {
            return AttemptSelectByUserProcedure(innerTile, innerTile.GetParentTile());
        }

        public bool AttemptSelectByUserProcedure(InnerTile innerTile, Tile parentTile)
        {
            PlayerData currentPlayer = GetCurrentPlayer();
            var parentTileInnerTiles = parentTile.GetInnerTiles();
            if (innerTile.IsSelectedByPlayer)
            {
                bool tmpIsFullOfInnerTiles = true;
                foreach (InnerTile parentTileInnerTile in parentTileInnerTiles.Values)
                {
                    if (!parentTileInnerTile.IsSelectedByPlayer)
                    {
                        tmpIsFullOfInnerTiles = false;
                    }
                }
                if (tmpIsFullOfInnerTiles)
                {
                    return false;
                }
            }
            if (!innerTile.IsSelectedByPlayer && parentTile.PlayerId < 1)
            {
                innerTile.SelectStateByUser(currentPlayer);
                parentTile.PlayerId = currentPlayer.PlayerId;
                currentPlayer.Score++;
                return true;
            }
            else if (currentPlayer.PlayerId == parentTile.PlayerId)
            {
                if (innerTile.IsSelectedByPlayer)
                {
                    var keys = new List<Direction>(parentTileInnerTiles.Keys);
                    var basicKeys = new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left };
                    basicKeys.Remove(innerTile.direction);
                    keys.Remove(innerTile.direction);

                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                            return true;
                        }
                    }
                }
                else
                {
                    innerTile.SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    return true;
                }
            }
            else if (currentPlayer.PlayerId != parentTile.PlayerId)
            {
                if (innerTile.IsSelectedByPlayer)
                {
                    var keys = new List<Direction>(parentTileInnerTiles.Keys);
                    var basicKeys = new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left };

                    foreach (Direction key in keys)
                    {
                        if (parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            PlayerDataManager.Instance.PlayersData[parentTile.PlayerId].Score--;
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                        }
                    }

                    basicKeys.Remove(innerTile.direction);
                    keys.Remove(innerTile.direction);
                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                            parentTile.PlayerId = currentPlayer.PlayerId;
                            return true;
                        }
                    }
                    parentTile.PlayerId = currentPlayer.PlayerId;
                }
                else
                {
                    foreach (var parentTileInnerTile in parentTileInnerTiles.Values)
                    {
                        if (parentTileInnerTile.IsSelectedByPlayer)
                        {
                            PlayerDataManager.Instance.PlayersData[parentTile.PlayerId].Score--;
                            parentTileInnerTile.SelectStateByUser(currentPlayer);
                            currentPlayer.Score++;
                        }
                    }
                    innerTile.SelectStateByUser(currentPlayer);
                    currentPlayer.Score++;
                    parentTile.PlayerId = currentPlayer.PlayerId;
                    return true;
                }
            }

            Debug.Log($"Exception undefined select InnerTile");
            return false;
        }

        private void UpdateBoardStats()
        {
            WaveCount++;
            GameManager.Instance.UpdatePlayerStats();
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
            var sortedKeys = PlayerDataManager.Instance.GetSortedKeys();
            var currentKeyIndex = sortedKeys.IndexOf(CurrentPlayerTurnIndex);
            if (currentKeyIndex == -1) {
                Debug.Log($"Warning: NextTurn: IndexOf {CurrentPlayerTurnIndex} not found");
                return;
            }

            CurrentPlayerTurnIndex = (currentKeyIndex + 1 >= PlayersCount) ? sortedKeys[0] : sortedKeys[currentKeyIndex + 1];
            while (!PlayerDataManager.Instance.PlayersData[CurrentPlayerTurnIndex].IsAlive)
            {
                if(currentKeyIndex + 1 >= PlayersCount)
                {
                    CurrentPlayerTurnIndex = sortedKeys[0];
                    checkState = true;
                } else {
                    currentKeyIndex++;
                    CurrentPlayerTurnIndex = sortedKeys[currentKeyIndex];
                    if (checkState)
                    {
                        bool res = GameManager.Instance.CheckForWinner();
                        if (!res)
                        {
                            GameManager.Instance.OnGameEnd(GetCurrentPlayer());
                        }
                    }
                }
            }
            IsMovementAnimationIsWorking = false;
        }

        public PlayerData GetCurrentPlayer()
        {
            return PlayerDataManager.Instance.PlayersData[CurrentPlayerTurnIndex];
        }

        public void StartScanProcedure(Tile tile)
        {
            tile.CheckForFullOfInnerTiles();
            GameManager.Instance.UpdatePlayerStats();
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
                if(!CurrentWaveSessionTiles.Contains(item))
                {
                    CurrentWaveSessionTiles.Add(item);
                }
            }
            NextWaveSessionTiles.Clear();
            m_currentAnimationInnerTiles.Clear();
            foreach (Tile tile in CurrentWaveSessionTiles)
            {
                var innerTiles = tile.GetInnerTiles();
                var neighbourTiles = tile.GetNeighbourTiles();
                foreach (var innerTile in innerTiles)
                {
                    AnimationInnerTile animInnerTile = GetAnimationInnerTile(tile, innerTile.Value, neighbourTiles[innerTile.Key].GetInnerTileByDirection(Directions.GetOppositeDirection(innerTile.Key)));
                    m_currentAnimationInnerTiles.Add(animInnerTile);
                }
                tile.ClearInnerTilesSelection();
            }
            StartAnimationInnerTileMovement();
        }

        public void StartAnimationInnerTileMovement()
        {
            foreach (AnimationInnerTile animInnerTile in m_currentAnimationInnerTiles)
            {
                animInnerTile.StartMove();
            }
            StartCoroutine(TileMovementCoroutine());
        }

        public void OnAnimationInnerTileMovementEnd()
        {
            List<AnimationInnerTile> activeAnimeInnertTiles = new();
            List<InnerTile> activeInnertTiles = new();

            foreach (AnimationInnerTile animTile in m_currentAnimationInnerTiles)
            {
                animTile.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (!activeInnertTiles.Contains(animTile.ToInnerTile))
                {
                    activeInnertTiles.Add(animTile.ToInnerTile);
                    activeAnimeInnertTiles.Add(animTile);
                }
            }
            foreach (AnimationInnerTile animTile in activeAnimeInnertTiles)
            {
                if (AttemptSelectByUserProcedure(animTile.ToInnerTile))
                {
                    PlayerDataManager.Instance.PlayersData[animTile.PlayerId].Score--;
                }
            }
            foreach (AnimationInnerTile animTile in m_currentAnimationInnerTiles)
            {
                Destroy(animTile.gameObject);
            }

            m_currentAnimationInnerTiles.Clear();
            UpdateBoardStats();
            GameManager.Instance.CheckForWinnerAndRaiseGameEnd();
            if(GameManager.Instance.CurrentGameStatus == GameStatus.FINISHED)
            {
                return;
            }
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

        public IEnumerator TileMovementCoroutine()
        {
            float moveTime = 2.0f;
            yield return new WaitForSeconds(moveTime);
            OnAnimationInnerTileMovementEnd();
        }

        public AnimationInnerTile GetAnimationInnerTile(Tile tileFrom, InnerTile innerTileFrom, InnerTile innerTileTo)
        {
            PlayerData currentPlayer = GetCurrentPlayer();
            AnimationInnerTile animInnerTile = Instantiate(m_animationInnerTilePrefab);
            animInnerTile.FromInnerTile = innerTileFrom;
            animInnerTile.ToInnerTile = innerTileTo;
            animInnerTile.MoveDirection = Directions.GetVector2FromDirection(innerTileFrom.direction);
            animInnerTile.transform.position = innerTileFrom.transform.position;
            animInnerTile.SelectColor = currentPlayer.PlayerColor;
            animInnerTile.PlayerId = currentPlayer.PlayerId;
            return animInnerTile;
        }
        
        public void Load(string filename = null)
        {
            string mapPath = PlayerPrefs.GetString("MapPath");
            if(mapPath.Trim().Length == 0)
            {
                Debug.Log("Warning: Load: MapPath not found in PlayerPrefs");
                mapPath = filename;
            }
            TileDataList tileDataList = LoadingMapManager.LoadMap(mapPath);
            foreach (TileData tileData in tileDataList.tiles)
            {
                var newTile = Instantiate(m_tilePrefab, tileData.position,  Quaternion.identity, transform);
                foreach (Direction key in tileData.directions)
                {
                    newTile.AddInnerTileInDirection(key);
                }
            }
        }

    }
}
