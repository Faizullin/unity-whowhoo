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
        private AnimationInnerTileControlBoardManager m_animationInnerTileController;

        public bool IsMovementAnimationIsWorking = false;
        
        public int WaveCount = 0;
        public int PlayersCount = 0;
        public ulong CurrentPlayerTurnIndex;

        [SerializeField] private Tile m_tilePrefab;

        public List<Tile> CurrentWaveSessionTiles = new();
        public List<Tile> NextWaveSessionTiles = new();
        // private List<Tile> tileList = new();

        private Dictionary<ulong, PlayerState> m_playerStatesToUpdate = new();

        public void Awake()
        {
            base.Awake();
            m_animationInnerTileController = GetComponent<AnimationInnerTileControlBoardManager>();
        }

        public void StartGame()
        {
            CurrentWaveSessionTiles.Clear();
            var playerIds = GameManager.Instance.GetSortedClientIds();
            CurrentPlayerTurnIndex = playerIds[0];
            PlayersCount = playerIds.Count;
            Load(m_mapFilePath);
        }

        public void OnInnerTileClick(InnerTile innerTile, Tile parentTile)
        {
            PlayerState currentPlayerState = (PlayerState)GetCurrentPlayer();
            if (!currentPlayerState.IsAlive)
            {
                Debug.Log($"Warning: OnInnerTileClickServerRpc: Player {currentPlayerState.PlayerName} is not alive");
                return;
            }
            if (!IsMovementAnimationIsWorking)
            {
                if (parentTile.PlayerId == 0 || parentTile.PlayerId == currentPlayerState.ClientId)
                {
                    IsMovementAnimationIsWorking = true;
                    parentTile.PlayerId = currentPlayerState.ClientId;
                    innerTile.SelectStateByUser(currentPlayerState);

                    if (!currentPlayerState.HasDoneFirstAction)
                    {
                        AddToPlayerStateToUpdate(currentPlayerState.ClientId, new PlayerState(currentPlayerState)
                        {
                            HasDoneFirstAction = true
                        });
                    }
                    StartScanProcedure(parentTile);
                }
            }
        }

        public bool AttemptSelectByUserProcedure(PlayerState currentPlayerState, InnerTile innerTile)
        {
            return AttemptSelectByUserProcedure(currentPlayerState, innerTile, innerTile.GetParentTile());
        }

        public bool AttemptSelectByUserProcedure(PlayerState currentPlayerState, InnerTile innerTile, Tile parentTile)
        {
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
                innerTile.SelectStateByUser(currentPlayerState);
                parentTile.PlayerId = currentPlayerState.ClientId;
                AddToPlayerStateToUpdate(currentPlayerState, 1);
                return true;
            }
            else if (currentPlayerState.ClientId == parentTile.PlayerId)
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
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayerState);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                            return true;
                        }
                    }
                }
                else
                {
                    innerTile.SelectStateByUser(currentPlayerState);
                    AddToPlayerStateToUpdate(currentPlayerState, 1);
                    return true;
                }
            }
            else if (currentPlayerState.ClientId != parentTile.PlayerId)
            {
                if (innerTile.IsSelectedByPlayer)
                {
                    var keys = new List<Direction>(parentTileInnerTiles.Keys);
                    var basicKeys = new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left };

                    foreach (Direction key in keys)
                    {
                        if (parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            AddToPlayerStateToUpdate(parentTile.PlayerId, -1);
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayerState);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                        }
                    }

                    parentTile.PlayerId = currentPlayerState.ClientId;
                    basicKeys.Remove(innerTile.direction); 
                    keys.Remove(innerTile.direction);

                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayerState);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                            return true;
                        }
                    }
                    parentTile.PlayerId = currentPlayerState.ClientId;
                }
                else
                {
                    foreach (var parentTileInnerTile in parentTileInnerTiles.Values)
                    {
                        if (parentTileInnerTile.IsSelectedByPlayer)
                        {
                            AddToPlayerStateToUpdate(parentTile.PlayerId, -1);
                            parentTileInnerTile.SelectStateByUser(currentPlayerState);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                        }
                    }
                    innerTile.SelectStateByUser(currentPlayerState);
                    AddToPlayerStateToUpdate(currentPlayerState, 1);
                    parentTile.PlayerId = currentPlayerState.ClientId;
                    return true;
                }
            }

            Debug.Log($"Exception undefined select InnerTile");
            return false;
        }

        private void UpdateBoardStats()
        {
            WaveCount++;
            GameManager.Instance.UpdatePlayerIsAlive();
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
            var sortedKeys = GameManager.Instance.GetSortedClientIds();
            var currentKeyIndex = sortedKeys.IndexOf(CurrentPlayerTurnIndex);
            if (currentKeyIndex == -1) {
                Debug.Log($"Warning: NextTurn: IndexOf {CurrentPlayerTurnIndex} not found");
                return;
            }
            CurrentPlayerTurnIndex = (currentKeyIndex + 1 >= PlayersCount) ? sortedKeys[0] : sortedKeys[currentKeyIndex + 1];
            while (!((PlayerState)GameManager.Instance.GetPlayerByClientId(CurrentPlayerTurnIndex)).IsAlive)
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
                        GameManager.Instance.CheckForWinnerAndRaiseGameEnd();
                    }
                }
            }
            IsMovementAnimationIsWorking = false;
        }

        public PlayerState? GetCurrentPlayer()
        {
            return GameManager.Instance.GetPlayerByClientId(CurrentPlayerTurnIndex);
        }

        public void StartScanProcedure(Tile tile)
        {
            tile.CheckForFullOfInnerTiles();
            var currentPlayerState = (PlayerState)GetCurrentPlayer();
            if (!tile.IsFullOfInnerTiles)
            {
                AddToPlayerStateToUpdate(currentPlayerState, 1);
                SendDataToUpdate();
                m_playerStatesToUpdate.Clear();
                NextTurn();
            }
            else
            {
                AddToPlayerStateToUpdate(currentPlayerState, 1);
                SendDataToUpdate();
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

            var currentPlayerState = (PlayerState)GetCurrentPlayer();
            foreach (Tile tile in CurrentWaveSessionTiles)
            {
                var innerTiles = tile.GetInnerTiles();
                var neighbourTiles = tile.GetNeighbourTiles();
                foreach (var innerTile in innerTiles)
                {
                    m_animationInnerTileController.GetAndAddAnimationInnerTile(
                        innerTile.Value, 
                        neighbourTiles[innerTile.Key].GetInnerTileByDirection(Directions.GetOppositeDirection(innerTile.Key)),
                        currentPlayerState
                    );
                }
                tile.ClearInnerTilesSelection();
            }

            m_animationInnerTileController.StartMoveAll();
        }

        public void OnAnimationInnerTileMovementEnd()
        {
            m_animationInnerTileController.StopMoveAll();
            List<AnimationInnerTile> activeAnimeInnertTiles = m_animationInnerTileController.GetUniqueAnimationInnerTiles();

            PlayerState currentPlayerState = (PlayerState)GetCurrentPlayer();
            foreach (AnimationInnerTile animTile in activeAnimeInnertTiles)
            {
                if (AttemptSelectByUserProcedure(currentPlayerState, animTile.ToInnerTile))
                {
                    AddToPlayerStateToUpdate(animTile.PlayerId, -1);
                }
            }
            m_animationInnerTileController.DestroyAll();

            SendDataToUpdate();
            m_playerStatesToUpdate.Clear();
            UpdateBoardStats();

            GameManager.Instance.CheckForWinnerAndRaiseGameEnd();
            if (GameManager.Instance.CurrentGameStatus == GameStatus.FINISHED)
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

        public void Load(string filename = null)
        {
            string mapPath = PlayerPrefs.GetString("MapPath");
            if (mapPath.Trim().Length == 0)
            {
                Debug.Log("Warning: Load: MapPath not found in PlayerPrefs");
                mapPath = filename;
            }
            TileData[] tileDataList = LoadingMapManager.LoadMap(mapPath).tiles.ToArray();
            foreach (TileData tileData in tileDataList)
            {
                var newTile = Instantiate(m_tilePrefab, tileData.position, Quaternion.identity, transform);
                newTile.Index = tileData.index;
                foreach (Direction key in tileData.directions)
                {
                    newTile.AddInnerTileInDirection(key);
                }
            }
        }

        private void SendDataToUpdate()
        {
            List<PlayerState> tmpPlayers = new();
            foreach (var item in m_playerStatesToUpdate.Values)
            {
                tmpPlayers.Add(item);
            }
            GameManager.Instance.UpdatePlayerHealth(tmpPlayers);
        }

        private void AddToPlayerStateToUpdate(ulong playerStateClientId, PlayerState newPlayerState)
        {
            if (m_playerStatesToUpdate.ContainsKey(playerStateClientId))
            {
                m_playerStatesToUpdate[playerStateClientId] = newPlayerState;
            }
            else
            {
                m_playerStatesToUpdate.Add(playerStateClientId, newPlayerState);
            }
        }
        private void AddToPlayerStateToUpdate(ulong playerStateClientId, int Score = 0)
        {
            var playerState = (PlayerState)GameManager.Instance.GetPlayerByClientId(playerStateClientId);
            AddToPlayerStateToUpdate(playerState, Score);
        }
        private void AddToPlayerStateToUpdate(PlayerState playerState, int Score = 0)
        {
            ulong index = playerState.ClientId;
            if (m_playerStatesToUpdate.ContainsKey(index))
            {
                var tmp = new PlayerState(m_playerStatesToUpdate[index]);
                tmp.Score += Score;
                m_playerStatesToUpdate[index] = tmp;
            }
            else
            {
                var tmp = new PlayerState(playerState);
                tmp.Score += Score;
                m_playerStatesToUpdate.Add(index, tmp);
            }
        }
    }
}
