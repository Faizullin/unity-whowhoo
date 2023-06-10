using System.Collections.Generic;
using UnityEngine;
using TileMapEditor.Tiles;
using Unity.Netcode;

namespace TileMapEditor
{
    public class TileMapEditorManager : Singleton<TileMapEditorManager>
    {
        [SerializeField]
        private Tile m_tilePrefab;
        [SerializeField]
        private GameObject m_tilesListBackground;
        
        public float gridSize = 4.0f;
        public List<TileData> tileDataList = new List<TileData>();
        public List<Tile> tileList = new List<Tile>();

        private void Start()
        {
            string path = PlayerPrefs.GetString("MapPath");
            if (LoadingSceneManager.Instance.IsEdit && path.Trim().Length != 0)
            {
                
                LoadingSceneManager.Instance.IsEdit = false;
                LoadMap(path);
            }
        }

        public void SpawnInBoard(Vector3 normalizedPosition)
        {
            var newTile = Instantiate(m_tilePrefab, normalizedPosition, Quaternion.identity, m_tilesListBackground.transform);
            NormalizeTilesInBoard(newTile);
            tileList.Add(newTile);
        }

        public void NormalizeTilesInBoard(Tile newTile, bool withDelete = false)
        {
            var neighbourTiles = newTile.GetNeighbourTilesByDirections(new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left, });
            if (!withDelete)
            {
                foreach (Direction key in neighbourTiles.Keys)
                {
                    newTile.AddInnerTileInDirection(key);
                }
            }
            foreach (var item in neighbourTiles)
            {
                if (withDelete)
                {
                    item.Value.RemoveInnerTile(Directions.GetOppositeDirection(item.Key));
                }
                else
                {
                    item.Value.AddInnerTileInDirection(Directions.GetOppositeDirection(item.Key));
                }
            }
        }

        public void SaveMap()
        {
            List<TileData> tileDataList = new List<TileData>();
            int indexCounter = 0;
            foreach (Tile tileObject in tileList)
            {
                var tileDataDirections = new List<Direction>();
                int type = tileObject.GetComponent<Tile>().HasInnerTiles ? 0 : 1;
                foreach (var item in tileObject.GetInnerTiles())
                {
                    if(item.Value != null)
                    {
                        tileDataDirections.Add(item.Key);
                    }
                    
                }
                TileData tileData = new TileData(type, tileObject.transform.position, tileDataDirections, indexCounter);
                indexCounter++;
                tileDataList.Add(tileData);
            }
            LoadingMapManager.SaveMap(tileDataList);
            LeaveScene();
        }

        private void LoadMap(string path)
        {
            TileDataList tileDataList = LoadingMapManager.LoadMap(path);
            foreach (TileData tileData in tileDataList.tiles)
            {
                var newTile = Instantiate(m_tilePrefab, tileData.position, Quaternion.identity, m_tilesListBackground.transform);
                foreach (Direction key in tileData.directions)
                {
                    newTile.AddInnerTileInDirection(key);
                }
            }
        }

        public void LeaveScene()
        {
            LoadingSceneManager.Instance.LoadScene(SceneName.MainMenuScene, false);
        }


        public Tile GetTileAtPosition(Vector3 coords)
        {
            Vector2 size = new Vector2(gridSize - 0.1f, gridSize - 0.1f);
            Collider2D overlap = Physics2D.OverlapBox(coords, size, 0f, LayerMask.NameToLayer("Tile"));
            if(overlap == null)
            {
                return null;
            }
            return overlap.GetComponent<Tile>();
        }
        public bool HasTileAtPosition(Vector3 coords)
        {
            Vector2 size = new Vector2(gridSize - 0.1f, gridSize - 0.1f);
            Collider2D overlap = Physics2D.OverlapBox(coords, size, 0f, LayerMask.NameToLayer("Tile"));
            return overlap != null;
        }
        public void DeleteTileAtPosition(Vector3 coords) {
            var tile = GetTileAtPosition(coords);
            if(tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveMap();
                }
            }
        }
    }    
    
}

[System.Serializable]
public class TileData: INetworkSerializable
{
    public int type;
    public int index;
    public Vector3 position;
    public List<Direction> directions;

    public TileData()
    {
        type = 0;
        index = 0;
        position = Vector3.zero;
        directions = new List<Direction>();
    }

    public TileData(int type, Vector3 position, List<Direction> directions, int index)
    {
        this.type = type;
        this.position = position;
        this.directions = directions;
        this.index = index;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref index);
        if (serializer.IsWriter)
        {
            Direction[] directionArray = directions.ToArray();
            serializer.SerializeValue(ref directionArray);
        }
        else
        {
            Direction[] directionArray = null;
            serializer.SerializeValue(ref directionArray);
            directions = new List<Direction>(directionArray);
        }
    }
}

[System.Serializable]
public class TileDataList
{
    public List<TileData> tiles;

    public TileDataList(List<TileData> tiles)
    {
        this.tiles = tiles;
    }
}
