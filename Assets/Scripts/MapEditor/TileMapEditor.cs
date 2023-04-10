using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Singleplayer;

namespace TileMapEditorScene
{
    public class TileMapEditor : MonoBehaviour
    {
        public static TileMapEditor instance;

        public GameObject tilePrefab;
        public float TileSize = 4.0f;
        public List<TileData> tileDataList = new List<TileData>();
        public List<Tile> tileList = new List<Tile>();

        //[MenuItem("Window/Map Editor")]
        //public static void ShowWindow()
        //{
        //    GetWindow<TileMapEditor>("Map Editor");
        //}

        //private void OnGUI()
        //{
        //    GUILayout.Label("Map Editor", EditorStyles.boldLabel);

        //    TileSize = EditorGUILayout.FloatField("Tile Size", TileSize);

        //    if (GUILayout.Button("Create Tile"))
        //    {
        //        GameObject newTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
        //        newTile.name = "Tile";
        //    }
        //    if (GUILayout.Button("Save Map"))
        //    {
        //        SaveMap();
        //    }
        //}

        private void Awake()
        {
            instance = this;
        }

        public void SaveMap()
        {
            List<TileData> tileDataList = new List<TileData>();
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
                TileData tileData = new TileData(type, tileObject.transform.position, tileDataDirections);
                tileDataList.Add(tileData);
            }
            string jsonData = JsonUtility.ToJson(new TileDataList(tileDataList), true);
            
            Debug.Log(jsonData);
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"Map_{date}.json";
            File.WriteAllText(Application.dataPath + "/Maps/" + filename, jsonData);
        }

        public bool HasTileInPosition(Vector3 coords)
        {
            Vector2 size = new Vector2(TileSize - 0.1f, TileSize - 0.1f);
            Collider2D overlap = Physics2D.OverlapBox(coords, size, 0f);
            return overlap != null;
        }
        public void NormalizeTilesInBoard(Tile newTile)
        {
            newTile.CollectInnerTiles();
            var ownInnerTiles =  newTile.GetInnerTiles();
            //foreach (var key in ownInnerTiles.Keys)
            //{
            //    Destroy(ownInnerTiles[key].gameObject);
            //    newTile.RemoveInnerTile(key);
            //}
            var neighbourTiles = newTile.GetNeighbourTilesByDirections(new List<Direction>() { Direction.up, Direction.right, Direction.down, Direction.left, });
            List<Direction> keysToRemove = new List<Direction>();
            foreach (Direction key in ownInnerTiles.Keys)
            {
                if (!neighbourTiles.ContainsKey(key))
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                Destroy(ownInnerTiles[key].gameObject);
                newTile.RemoveInnerTile(key);
            }
            foreach (var item in neighbourTiles)
            {
                item.Value.AddInnerTileInDirection(GetOppositeDirection(item.Key));
            }
            tileList.Add(newTile);
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
        public void LeaveScene()
        {
            SceneManager.LoadScene("MainMenuScene");
        }

    }    
    
}

[System.Serializable]
public class TileData
{
    public int type;
    public Vector3 position;
    public List<Direction> directions;

    public TileData(int type, Vector3 position, List<Direction> directions)
    {
        this.type = type;
        this.position = position;
        this.directions = directions;
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