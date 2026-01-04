using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance; // どこからでも呼べるようにシングルトン化
    public MapGenerator mapGenerator;  // 既存のジェネレーター
    public string currentMapId;
    
    // 全マップデータのリスト（簡易的にInspectorで登録、またはResourcesからロード）
    public TextAsset[] mapFiles; 
    private Dictionary<string, string> _mapDatabase = new Dictionary<string, string>();

    void Awake()
    {
        Instance = this;
        // マップデータを辞書に登録 (ID -> JSONの中身)
        foreach (var file in mapFiles)
        {
            MapData data = JsonUtility.FromJson<MapData>(file.text);
            _mapDatabase[data.mapId] = file.text;
        }
    }

    void Start()
    {
        // 最初のマップをロード
        LoadMap(currentMapId, 0);
    }

    public void LoadMap(string mapId, int spawnNumber)
    {
        if (!_mapDatabase.ContainsKey(mapId))
        {
            Debug.LogError($"Map ID {mapId} not found!");
            return;
        }

        string json = _mapDatabase[mapId];
        
        // ジェネレーターに「作れ！」と命令
        mapGenerator.GenerateMapFromJson(json, spawnNumber);
    }
}