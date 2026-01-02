using System;

[Serializable]
public class MapData
{
    public string mapId;
    public float tileSize;
    public string[] layout;
    public PortalDef[] portals;
}

[Serializable]
public class PortalDef
{
    public string triggerChar;   // "1" とか
    public string targetMapId;   // 移動先
    public int targetSpawnId;    // 向こうのスポーン位置番号
}