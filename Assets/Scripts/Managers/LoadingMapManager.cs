using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class LoadingMapManager
{
    public static float TileSize = 4f;

    public static string[] GetMapsList(string mapsDirectory = null)
    {
        if(mapsDirectory == null)
        {
            mapsDirectory = Application.dataPath + "/Maps";
        }
        if (!Directory.Exists(mapsDirectory))
        {
            Debug.Log($"Directory for Maps does not exist!");
            Directory.CreateDirectory(mapsDirectory);
        }
        return GetMapNames(mapsDirectory);
    }

    public static string[] GetMapNames(string mapsFolder)
    {
        string[] mapPaths = Directory.GetFiles(mapsFolder, "*.json");
        string[] mapNames = new string[mapPaths.Length];
        for (int i = 0; i < mapPaths.Length; i++)
        {
            mapNames[i] = Path.GetFileNameWithoutExtension(mapPaths[i]);
        }

        return mapNames;
    }
    public static void DeleteMap(string filename)
    {
        string path = Application.dataPath + "/Maps/" + filename + ".json"; ;
        if (!File.Exists(path))
        {
            Debug.Log($"File not found!");
            return;
        }
        File.Delete(path);
    }
    public static TileDataList LoadMap(string filename)
    {
        string path = Application.dataPath + "/Maps/" + filename + ".json"; ;
        if (!File.Exists(path))
        {
            Debug.Log($"File not found!");
            return new TileDataList(new List<TileData>());
        }
        string json = File.ReadAllText(path);
        TileDataList tileDataList = JsonUtility.FromJson<TileDataList>(json);
        foreach (TileData tileData in tileDataList.tiles)
        {
            tileData.position = NormalizePosition(tileData.position);
        }
        return tileDataList;
    }

    public static void SaveMap(List<TileData> tileDataList)
    {
        SaveMap(new TileDataList(tileDataList));
    }
    public static void SaveMap(TileDataList tileDataList)
    {
        string jsonData = JsonUtility.ToJson(tileDataList, true);
        string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"Map_{date}.json";
        File.WriteAllText(Application.dataPath + "/Maps/" + filename, jsonData);
    }


    public static Vector3 NormalizePosition(Vector3 oldPosition)
    {
        Vector3 normalizedPosition = new Vector2();
        normalizedPosition.x = Mathf.Round(oldPosition.x / TileSize) * TileSize;
        normalizedPosition.y = Mathf.Round(oldPosition.y / TileSize) * TileSize;
        normalizedPosition.z = 0;
        return normalizedPosition;
    }
}
