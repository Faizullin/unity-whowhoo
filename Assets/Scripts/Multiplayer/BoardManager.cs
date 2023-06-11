using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Multiplayer.Tiles;
using Multiplayer.Player;

namespace Multiplayer
{
    public class BoardManager : SingletonNetwork<BoardManager>
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
        private List<Tile> tileList = new();
        private Dictionary<int, InnerTileData> m_innerTilesToUpdate = new();
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
            OnInnerTileClickServerRpc(innerTile.direction, parentTile.Index);
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnInnerTileClickServerRpc(Direction direction, int parentTileIndex, ServerRpcParams serverParams = default)
        {
            if (serverParams.Receive.SenderClientId != CurrentPlayerTurnIndex)
            {
                return;
            }
            if (tileList.Count <= parentTileIndex)
            {
                Debug.Log($"Warning: OnInnerTileClickServerRpc: {parentTileIndex} out of range {tileList.Count}");
                return;
            }
            PlayerState currentPlayerState = (PlayerState)GetCurrentPlayer();
            if (!currentPlayerState.IsAlive)
            {
                Debug.Log($"Warning: OnInnerTileClickServerRpc: Player {currentPlayerState} is not alive");
                return;
            }
            
            var parentTile = tileList[parentTileIndex];
            var innerTile = parentTile.GetInnerTileByDirection(direction);
            if (!IsMovementAnimationIsWorking)
            {
                if (parentTile.PlayerId < 0 || (long)currentPlayerState.ClientId == parentTile.PlayerId)
                {
                    IsMovementAnimationIsWorking = true;
                    parentTile.PlayerId = (long)currentPlayerState.ClientId;
                    innerTile.SelectStateByUser(currentPlayerState);
                    AddToInnerTilesStateToUpdate(parentTile, innerTile);
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
                        break;
                    }
                }
                if (tmpIsFullOfInnerTiles)
                {
                    return false;
                }
            }
            if (!innerTile.IsSelectedByPlayer && parentTile.PlayerId < 0)
            {
                innerTile.SelectStateByUser(currentPlayerState);
                AddToInnerTilesStateToUpdate(parentTile, innerTile);
                parentTile.PlayerId = (long)currentPlayerState.ClientId;
                AddToPlayerStateToUpdate(currentPlayerState, 1);
                return true;
            }
            else if ((long)currentPlayerState.ClientId == parentTile.PlayerId)
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
                            AddToInnerTilesStateToUpdate(parentTileInnerTiles[key].GetParentTile(), parentTileInnerTiles[key]);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                            return true;
                        }
                    }
                }
                else
                {
                    innerTile.SelectStateByUser(currentPlayerState);
                    AddToInnerTilesStateToUpdate(parentTile, innerTile);
                    AddToPlayerStateToUpdate(currentPlayerState, 1);
                    return true;
                }
            }
            else if ((long)currentPlayerState.ClientId != parentTile.PlayerId)
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
                            AddToInnerTilesStateToUpdate(parentTile, parentTileInnerTiles[key]);

                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                        }
                    }

                    parentTile.PlayerId = (long)currentPlayerState.ClientId;
                    basicKeys.Remove(innerTile.direction);
                    keys.Remove(innerTile.direction);

                    foreach (Direction key in basicKeys)
                    {
                        if (keys.Contains(key) && !parentTileInnerTiles[key].IsSelectedByPlayer)
                        {
                            parentTileInnerTiles[key].SelectStateByUser(currentPlayerState);
                            AddToInnerTilesStateToUpdate(parentTile, parentTileInnerTiles[key]);
                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                            return true;
                        }
                    }
                }
                else
                {
                    foreach (var parentTileInnerTile in parentTileInnerTiles.Values)
                    {
                        if (parentTileInnerTile.IsSelectedByPlayer)
                        {
                            AddToPlayerStateToUpdate(parentTile.PlayerId, -1);
                            
                            parentTileInnerTile.SelectStateByUser(currentPlayerState);
                            AddToInnerTilesStateToUpdate(parentTile, parentTileInnerTile);

                            AddToPlayerStateToUpdate(currentPlayerState, 1);
                        }
                    }
                    innerTile.SelectStateByUser(currentPlayerState);
                    AddToPlayerStateToUpdate(currentPlayerState, 1);
                    parentTile.PlayerId = (long)currentPlayerState.ClientId;
                    return true;
                }
            }
            Debug.Log($"Warning: AttemptSelectByUserProcedure: undefined select InnerTile");
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
                        bool res = GameManager.Instance.CheckForWinner();
                        if (!res)
                        {
                            GameManager.Instance.OnGameEnd((PlayerState)GetCurrentPlayer());
                        }
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
                // AddToPlayerStateToUpdate(parentTile.PlayerId, currentPlayerState.Score + 1);
                // AddToInnerTilesStateToUpdate(parentTile, innerTile);
                SendDataToUpdate(currentPlayerState);
                m_playerStatesToUpdate.Clear();
                m_innerTilesToUpdate.Clear();
                NextTurn();
            }
            else
            {
                m_innerTilesToUpdate.Clear();
                SendDataToUpdate(currentPlayerState);
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
            AnimationInnerTileData[] animationInnerTileDataList = m_animationInnerTileController.GetAnimationInnerTileDataList();
            foreach (var tile in CurrentWaveSessionTiles)
            {
                tile.ClearInnerTilesSelection();
            }
            InitAnimationInnerTilesClientRpc(animationInnerTileDataList);
        }

        [ClientRpc]
        public void InitAnimationInnerTilesClientRpc(AnimationInnerTileData[] animationInnerTileDataList)
        {
            m_animationInnerTileController.DestroyAllWithCoroutine();

            foreach (AnimationInnerTileData animInnerTileData in animationInnerTileDataList)
            {
                AnimationInnerTile animInnerTile = GetAndAddAnimationInnerTile(animInnerTileData);
                tileList[animInnerTile.FromInnerTile.GetParentTile().Index].ClearInnerTilesSelection();
            }
            m_animationInnerTileController.StartMoveAll();
            
        }

        public void OnAnimationInnerTileMovementEnd()
        {
            m_animationInnerTileController.StopMoveAll();
            PlayerState currentPlayerState;
            if (IsServer)
            {
                List<AnimationInnerTile> activeAnimeInnertTiles = m_animationInnerTileController.GetUniqueAnimationInnerTiles();
                currentPlayerState = (PlayerState)GetCurrentPlayer();

                foreach (AnimationInnerTile animTile in activeAnimeInnertTiles)
                {
                    if (AttemptSelectByUserProcedure(currentPlayerState, animTile.ToInnerTile))
                    {
                        AddToPlayerStateToUpdate(animTile.PlayerId, -1);
                    }
                }
            }
            m_animationInnerTileController.DestroyAll();

            if (!IsServer) return;

            currentPlayerState = (PlayerState)GetCurrentPlayer();
            SendDataToUpdate(currentPlayerState);
            m_innerTilesToUpdate.Clear();
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

        private void SendDataToUpdate(PlayerState currentPlayerState)
        {
            List<PlayerState> tmpPlayers = new();
            foreach (var item in m_playerStatesToUpdate.Values)
            {
                tmpPlayers.Add(item);
            }
            GameManager.Instance.UpdatePlayerHealth(tmpPlayers);
            List<InnerTileData> tmp = new();
            foreach (var item in m_innerTilesToUpdate.Values)
            {
                tmp.Add(item);
            }
            SendDataToUpdateClientRpc(currentPlayerState, tmp.ToArray());
        }
        [ClientRpc]
        private void SendDataToUpdateClientRpc(PlayerState currentPlayerState, InnerTileData[] innerTileDataList)
        {
            for (int i = 0; i < innerTileDataList.Length; i++)
            {
                var tmp = tileList[innerTileDataList[i].tileIndex].GetInnerTileByDirection(innerTileDataList[i].direction);
                tmp.SelectStateByUser(currentPlayerState);
            }
        }

        public AnimationInnerTile GetAndAddAnimationInnerTile(AnimationInnerTileData animInnerTileData)
        {
            InnerTile fromInnerTile = tileList[animInnerTileData.FromInnerTileData.tileIndex].GetInnerTileByDirection(animInnerTileData.FromInnerTileData.direction);
            InnerTile toInnerTile = tileList[animInnerTileData.ToInnerTileData.tileIndex].GetInnerTileByDirection(animInnerTileData.ToInnerTileData.direction);
            return m_animationInnerTileController.GetAndAddAnimationInnerTile(animInnerTileData, fromInnerTile, toInnerTile);
        }

        public void Load(string filename = null)
        {
            if (!IsServer) return;
            string mapPath = PlayerPrefs.GetString("MapPath");
            if (mapPath.Trim().Length == 0)
            {
                Debug.Log($"Warning: Load: MapPath not found in PlayerPrefs");
                mapPath = filename; 
            }
            TileData[] tileDataList = LoadingMapManager.LoadMap(mapPath).tiles.ToArray();
            LoadClientRpc(tileDataList);
        }
        [ClientRpc]
        public void LoadClientRpc(TileData[] tileDataList)
        {
            tileList = new(tileDataList.Length);
            foreach (TileData tileData in tileDataList)
            {
                var newTile = Instantiate(m_tilePrefab, tileData.position, Quaternion.identity, transform);
                newTile.Index = tileData.index;
                tileList.Add(newTile);
                foreach (Direction key in tileData.directions)
                {
                    newTile.AddInnerTileInDirection(key);
                }
            }
            GameManager.Instance.CurrentGameStatus = GameStatus.RUNNING;
        }


        private void AddToInnerTilesStateToUpdate(Tile parentTile, InnerTile innerTile)
        {
            
            AddToInnerTilesStateToUpdate(parentTile, innerTile.direction);
        }
        private void AddToInnerTilesStateToUpdate(Tile parentTile, Direction innerTileDirection)
        {
            int index = parentTile.Index * 10 + (int)innerTileDirection;
            if (m_innerTilesToUpdate.ContainsKey(index))
            {
                var tmp = new InnerTileData(parentTile.Index, innerTileDirection);
                m_innerTilesToUpdate[index] = tmp;
            }
            else
            {
                var tmp = new InnerTileData(parentTile.Index, innerTileDirection);
                m_innerTilesToUpdate.Add(index, tmp);
            }
        }
        private void AddToPlayerStateToUpdate(long playerStateClientId, int Score = 0)
        {
            var playerState = (PlayerState)GameManager.Instance.GetPlayerByClientId((ulong)playerStateClientId);
            
            AddToPlayerStateToUpdate(playerState, Score);
        }
        private void AddToPlayerStateToUpdate(PlayerState playerState, int Score = 0)
        {
            ulong index = (ulong)playerState.ClientId;
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

    }
}
