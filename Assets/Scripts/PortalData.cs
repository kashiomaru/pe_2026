using System;

[Serializable]
public class MapData
{
    public string mapId;
    public float tileSize;
    public string[] layout;
    public PortalDef[] portals;
    
    // マップごとのプレハブ設定（オプショナル、未指定時はMapGeneratorのデフォルトを使用）
    public string floorPrefabName;  // Resourcesフォルダからの相対パス（例: "Prefabs/Floor01"）
    public string wallPrefabName;   // Resourcesフォルダからの相対パス（例: "Prefabs/Wall01"）
}

[Serializable]
public class PortalDef
{
    public string triggerChar;   // "1" とか
    public string targetMapId;   // 移動先
    public int targetSpawnId;    // 向こうのスポーン位置番号
    
    // ドアのプレハブ設定（オプショナル、未指定時はMapGeneratorのデフォルトを使用）
    public string doorPrefabName;  // Resourcesフォルダからの相対パス（例: "Prefabs/Door01"）
}